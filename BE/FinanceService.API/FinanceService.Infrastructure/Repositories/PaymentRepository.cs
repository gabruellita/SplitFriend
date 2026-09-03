using System.Data;
using Dapper;
using FinanceService.Infrastructure.Models;
using FinanceService.Infrastructure.Repositories.Interfaces;

namespace FinanceService.Infrastructure.Repositories;

public class PaymentRepository(IDbConnectionFactory connectionFactory) : IPaymentRepository
{
    public async Task<IEnumerable<Payment>> GetAllAsync(long groupId)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QueryAsync<Payment>(
            "sp_get_payments",
            new { p_group_id = groupId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<CreditorCurrency?> GetCreditorCurrencyAsync(long groupId, long fromUser, long toUser)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<CreditorCurrency>(
            "sp_get_creditor_currency",
            new { p_group_id = groupId, p_from_user = fromUser, p_to_user = toUser },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<long> CreateAsync(
        long groupId, long fromUser, long toUser,
        decimal amount, long currencyId,
        decimal originalAmount, long originalCurrencyId,
        decimal exchangeRate, DateOnly rateDate, string? method)
    {
        using var conn = connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<long>(
            "sp_create_payment",
            new
            {
                p_group_id            = groupId,
                p_from_user           = fromUser,
                p_to_user             = toUser,
                p_amount              = amount,
                p_currency_id         = currencyId,
                p_original_amount     = originalAmount,
                p_original_currency_id = originalCurrencyId,
                p_exchange_rate       = exchangeRate,
                p_rate_date           = rateDate,
                p_method              = method
            },
            commandType: CommandType.StoredProcedure);
    }
}
