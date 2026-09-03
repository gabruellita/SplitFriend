using System.Text;
using System.Threading.RateLimiting;
using GateWay.API.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using GateWay.Infrastructure.Extensions;
using GateWay.Services.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "Gateway")
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "Gateway"));

// ── Infrastructure layer ──────────────────────────────────────────────────────
// Înregistrează: JwtSettings, GatewaySettings, IRouteConfigRepository, IClusterConfigRepository
builder.Services.AddGatewayInfrastructure(builder.Configuration);

// ── Services layer ────────────────────────────────────────────────────────────
// Înregistrează: IRouteConfigService
builder.Services.AddGatewayServices();

// ── JWT — validare centralizată ───────────────────────────────────────────────
// Microserviciile downstream NU mai validează JWT-ul.
// Gateway-ul validează, extrage claims și le propagă prin headere X-User-*.
var jwtKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:SecretKey lipsește sau este goală (seteaz-o prin user-secrets).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        // Pastreaza numele originale ale claim-urilor ("sub", "email", ...) — fara
        // re-maparea implicita la URI-urile lungi ClaimTypes.*. ForwardClaimsMiddleware
        // citeste ctx.User.FindFirst("sub")/("email"), deci au nevoie de numele scurte.
        opt.MapInboundClaims = false;

        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero   // fără toleranță la expirare
        };

        // SignalR (WebSocket) nu poate trimite header Authorization din browser →
        // citeste JWT-ul din query string `access_token` pe caile de hub chat.
        opt.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/chat/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── YARP — citește rutele și clusterele din appsettings.json ──────────────────
// Rutele marcate cu AuthorizationPolicy: "RequireAuthenticatedUser" sunt
// protejate automat de ASP.NET Core Authorization înainte de proxy.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── CORS centralizat — o singură politică pentru tot frontend-ul ──────────────
// Microserviciile downstream NU mai au CORS propriu.
var allowedOrigins = builder.Configuration
    .GetSection("Gateway:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173"];

builder.Services.AddCors(o => o.AddPolicy("FrontendPolicy", p =>
    p.WithOrigins(allowedOrigins)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── Rate limiting — politica stricta pe rutele de auth (anti brute-force) ─────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Sliding window, partitionat pe IP client. ~10 req/min pe rutele de auth.
    options.AddPolicy("auth-strict", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit          = 10,
                Window               = TimeSpan.FromMinutes(1),
                SegmentsPerWindow    = 6,
                QueueLimit           = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    // Raspuns 429 in forma standard a aplicatiei { "error": "..." } + Retry-After.
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        // Aceeasi forma { "error": "..." } ca GlobalExceptionMiddleware (WriteAsJsonAsync seteaza content-type).
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Prea multe incercari. Reincearca mai tarziu." }, token);
    };
});

var app = builder.Build();

// ── Middleware pipeline — ORDINEA ESTE CRITICĂ ────────────────────────────────
app.UseMiddleware<CorrelationIdMiddleware>();      // 1. X-Correlation-Id + LogContext (outermost)
app.UseSerilogRequestLogging();                    // 1b. Un log-summary per request
app.UseMiddleware<GlobalExceptionMiddleware>();    // 1c. Prinde excepții — cu CorrelationId în context
app.UseCors("FrontendPolicy");                     // 2. CORS înainte de auth
app.UseAuthentication();                          // 3. Parsează JWT → populează ctx.User
app.UseAuthorization();                           // 4. Verifică AuthorizationPolicy pe rute
app.UseMiddleware<ForwardClaimsMiddleware>();      // 5. Injectează X-User-* în request headers
app.UseRateLimiter();                             // 5b. Aplica politicile de rate limiting
app.MapHealthChecks("/health/live",  new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapReverseProxy();                            // 6. Proxy-iază cererea downstream

app.Run();
