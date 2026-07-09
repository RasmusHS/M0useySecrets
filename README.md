# M0useySecrets

A CLI tool for storing and retrieving secrets (API keys, passwords, tokens) with encrypted storage. Secrets are encrypted at rest using envelope encryption with AES-256-GCM, unlocked by a master password derived via PBKDF2. Supports namespaces, expiry, clipboard copy, TOTP generation, and template injection.
 
Built as part of a 12-projects-in-12-weeks course (Week 9), focusing on `System.Security.Cryptography`, secure key derivation, and `System.CommandLine`.
 
## Architecture
 
M0useySecrets uses **envelope encryption** — the same pattern used by AWS KMS and 1Password:
 
```
Master Password + Salt
        │
        ▼  PBKDF2 (600,000 iterations, SHA-256)
       KEK  (Key Encryption Key — derived on unlock, never stored)
        │
        ├── wraps DEK₁ ──► encrypts Secret₁
        ├── wraps DEK₂ ──► encrypts Secret₂
        └── wraps DEK₃ ──► encrypts Secret₃
```
 
Each secret gets its own randomly generated **DEK** (Data Encryption Key). The DEK encrypts the secret value via AES-256-GCM. The DEK itself is wrapped (encrypted) with the KEK and stored alongside the entry. Changing the master password only re-wraps the DEKs — secret values are never re-encrypted.
 
The vault file is stored in the user's home directory under `.m0useysecrets/vault.enc`:
 
| Platform | Vault Location |
|----------|---------------|
| Linux    | `/home/<username>/.m0useysecrets/vault.enc` |
| macOS    | `/Users/<username>/.m0useysecrets/vault.enc` |
| Windows  | `C:\Users\<username>\.m0useysecrets\vault.enc` |
 
The file uses a two-tier structure: a plaintext JSON header (salt, iterations, format version, password verification sentinel) wrapping an AES-256-GCM encrypted payload containing the serialized secrets.

 
## Tech Stack
 
- **Language:** C# / .NET 10
- **CLI Framework:** System.CommandLine 2.0
- **Cryptography:** System.Security.Cryptography (AES-GCM, PBKDF2 via Rfc2898DeriveBytes)
- **TOTP:** OtpNet
- **Serialization:** System.Text.Json
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection
## Project Structure
 
```
M0useySecrets.slnx
├── src/
│   ├── M0useySecrets.Core/          # Class library — zero presentation dependencies
│   │   ├── Crypto/
│   │   │   ├── AesEncryptor.cs       # AES-256-GCM encrypt/decrypt, key wrapping
│   │   │   ├── KeyDerivation.cs      # PBKDF2 key derivation, salt/DEK generation
│   │   │   ├── SecureKeyContext.cs    # Holds KEK in memory, IDisposable with ZeroMemory
│   │   │   └── TotpGenerator.cs      # TOTP code generation via OtpNet
│   │   ├── Models/
│   │   │   ├── DecryptedSecret.cs    # User-facing DTO (never serialized)
│   │   │   ├── EncryptionResult.cs   # Crypto return type (ciphertext, nonce, tag)
│   │   │   ├── SecretEntry.cs        # Encrypted secret with per-entry crypto fields
│   │   │   ├── Vault.cs              # In-memory working state
│   │   │   ├── VaultFile.cs          # On-disk format (serialization only)
│   │   │   └── VaultHeader.cs        # Pre-auth metadata (salt, iterations)
│   │   ├── Storage/
│   │   │   ├── VaultStore.cs         # Serialize → encrypt → atomic write round-trip
│   │   │   └── VaultPathResolver.cs  # Cross-platform vault file location
│   │   ├── Services/
│   │   │   ├── VaultService.cs       # Init, unlock, lock, change password
│   │   │   ├── SecretService.cs      # CRUD for secrets with envelope encryption
│   │   │   ├── ExpiryService.cs      # Expired secret detection and purge
│   │   │   └── TemplateInjector.cs   # {{placeholder}} replacement in template files
│   │   └── Exceptions/
│   │       ├── VaultLockedException.cs
│   │       ├── SecretNotFoundException.cs
│   │       └── InvalidPasswordException.cs
│   │
│   └── M0useySecrets.CLI/           # Console app — presentation layer
│       ├── Commands/
│       │   ├── InitCommand.cs        # Create a new vault
│       │   ├── AddCommand.cs         # Store a secret (stdin, prompt, or positional)
│       │   ├── GetCommand.cs         # Retrieve and display or copy a secret
│       │   ├── ListCommand.cs        # List secrets (filterable by namespace)
│       │   ├── RemoveCommand.cs      # Delete a secret (with confirmation)
│       │   ├── UpdateCommand.cs      # Update a secret's value
│       │   ├── ExportCommand.cs      # Export secrets to JSON or .env
│       │   ├── ImportCommand.cs      # Import secrets from JSON or .env
│       │   ├── InjectCommand.cs      # Template injection
│       │   └── TotpCommand.cs        # TOTP seed storage and code generation
│       ├── Helpers/
│       │   ├── PasswordPrompt.cs     # Masked password input
│       │   ├── ConsoleOutput.cs      # Colored, formatted terminal output
│       │   ├── ClipboardHelper.cs    # Cross-platform clipboard with auto-clear
│       │   └── UnlockVault.cs        # Shared unlock/try/finally/lock ceremony
│       ├── templates/                # Template files for inject command
│       ├── output/                   # Default output directory (gitignored)
│       └── Program.cs               # DI setup and command tree wiring
```
 
