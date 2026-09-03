using Dapper;

namespace IdentityService.Infrastructure;

/// <summary>
/// Configureaza Dapper sa mapeze automat coloanele snake_case din PostgreSQL
/// la proprietati PascalCase din C#.
/// TREBUIE apelat INAINTE de orice repository in Program.cs.
/// </summary>
public static class DapperConfiguration
{
    public static void Configure()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}
