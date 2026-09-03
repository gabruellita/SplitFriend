namespace FinanceService.API.Jobs;

/// <summary>
/// Config pentru <see cref="RecurringGenerationJob"/>. Valorile default reproduc
/// comportamentul de productie (porneste la 30s dupa boot, ruleaza din 24h in 24h).
/// In dev se suprascriu din appsettings.Development.json (ex. 5s / 30s) ca sa observi
/// mai multe cicluri rapid — "butonul de interval scurt" pentru testare.
/// </summary>
public class RecurringJobOptions
{
    public const string SectionName = "RecurringJob";

    /// <summary>Intarziere la pornire inainte de prima rulare (secunde). Default 30.</summary>
    public double StartupDelaySeconds { get; set; } = 30;

    /// <summary>Interval intre rulari (secunde). Default 86400 = 24h.</summary>
    public double IntervalSeconds { get; set; } = 86400;
}
