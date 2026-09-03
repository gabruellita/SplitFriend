using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinanceService.Tests;

/// <summary>
/// Teste pentru motorul de generare a tranzactiilor recurente
/// (RecurringGenerationEngine, sectiunea 4.6.3 / 5.5.3): comportament "catch-up",
/// idempotenta la nivel de zi, oprire/dezactivare la end_date si pasul fiecarei frecvente.
/// Repo-urile sunt inlocuite cu fake-uri in memorie (fara DB).
/// </summary>
public class RecurringGenerationTests
{
    // ─── Fake-uri ───────────────────────────────────────────────────────────—

    private sealed class FakeTxRepo : ITransactionRepository
    {
        public List<DateOnly> CreatedDates { get; } = [];

        public Task<long> CreateAsync(long userId, long? categoryId, decimal amount, long currencyId,
                                      string kind, string? description, DateOnly date, long? templateId)
        {
            CreatedDates.Add(date);
            return Task.FromResult((long)CreatedDates.Count);
        }

        public Task<IEnumerable<Transaction>> GetAllAsync(long u, DateOnly? f, DateOnly? t, long? c, string? k) => throw new NotImplementedException();
        public Task<Transaction?> GetByIdAsync(long id, long u) => throw new NotImplementedException();
        public Task<int> UpdateAsync(long id, long u, long? c, decimal a, long cur, string k, string? d, DateOnly date) => throw new NotImplementedException();
        public Task<int> VoidAsync(long id, long u) => throw new NotImplementedException();
        public Task<IEnumerable<SummaryRow>> GetSummaryAsync(long u, DateOnly? f, DateOnly? t) => throw new NotImplementedException();
    }

    private sealed class FakeTemplateRepo : IRecurringTemplateRepository
    {
        public DateOnly? AdvancedTo { get; private set; }
        public bool? AdvancedActive { get; private set; }

        public Task AdvanceAsync(long id, DateOnly nextRunDate, bool isActive)
        {
            AdvancedTo = nextRunDate;
            AdvancedActive = isActive;
            return Task.CompletedTask;
        }

        public Task<long> CreateAsync(long u, long? c, decimal a, long cur, string k, string? d, string fr, int ic, DateOnly s, DateOnly? e, DateOnly n) => throw new NotImplementedException();
        public Task<IEnumerable<RecurringTransactionTemplate>> GetAllAsync(long u) => throw new NotImplementedException();
        public Task<RecurringTransactionTemplate?> GetByIdAsync(long id, long u) => throw new NotImplementedException();
        public Task<int> UpdateAsync(long id, long u, long? c, decimal a, long cur, string k, string? d, string fr, int ic, DateOnly? e) => throw new NotImplementedException();
        public Task<int> DeactivateAsync(long id, long u) => throw new NotImplementedException();
        public Task<IEnumerable<RecurringTransactionTemplate>> GetDueAsync(long u, DateOnly r) => throw new NotImplementedException();
        public Task<IEnumerable<RecurringTransactionTemplate>> GetAllDueAsync(DateOnly r) => throw new NotImplementedException();
    }

    private static RecurringTransactionTemplate Template(
        string frequency, int interval, DateOnly nextRun, DateOnly? endDate = null, long id = 1)
        => new()
        {
            Id = id, UserId = 1, Amount = 100m, CurrencyId = 1, Kind = "EXPENSE",
            Frequency = frequency, IntervalCount = interval,
            StartDate = nextRun, NextRunDate = nextRun, EndDate = endDate, IsActive = true
        };

    private static (RecurringGenerationEngine engine, FakeTxRepo tx, FakeTemplateRepo tpl) NewEngine()
    {
        var tx = new FakeTxRepo();
        var tpl = new FakeTemplateRepo();
        var engine = new RecurringGenerationEngine(tx, tpl, NullLogger<RecurringGenerationEngine>.Instance);
        return (engine, tx, tpl);
    }

    // ─── Catch-up ─────────────────────────────────────────────────────────—

    [Fact]
    public async Task CatchUp_SablonLunarRestant_GenereazaCateOTranzactiePerScadenta()
    {
        var (engine, tx, tpl) = NewEngine();
        var t = Template("MONTHLY", 1, new DateOnly(2026, 3, 4));
        var today = new DateOnly(2026, 6, 16);

        var generated = await engine.GenerateAsync([t], today);

        Assert.Equal(4, generated); // 04.03, 04.04, 04.05, 04.06
        Assert.Equal(
            [new(2026, 3, 4), new(2026, 4, 4), new(2026, 5, 4), new(2026, 6, 4)],
            tx.CreatedDates);
        Assert.Equal(new DateOnly(2026, 7, 4), tpl.AdvancedTo); // next_run_date depaseste azi
        Assert.True(tpl.AdvancedActive);
    }