## Getting Started
 
### Prerequisites
 
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
### Build and Run
 
```bash
cd src/M0useySecrets.CLI
dotnet build
```
 
### Initialize a Vault
 
```bash
dotnet run -- init
```
 
You'll be prompted to create and confirm a master password.
 
## Usage
 
### Managing Secrets
 
```bash
# Add a secret (interactive prompt — most secure)
dotnet run -- add stripe-key
 
# Add a secret via stdin (for scripting)
echo "sk_live_abc123" | dotnet run -- add stripe-key --stdin
 
# Add with namespace and expiry
dotnet run -- add db-password --namespace prod --expires 2026-12-31
 
# Retrieve a secret
dotnet run -- get stripe-key
 
# Copy to clipboard (auto-clears after 60 seconds)
dotnet run -- get stripe-key --copy
 
# List all secrets
dotnet run -- list
 
# List secrets in a namespace
dotnet run -- list --namespace prod
 
# Update a secret
dotnet run -- update stripe-key
 
# Remove a secret (with confirmation)
dotnet run -- remove stripe-key
 
# Remove without confirmation
dotnet run -- remove stripe-key --force
```
 
### Export and Import
 
```bash
# Export to JSON (default: output/secrets.json)
dotnet run -- export
 
# Export to .env format
dotnet run -- export --format env
 
# Export with custom filename and directory
dotnet run -- export --filename prod-secrets --format json --output-dir /backups
 
# Import from JSON
dotnet run -- import secrets.json
 
# Import from .env
dotnet run -- import config.env --format env --namespace prod
```
 
### Template Injection
 
Create a template file in `templates/` with `{{namespace/name}}` placeholders:
 
```
# templates/.env.template
DATABASE_URL={{prod/db-connection}}
STRIPE_KEY={{stripe-key}}
API_TOKEN={{default/api-token}}
```
 
Placeholders without a namespace prefix default to the `default` namespace.
 
```bash
# Inject secrets into template (default: output/.env)
dotnet run -- inject .env.template
 
# Custom output
dotnet run -- inject .env.template --filename .env.local --output-dir /app
```
 
Missing secrets leave their placeholder intact with a warning.
 
### TOTP Authenticator
 
```bash
# Store a TOTP seed
dotnet run -- totp add github JBSWY3DPEHPK3PXP
 
# Generate current code
dotnet run -- totp get github
 
# Copy code to clipboard (auto-clears when code expires)
dotnet run -- totp get github --copy
```
 
## Security Notes
 
- **Master password** is never stored — only the PBKDF2-derived KEK exists in memory during a session
- **KEK is zeroed** from memory via `CryptographicOperations.ZeroMemory` when the vault is locked
- **Each secret** has its own randomly generated DEK, wrapped with the KEK
- **AES-256-GCM** provides authenticated encryption — tampering is detected
- **Password verification** uses a sentinel value with constant-time comparison (`CryptographicOperations.FixedTimeEquals`)
- **Atomic writes** prevent vault corruption on crash (write to temp file, then rename)
- **Shell history safety** — the `add` and `update` commands default to interactive prompts; `--stdin` supports piping for automation
- **Clipboard auto-clear** wipes copied secrets after a timeout
## Output Files
 
Exported secrets and injected template output are written to `output/` by default, which is gitignored. Do not commit plaintext secret files to version control.
 
## License
 
See [LICENSE.txt](LICENSE.txt) for details.
