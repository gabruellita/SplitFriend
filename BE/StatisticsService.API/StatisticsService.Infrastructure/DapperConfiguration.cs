using System.Data;
using Dapper;

namespace StatisticsService.Infrastructure;

/// <summary>
/// Configureaza Dapper sa mapeze coloanele snake_case din PostgreSQL la proprietati
/// PascalCase din C#. TREBUIE apelat INAINTE de orice repository.
/// </summary>
public static class DapperConfiguration
{
    public static void Configure()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Dapper 2.1.35 nu trimite System.DateOnly ca parametru (NotSupportedException la
        // LookupDbType). Npgsql 9 mapeaza nativ DateOnly -> coloana `date`. Acopera si DateOnly?.
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }
}

/// <summary>Type handler Dapper pentru <see cref="DateOnly"/> (parametri DATE: p_from / p_to).</summary>
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
