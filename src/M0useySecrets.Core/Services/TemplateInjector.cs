using M0useySecrets.Core.Services.Interfaces;

namespace M0useySecrets.Core.Services;

public class TemplateInjector : ITemplateInjector
{
    private readonly ISecretService _secretService;

    public TemplateInjector(ISecretService secretService)
    {
        _secretService = secretService;
    }

    public string InjectSecrets(string templateContent)
    {
        // 1. find all placeholders matching a pattern like {{namespace/name}}
        //    Regex: \{\{([^/}]+)/([^}]+)\}\}
        //    group 1 = namespace, group 2 = secret name
        //


        // 2. for each match:
        //    a. call _secretService.GetSecret(name, namespace)
        //    b. replace the placeholder with the decrypted value
        //    c. if secret not found → decide on behavior:
        //       option A: throw (fail fast, safest)
        //       option B: leave placeholder untouched + warn
        //


        // 3. return the processed string


        throw new NotImplementedException();
    }

    public void InjectToFile(string templatePath, string outputPath)
    {
        // 1. read template from disk


        // 2. content = InjectSecrets(templateContent)


        // 3. write to outputPath
        //    guard: don't allow outputPath == templatePath (would destroy the template)


        throw new NotImplementedException();
    }
}
