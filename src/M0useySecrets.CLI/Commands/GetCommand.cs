using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class GetCommand
{
    public static Command Create(ServiceProvider services)
    {
        var nameArg = new Argument<string>("name") { Description = "Secret name" };
        var nsOption = new Option<string>("--namespace", "-ns")
        {
            Description = "Namespace",
            DefaultValueFactory = _ => "default"
        };
        var copyOption = new Option<bool>("--copy", "-c") { Description = "Copy to clipboard" };

        var command = new Command("get", "Get a secret");
        command.Arguments.Add(nameArg);
        command.Options.Add(nsOption);
        command.Options.Add(copyOption);

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var ns = parseResult.GetValue(nsOption);
            var copy = parseResult.GetValue(copyOption);

            UnlockVault.WithUnlockedVault(services, () =>
            {
                var secretService = services.GetRequiredService<ISecretService>();
                var secret = secretService.GetSecret(name, ns).Value;
                ConsoleOutput.PrintSuccess($"Secret '{secret}' copied to clipboard");
                ClipboardHelper.CopyWithAutoClear(secret, 60);
            });
        });

        return command;
    }
}
