using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class InitCommand
{
    public static Command Create(ServiceProvider services)
    {
        var command = new Command("init", "Create a new vault");

        command.SetAction(parseResult =>
        {
            var vaultService = services.GetRequiredService<IVaultService>();

            string password = PasswordPrompt.ReadPassword("Create master password: ");
            string confirm = PasswordPrompt.ReadPassword("Confirm master password: ");

            if (password != confirm)
            {
                ConsoleOutput.PrintError("Passwords do not match.");
                return 1;
            }

            vaultService.Initialize(password);
            ConsoleOutput.PrintSuccess("Vault created.");
            return 0;
        });

        return command;
    }
}