    [Fact]
    public async Task TranzactiileSuntDatateLaScadentaReala_NuLaDataRularii()
    {
        var (engine, tx, _) = NewEngine();
        var t = Template("MONTHLY", 1, new DateOnly(2026, 1, 31));

        await engine.GenerateAsync([t], new DateOnly(2026, 3, 31));

        // .NET "clamp"-eaza 31 feb la 28 feb (AddMonths); proprietatea testata e ca
        // datele sunt cele calculate de motor, nu data rularii.
        Assert.Contains(new DateOnly(2026, 1, 31), tx.CreatedDates);
        Assert.DoesNotContain(new DateOnly(2026, 3, 31), tx.CreatedDates.Skip(2)); // nu toate sunt "azi"
    }

    // ─── Idempotenta ─────────────────────────────────────────────────────—

    [Fact]
    public async Task Idempotenta_ADouaRulareInAceeasiZi_NuMaiGenereazaNimic()
    {
        var (engine, tx, _) = NewEngine();
        var today = new DateOnly(2026, 6, 16);

        // prima rulare avanseaza next_run_date dincolo de azi
        var afterFirst = Template("MONTHLY", 1, new DateOnly(2026, 6, 16));
        await engine.GenerateAsync([afterFirst], today);

        // a doua rulare: next_run_date este deja in viitor -> 0
        var alreadyAdvanced = Template("MONTHLY", 1, new DateOnly(2026, 7, 16));
        var generatedAgain = await engine.GenerateAsync([alreadyAdvanced], today);

        Assert.Equal(0, generatedAgain);
    }

    [Fact]
    public async Task SablonInViitor_NuGenereazaNimic()
    {
        var (engine, tx, _) = NewEngine();
        var t = Template("DAILY", 1, new DateOnly(2026, 12, 1));

        var generated = await engine.GenerateAsync([t], new DateOnly(2026, 6, 16));

        Assert.Equal(0, generated);
        Assert.Empty(tx.CreatedDates);
    }

    // ─── End date ────────────────────────────────────────────────────────—

    [Fact]
    public async Task EndDate_OpresteGenerareaSiDezactiveazaSablonul()
    {
        var (engine, tx, tpl) = NewEngine();
        var t = Template("MONTHLY", 1, new DateOnly(2026, 1, 1), endDate: new DateOnly(2026, 3, 1));

        var generated = await engine.GenerateAsync([t], new DateOnly(2026, 12, 31));

        Assert.Equal(3, generated); // 01.01, 01.02, 01.03 (<= end); 01.04 depaseste end
        Assert.False(tpl.AdvancedActive); // dezactivat: next_run_date > end_date
    }

    // ─── Pasul fiecarei frecvente ───────────────────────────────────────—

    [Theory]
    [InlineData("DAILY", 1, "2026-06-14", "2026-06-16", 3)]
    [InlineData("WEEKLY", 2, "2026-06-01", "2026-06-16", 2)]   // pas 14 zile: 01.06, 15.06
    [InlineData("MONTHLY", 1, "2026-04-10", "2026-06-16", 3)]  // 10.04, 10.05, 10.06
    [InlineData("YEARLY", 1, "2024-01-01", "2026-06-16", 3)]   // 2024, 2025, 2026
    public async Task Frecventa_ProduceNumarulCorectDeScadente(
        string freq, int interval, string nextRun, string today, int expected)
    {
        var (engine, tx, _) = NewEngine();
        var t = Template(freq, interval, DateOnly.Parse(nextRun));

        var generated = await engine.GenerateAsync([t], DateOnly.Parse(today));

        Assert.Equal(expected, generated);
    }

    // ─── Mai multe sabloane ────────────────────────────────────────────—

    [Fact]
    public async Task MaiMulteSabloane_InsumeazaTranzactiileGenerate()
    {
        var (engine, tx, _) = NewEngine();
        var monthly = Template("MONTHLY", 1, new DateOnly(2026, 4, 16), id: 1);
        var weekly  = Template("WEEKLY", 1, new DateOnly(2026, 6, 2), id: 2);

        var generated = await engine.GenerateAsync([monthly, weekly], new DateOnly(2026, 6, 16));

        Assert.Equal(tx.CreatedDates.Count, generated);
        Assert.True(generated >= 5); // 3 lunare (16.04, 16.05, 16.06) + 2 saptamanale (02.06, 09.06, 16.06)
    }
}
