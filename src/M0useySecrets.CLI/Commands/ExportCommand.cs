using System.CommandLine;
using System.Text.Json;
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

        var command = new Command("export", "Export secret to a plaintext");
        command.Arguments.Add(outputArg);
        command.Options.Add(nsOption);
        command.Options.Add(formatOption);

        command.SetAction(parseResult =>
        {
            var output = parseResult.GetValue(outputArg);
            var ns = parseResult.GetValue(nsOption);
            var format = parseResult.GetValue(formatOption);

            if (format is not "json" and not "env")
            {
                ConsoleOutput.PrintError($"Unknown format: {format}. Use 'json' or 'env'.");
                return;
            }

            UnlockVault.WithUnlockedVault(services, () =>
            {
                var secretService = services.GetRequiredService<ISecretService>();
                var secrets = secretService.ListSecrets(ns);

                var decrypted = secrets.Select(s =>
                    secretService.GetSecret(s.Name, s.Namespace)).ToList();
                var options = new JsonSerializerOptions { WriteIndented = true };

                ConsoleOutput.PrintWarning("Warning: this will write secrets in plaintext.");
                Console.Write($"Export {decrypted.Count} secrets to {output}? [y/N] ");
                string? confirmation = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (confirmation is not "y" and not "yes")
                {
                    ConsoleOutput.PrintWarning("Cancelled.");
                    return;
                }

                try
                {
                    string content = format switch
                    {
                        "json" => JsonSerializer.Serialize(decrypted, options),
                        "env" => string.Join("\n", decrypted.Select(s =>
                                  $"{s.Name.ToUpperInvariant().Replace('-', '_')}={s.Value}")),
                        _ => throw new ArgumentException($"Unknown format: {format}")
                    };

                    File.WriteAllText(output, content);
                    ConsoleOutput.PrintSuccess($"Exported {decrypted.Count} secrets to {output}");
                }
                catch (IOException ex)
                {
                    ConsoleOutput.PrintError($"Failed to write file: {ex.Message}");
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
