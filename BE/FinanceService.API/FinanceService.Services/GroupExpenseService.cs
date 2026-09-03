using System.Text.Json;
using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;
using FinanceService.Infrastructure.Exceptions;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Infrastructure.Security;
using FinanceService.Services.Interfaces;

namespace FinanceService.Services;

public class GroupExpenseService(
    IGroupExpenseRepository expenseRepo,
    IGroupRepository        groupRepo,
    ICurrentUser            currentUser
) : IGroupExpenseService
{
    public async Task<IEnumerable<GroupExpenseResponse>> GetAllAsync(long groupId)
    {
        await EnsureMemberAsync(groupId);
        var expenses = (await expenseRepo.GetAllAsync(groupId)).ToList();
        var result = new List<GroupExpenseResponse>(expenses.Count);
        foreach (var e in expenses)
        {
            var splits = await expenseRepo.GetSplitsAsync(e.Id);
            result.Add(MapToResponse(e, splits));
        }
        return result;
    }

    public async Task<GroupExpenseResponse> GetByIdAsync(long groupId, long expenseId)
    {
        await EnsureMemberAsync(groupId);
        var e = await expenseRepo.GetByIdAsync(expenseId, groupId)
            ?? throw new NotFoundException("Cheltuiala nu exista in acest grup.");
        var splits = await expenseRepo.GetSplitsAsync(e.Id);
        return MapToResponse(e, splits);
    }

    public async Task<long> CreateAsync(long groupId, CreateGroupExpenseRequest request)
    {
        _ = await groupRepo.GetByIdAsync(groupId, currentUser.UserId)
            ?? throw new ForbiddenException("Nu esti membru al acestui grup.");

        // Model multi-valuta: cheltuiala se inregistreaza in moneda creatorului (= platitorul).
        if (request.PaidByUserId != currentUser.UserId)
            throw new ValidationException("In modelul multi-valuta poti crea doar cheltuieli platite de tine.");

        var currencyId = currentUser.CurrencyId
            ?? throw new ValidationException("Lipseste moneda utilizatorului curent.");

        // Platitorul si toti participantii trebuie sa fie membri activi ai grupului.
        await EnsureActiveMemberAsync(groupId, request.PaidByUserId, "Platitorul nu este membru activ al grupului.");
        foreach (var p in request.Participants)
            await EnsureActiveMemberAsync(groupId, p.UserId, $"Participantul {p.UserId} nu este membru activ al grupului.");

        var owed = ComputeOwedAmounts(request);   // user_id → owed_amount (suma = Amount exact)

        var splitsJson = JsonSerializer.Serialize(
            owed.Select(kv => new { user_id = kv.Key, owed_amount = kv.Value }));

        return await expenseRepo.CreateAsync(
            groupId, request.PaidByUserId, request.Title.Trim(), request.Amount,
            currencyId, request.SplitType, request.ExpenseDate, splitsJson);
    }

    public async Task CancelAsync(long groupId, long expenseId)
    {
        await EnsureMemberAsync(groupId);
        var rows = await expenseRepo.CancelAsync(expenseId, groupId);
        if (rows == 0)
            throw new NotFoundException("Cheltuiala nu exista sau este deja anulata.");
    }

    public async Task<IEnumerable<GroupBalanceResponse>> GetBalancesAsync(long groupId)
    {
        await EnsureMemberAsync(groupId);
        var rows = await expenseRepo.GetBalancesAsync(groupId);
        return rows.Select(b => new GroupBalanceResponse(b.UserId, b.Username, b.CurrencyId, b.CurrencyCode, b.NetAmount));
    }

    // ─── CALCUL SPLIT-URI ─────────────────────────────────────────────────────

    /// <summary>
    /// Calculeaza owed_amount per participant pentru cele 4 tipuri. Ultimul participant
    /// absoarbe restul de rotunjire, ca suma split-urilor sa fie EXACT egala cu Amount.
    /// </summary>
    /// <remarks>internal (nu private) pentru a fi acoperit direct de testele unitare.</remarks>
    internal static Dictionary<long, decimal> ComputeOwedAmounts(CreateGroupExpenseRequest r)
    {
        var participants = r.Participants;
        if (participants.Select(p => p.UserId).Distinct().Count() != participants.Count)
            throw new ValidationException("Participantii trebuie sa fie distincti.");

        var owed = new Dictionary<long, decimal>();

        switch (r.SplitType)
        {
            case "EQUAL":
            {
                var per = Math.Round(r.Amount / participants.Count, 2, MidpointRounding.AwayFromZero);
                for (int i = 0; i < participants.Count; i++)
                    owed[participants[i].UserId] = i == participants.Count - 1
                        ? r.Amount - per * (participants.Count - 1)
                        : per;
                break;
            }
            case "EXACT":
            {
                if (participants.Any(p => p.ExactAmount is null or <= 0))
                    throw new ValidationException("La EXACT, fiecare participant are nevoie de o suma > 0 (exactAmount).");
                var sum = participants.Sum(p => p.ExactAmount!.Value);
                if (sum != r.Amount)
                    throw new ValidationException($"Suma sumelor exacte ({sum:0.00}) trebuie sa fie egala cu totalul ({r.Amount:0.00}).");
                foreach (var p in participants) owed[p.UserId] = p.ExactAmount!.Value;
                break;
            }
            case "PERCENT":
            {
                if (participants.Any(p => p.Percent is null or <= 0))
                    throw new ValidationException("La PERCENT, fiecare participant are nevoie de un procent > 0.");
                var totalPct = participants.Sum(p => p.Percent!.Value);
                if (totalPct != 100m)
                    throw new ValidationException($"Procentele trebuie sa insumeze 100 (acum {totalPct:0.##}).");
                decimal running = 0;
                for (int i = 0; i < participants.Count; i++)
                {
                    var p = participants[i];
                    if (i == participants.Count - 1) owed[p.UserId] = r.Amount - running;
                    else
                    {
                        var part = Math.Round(r.Amount * p.Percent!.Value / 100m, 2, MidpointRounding.AwayFromZero);
                        owed[p.UserId] = part;
                        running += part;
                    }
                }
                break;
            }
            case "SHARES":
            {
                if (participants.Any(p => p.Shares is null or <= 0))
                    throw new ValidationException("La SHARES, fiecare participant are nevoie de un numar de parti > 0.");
                var totalShares = participants.Sum(p => p.Shares!.Value);
                decimal running = 0;
                for (int i = 0; i < participants.Count; i++)
                {
                    var p = participants[i];
                    if (i == participants.Count - 1) owed[p.UserId] = r.Amount - running;
                    else
                    {
                        var part = Math.Round(r.Amount * p.Shares!.Value / totalShares, 2, MidpointRounding.AwayFromZero);
                        owed[p.UserId] = part;
                        running += part;
                    }
                }
                break;
            }
            default:
                throw new ValidationException("Tip de split necunoscut.");
        }

        return owed;
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    private async Task EnsureMemberAsync(long groupId)
    {
        if (!await groupRepo.IsMemberAsync(groupId, currentUser.UserId))
            throw new ForbiddenException("Nu esti membru al acestui grup.");
    }

    private async Task EnsureActiveMemberAsync(long groupId, long userId, string message)
    {
        if (!await groupRepo.IsMemberAsync(groupId, userId))
            throw new ValidationException(message);
    }

    private static GroupExpenseResponse MapToResponse(GroupExpense e, IEnumerable<ExpenseSplit> splits)
        => new(e.Id, e.GroupId, e.PaidByUserId, e.Title, e.Amount, e.CurrencyId, e.CurrencyCode,
               e.SplitType, e.Status, e.ExpenseDate, e.CreatedAt,
               splits.Select(s => new ExpenseSplitResponse(s.UserId, s.OwedAmount, s.PaidAmount, s.IsSettled)).ToList());
}
