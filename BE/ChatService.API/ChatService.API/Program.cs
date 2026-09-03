using ChatService.API.Hubs;
using ChatService.API.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ChatService.API.Swagger;
using ChatService.Infrastructure;
using ChatService.Infrastructure.Repositories;
using ChatService.Infrastructure.Repositories.Interfaces;
using ChatService.Infrastructure.Security;
using ChatService.Services;
using ChatService.Services.Interfaces;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// ── Npgsql 7+ compat: SP-urile sunt FUNCTION-uri ──
AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);

// ── Dapper snake_case ──
DapperConfiguration.Configure();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ChatService")
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ChatService"));

builder.Services.AddControllers();

// ── SignalR cu Redis backplane ──
var redisConn = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' lipseste.");
builder.Services.AddSignalR().AddStackExchangeRedis(redisConn, o =>
{
    o.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("chat-signalr");
});

builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
    _ => StackExchange.Redis.ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();
builder.Services.AddSingleton<IUnreadService, UnreadService>();

// ── Swagger ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FinanceApp — Chat Service",
        Version     = "v1",
        Description = "Mesagerie real-time pe grupuri (SignalR + Redis). REST pentru istoric/prezenta/unread."
    });
    options.OperationFilter<XUserIdHeaderFilter>();
});

// ── DI — Infrastructure ──
builder.Services.AddScoped<IDbConnectionFactory, DatabaseConnectionFactory>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IGroupMembershipRepository, GroupMembershipRepository>();

// ── DI — Services ──
builder.Services.AddScoped<IChatService, ChatAppService>();

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres", tags: ["ready"])
    .AddRedis(redisConn, name: "redis", tags: ["ready"]);

// ── CORS ──
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat Service v1");
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
app.MapHub<ChatHub>("/api/hubs/chat");

app.Run();
