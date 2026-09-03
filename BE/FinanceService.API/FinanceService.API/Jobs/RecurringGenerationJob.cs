using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace FinanceService.API.Jobs;

/// <summary>
/// Job de fundal care genereaza zilnic tranzactiile recurente scadente pentru TOTI userii.
/// Plasa de siguranta peste run-due (login). Isi creeaza scope DI propriu fiindca
/// repo-urile/motorul sunt scoped. Nu depinde de ICurrentUser (job-ul n-are request HTTP).
/// Cadenta (intarziere pornire + interval) e configurabila prin sectiunea "RecurringJob"
/// (vezi <see cref="RecurringJobOptions"/>); in dev se scurteaza pentru testare.
/// </summary>
public class RecurringGenerationJob(
    IServiceScopeFactory            scopeFactory,
    IOptions<RecurringJobOptions>   options,
    ILogger<RecurringGenerationJob> logger
) : BackgroundService
{
    private readonly RecurringJobOptions _opt = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Job recurenta pornit: prima rulare in {Delay}s, apoi la fiecare {Interval}s.",
            _opt.StartupDelaySeconds, _opt.IntervalSeconds);

        // Mica intarziere la pornire ca sa nu concureze cu init-ul aplicatiei.
        await Task.Delay(TimeSpan.FromSeconds(_opt.StartupDelaySeconds), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var repo   = scope.ServiceProvider.GetRequiredService<IRecurringTemplateRepository>();
                var engine = scope.ServiceProvider.GetRequiredService<IRecurringGenerationEngine>();

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var due   = await repo.GetAllDueAsync(today);
                var count = await engine.GenerateAsync(due, today);

                logger.LogInformation("Job recurenta: {Count} tranzactii generate la {Date}", count, today);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job recurenta a esuat; reincerc la urmatorul ciclu.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_opt.IntervalSeconds), stoppingToken);
        }
    }
}
