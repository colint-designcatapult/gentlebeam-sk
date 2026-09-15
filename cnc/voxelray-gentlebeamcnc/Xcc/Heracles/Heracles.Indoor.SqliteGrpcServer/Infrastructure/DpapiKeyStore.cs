using System.Security.Cryptography;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

public sealed class DpapiKeyStore
{
    private readonly string _keyPath;

    public DpapiKeyStore(string keyPath) => _keyPath = Path.GetFullPath(keyPath);

    public bool TryLoad(out byte[] mek)
    {
        mek = Array.Empty<byte>();
        byte[]? protectedBytes = null;
        byte[]? decoded = null;
        try
        {
            protectedBytes = File.ReadAllBytes(_keyPath);
            decoded = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            if (decoded.Length != 16)
                return false;
            mek = decoded;
            decoded = null;
            return true;
        }
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
        catch (CryptographicException) { return false; }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new IOException("The database key file cannot be read. Check storage access and permissions.");
        }
        finally
        {
            if (protectedBytes is not null)
                CryptographicOperations.ZeroMemory(protectedBytes);
            if (decoded is not null)
                CryptographicOperations.ZeroMemory(decoded);
        }
    }

    public void Save(ReadOnlySpan<byte> mek)
    {
        if (mek.Length != 16)
            throw new ArgumentException("A database key requires exactly 16 bytes.", nameof(mek));
        var plainBytes = mek.ToArray();
        byte[]? protectedBytes = null;
        var temporaryPath = Path.Combine(Path.GetDirectoryName(_keyPath)!, $".heracles-key-{Guid.NewGuid():N}.tmp");
        try
        {
            protectedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            using (var file = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                file.Write(protectedBytes);
                file.Flush(flushToDisk: true);
            }
            if (File.Exists(_keyPath))
                File.Replace(temporaryPath, _keyPath, null);
            else
                File.Move(temporaryPath, _keyPath);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or CryptographicException)
        {
            throw new IOException("The database key cannot be saved. Check the Windows account and storage permissions.");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainBytes);
            if (protectedBytes is not null)
                CryptographicOperations.ZeroMemory(protectedBytes);
            try { File.Delete(temporaryPath); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
