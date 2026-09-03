using ExportService.DTO.Requests;

namespace ExportService.Services.Interfaces;

public interface IReportService
{
    /// <summary>Construieste PDF-ul si returneaza (continut, numeFisier).</summary>
    Task<(byte[] Pdf, string FileName)> GenerateAsync(ExportReportRequest request, string currencyCode, string userLabel, CancellationToken ct = default);
}
