using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ChatService.API.Swagger;

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
            Description = "ID utilizator (testare directa; injectat normal de Gateway).",
            Schema      = new OpenApiSchema { Type = "integer", Format = "int64" }
        });
    }
}
