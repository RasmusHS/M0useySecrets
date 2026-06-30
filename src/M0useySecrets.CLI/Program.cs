using System.CommandLine;
using M0useySecrets.CLI.Commands;
using M0useySecrets.Core.Crypto;
using M0useySecrets.Core.Crypto.Interfaces;
using M0useySecrets.Core.Services;
using M0useySecrets.Core.Services.Interfaces;
using M0useySecrets.Core.Storage;
using M0useySecrets.Core.Storage.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddSingleton<IAesEncryptor, AesEncryptor>()
    .AddSingleton<IKeyDerivation, KeyDerivation>()
    .AddSingleton<IVaultPathResolver, VaultPathResolver>()
    .AddSingleton<IVaultStore, VaultStore>()
    .AddSingleton<IVaultService, VaultService>()
    .AddSingleton<ISecretService, SecretService>()
    .AddSingleton<IExpiryService, ExpiryService>()
    .BuildServiceProvider();

var rootCommand = new RootCommand("M0useySecrets — encrypted secrets manager");

rootCommand.Subcommands.Add(InitCommand.Create(services));
rootCommand.Subcommands.Add(AddCommand.Create(services));
rootCommand.Subcommands.Add(GetCommand.Create(services));
rootCommand.Subcommands.Add(ListCommand.Create(services));
rootCommand.Subcommands.Add(RemoveCommand.Create(services));
rootCommand.Subcommands.Add(UpdateCommand.Create(services));

return rootCommand.Parse(args).Invoke();
