using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class ExportCommand
{
    public static Command Create(ServiceProvider services)
    {
        var outputArg = new Argument<string>("output") { Description = "Output file path" };
        var nsOption = new Option<string?>("--namespace", "-ns") { Description = "Filter by namespace" };
        var formatOption = new Option<string>("--format", "-f")
        {
            Description = "Output format",
            DefaultValueFactory = _ => "json"
        };
        // optional: add a validator for format to restrict to "json" or "env"

        var command = new Command("export", "Export secret to a plaintext");

        command.SetAction(parseResult =>
        {
            var output = parseResult.GetValue(outputArg);
            var ns = parseResult.GetValue(nsOption);
            var format = parseResult.GetValue(formatOption);

            UnlockVault.WithUnlockedVault(services, () =>
            {
                var secretService = services.GetRequiredService<ISecretService>();
                var secrets = secretService.ListSecrets(ns);

                // for export you DO need decrypted values, unlike list
                // so fetch each one individually:
                var decrypted = secrets.Select(s =>
                    secretService.GetSecret(s.Name, s.Namespace)).ToList();

                string content = format switch
                {
                    //"json" => serialize decrypted to indented JSON
                    //          (only export Name, Namespace, Value, ExpiresAt, Notes
                    //            — not crypto fields),
                    //"env" => string.Join("\n", decrypted.Select(s =>
                    //          $"{s.Name.ToUpperInvariant().Replace('-', '_')}={s.Value}")),
                    //_ => throw new ArgumentException($"Unknown format: {format}")
                };

                File.WriteAllText(output, content);
                ConsoleOutput.PrintSuccess($"Exported {decrypted.Count} secrets to {output}");
            });
        });
    }
}
