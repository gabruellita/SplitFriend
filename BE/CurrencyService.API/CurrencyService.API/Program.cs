using CurrencyService.API.Middleware;
using CurrencyService.Infrastructure.Cache;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using CurrencyService.Infrastructure.Frankfurter;
using CurrencyService.Services;
using CurrencyService.Services.Interfaces;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "CurrencyService")
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "CurrencyService"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "FinanceApp — Currency Service", Version = "v1",
    Description = "Curs valutar (Frankfurter.app) cu cache Redis. Rute publice."
}));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Redis (acelasi container ca restul serviciilor)
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration["Redis:ConnectionString"];
    o.InstanceName  = "CurrencyService:";
});

// Typed HttpClient catre Frankfurter
builder.Services.AddHttpClient<IFrankfurterClient, FrankfurterClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Frankfurter:BaseUrl"] ?? "https://api.frankfurter.dev/v1/");
    c.Timeout     = TimeSpan.FromSeconds(5);
});

builder.Services.AddScoped<IRateCache, RedisRateCache>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!, name: "redis", tags: ["ready"]);

builder.Services.AddCors(o => o.AddPolicy("FrontendPolicy", p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Currency Service v1"); c.RoutePrefix = "swagger"; });
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("FrontendPolicy");
app.MapHealthChecks("/health/live",  new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapControllers();
app.Run();
