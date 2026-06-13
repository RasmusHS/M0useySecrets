using System.Security.Cryptography;
using M0useySecrets.Core.Exceptions;

namespace M0useySecrets.Core.Crypto;

public class SecureKeyContext : IDisposable
{
    private byte[] _key;
    private bool _isLocked;
    private bool _disposed;

    public SecureKeyContext(byte[] key)
    {
        _key = key;
        //_key = new byte[key.Length];
        _isLocked = false;
        _disposed = false;
    }

    public byte[] Key
    {
        get
        {
            // if _disposed → throw ObjectDisposedException
            // if _isLocked → throw VaultLockedException
            // return _key
            if (_disposed)
                throw new ObjectDisposedException(nameof(SecureKeyContext));

            if (_isLocked)
                throw new VaultLockedException();

            return _key;
        }
        //set { _key = value; }
    }

    public void Lock()
    {
        // if already locked, no-op (safe to call multiple times)
        if (!_isLocked)
        {
            CryptographicOperations.ZeroMemory(_key);
            _isLocked = true;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        // Zeroes the key if not already done
        Lock();

        _disposed = true;
    }

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
