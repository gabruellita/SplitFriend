using System.IdentityModel.Tokens.Jwt;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Configuration;
using IdentityService.Infrastructure.Models;
using Microsoft.Extensions.Options;

namespace IdentityService.Tests;

/// <summary>
/// Teste pentru emiterea token-urilor (JwtTokenService, sectiunea 4.5.1):
/// continutul revendicarilor (claims), expirarea de 15 minute si token-ul de reimprospatare opac.
/// </summary>
public class JwtTokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        SecretKey = "cheie-secreta-de-test-suficient-de-lunga-pentru-HS256-1234567890",
        Issuer = "FinanceApp",
        Audience = "FinanceAppClient",
        ExpiryMinutes = 15
    };

    private static JwtTokenService NewService() => new(Options.Create(Settings));

    private static User SampleUser() => new()
    {
        Id = 42, Email = "user@test.ro", Username = "tester",
        PreferredCurrencyId = 1, Status = "ACTIVE"
    };

    [Fact]
    public void AccessToken_ContineRevendicarileDeIdentitate()
    {
        var token = NewService().GenerateAccessToken(SampleUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("42", jwt.Claims.First(c => c.Type == "sub").Value);
        Assert.Equal("user@test.ro", jwt.Claims.First(c => c.Type == "email").Value);
        Assert.Equal("tester", jwt.Claims.First(c => c.Type == "username").Value);
        Assert.Equal("1", jwt.Claims.First(c => c.Type == "currency").Value);
        Assert.Equal("ACTIVE", jwt.Claims.First(c => c.Type == "status").Value);
        Assert.Contains(jwt.Claims, c => c.Type == "jti"); // identificator unic token
    }

    [Fact]
    public void AccessToken_AreIssuerSiAudienceConfigurate()
    {
        var token = NewService().GenerateAccessToken(SampleUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("FinanceApp", jwt.Issuer);
        Assert.Contains("FinanceAppClient", jwt.Audiences);
    }

    [Fact]
    public void AccessToken_ExpiraInAproximativ15Minute()
    {
        var token = NewService().GenerateAccessToken(SampleUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var minutes = (jwt.ValidTo - DateTime.UtcNow).TotalMinutes;
        Assert.InRange(minutes, 14, 15.5); // 15 min, cu mica toleranta de executie
    }

    [Fact]
    public void RefreshToken_EsteOpacSiUnic()
    {
        var svc = NewService();
        var t1 = svc.GenerateRefreshToken();
        var t2 = svc.GenerateRefreshToken();

        Assert.NotEqual(t1, t2);                                   // aleator
        Assert.Equal(88, t1.Length);                               // 64 octeti in Base64
        Assert.False(t1.Contains('.'));                            // NU este JWT (n-are puncte)
    }
}
