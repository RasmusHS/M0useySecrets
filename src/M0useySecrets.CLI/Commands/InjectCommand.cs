using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class InjectCommand
{
    public static Command Create(ServiceProvider services)
    {
        var templateArg = new Argument<string>("template") { Description = "Template file path" };
        var outputArg = new Argument<string>("output") { Description = "Output file path" };

        var command = new Command("inject", "Inject into template");
        command.Arguments.Add(templateArg);
        command.Arguments.Add(outputArg);

        command.SetAction(parseResult =>
        {
            var templatePath = parseResult.GetValue(templateArg);
            var outputPath = parseResult.GetValue(outputArg);

            // guard: template must exist
            if (!File.Exists(templatePath))
            {
                ConsoleOutput.PrintError($"Template not found: {templatePath}");
                return 1;
            }

            // guard: don't overwrite the template
            if (Path.GetFullPath(templatePath) == Path.GetFullPath(outputPath))
            {
                ConsoleOutput.PrintError("Output path cannot be the same as template.");
                return 1;
            }

            UnlockVault.WithUnlockedVault(services, () =>
            {
                try
                {
                    var injector = services.GetRequiredService<ITemplateInjector>();

                    injector.InjectToFile(templatePath, outputPath, msg => ConsoleOutput.PrintWarning(msg));
                    ConsoleOutput.PrintSuccess($"Injected secrets → {outputPath}");
                }
                catch (IOException ex)
                {
                    ConsoleOutput.PrintError($"File error: {ex.Message}");
                }
            });

            return 0;
        });

        return command;
    }
}
