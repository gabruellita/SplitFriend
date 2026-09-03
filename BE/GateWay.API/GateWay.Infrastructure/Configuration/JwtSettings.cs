namespace GateWay.Infrastructure.Configuration;

/// <summary>
/// Model de configurare pentru JWT — identic cu cel din IdentityService.
/// Gateway-ul validează centralizat token-ul; microserviciile downstream
/// primesc identitatea prin headere X-User-* injectate de ForwardClaimsMiddleware.
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer    { get; set; } = string.Empty;
    public string Audience  { get; set; } = string.Empty;
}
