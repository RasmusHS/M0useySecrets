using System.Text.RegularExpressions;
using M0useySecrets.Core.Exceptions;
using M0useySecrets.Core.Services.Interfaces;

namespace M0useySecrets.Core.Services;

public class TemplateInjector : ITemplateInjector
{
    private readonly ISecretService _secretService;
    private static readonly Regex PlaceholderPattern = new(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);

    public TemplateInjector(ISecretService secretService)
    {
        _secretService = secretService;
    }

    public string InjectSecrets(string templateContent, Action<string>? onWarning = null)
    {
        return PlaceholderPattern.Replace(templateContent, match =>
        {
            string placeholder = match.Groups[1].Value.Trim();

            string ns;
            string name;

            int separatorIndex = placeholder.IndexOf('/');
            if (separatorIndex >= 0)
            {
                ns = placeholder[..separatorIndex];
                name = placeholder[(separatorIndex + 1)..];
            }
            else
            {
                ns = "default";
                name = placeholder;
            }

            try
            {
                var secret = _secretService.GetSecret(name, ns);
                return secret.Value;
            }
            catch (SecretNotFoundException)
            {
                onWarning?.Invoke($"Secret '{ns}/{name}' not found, leaving placeholder.");
                return match.Value;
            }
        });
    }

    public void InjectToFile(string templatePath, string outputPath, Action<string>? onWarning = null)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Template not found.", templatePath);

        if (Path.GetFullPath(templatePath) == Path.GetFullPath(outputPath))
            throw new InvalidOperationException("Output path cannot be the same as template.");

        string template = File.ReadAllText(templatePath);
        string result = InjectSecrets(template, onWarning);
        File.WriteAllText(outputPath, result);
    }
}
