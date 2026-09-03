using ExportService.DTO.Upstream;
using ScottPlot;

namespace ExportService.Infrastructure.Charts;

public class ChartRenderer : IChartRenderer
{
    private const int Width  = 900;
    private const int Height = 360;

    public byte[] RenderTrend(IReadOnlyList<TimeseriesPoint> points)
    {
        var plot = new Plot();

        // grupeaza pe data, separa pe kind
        var income  = points.Where(p => p.Kind == "INCOME").OrderBy(p => p.Bucket).ToList();
        var expense = points.Where(p => p.Kind == "EXPENSE").OrderBy(p => p.Bucket).ToList();

        if (income.Count > 0)
        {
            var xs = income.Select(p => p.Bucket.ToDateTime(TimeOnly.MinValue).ToOADate()).ToArray();
            var ys = income.Select(p => (double)p.Total).ToArray();
            var s  = plot.Add.Scatter(xs, ys);
            s.LegendText = "Venituri";
            s.Color      = Colors.Green;
        }
        if (expense.Count > 0)
        {
            var xs = expense.Select(p => p.Bucket.ToDateTime(TimeOnly.MinValue).ToOADate()).ToArray();
            var ys = expense.Select(p => (double)p.Total).ToArray();
            var s  = plot.Add.Scatter(xs, ys);
            s.LegendText = "Cheltuieli";
            s.Color      = Colors.Red;
        }

        plot.Axes.DateTimeTicksBottom();
        plot.ShowLegend();

        // ScottPlot 5.1.x: GetImageBytes(width, height, format) direct pe Plot
        return plot.GetImageBytes(Width, Height, ImageFormat.Png);
    }

    public byte[] RenderCategoryBreakdown(IReadOnlyList<CategorySlice> slices)
    {
        var plot = new Plot();
        var top  = slices.OrderByDescending(s => s.Total).Take(8).ToList();

        var bars = new List<Bar>();
        for (int i = 0; i < top.Count; i++)
            bars.Add(new Bar { Position = i, Value = (double)top[i].Total });

        plot.Add.Bars(bars);

        // SetTicks(positions, labels) disponibil pe IAxis in 5.1.x
        var positions = Enumerable.Range(0, top.Count).Select(i => (double)i).ToArray();
        var labels    = top.Select(s => s.CategoryName ?? "(fara)").ToArray();
        plot.Axes.Bottom.SetTicks(positions, labels);

        return plot.GetImageBytes(Width, Height, ImageFormat.Png);
    }
}
