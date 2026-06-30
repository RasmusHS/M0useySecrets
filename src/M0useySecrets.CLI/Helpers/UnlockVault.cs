using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Helpers;

public static class UnlockVault
{
    public static void WithUnlockedVault(ServiceProvider services, Action action)
    {
        var vaultService = services.GetRequiredService<IVaultService>();
        string password = PasswordPrompt.ReadPassword();
        vaultService.Unlock(password);
        try
        {
            action();
        }
        finally
        {
            vaultService.Lock();
        }
    }
}
