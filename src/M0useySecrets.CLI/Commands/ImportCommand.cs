using System.CommandLine;
using System.Text.Json;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Models;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class ImportCommand
{
    public static Command Create(ServiceProvider services)
    {
        var inputArg = new Argument<string>("input") { Description = "Input file path" };
        var formatOption = new Option<string>("--format", "-f")
        {
            Description = "Input format",
            DefaultValueFactory = _ => "json"
        };
        var nsOption = new Option<string>("--namespace", "-ns")
        {
            Description = "Namespace for imported secrets (used by env format)",
            DefaultValueFactory = _ => "default"
        };

        var command = new Command("import", "Import secrets to the vault.");
        command.Arguments.Add(inputArg);
        command.Options.Add(nsOption);
        command.Options.Add(formatOption);

        command.SetAction(parseResult =>
        {
            var input = parseResult.GetValue(inputArg);
            var ns = parseResult.GetValue(nsOption);
            var format = parseResult.GetValue(formatOption);

            if (!File.Exists(input))
            {
                ConsoleOutput.PrintError($"File not found: {input}");
                return;
            }

            if (format is not "json" and not "env")
            {
                ConsoleOutput.PrintError($"Unknown format: {format}. Use 'json' or 'env'.");
                return;
            }

            // read and deserialize
            List<DecryptedSecret> secrets;
            try
            {
                secrets = format switch
                {
                    "json" => JsonSerializer.Deserialize<List<DecryptedSecret>>(
                    File.ReadAllText(input)) ?? [],

                    "env" => File.ReadAllLines(input)
                        .Where(line => !string.IsNullOrWhiteSpace(line)
                                    && !line.TrimStart().StartsWith('#'))
                        .Select(line =>
                        {
                            int separator = line.IndexOf('=');
                            if (separator < 0) return null;

                            return new DecryptedSecret
                            {
                                Name = line[..separator].Trim(),
                                Value = line[(separator + 1)..].Trim(),
                                Namespace = ns,
                            };
                        })
                        .Where(s => s is not null)
                        .ToList()!,

                    _ => throw new ArgumentException($"Unknown format: {format}")
                };
            }
            catch (IOException ex)
            {
                ConsoleOutput.PrintError($"Failed to read file: {ex.Message}");
                return;
            }
            catch (JsonException ex)
            {
                ConsoleOutput.PrintError($"Invalid JSON: {ex.Message}");
                return;
            }

            if (secrets is null || secrets.Count == 0)
            {
                ConsoleOutput.PrintWarning("No secrets found in file.");
                return;
            }

            UnlockVault.WithUnlockedVault(services, () =>
            {
                var secretService = services.GetRequiredService<ISecretService>();
                int added = 0;

                foreach (var secret in secrets)
                {
                    try
                    {
                        secretService.AddSecret(
                            secret.Name,
                            secret.Value,
                            secret.Namespace,
                            secret.ExpiresAt,
                            secret.Notes);
                        added++;
                    }
                    catch (InvalidOperationException)
                    {
                        ConsoleOutput.PrintWarning(
                            $"Skipped '{secret.Name}' — already exists in '{secret.Namespace}'.");
                    }
                }

                ConsoleOutput.PrintSuccess($"Imported {added} of {secrets.Count} secrets.");
            });
        });

        return command;
    }
}
