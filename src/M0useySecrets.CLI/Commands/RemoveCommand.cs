using System.CommandLine;
using M0useySecrets.CLI.Helpers;
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

        var command = new Command("remove", "Remove a secret");
        command.Arguments.Add(nameArg);
        command.Options.Add(nsOption);
        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var ns = parseResult.GetValue(nsOption);

            UnlockVault.WithUnlockedVault(services, () =>
            {
                var secretService = services.GetRequiredService<ISecretService>();
                // TODO: Add confirmation prompt
                secretService.RemoveSecret(name, ns);
                ConsoleOutput.PrintSuccess($"Secret '{name}' removed.");
            });
        });

        return command;
    }
}
