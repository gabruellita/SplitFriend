using ExportService.API.Middleware;
using ExportService.API.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ExportService.Infrastructure.Charts;
using ExportService.Infrastructure.Clients;
using ExportService.Infrastructure.Clients.Interfaces;
using ExportService.Infrastructure.Http;
using ExportService.Infrastructure.Pdf;
using ExportService.Infrastructure.Security;
using ExportService.Services;
using ExportService.Services.Interfaces;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;

// QuestPDF — licenta Community (gratuita pentru proiect de licenta)
QuestPDF.Settings.License = LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ExportService")
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ExportService"));

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FinanceApp — Export Service", Version = "v1",
        Description = "Genereaza rapoarte PDF. Apelat prin Gateway (:5010); X-User-* injectate de acolo. " +
                      "Are nevoie de Statistics (:5004) si Finance (:5002) pornite."
    });
    options.OperationFilter<XUserIdHeaderFilter>();
});

// Identitate per-request
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());

// DelegatingHandler care forwardeaza X-User-* downstream
builder.Services.AddTransient<ForwardUserHeadersHandler>();

// Clienti tipizati catre Statistics + Finance
builder.Services.AddHttpClient<IStatisticsClient, StatisticsClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Statistics:BaseUrl"] ?? "http://localhost:5004");
    c.Timeout     = TimeSpan.FromSeconds(15);
}).AddHttpMessageHandler<ForwardUserHeadersHandler>();

builder.Services.AddHttpClient<IFinanceClient, FinanceClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Finance:BaseUrl"] ?? "http://localhost:5002");
    c.Timeout     = TimeSpan.FromSeconds(15);
}).AddHttpMessageHandler<ForwardUserHeadersHandler>();

// Randare + orchestrare
builder.Services.AddSingleton<IChartRenderer, ChartRenderer>();
builder.Services.AddSingleton<IPdfReportBuilder, PdfReportBuilder>();
builder.Services.AddScoped<IReportService, ReportService>();

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// CORS (paritate; centralizat in Gateway)
builder.Services.AddCors(options =>
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Export Service v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("FrontendPolicy");
app.UseMiddleware<CurrentUserMiddleware>();
app.MapHealthChecks("/health/live",  new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapControllers();

app.Run();
