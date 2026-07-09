using System.CommandLine;
using M0useySecrets.CLI.Helpers;
using M0useySecrets.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace M0useySecrets.CLI.Commands;

public static class InjectCommand
{
    public static Command Create(ServiceProvider services)
    {
        var templateNameArg = new Argument<string>("template") { Description = "Template file path" };
        var templateDirOption = new Option<string>("--template-dir", "-td")
        {
            Description = "Template directory",
            DefaultValueFactory = _ => "templates"
        };
        var filenameArg = new Option<string>("--filename", "-fn")
        {
            Description = "Output filename",
            DefaultValueFactory = _ => ".env"
        };
        var outputDirOption = new Option<string>("--output-dir", "-od")
        {
            Description = "Output directory",
            DefaultValueFactory = _ => "output"
        };

        var command = new Command("inject", "Inject into template");
        command.Arguments.Add(templateNameArg);
        command.Options.Add(templateDirOption);
        command.Options.Add(filenameArg);
        command.Options.Add(outputDirOption);

        command.SetAction(parseResult =>
        {
            var templateName = parseResult.GetValue(templateNameArg);
            var templateDir = parseResult.GetValue(templateDirOption);
            var templatePath = Path.Combine(templateDir, templateName);

            var filename = parseResult.GetValue(filenameArg);
            var outputDir = parseResult.GetValue(outputDirOption);
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, filename);

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
