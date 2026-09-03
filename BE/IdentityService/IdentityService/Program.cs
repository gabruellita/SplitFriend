using FluentValidation;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using IdentityService.Infrastructure.Configuration;
using IdentityService.Infrastructure.Interfaces;
using IdentityService.API.Middleware;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using Serilog.Events;

// ── Npgsql 7+ compat: CommandType.StoredProcedure foloseste implicit `CALL fn(..)`,
// care in Postgres merge doar pe PROCEDURE-uri reale. Procedurile noastre sunt
// FUNCTION-uri (CREATE FUNCTION + RETURNS TABLE), deci revenim la `SELECT * FROM fn(..)`.
AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);

// ── Dapper: mapare snake_case ↔ PascalCase (PRIMUL apel, inainte de DI) ──────
DapperConfiguration.Configure();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "IdentityService")
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "IdentityService"));

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ──────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FinanceApp — Identity Service",
        Version     = "v1",
        Description = "API de autentificare: register, login, confirm-email, refresh token, logout."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Introdu JWT-ul astfel: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── FluentValidation ───────────────────────────────────────────────────────────
// Auto-validare dezactivata: validatorii cu MustAsync nu pot rula sincron in pipeline-ul MVC.
// Fiecare controller apeleaza ValidateAsync() manual.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── JWT Authentication ─────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:SecretKey lipsește sau este goală (seteaz-o prin user-secrets).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// ── Redis ──────────────────────────────────────────────────────────────────────
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName  = "IdentityService:";
});

// ── JwtSettings bindat din appsettings.json ───────────────────────────────────
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// ── DI — Infrastructure layer ─────────────────────────────────────────────────
builder.Services.AddScoped<IDbConnectionFactory, DatabaseConnectionFactory>();
builder.Services.AddSingleton<IJwtTokenService,     JwtTokenService>();

// ── DI — identitatea curenta din header-ul X-User-Id injectat de Gateway ──────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IdentityService.API.Security.ICurrentUser, IdentityService.API.Security.CurrentUser>();

// ── DI — Services layer (Repositories) ───────────────────────────────────────
builder.Services.AddScoped<IUserRepository,         UserRepository>();
builder.Services.AddScoped<CurrencyRepository>();
builder.Services.AddScoped<ICurrencyRepository>(sp =>
    new CachedCurrencyRepository(
        sp.GetRequiredService<CurrencyRepository>(),
        sp.GetRequiredService<IDistributedCache>()));
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

// ── DI — Notification Service (typed HttpClient catre :5005) ──────────────────
builder.Services.AddHttpClient<IdentityService.Infrastructure.Notifications.INotificationClient,
                               IdentityService.Infrastructure.Notifications.NotificationClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Notification:BaseUrl"] ?? "http://localhost:5005");
    c.Timeout     = TimeSpan.FromSeconds(5);
});

// ── DI — Services layer (Business Logic) ─────────────────────────────────────
builder.Services.AddScoped<IAuthService,    AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IEmailService,   EmailService>();

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres", tags: ["ready"]);

// ── CORS (frontend React pe port 5173) ────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health/live",  new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapControllers();

app.Run();
