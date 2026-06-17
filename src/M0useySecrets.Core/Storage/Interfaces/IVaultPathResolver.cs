namespace M0useySecrets.Core.Storage.Interfaces;

public interface IVaultPathResolver
{
    /// <summary>
    /// The name of the directory where the vault file is stored. 
    /// This is a hidden directory named ".m0useysecrets" located in the user's home folder.
    /// </summary>
    private const string VaultDirectory = ".m0useysecrets";

    /// <summary>
    /// The name of the vault file that contains the encrypted secrets. 
    /// This file is named "vault.enc" and is located within the vault directory.
    /// </summary>
    private const string VaultFileName = "vault.enc";

    /// <summary>
    /// Gets the directory where the vault file is stored. This is typically a hidden directory in the user's home folder.
    /// For Windows, this would be something like "C:\Users\Username\.m0useysecrets". 
    /// For Linux and macOS, this would be something like "/home/username/.m0useysecrets" or "/Users/username/.m0useysecrets".
    /// </summary>
    /// <returns>The full path to the vault directory.</returns>
    string GetVaultDirectory();

    /// <summary>
    /// Gets the full path to the vault file, which is located within the vault directory. The vault file is named "vault.enc".
    /// </summary>
    /// <returns>The full path to the vault file.</returns>
    string GetVaultPath();

    /// <summary>
    /// Checks if the vault file exists at the expected location. 
    /// This method returns true if the vault file is found, and false otherwise.
    /// </summary>
    /// <returns>True if the vault file exists, false otherwise.</returns>
    bool VaultExists();

    /// <summary>
    /// Ensures that the vault directory exists. 
    /// If the directory does not exist, it will be created. 
    /// This method does not check for the existence of the vault file itself, only the directory that contains it.
    /// </summary>
    void EnsureDirectoryExists();
}
