using StatisticsService.API.Middleware;
using StatisticsService.API.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using StatisticsService.Infrastructure;
using StatisticsService.Infrastructure.Repositories;
using StatisticsService.Infrastructure.Repositories.Interfaces;
using StatisticsService.Infrastructure.Security;
using StatisticsService.Services;
using StatisticsService.Services.Interfaces;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// ── Npgsql 7+ compat: SP-urile noastre sunt FUNCTION-uri, nu PROCEDURE → SELECT * FROM fn(..)
AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);

// ── Dapper: snake_case ↔ PascalCase (PRIMUL apel, inainte de DI) ──
DapperConfiguration.Configure();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "StatisticsService")
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "StatisticsService"));

builder.Services.AddControllers();

// ── Swagger ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FinanceApp — Statistics Service",
        Version     = "v1",
        Description = "Agregari read-only pentru graficele aplicatiei. Apelat normal prin Gateway (:5010); " +
                      "X-User-Id e injectat de acolo."
    });
    options.OperationFilter<XUserIdHeaderFilter>();
});

// ── Redis (IDistributedCache) — caching rezultate agregari ──
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName  = "stats:";
});

// ── DI — Infrastructure ──
builder.Services.AddScoped<IDbConnectionFactory, DatabaseConnectionFactory>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
builder.Services.AddScoped<IStatsRepository, StatsRepository>();

// ── DI — Services ──
builder.Services.AddScoped<IStatsService, StatsService>();

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres", tags: ["ready"])
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis", tags: ["ready"]);

// ── CORS (paritate; in mod normal centralizat in Gateway) ──
builder.Services.AddCors(options =>
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Statistics Service v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();   // 1. exceptii → status
app.UseCors("FrontendPolicy");                    // 2. CORS
app.UseMiddleware<CurrentUserMiddleware>();        // 3. X-User-Id → ICurrentUser
app.MapHealthChecks("/health/live",  new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapControllers();                             // 4. controllere

app.Run();
