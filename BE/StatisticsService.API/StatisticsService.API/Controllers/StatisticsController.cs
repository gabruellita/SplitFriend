using Microsoft.AspNetCore.Mvc;
using StatisticsService.Services.Interfaces;

namespace StatisticsService.API.Controllers;

[ApiController]
[Route("api/statistics")]
[Produces("application/json")]
public class StatisticsController(IStatsService service) : ControllerBase
{
    /// <summary>Evolutie venituri vs cheltuieli in timp (granularity: day/week/month/year).</summary>
    [HttpGet("timeseries")]
    public async Task<IActionResult> Timeseries([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? granularity)
        => Ok(await service.TimeseriesAsync(from, to, granularity));

    /// <summary>Breakdown pe categorii pentru un tip (kind: INCOME/EXPENSE).</summary>
    [HttpGet("category-breakdown")]
    public async Task<IActionResult> CategoryBreakdown([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string kind)
        => Ok(await service.CategoryBreakdownAsync(from, to, kind));

    /// <summary>Top N categorii + procent din total.</summary>
    [HttpGet("top-categories")]
    public async Task<IActionResult> TopCategories([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string kind, [FromQuery] int? limit)
        => Ok(await service.TopCategoriesAsync(from, to, kind, limit));

    /// <summary>Heatmap calendar (densitate tranzactii pe zile).</summary>
    [HttpGet("calendar")]
    public async Task<IActionResult> Calendar([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        => Ok(await service.CalendarAsync(from, to));

    /// <summary>Histogram distributie sume (max + buckets).</summary>
    [HttpGet("histogram")]
    public async Task<IActionResult> Histogram([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] decimal? max, [FromQuery] int? buckets)
        => Ok(await service.HistogramAsync(from, to, max, buckets));

    /// <summary>Rata de economisire pe luna.</summary>
    [HttpGet("savings-rate")]
    public async Task<IActionResult> SavingsRate([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        => Ok(await service.SavingsRateAsync(from, to));

    /// <summary>Sold cumulativ in timp.</summary>
    [HttpGet("running-balance")]
    public async Task<IActionResult> RunningBalance([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        => Ok(await service.RunningBalanceAsync(from, to));

    /// <summary>Comparatie MoM / YoY (kind + granularity month/year).</summary>
    [HttpGet("mom")]
    public async Task<IActionResult> MoM([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string kind, [FromQuery] string? granularity)
        => Ok(await service.MoMAsync(from, to, kind, granularity));

    /// <summary>Pareto 80/20 pe categorii de cheltuieli.</summary>
    [HttpGet("pareto")]
    public async Task<IActionResult> Pareto([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        => Ok(await service.ParetoAsync(from, to));

    /// <summary>Cheltuieli pe ziua saptamanii.</summary>
    [HttpGet("weekday")]
    public async Task<IActionResult> Weekday([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string kind)
        => Ok(await service.WeekdayAsync(from, to, kind));

    /// <summary>Recurente vs spontane.</summary>
    [HttpGet("recurring-split")]
    public async Task<IActionResult> RecurringSplit([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string kind)
        => Ok(await service.RecurringSplitAsync(from, to, kind));
}
