using FinanceService.DTO.Requests;
using FinanceService.Infrastructure.Exceptions;
using FinanceService.Services;

namespace FinanceService.Tests;

/// <summary>
/// Teste pentru algoritmul de impartire a unei cheltuieli de grup
/// (GroupExpenseService.ComputeOwedAmounts, sectiunea 4.6.1 / 5.5.1).
/// Proprietatea centrala verificata: suma cotelor este EXACT egala cu totalul,
/// indiferent de rotunjire — ultimul participant absoarbe reziduul.
/// </summary>
public class SplitAllocationTests
{
    private static CreateGroupExpenseRequest Req(
        decimal amount, string splitType, params ExpenseParticipantInput[] participants)
        => new("Test", amount, PaidByUserId: 1, splitType,
               new DateOnly(2026, 1, 1), participants);

    private static ExpenseParticipantInput P(
        long id, decimal? exact = null, decimal? percent = null, int? shares = null)
        => new(id, exact, percent, shares);

    // ─── EQUAL ────────────────────────────────────────────────────────────────

    [Fact]
    public void Equal_Indivizibil_SumaRamaneExacta_UltimulAbsoarbeRestul()
    {
        // 100 / 3 = 33,33 + 33,33 + 33,34 = 100,00 (nu 99,99)
        var owed = GroupExpenseService.ComputeOwedAmounts(
            Req(100m, "EQUAL", P(1), P(2), P(3)));

        Assert.Equal(100m, owed.Values.Sum());
        Assert.Equal(33.33m, owed[1]);
        Assert.Equal(33.33m, owed[2]);
        Assert.Equal(33.34m, owed[3]);
    }

    [Fact]
    public void Equal_Divizibil_CoteEgale()
    {
        var owed = GroupExpenseService.ComputeOwedAmounts(
            Req(100m, "EQUAL", P(1), P(2), P(3), P(4)));

        Assert.All(owed.Values, v => Assert.Equal(25m, v));
        Assert.Equal(100m, owed.Values.Sum());
    }

    [Theory]
    [InlineData(100.00, 2)]
    [InlineData(100.00, 3)]
    [InlineData(99.99, 7)]
    [InlineData(0.01, 3)]
    [InlineData(1000.03, 6)]
    [InlineData(10.00, 9)]
    public void Equal_SumaCotelor_IntotdeaunaEgalaCuTotalul(decimal amount, int n)
    {
        var participants = Enumerable.Range(1, n).Select(i => P(i)).ToArray();

        var owed = GroupExpenseService.ComputeOwedAmounts(Req(amount, "EQUAL", participants));

        Assert.Equal(amount, owed.Values.Sum());
        // toate cotele in afara de ultima sunt egale; abaterea ultimei e marginita
        // de eroarea de rotunjire cumulata, sub N * 0,005 unitati monetare.
        Assert.True(Math.Abs(owed.Values.Max() - owed.Values.Min()) <= n * 0.005m + 0.0001m);
    }

    // ─── EXACT ──────────────────────────────────────────────────────────────—

    [Fact]
    public void Exact_SumeValide_SePastreazaIntocmai()
    {
        var owed = GroupExpenseService.ComputeOwedAmounts(
            Req(100m, "EXACT", P(1, exact: 70m), P(2, exact: 30m)));

        Assert.Equal(70m, owed[1]);
        Assert.Equal(30m, owed[2]);
    }

    [Fact]
    public void Exact_SumaDiferitaDeTotal_AruncaValidationException()
    {
        var ex = Assert.Throws<ValidationException>(() =>
            GroupExpenseService.ComputeOwedAmounts(
                Req(100m, "EXACT", P(1, exact: 70m), P(2, exact: 25m))));
        Assert.Contains("egala cu totalul", ex.Message);
    }

    [Fact]
    public void Exact_SumaNepozitiva_AruncaValidationException()
        => Assert.Throws<ValidationException>(() =>
            GroupExpenseService.ComputeOwedAmounts(
                Req(100m, "EXACT", P(1, exact: 100m), P(2, exact: 0m))));

    // ─── PERCENT ───────────────────────────────────────────────────────────—

    [Fact]
    public void Percent_RotunjireAbsorbita_SumaExacta()
    {
        // 33,33% din 100 = 33,33; ultimul (33,34%) preia restul -> suma 100,00
        var owed = GroupExpenseService.ComputeOwedAmounts(
            Req(100m, "PERCENT", P(1, percent: 33.33m), P(2, percent: 33.33m), P(3, percent: 33.34m)));

        Assert.Equal(100m, owed.Values.Sum());
        Assert.Equal(33.33m, owed[1]);
    }

    [Fact]
    public void Percent_NuInsumeaza100_AruncaValidationException()
        => Assert.Throws<ValidationException>(() =>
            GroupExpenseService.ComputeOwedAmounts(
                Req(100m, "PERCENT", P(1, percent: 60m), P(2, percent: 30m))));

    // ─── SHARES ────────────────────────────────────────────────────────────—

    [Fact]
    public void Shares_DoiLaUnu_ImparteProportional()
    {
        // 90 cu cote 2:1 -> 60 / 30
        var owed = GroupExpenseService.ComputeOwedAmounts(
            Req(90m, "SHARES", P(1, shares: 2), P(2, shares: 1)));

        Assert.Equal(60m, owed[1]);
        Assert.Equal(30m, owed[2]);
        Assert.Equal(90m, owed.Values.Sum());
    }

    [Fact]
    public void Shares_SumaExacta_ChiarLaImpartireIndivizibila()
    {
        var owed = GroupExpenseService.ComputeOwedAmounts(
            Req(100m, "SHARES", P(1, shares: 1), P(2, shares: 1), P(3, shares: 1)));
        Assert.Equal(100m, owed.Values.Sum());
    }

    [Fact]
    public void Shares_CotaNepozitiva_AruncaValidationException()
        => Assert.Throws<ValidationException>(() =>
            GroupExpenseService.ComputeOwedAmounts(
                Req(100m, "SHARES", P(1, shares: 2), P(2, shares: 0))));

    // ─── Validari generale ───────────────────────────────────────────────────

    [Fact]
    public void ParticipantiDuplicati_AruncaValidationException()
        => Assert.Throws<ValidationException>(() =>
            GroupExpenseService.ComputeOwedAmounts(
                Req(100m, "EQUAL", P(1), P(1))));

    [Fact]
    public void TipSplitNecunoscut_AruncaValidationException()
        => Assert.Throws<ValidationException>(() =>
            GroupExpenseService.ComputeOwedAmounts(
                Req(100m, "BOGUS", P(1), P(2))));
}
