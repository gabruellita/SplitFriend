using FinanceService.DTO.Requests;
using FinanceService.DTO.Responses;
using FinanceService.Infrastructure.Exceptions;
using FinanceService.Infrastructure.Exchange;
using FinanceService.Infrastructure.Repositories.Interfaces;
using FinanceService.Infrastructure.Security;
using FinanceService.Services.Interfaces;

namespace FinanceService.Services;

public class PaymentService(
    IPaymentRepository        paymentRepo,
    IGroupRepository          groupRepo,
    IExchangeRateClient       fx,
    ICurrencyLookupRepository currencyLookup,
    ICurrentUser              currentUser
) : IPaymentService
{
    public async Task<IEnumerable<PaymentResponse>> GetAllAsync(long groupId)
    {
        await EnsureMemberAsync(groupId);
        var payments = await paymentRepo.GetAllAsync(groupId);
        return payments.Select(p => new PaymentResponse(
            p.Id, p.FromUserId, p.ToUserId,
            p.Amount, p.CurrencyId, p.CurrencyCode,
            p.OriginalAmount, p.OriginalCurrencyId, p.OriginalCurrencyCode,
            p.ExchangeRate, p.RateDate,
            p.PaymentMethod, p.PaidAt));
    }

    public async Task<long> CreateAsync(long groupId, CreatePaymentRequest request)
    {
        _ = await groupRepo.GetByIdAsync(groupId, currentUser.UserId)
            ?? throw new ForbiddenException("Nu esti membru al acestui grup.");

        if (request.ToUserId == currentUser.UserId)
            throw new ValidationException("Nu poti face o plata catre tine insuti.");
        if (!await groupRepo.IsMemberAsync(groupId, request.ToUserId))
            throw new ValidationException("Destinatarul nu este membru activ al grupului.");

        // Moneda datoriei = moneda cheltuielilor creditorului. request.Amount e in aceasta moneda.
        var creditor = await paymentRepo.GetCreditorCurrencyAsync(groupId, currentUser.UserId, request.ToUserId)
            ?? throw new ValidationException("Nu ai datorii nesettled catre acest utilizator.");

        if (request.Amount > creditor.RemainingOwed)
            throw new ValidationException(
                $"Suma depaseste datoria curenta ({creditor.RemainingOwed:0.00} {creditor.CurrencyCode}).");

        // Moneda platitorului (debitor) — userul curent.
        var debtorCurrencyId = currentUser.CurrencyId
            ?? throw new ValidationException("Lipseste moneda utilizatorului curent.");
        var debtorCode = await currencyLookup.GetCodeAsync(debtorCurrencyId)
            ?? throw new ValidationException("Moneda utilizatorului curent este invalida.");

        // Conversie autoritara: creditor -> platitor. original_amount = cat da platitorul.
        var conv = await fx.ConvertAsync(creditor.CurrencyCode, debtorCode, request.Amount);

        return await paymentRepo.CreateAsync(
            groupId, currentUser.UserId, request.ToUserId,
            request.Amount, creditor.CurrencyId,
            conv.Result, debtorCurrencyId, conv.Rate, conv.Date,
            request.Method?.Trim());
    }

    private async Task EnsureMemberAsync(long groupId)
    {
        if (!await groupRepo.IsMemberAsync(groupId, currentUser.UserId))
            throw new ForbiddenException("Nu esti membru al acestui grup.");
    }
}
