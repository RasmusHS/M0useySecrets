using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class AddCommand
{
    public static Command Create(ServiceProvider services)
    {
        var nameArg = new Argument<string>("name") { Description = "Secret name" };
        var valueArg = new Argument<string>("value") { Description = "Secret value" };
        var nsOption = new Option<string>("--namespace", "-ns")
        {
            Description = "Namespace",
            DefaultValueFactory = _ => "default"
        };
        var notesOption = new Option<string?>("--notes") { Description = "Notes" };
        var expiresOption = new Option<DateTime?>("--expires") { Description = "Expiry date (UTC)" };

        var command = new Command("add", "Add a secret");
        command.Arguments.Add(nameArg);
        command.Arguments.Add(valueArg);
        command.Options.Add(nsOption);
        command.Options.Add(notesOption);
        command.Options.Add(expiresOption);

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var value = parseResult.GetValue(valueArg);
            var ns = parseResult.GetValue(nsOption);
            var notes = parseResult.GetValue(notesOption);
            var expires = parseResult.GetValue(expiresOption);

            UnlockVault.WithUnlockedVault(services, () =>
            {
                var secretService = services.GetRequiredService<ISecretService>();
                secretService.AddSecret(name, value, ns, expires, notes);
                ConsoleOutput.PrintSuccess($"Secret '{name}' added.");
            });
        });

        return command;
    }
}
