using System.Security.Claims;
using GateWay.API.Middleware;
using Microsoft.AspNetCore.Http;

namespace GateWay.Tests;

/// <summary>
/// Teste pentru propagarea identitatii si protectia anti-falsificare
/// (ForwardClaimsMiddleware, sectiunea 4.3.1 / 5.3.1). Acestea valideaza decizia
/// de securitate centrala: un client nu poate forja antetele X-User-* fara token valid.
/// </summary>
public class ForwardClaimsMiddlewareTests
{
    private static HttpContext AuthenticatedContext(params (string type, string value)[] claims)
    {
        var identity = new ClaimsIdentity(
            claims.Select(c => new Claim(c.type, c.value)), authenticationType: "TestJwt");
        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    private static HttpContext AnonymousContext()
        => new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }; // neautentificat

    private static ForwardClaimsMiddleware Middleware()
        => new(_ => Task.CompletedTask); // next no-op

    [Fact]
    public async Task CerereAutentificata_InjecteazaAnteteleXUserDinClaims()
    {
        var ctx = AuthenticatedContext(
            ("sub", "42"), ("email", "user@test.ro"), ("status", "ACTIVE"), ("currency", "1"));

        await Middleware().InvokeAsync(ctx);

        Assert.Equal("42", ctx.Request.Headers["X-User-Id"]);
        Assert.Equal("user@test.ro", ctx.Request.Headers["X-User-Email"]);
        Assert.Equal("ACTIVE", ctx.Request.Headers["X-User-Status"]);
        Assert.Equal("1", ctx.Request.Headers["X-User-Currency"]);
    }

    [Fact]
    public async Task CerereAutentificata_SuprascrieAntetulForjatDeClient()
    {
        var ctx = AuthenticatedContext(("sub", "42"));
        ctx.Request.Headers["X-User-Id"] = "999"; // client rau-intentionat incearca sa se dea drept user 999

        await Middleware().InvokeAsync(ctx);

        Assert.Equal("42", ctx.Request.Headers["X-User-Id"]); // valoarea reala din JWT castiga
    }

    [Fact]
    public async Task CerereNeautentificata_StergeAnteteleXUserTrimiseDeClient()
    {
        var ctx = AnonymousContext();
        ctx.Request.Headers["X-User-Id"] = "1";       // tentativa de spoofing fara token
        ctx.Request.Headers["X-User-Email"] = "admin@test.ro";

        await Middleware().InvokeAsync(ctx);

        Assert.False(ctx.Request.Headers.ContainsKey("X-User-Id"));
        Assert.False(ctx.Request.Headers.ContainsKey("X-User-Email"));
    }

    [Fact]
    public async Task ApeleazaUrmatorulMiddleware()
    {
        var called = false;
        var mw = new ForwardClaimsMiddleware(_ => { called = true; return Task.CompletedTask; });

        await mw.InvokeAsync(AnonymousContext());

        Assert.True(called);
    }
}
