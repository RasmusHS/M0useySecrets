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
        var valueArg = new Argument<string>("value")
        {
            Description = "Secret value (caution: visible in shell history)",
            DefaultValueFactory = _ => ""
        };
        var stdinOption = new Option<bool>("--stdin")
        {
            Description = "Read value from stdin (for piping)"
        };
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
        command.Options.Add(stdinOption);
        command.Options.Add(nsOption);
        command.Options.Add(notesOption);
        command.Options.Add(expiresOption);

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var value = parseResult.GetValue(valueArg);
            var useStdin = parseResult.GetValue(stdinOption);
            var ns = parseResult.GetValue(nsOption);
            var notes = parseResult.GetValue(notesOption);
            var expires = parseResult.GetValue(expiresOption);

            if (useStdin)
            {
                value = Console.In.ReadToEnd().Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    ConsoleOutput.PrintError("No input received from stdin.");
                    return;
                }
            }
            else if (string.IsNullOrWhiteSpace(value))
            {
                value = PasswordPrompt.ReadPassword("Secret value: ");
                if (string.IsNullOrWhiteSpace(value))
                {
                    ConsoleOutput.PrintError("Value cannot be empty.");
                    return;
                }
            }

            UnlockVault.WithUnlockedVault(services, () =>
            {
                try
                {
                    var secretService = services.GetRequiredService<ISecretService>();
                    secretService.AddSecret(name, value, ns, expires, notes);
                    ConsoleOutput.PrintSuccess($"Secret '{name}' added.");
                }
                catch (InvalidOperationException ex)
                {
                    ConsoleOutput.PrintError(ex.Message);
                }
            });
        });

        return command;
    }
}
