namespace M0useySecrets.Core.Crypto.Interfaces;

public interface ITotpGenerator
{
    string GenerateCode(string base32Seed);

    int GetRemainingSeconds();
}
