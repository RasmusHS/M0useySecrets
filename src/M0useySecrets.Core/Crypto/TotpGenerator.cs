using M0useySecrets.Core.Crypto.Interfaces;
using OtpNet;

namespace M0useySecrets.Core.Crypto;

public class TotpGenerator : ITotpGenerator
{
    private const int TimePeriodSeconds = 30;
    private const int CodeDigits = 6;

    public string GenerateCode(string base32Seed)
    {
        byte[] seedBytes = Base32Encoding.ToBytes(base32Seed);
        var totp = new Totp(seedBytes, step: TimePeriodSeconds, totpSize: CodeDigits);
        return totp.ComputeTotp();
    }

    public int GetRemainingSeconds()
    {
        // seconds until current code expires:
        return TimePeriodSeconds - (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % TimePeriodSeconds);
    }
}
