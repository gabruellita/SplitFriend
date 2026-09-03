using System.Data;
using Dapper;

namespace FinanceService.Infrastructure;

/// <summary>
/// Configureaza Dapper sa mapeze automat coloanele snake_case din PostgreSQL
/// la proprietati PascalCase din C#. TREBUIE apelat INAINTE de orice repository.
/// </summary>
public static class DapperConfiguration
{
    public static void Configure()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Dapper 2.1.35 nu stie sa trimita System.DateOnly ca parametru (arunca
        // NotSupportedException la LookupDbType). Npgsql 9 suporta nativ DateOnly →
        // coloana `date`, deci handler-ul doar ii preda valoarea. Acopera si DateOnly?.
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }
}

/// <summary>
/// Type handler Dapper pentru <see cref="DateOnly"/> (scriere parametru + citire coloana `date`).
/// </summary>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
        => parameter.Value = value;

    public override DateOnly Parse(object value) => value switch
    {
        DateOnly d  => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        string s    => DateOnly.Parse(s),
        _ => throw new InvalidCastException($"Nu pot converti {value?.GetType()} in DateOnly")
    };
}
