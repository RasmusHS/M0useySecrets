using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Exceptions;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class UpdateCommand
{
    public static Command Create(ServiceProvider services)
    {
        var nameArg = new Argument<string>("name") { Description = "Secret name" };
        var newValueArg = new Argument<string>("newValue")
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

        var command = new Command("update", "Update a secret");
        command.Arguments.Add(nameArg);
        command.Arguments.Add(newValueArg);
        command.Options.Add(stdinOption);
        command.Options.Add(nsOption);

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var newValue = parseResult.GetValue(newValueArg);
            var useStdin = parseResult.GetValue(stdinOption);
            var ns = parseResult.GetValue(nsOption);

            if (useStdin)
            {
                newValue = Console.In.ReadToEnd().Trim();
                if (string.IsNullOrWhiteSpace(newValue))
                {
                    ConsoleOutput.PrintError("No input received from stdin.");
                    return;
                }
            }
            else if (string.IsNullOrWhiteSpace(newValue))
            {
                newValue = PasswordPrompt.ReadPassword("Secret value: ");
                if (string.IsNullOrWhiteSpace(newValue))
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
                    secretService.UpdateSecret(name, newValue, ns);
                    ConsoleOutput.PrintSuccess($"Secret '{name}' was updated.");
                }
                catch (SecretNotFoundException)
                {
                    ConsoleOutput.PrintError($"Secret '{name}' not found in namespace '{ns}'.");
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
