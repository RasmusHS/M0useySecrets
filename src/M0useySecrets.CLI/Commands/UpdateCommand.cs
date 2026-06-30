using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class UpdateCommand
{
    public static Command Create(ServiceProvider services)
    {
        var nameArg = new Argument<string>("name") { Description = "Secret name" };
        var newValueArg = new Argument<string>("newValue") { Description = "Secret value" };
        var nsOption = new Option<string>("--namespace", "-ns")
        {
            Description = "Namespace",
            DefaultValueFactory = _ => "default"
        };

        var command = new Command("update", "Update a secret");
        command.Arguments.Add(nameArg);
        command.Arguments.Add(newValueArg);
        command.Options.Add(nsOption);

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var newValue = parseResult.GetValue(newValueArg);
            var ns = parseResult.GetValue(nsOption);

            UnlockVault.WithUnlockedVault(services, () =>
            {
                var secretService = services.GetRequiredService<ISecretService>();
                secretService.UpdateSecret(name, newValue, ns);
                ConsoleOutput.PrintSuccess($"Secret '{name}' was updated.");
            });
        });

        return command;
    }
}
