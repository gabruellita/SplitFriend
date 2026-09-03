using IdentityService.Infrastructure;

namespace IdentityService.Tests;

/// <summary>
/// Teste pentru stocarea parolelor cu bcrypt (PasswordHasher, sectiunea 4.5.1 / 4.7).
/// </summary>
public class PasswordHasherTests
{
    [Fact]
    public void Hash_NuPastreazaParolaInClar()
    {
        var hash = PasswordHasher.Hash("Parola123!");
        Assert.NotEqual("Parola123!", hash);
        Assert.StartsWith("$2", hash); // prefix bcrypt
    }

    [Fact]
    public void Verify_ParolaCorecta_IntoarceTrue()
    {
        var hash = PasswordHasher.Hash("Parola123!");
        Assert.True(PasswordHasher.Verify("Parola123!", hash));
    }

    [Fact]
    public void Verify_ParolaGresita_IntoarceFalse()
    {
        var hash = PasswordHasher.Hash("Parola123!");
        Assert.False(PasswordHasher.Verify("AltaParola", hash));
    }

    [Fact]
    public void Hash_AceeasiParola_ProduceHashuriDiferite_DatoritaSalt()
    {
        var h1 = PasswordHasher.Hash("Parola123!");
        var h2 = PasswordHasher.Hash("Parola123!");
        Assert.NotEqual(h1, h2);                       // salt aleator diferit
        Assert.True(PasswordHasher.Verify("Parola123!", h1));
        Assert.True(PasswordHasher.Verify("Parola123!", h2));
    }

    [Fact]
    public void Hash_FolosesteCostFactor12()
    {
        // formatul bcrypt: $2<x>$<cost>$... — verificam factorul de cost adaptiv
        var hash = PasswordHasher.Hash("Parola123!");
        var cost = hash.Split('$')[2];
        Assert.Equal("12", cost);
    }
}
