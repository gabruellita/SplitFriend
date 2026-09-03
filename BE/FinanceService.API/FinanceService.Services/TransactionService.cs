using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;
using FinanceService.Infrastructure.Exceptions;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Infrastructure.Security;
using FinanceService.Services.Interfaces;

namespace FinanceService.Services;

public class TransactionService(
    ITransactionRepository txRepo,
    ICategoryRepository    categoryRepo,
    ICurrencyRepository    currencyRepo,
    ICurrentUser           currentUser
) : ITransactionService
{
    public async Task<IEnumerable<TransactionResponse>> GetAllAsync(DateOnly? from, DateOnly? to, long? categoryId, string? kind)
    {
        var normalizedKind = NormalizeKindFilter(kind);
        var list = await txRepo.GetAllAsync(currentUser.UserId, from, to, categoryId, normalizedKind);
        return list.Select(MapToResponse);
    }

    private static string? NormalizeKindFilter(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind)) return null;
        var upper = kind.Trim().ToUpperInvariant();
        if (upper is not ("INCOME" or "EXPENSE"))
            throw new ValidationException("Parametrul 'kind' trebuie sa fie INCOME sau EXPENSE.");
        return upper;
    }

    public async Task<TransactionResponse> GetByIdAsync(long id)
    {
        var tx = await txRepo.GetByIdAsync(id, currentUser.UserId)
            ?? throw new NotFoundException("Tranzactia nu exista.");
        return MapToResponse(tx);
    }

    public async Task<long> CreateAsync(CreateTransactionRequest request)
    {
        var currencyId = await ResolveCurrencyAsync(request.CurrencyId);
        await EnsureCategoryAsync(request.CategoryId, request.Kind);

        return await txRepo.CreateAsync(
            currentUser.UserId, request.CategoryId, request.Amount, currencyId,
            request.Kind, request.Description?.Trim(), request.TransactionDate, templateId: null);
    }

    public async Task UpdateAsync(long id, UpdateTransactionRequest request)
    {
        var currencyId = await ResolveCurrencyAsync(request.CurrencyId);
        await EnsureCategoryAsync(request.CategoryId, request.Kind);

        var rows = await txRepo.UpdateAsync(
            id, currentUser.UserId, request.CategoryId, request.Amount, currencyId,
            request.Kind, request.Description?.Trim(), request.TransactionDate);

        if (rows == 0)
            throw new NotFoundException("Tranzactia nu exista sau este anulata (VOIDED).");
    }

    public async Task VoidAsync(long id)
    {
        var rows = await txRepo.VoidAsync(id, currentUser.UserId);
        if (rows == 0)
            throw new NotFoundException("Tranzactia nu exista sau este deja anulata.");
    }

    public async Task<SummaryResponse> GetSummaryAsync(DateOnly? from, DateOnly? to)
    {
        var rows = (await txRepo.GetSummaryAsync(currentUser.UserId, from, to)).ToList();

        var totalIncome  = rows.Where(r => r.Kind == "INCOME").Sum(r => r.TotalAmount);
        var totalExpense = rows.Where(r => r.Kind == "EXPENSE").Sum(r => r.TotalAmount);

        var byCategory = rows
            .Select(r => new SummaryCategory(r.Kind, r.CategoryId, r.CategoryName, r.TotalAmount, r.TransactionCount))
            .ToList();

        return new SummaryResponse(totalIncome, totalExpense, totalIncome - totalExpense, byCategory);
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    /// <summary>Foloseste moneda din request sau, daca lipseste, moneda preferata a userului (din JWT).</summary>
    private async Task<long> ResolveCurrencyAsync(long? requested)
    {
        var currencyId = requested ?? currentUser.CurrencyId
            ?? throw new ValidationException("Moneda este obligatorie (lipseste din request si din profil).");

        if (!await currencyRepo.ExistsActiveAsync(currencyId))
            throw new ValidationException("Moneda selectata nu exista sau este inactiva.");

        return currencyId;
    }

    private async Task EnsureCategoryAsync(long? categoryId, string kind)
    {
        if (categoryId is null) return;
        if (!await categoryRepo.ValidForUserAsync(categoryId.Value, currentUser.UserId, kind))
            throw new ValidationException("Categoria nu exista, nu va apartine sau nu se potriveste cu tipul tranzactiei.");
    }

    private static TransactionResponse MapToResponse(Transaction t)
        => new(t.Id, t.Amount, t.Kind, t.TransactionDate, t.CategoryId, t.CategoryName,
               t.CurrencyId, t.CurrencyCode, t.Description, t.Status, t.TemplateId, t.CreatedAt);
}
