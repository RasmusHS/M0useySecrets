using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Exceptions;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class RemoveCommand
{
    public static Command Create(ServiceProvider services)
    {
        var nameArg = new Argument<string>("name") { Description = "Secret name" };
        var nsOption = new Option<string>("--namespace", "-ns")
        {
            Description = "Namespace",
            DefaultValueFactory = _ => "default"
        };
        var forceOption = new Option<bool>("--force", "-f") { Description = "Skip confirmation prompt" };

        var command = new Command("remove", "Remove a secret");
        command.Arguments.Add(nameArg);
        command.Options.Add(nsOption);
        command.Options.Add(forceOption);
        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var ns = parseResult.GetValue(nsOption);
            var force = parseResult.GetValue(forceOption);

            if (!force)
            {
                Console.Write($"Remove secret '{name}' from namespace '{ns}'? [y/N] ");
                string? confirmation = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (confirmation is not "y" and not "yes")
                {
                    ConsoleOutput.PrintWarning("Cancelled.");
                    return;
                }
            }

            UnlockVault.WithUnlockedVault(services, () =>
            {
                try
                {
                    var secretService = services.GetRequiredService<ISecretService>();
                    secretService.RemoveSecret(name, ns);
                    ConsoleOutput.PrintSuccess($"Secret '{name}' removed.");
                }
                catch (SecretNotFoundException)
                {
                    ConsoleOutput.PrintError($"Secret '{name}' not found in namespace '{ns}'.");
                }
            });
        });

        return command;
    }
}
