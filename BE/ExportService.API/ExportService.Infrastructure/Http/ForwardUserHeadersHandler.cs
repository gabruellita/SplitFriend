using Microsoft.AspNetCore.Http;

namespace ExportService.Infrastructure.Http;

/// <summary>
/// Copiaza X-User-Id si X-User-Currency de pe request-ul curent pe apelurile catre
/// Statistics/Finance, ca acele servicii sa agrege pentru utilizatorul corect.
/// </summary>
public class ForwardUserHeadersHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var ctx = accessor.HttpContext;
        if (ctx is not null)
        {
            CopyHeader(ctx, request, "X-User-Id");
            CopyHeader(ctx, request, "X-User-Currency");
            CopyHeader(ctx, request, "X-User-Email");
            CopyHeader(ctx, request, "X-User-Status");
        }
        return base.SendAsync(request, ct);
    }

    private static void CopyHeader(HttpContext ctx, HttpRequestMessage request, string name)
    {
        var value = ctx.Request.Headers[name].FirstOrDefault();
        if (!string.IsNullOrEmpty(value))
        {
            request.Headers.Remove(name);
            request.Headers.TryAddWithoutValidation(name, value);
        }
    }
}
