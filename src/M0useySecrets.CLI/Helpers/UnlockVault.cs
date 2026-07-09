using M0useySecrets.Core.Exceptions;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Helpers;

public static class UnlockVault
{
    public static void WithUnlockedVault(ServiceProvider services, Action action)
    {
        var vaultService = services.GetRequiredService<IVaultService>();
        string password = PasswordPrompt.ReadPassword();
        try
        {
            vaultService.Unlock(password);
        }
        catch (InvalidPasswordException)
        {
            ConsoleOutput.PrintError("Wrong password.");
            return;
        }

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
