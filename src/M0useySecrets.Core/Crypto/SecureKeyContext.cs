using System.Security.Cryptography;
using M0useySecrets.Core.Exceptions;

namespace M0useySecrets.Core.Crypto;

/// <summary>
/// Represents a secure context for holding the KEK in memory while the vault is unlocked.
/// </summary>
public class SecureKeyContext : IDisposable
{
    private byte[] _key;
    private bool _isLocked;
    private bool _disposed;

    public SecureKeyContext(byte[] key)
    {
        _key = key; // we take ownership of the key array, and will zero it out on lock/dispose. 
        //_key = new byte[key.Length];
        _isLocked = false;
        _disposed = false;
    }

    /// <summary>
    /// Gets the KEK if the vault is unlocked, or throws if the vault is locked or the context is disposed.
    /// </summary>
    public byte[] Key
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SecureKeyContext));

            if (_isLocked)
                throw new VaultLockedException();

            return _key;
        }
    }

    /// <summary>
    /// Locks the vault by zeroing out the KEK in memory and marking the context as locked.
    /// </summary>
    public void Lock()
    {
        // if already locked, no-op (safe to call multiple times)
        if (!_isLocked)
        {
            CryptographicOperations.ZeroMemory(_key);
            _isLocked = true;
        }
    }

    /// <summary>
    /// Disposes the context by zeroing out the KEK if not already done, and marking the context as disposed.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        // Zeroes the key if not already done
        Lock();

        _disposed = true;
    }

    /// <summary>
    /// Disposes the context by zeroing out the KEK if not already done, and marking the context as disposed.
    /// </summary>
    public void Dispose()
    {
        // call Lock() if not already locked
        if (!_isLocked)
        {
            Lock();
            Dispose(true);
        }

        GC.SuppressFinalize(this);
    }
}
