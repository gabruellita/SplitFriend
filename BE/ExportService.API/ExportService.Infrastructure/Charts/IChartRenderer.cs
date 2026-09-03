using ExportService.DTO.Upstream;

namespace ExportService.Infrastructure.Charts;

public interface IChartRenderer
{
    /// <summary>Grafic evolutie venituri vs cheltuieli (doua serii) → PNG.</summary>
    byte[] RenderTrend(IReadOnlyList<TimeseriesPoint> points);

    /// <summary>Defalcare cheltuieli pe categorii (bar orizontal) → PNG.</summary>
    byte[] RenderCategoryBreakdown(IReadOnlyList<CategorySlice> slices);
}
