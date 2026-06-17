namespace M0useySecrets.Core.Services.Interfaces;

public interface ITemplateInjector
{
    string InjectSecrets(string templateContent);

    void InjectToFile(string templatePath, string outputPath);
}
