using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StatisticsService.API.Swagger;

/// <summary>Adauga header-ul X-User-Id la fiecare endpoint in Swagger (testare directa fara Gateway).</summary>
public class XUserIdHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name        = "X-User-Id",
            In          = ParameterLocation.Header,
            Required    = false,
            Description = "ID utilizator (doar pentru testare directa; injectat normal de Gateway).",
            Schema      = new OpenApiSchema { Type = "integer", Format = "int64" }
        });
    }
}
