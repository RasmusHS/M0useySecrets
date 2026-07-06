using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Crypto.Interfaces;
using M0useySecrets.Core.Exceptions;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;

namespace M0useySecrets.CLI.Commands;

public static class TotpCommand
{
    public static Command Create(ServiceProvider services)
    {
        // --- totp add (store a TOTP seed as a secret) ---
        var addNameArg = new Argument<string>("name") { Description = "TOTP account name" };
        var seedArg = new Argument<string>("seed") { Description = "Base32-encoded TOTP seed" };

        var addCommand = new Command("add", "Store a TOTP seed");
        addCommand.Arguments.Add(addNameArg);
        addCommand.Arguments.Add(seedArg);

        addCommand.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(addNameArg);
            var seed = parseResult.GetValue(seedArg);

            // validate seed is valid base32 before storing
            // try decoding — if it throws, reject
            try
            {
                Base32Encoding.ToBytes(seed);
            }
            catch
            {
                ConsoleOutput.PrintError("Invalid base32 seed.");
                return;
            }

            UnlockVault.WithUnlockedVault(services, () =>
            {
                try
                {
                    var secretService = services.GetRequiredService<ISecretService>();
                    // store in a "totp" namespace to keep them separate
                    secretService.AddSecret(name, seed, ns: "totp");
                    ConsoleOutput.PrintSuccess($"TOTP seed for '{name}' stored.");
                }
                catch (InvalidOperationException ex)
                {
                    ConsoleOutput.PrintError(ex.Message);
                }
            });
        });

        // --- totp get (generate current code) ---
        var getNameArg = new Argument<string>("name") { Description = "TOTP account name" };
        var copyOption = new Option<bool>("--copy", "-c") { Description = "Copy to clipboard" };

        var getCommand = new Command("get", "Generate current TOTP code");
        getCommand.Arguments.Add(getNameArg);
        getCommand.Options.Add(copyOption);

        getCommand.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(getNameArg);
            var copy = parseResult.GetValue(copyOption);

            UnlockVault.WithUnlockedVault(services, () =>
            {
                try
                {
                    var secretService = services.GetRequiredService<ISecretService>();
                    var secret = secretService.GetSecret(name, "totp");

                    var totpGen = services.GetRequiredService<ITotpGenerator>();
                    string code = totpGen.GenerateCode(secret.Value);
                    int remaining = totpGen.GetRemainingSeconds();

                    if (copy)
                    {
                        ClipboardHelper.CopyWithAutoClear(code, clearAfterSeconds: remaining);
                        ConsoleOutput.PrintSuccess($"TOTP code copied to clipboard.");
                    }
                    else
                        Console.WriteLine($"{code}  (expires in {remaining}s)");
                }
                catch (SecretNotFoundException)
                {
                    ConsoleOutput.PrintError($"TOTP seed '{name}' not found.");
                }
            });
        });

        // --- wire up parent command ---
        var command = new Command("totp", "TOTP authenticator");
        command.Subcommands.Add(addCommand);
        command.Subcommands.Add(getCommand);
        return command;
    }
}
