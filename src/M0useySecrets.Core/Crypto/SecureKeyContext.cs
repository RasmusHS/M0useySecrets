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
        _isLocked = false;
        _disposed = false

        // 1. defensive copy: _key = new byte[key.Length], copy bytes in
        //    (so the caller can't mutate your internal state)
        // 2. _isLocked = false
        // 3. _disposed = false
    }

    public byte[] Key
    {
        get
        {
            // if _disposed → throw ObjectDisposedException
            // if _isLocked → throw VaultLockedException
            // return _key
            if (_disposed)
                throw new ObjectDisposedException();

            if (_isLocked)
                throw new VaultLockedException();

            return _key;
        }
        //set { _key = value; }
    }

    public void Lock()
    {
        // if already locked, no-op (safe to call multiple times)
        CryptographicOperations.ZeroMemory(_key);
        _isLocked = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        // if _disposed → return
        if (_disposed) return;

        // Lock()   ← zeroes the key if not already done
        Lock();

        _disposed = true;
    }

    public void Dispose()
    {
        // call Lock() if not already locked
        // optionally set a _disposed flag to make double-dispose safe

        Dispose(true);
        GC.SuppressFinalize(this);
        //throw new NotImplementedException();
    }
}
