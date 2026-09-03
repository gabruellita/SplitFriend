using ExportService.DTO.Models;

namespace ExportService.Infrastructure.Pdf;

public interface IPdfReportBuilder
{
    byte[] Build(ReportModel model);
}
