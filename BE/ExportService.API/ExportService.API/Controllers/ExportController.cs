using ExportService.API.Validators;
using ExportService.DTO.Requests;
using ExportService.Infrastructure.Security;
using ExportService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExportService.API.Controllers;

[ApiController]
[Route("api/export")]
public class ExportController(IReportService reports, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Genereaza raportul PDF si il returneaza ca attachment.</summary>
    [HttpPost("report")]
    public async Task<IActionResult> Report([FromBody] ExportReportRequest request, CancellationToken ct)
    {
        var validator  = new ExportReportRequestValidator();
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var currencyCode = Request.Headers["X-User-Currency-Code"].FirstOrDefault() ?? "RON";
        var userLabel    = Request.Headers["X-User-Email"].FirstOrDefault() ?? $"Utilizator #{currentUser.UserId}";

        var (pdf, fileName) = await reports.GenerateAsync(request, currencyCode, userLabel, ct);
        return File(pdf, "application/pdf", fileName);
    }
}
