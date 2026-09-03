using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;
using FinanceService.Infrastructure.Exceptions;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Infrastructure.Security;
using FinanceService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceService.Services;

public class RecurringTemplateService(
    IRecurringTemplateRepository templateRepo,
    IRecurringGenerationEngine   engine,
    ICategoryRepository          categoryRepo,
    ICurrencyRepository          currencyRepo,
    ICurrentUser                 currentUser,
    ILogger<RecurringTemplateService> logger
) : IRecurringTemplateService
{
    public async Task<IEnumerable<RecurringTemplateResponse>> GetAllAsync()
    {
        var list = await templateRepo.GetAllAsync(currentUser.UserId);
        return list.Select(MapToResponse);
    }

    public async Task<RecurringTemplateResponse> GetByIdAsync(long id)
    {
        var template = await templateRepo.GetByIdAsync(id, currentUser.UserId)
            ?? throw new NotFoundException("Template-ul recurent nu exista.");
        return MapToResponse(template);
    }

    public async Task<long> CreateAsync(CreateRecurringTemplateRequest request)
    {
        var currencyId = await ResolveCurrencyAsync(request.CurrencyId);
        await EnsureCategoryAsync(request.CategoryId, request.Kind);

        // next_run_date initial = start_date; run-due genereaza tranzactiile scadente.
        return await templateRepo.CreateAsync(
            currentUser.UserId, request.CategoryId, request.Amount, currencyId, request.Kind,
            request.Description?.Trim(), request.Frequency, request.IntervalCount,
            request.StartDate, request.EndDate, nextRunDate: request.StartDate);
    }

    public async Task UpdateAsync(long id, UpdateRecurringTemplateRequest request)
    {
        var currencyId = await ResolveCurrencyAsync(request.CurrencyId);
        await EnsureCategoryAsync(request.CategoryId, request.Kind);

        var rows = await templateRepo.UpdateAsync(
            id, currentUser.UserId, request.CategoryId, request.Amount, currencyId, request.Kind,
            request.Description?.Trim(), request.Frequency, request.IntervalCount, request.EndDate);

        if (rows == 0)
            throw new NotFoundException("Template-ul recurent nu exista.");
    }

    public async Task DeactivateAsync(long id)
    {
        var rows = await templateRepo.DeactivateAsync(id, currentUser.UserId);
        if (rows == 0)
            throw new NotFoundException("Template-ul recurent nu exista.");
    }

    /// <summary>
    /// Genereaza tranzactiile scadente pentru template-urile active ale userului curent.
    /// Delega logica de generare catre IRecurringGenerationEngine (fara dependenta de ICurrentUser).
    /// </summary>
    public async Task<int> RunDueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueTemplates = await templateRepo.GetDueAsync(currentUser.UserId, today);
        var generated = await engine.GenerateAsync(dueTemplates, today);
        logger.LogInformation("RunDue user {UserId}: {Count} tranzactii generate", currentUser.UserId, generated);
        return generated;
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

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
            throw new ValidationException("Categoria nu exista, nu va apartine sau nu se potriveste cu tipul template-ului.");
    }

    private static RecurringTemplateResponse MapToResponse(RecurringTransactionTemplate t)
        => new(t.Id, t.Amount, t.Kind, t.Frequency, t.IntervalCount, t.StartDate, t.EndDate, t.NextRunDate,
               t.IsActive, t.CategoryId, t.CategoryName, t.CurrencyId, t.CurrencyCode, t.Description);
}
