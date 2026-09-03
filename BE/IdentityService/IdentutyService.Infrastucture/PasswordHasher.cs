namespace IdentityService.Infrastructure;

public static class PasswordHasher
{
    private const int WorkFactor = 12; // Nu cobori sub 10 in productie

    public static string Hash(string plainPassword)
        => BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);

    public static bool Verify(string plainPassword, string hash)
        => BCrypt.Net.BCrypt.Verify(plainPassword, hash);
}
