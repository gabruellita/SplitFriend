using FinanceService.API.Middleware;
using FinanceService.API.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FinanceService.Infrastructure;
using FinanceService.Infrastructure.Notifications;
using FinanceService.Infrastructure.Repositories;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Infrastructure.Security;
using FinanceService.Services;
using FinanceService.Services.Interfaces;
using FluentValidation;
using Microsoft.OpenApi.Models;
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
    .Enrich.WithProperty("ServiceName", "FinanceService")
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "FinanceService"));

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ──────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FinanceApp — Finance Service",
        Version     = "v1",
        Description = "Tranzactii personale (venituri/cheltuieli), categorii si template-uri recurente. " +
                      "In mod normal apelat prin Gateway (:5010); X-User-Id e injectat de acolo."
    });
    // Permite testarea directa pe :5002 — adauga header-ul X-User-Id la fiecare endpoint.
    options.OperationFilter<XUserIdHeaderFilter>();
});

// ── FluentValidation (validare manuala in controllere) ──────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── DI — Infrastructure ─────────────────────────────────────────────────────
builder.Services.AddScoped<IDbConnectionFactory, DatabaseConnectionFactory>();

// CurrentUser — scoped, populat de CurrentUserMiddleware din header-ele X-User-*
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());

// ── DI — Repositories ────────────────────────────────────────────────────────
builder.Services.AddScoped<ICategoryRepository,          CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository,       TransactionRepository>();
builder.Services.AddScoped<IRecurringTemplateRepository, RecurringTemplateRepository>();
builder.Services.AddScoped<ICurrencyRepository,          CurrencyRepository>();
builder.Services.AddScoped<IGroupRepository,             GroupRepository>();
builder.Services.AddScoped<IGroupExpenseRepository,      GroupExpenseRepository>();
builder.Services.AddScoped<IPaymentRepository,           PaymentRepository>();

// ── DI — Services ────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICategoryService,          CategoryService>();
builder.Services.AddScoped<ITransactionService,       TransactionService>();
builder.Services.AddScoped<IRecurringTemplateService, RecurringTemplateService>();
builder.Services.AddScoped<IRecurringGenerationEngine, RecurringGenerationEngine>();
builder.Services.AddScoped<IGroupService,              GroupService>();
builder.Services.AddScoped<IGroupExpenseService,       GroupExpenseService>();
builder.Services.AddScoped<IPaymentService,            PaymentService>();

// ── HttpClient tipizat catre Notification Service (best-effort, intern) ──
builder.Services.AddHttpClient<INotificationClient, NotificationClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Notification:BaseUrl"] ?? "http://localhost:5005");
    client.Timeout     = TimeSpan.FromSeconds(5);
});

// ── HttpClient tipizat catre Currency Service (curs de schimb autoritar) ──
builder.Services.AddHttpClient<FinanceService.Infrastructure.Exchange.IExchangeRateClient,
                               FinanceService.Infrastructure.Exchange.ExchangeRateClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Currency:BaseUrl"] ?? "http://localhost:5006");
    c.Timeout     = TimeSpan.FromSeconds(5);
});
builder.Services.AddScoped<FinanceService.Infrastructure.Repositories.Interfaces.ICurrencyLookupRepository,
                           FinanceService.Infrastructure.Repositories.CurrencyLookupRepository>();

// ── Background jobs ───────────────────────────────────────────────────────────
// Cadenta jobului (pornire + interval) vine din sectiunea "RecurringJob";
// default = productie (30s / 24h), scurtata in appsettings.Development.json pentru testare.
builder.Services.Configure<FinanceService.API.Jobs.RecurringJobOptions>(
    builder.Configuration.GetSection(FinanceService.API.Jobs.RecurringJobOptions.SectionName));
builder.Services.AddHostedService<FinanceService.API.Jobs.RecurringGenerationJob>();

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres", tags: ["ready"]);

// ── CORS (paritate cu Identity; in mod normal CORS e centralizat in Gateway) ──
builder.Services.AddCors(options =>
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var app = builder.Build();

// ── Middleware pipeline ──────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance Service v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();   // 1. Prinde orice exceptie
app.UseCors("FrontendPolicy");                    // 2. CORS
app.UseMiddleware<CurrentUserMiddleware>();        // 3. Extrage X-User-Id → ICurrentUser
app.MapHealthChecks("/health/live",  new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapControllers();                             // 4. Controllere

app.Run();
