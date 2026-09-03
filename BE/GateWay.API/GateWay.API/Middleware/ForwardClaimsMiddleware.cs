namespace GateWay.API.Middleware;

/// <summary>
/// Extrage claims-urile JWT ale utilizatorului autentificat și le injectează
/// ca headere HTTP custom în cererea trimisă downstream (X-User-Id, X-User-Email, etc.).
///
/// Securitate anti-spoofing:
/// - Pentru cereri AUTENTIFICATE: suprascrie headerele X-User-* cu valorile extrase din JWT
///   (clientul nu poate falsifica identitatea — Gateway-ul are ultimul cuvânt)
/// - Pentru cereri NEAUTENTIFICATE: șterge orice header X-User-* trimis de client
///   → previne un atacator care ar injecta manual "X-User-Id: 1" fără token valid
///
/// Notă: Middleware-ul trebuie plasat DUPĂ UseAuthentication() și UseAuthorization()
/// pentru ca ctx.User să fie deja populat cu claims-urile din JWT.
/// </summary>
public class ForwardClaimsMiddleware(RequestDelegate next)
{
    // Headerele custom propagate către microserviciile downstream
    private static readonly string[] UserHeaders =
    [
        "X-User-Id",
        "X-User-Email",
        "X-User-Status",
        "X-User-Currency"
    ];

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            // Suprascrie cu valorile din JWT — ignora ce a trimis clientul
            ctx.Request.Headers["X-User-Id"]       = ctx.User.FindFirst("sub")?.Value;
            ctx.Request.Headers["X-User-Email"]    = ctx.User.FindFirst("email")?.Value;
            ctx.Request.Headers["X-User-Status"]   = ctx.User.FindFirst("status")?.Value;
            ctx.Request.Headers["X-User-Currency"] = ctx.User.FindFirst("currency")?.Value;
        }
        else
        {
            // Anti-spoofing: elimina orice header X-User-* pentru cereri neautentificate
            foreach (var header in UserHeaders)
                ctx.Request.Headers.Remove(header);
        }

        await next(ctx);
    }
}
