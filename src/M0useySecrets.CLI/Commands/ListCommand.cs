using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class ListCommand
{
    public static Command Create(ServiceProvider services)
    {
        var nsOption = new Option<string?>("--namespace", "-ns")
        {
            Description = "Namespace",
            DefaultValueFactory = _ => null
        };

        var command = new Command("list", "List all secrets or all within a namespace.");
        command.Options.Add(nsOption);

        command.SetAction(parseResult =>
        {
            var ns = parseResult.GetValue(nsOption);

            UnlockVault.WithUnlockedVault(services, () =>
            {
                var secretService = services.GetRequiredService<ISecretService>();
                var secrets = secretService.ListSecrets(ns);
                ConsoleOutput.PrintSecretTable(secrets);
            });
        });

        return command;
    }
}
