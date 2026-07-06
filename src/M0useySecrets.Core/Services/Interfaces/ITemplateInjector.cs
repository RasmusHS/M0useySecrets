namespace M0useySecrets.Core.Services.Interfaces;

public interface ITemplateInjector
{
    string InjectSecrets(string templateContent, Action<string>? onWarning = null);

    void InjectToFile(string templatePath, string outputPath, Action<string>? onWarning = null);
}
