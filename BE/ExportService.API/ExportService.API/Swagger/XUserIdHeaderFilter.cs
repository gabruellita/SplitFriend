using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ExportService.API.Swagger;

/// <summary>Adauga X-User-Id si X-User-Currency in Swagger (testare directa fara Gateway).</summary>
public class XUserIdHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-User-Id", In = ParameterLocation.Header, Required = false,
            Description = "ID utilizator (doar testare directa; injectat normal de Gateway).",
            Schema = new OpenApiSchema { Type = "integer", Format = "int64" }
        });
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-User-Currency", In = ParameterLocation.Header, Required = false,
            Description = "ID moneda preferata (doar testare directa).",
            Schema = new OpenApiSchema { Type = "integer", Format = "int64" }
        });
    }
}
