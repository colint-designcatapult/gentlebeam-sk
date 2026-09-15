using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Win32.SafeHandles;
using Com.Empyreanmed.Heracles.Enums.V1;
using Com.Empyreanmed.Heracles.Logs.V1;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

/// <summary>Owns the local protector, encrypted database lifecycle, and whole-database transfers.</summary>
public sealed class SqlCipherDatabase : IDisposable
{
    private const string InvalidDatabaseMessage = "The password is incorrect, or the database is unreadable or damaged.";
    private static readonly string[] SidecarSuffixes = ["-wal", "-shm", "-journal"];
    private readonly string _storageRoot;
    private readonly string _databasePath;
    private readonly string _legacyPath;
    private readonly string _keyPath;
    private readonly DpapiKeyStore _keyStore;
    private readonly SemaphoreSlim _transfer = new(1, 1);
    private readonly object _stateLock = new();
    private readonly HashSet<string> _ownedStaging = new(StringComparer.OrdinalIgnoreCase);
    private byte[]? _mek;
    private SqlCipherConnectionFactory? _connections;
    private string? _pendingImport;
    private bool _disposed;
    private bool _installed;

    public SqlCipherDatabase(string storageRoot)
    {
        _storageRoot = Path.GetFullPath(storageRoot);
        _databasePath = Path.Combine(_storageRoot, "heracles.encrypted.db");
        _legacyPath = Path.Combine(_storageRoot, "heracles.db");
        _keyPath = Path.Combine(_storageRoot, "heracles.db.key");
        _keyStore = new DpapiKeyStore(_keyPath);
    }

    public SqlCipherConnectionFactory Connections
    {
        get
        {
            ThrowIfUnavailable();
            return _connections!;
        }
    }

    public void Initialize(Func<string, bool> confirmNewRecoveryKey, Func<string?, string?> requestRecoveryKey)
    {
        ArgumentNullException.ThrowIfNull(confirmNewRecoveryKey);
        ArgumentNullException.ThrowIfNull(requestRecoveryKey);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connections is not null)
            throw new InvalidOperationException("The encrypted database is already initialized.");

        byte[]? candidate = null;
        try
        {
            SqlCipherConnectionFactory.EnsureProvider();
            Directory.CreateDirectory(_storageRoot);
            var databaseExists = Exists(_databasePath);
            var loaded = _keyStore.TryLoad(out var loadedKey);
            candidate = loadedKey;
            if (databaseExists)
            {
                RequireEncryptedFile(_databasePath);
                if (!loaded || !TryValidateLocal(candidate))
                {
                    CryptographicOperations.ZeroMemory(candidate);
                    candidate = null;
                    string? error = null;
                    while (true)
                    {
                        var text = requestRecoveryKey(error);
                        if (text is null)
                            throw new OperationCanceledException("Database recovery was cancelled.");
                        if (RecoveryKeyCodec.TryParse(text, out var recovered))
                        {
                            candidate = recovered;
                            if (TryValidateLocal(candidate))
                                break;
                            CryptographicOperations.ZeroMemory(candidate);
                            candidate = null;
                        }
                        error = "The recovery key is invalid or cannot unlock this database. Check the saved recovery key and try again.";
                    }
                    _keyStore.Save(candidate!);
                }
                // The encrypted database is authoritative, including after interrupted legacy cleanup.
                DeleteDatabaseFiles(_legacyPath);
            }
            else
            {
                if (!loaded)
                    candidate = RandomNumberGenerator.GetBytes(16);
                if (!confirmNewRecoveryKey(RecoveryKeyCodec.Format(candidate!)))
                    throw new OperationCanceledException("Database provisioning was cancelled.");
                _keyStore.Save(candidate!);
                var created = false;
                var validated = false;
                try
                {
                    ReserveFile(_databasePath);
                    created = true;
                    if (Exists(_legacyPath))
                        CopyDatabase(_legacyPath, "", _databasePath, Convert.ToHexString(candidate!), true, null, CancellationToken.None);
                    else
                        CreateEmptyDatabase(_databasePath, Convert.ToHexString(candidate!));
                    validated = true;
                }
                finally
                {
                    if (created && !validated)
                        DeleteDatabaseFiles(_databasePath);
                }
                DeleteDatabaseFiles(_legacyPath);
            }

            _connections = new SqlCipherConnectionFactory(_databasePath, Convert.ToHexString(candidate!));
            _mek = candidate;
            candidate = null;
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidDataException) { throw; }
        catch (SqliteException)
        {
            throw new InvalidDataException("The encrypted database could not be initialized. Check the database and saved recovery key; no replacement database was created over an existing database.");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new IOException("Database storage or the key file is unavailable. Check storage permissions and any remaining development database files before restarting.");
        }
        finally
        {
            if (candidate is not null)
                CryptographicOperations.ZeroMemory(candidate);
        }
    }

    public string GetRecoveryKey()
    {
        lock (_stateLock)
        {
            ThrowIfUnavailable();
            return RecoveryKeyCodec.Format(_mek!);
        }
    }

    public string GetDatabasePassword()
    {
        lock (_stateLock)
        {
            ThrowIfUnavailable();
            return Convert.ToHexString(_mek!);
        }
    }

    public async Task ExportAsync(string destinationPath, string password, CancellationToken cancellationToken = default,
        string? auditRecord = null)
    {
        if (password != string.Empty)
            ValidatePassword(password);
        await _transfer.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? temporaryPath = null;
        var created = false;
        try
        {
            ThrowIfUnavailable();
            var auditLogs = auditRecord is null ? null : new SqliteProtoRepository<Log>(_connections!, "logs");
            var destination = ValidateSelectedPath(destinationPath);
            var localPassword = Convert.ToHexString(_mek!);
            temporaryPath = Path.Combine(Path.GetDirectoryName(destination)!, $".heracles-export-{Guid.NewGuid():N}.db");
            var output = temporaryPath;
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReserveFile(output);
                created = true;
                CopyDatabase(_databasePath, localPassword, output, password, false, null, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                ValidateSelectedPath(destination);
                // Replace the directory entry, never write through a selected link or destroy an old backup on failure.
                if (Exists(destination))
                    File.Replace(output, destination, null);
                else
                    File.Move(output, destination);
            }, cancellationToken).ConfigureAwait(false);
            if (auditRecord is not null)
                await AppendUserAuditAsync(auditLogs!, auditRecord).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidDataException) { throw; }
        catch (SqliteException) { throw new InvalidDataException("The database could not be exported because its snapshot is unreadable or damaged."); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new IOException("The export location is unavailable or cannot be replaced. The previous destination has not been overwritten by an incomplete export.");
        }
        finally
        {
            try
            {
                if (created && temporaryPath is not null)
                    DeleteDatabaseFiles(temporaryPath);
            }
            finally { _transfer.Release(); }
        }
    }

    public async Task<string> PrepareImportAsync(string sourcePath, string password, CancellationToken cancellationToken = default)
    {
        if (password != string.Empty)
            ValidatePassword(password);
        await _transfer.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? stagingPath = null;
        var prepared = false;
        var created = false;
        try
        {
            ThrowIfUnavailable();
            var source = ValidateSelectedPath(sourcePath);
            var localPassword = Convert.ToHexString(_mek!);
            stagingPath = Path.Combine(_storageRoot, $".heracles-import-{Guid.NewGuid():N}.db");
            var output = stagingPath;
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var plaintextSource = IsPlaintextFile(source);
                Dictionary<string, Dictionary<string, Column>> requiredSchema;
                using (var live = OpenPrivate(_databasePath, localPassword, SqliteOpenMode.ReadOnly))
                using (var snapshot = live.BeginTransaction(deferred: true))
                    requiredSchema = ReadSchema(live, "main", snapshot);
                ReserveFile(output);
                created = true;
                lock (_stateLock) { _ownedStaging.Add(output); }
                CopyDatabase(source, plaintextSource ? string.Empty : password, output, localPassword,
                    plaintextSource, requiredSchema, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }, cancellationToken).ConfigureAwait(false);
            lock (_stateLock) { _pendingImport = stagingPath; }
            prepared = true;
            return stagingPath;
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidDataException) { throw; }
        catch (SqliteException) { throw new InvalidDataException(InvalidDatabaseMessage); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new IOException("The import source or local staging location is unavailable. Check the selected file and storage permissions.");
        }
        finally
        {
            if (!prepared)
            {
                try
                {
                    if (created && stagingPath is not null)
                        DeleteDatabaseFiles(stagingPath);
                }
                finally { _transfer.Release(); }
            }
        }
    }

    public void InstallPreparedImport(string stagingPath, string? auditRecord = null)
    {
        var owned = TakePendingImport(stagingPath);
        try
        {
            ThrowIfUnavailable();
            _connections!.ClosePool();
            // All application consumers must already be stopped. Invalidate their factory permanently.
            _installed = true;
            var password = Convert.ToHexString(_mek!);
            if (auditRecord is not null)
            {
                // The record becomes visible only if the prepared database is installed.
                // The live gRPC host has already stopped, so audit directly in staging.
                var stagedConnections = new SqlCipherConnectionFactory(owned, password);
                try { AppendUserAuditAsync(new SqliteProtoRepository<Log>(stagedConnections, "logs"), auditRecord).GetAwaiter().GetResult(); }
                finally { stagedConnections.ClosePool(); }
            }
            ValidateFile(owned, password);
            using (var live = OpenPrivate(_databasePath, password, SqliteOpenMode.ReadWrite))
            {
                var mode = Scalar(live, "PRAGMA journal_mode") as string;
                if (string.Equals(mode, "wal", StringComparison.OrdinalIgnoreCase))
                {
                    using var command = Command(live, "PRAGMA wal_checkpoint(TRUNCATE)");
                    using var reader = command.ExecuteReader();
                    if (!reader.Read() || reader.GetInt64(0) != 0 || reader.GetInt64(1) != reader.GetInt64(2))
                        throw new IOException("The live database is busy and could not be finalized. Restart the application before trying import again.");
                }
                if (!string.Equals(Scalar(live, "PRAGMA journal_mode=DELETE") as string, "delete", StringComparison.OrdinalIgnoreCase))
                    throw new IOException("The live database journal could not be finalized. Restart the application before trying import again.");
            }
            DeleteSidecars(_databasePath);
            File.Replace(owned, _databasePath, null);
        }
        catch (InvalidDataException) { throw; }
        catch (Exception error) when (error is SqliteException or IOException or UnauthorizedAccessException)
        {
            throw new IOException("Database replacement failed. The previous database was retained; close and restart the application before trying again.");
        }
        finally
        {
            try { DeleteDatabaseFiles(owned); }
            finally { _transfer.Release(); }
        }
    }

    private static Task AppendUserAuditAsync(SqliteProtoRepository<Log> logs, string message) =>
        logs.CreateAsync(new Log
        {
            Message = message,
            Type = LOGTYPE.User,
            Severity = SEVERITY.Info
        });

    public void DiscardPreparedImport(string stagingPath)
    {
        var owned = TakePendingImport(stagingPath);
        try { DeleteDatabaseFiles(owned); }
        finally { _transfer.Release(); }
    }

    public void Dispose()
    {
        lock (_stateLock)
        {
            if (_disposed)
                return;
            // Do not zero the MEK while a worker is using it. Import must be installed/discarded first.
            if (!_transfer.Wait(0))
                throw new InvalidOperationException("Finish or discard the database transfer before closing database storage.");
            try
            {
                _disposed = true;
                _connections?.ClosePool();
                if (_mek is not null)
                {
                    CryptographicOperations.ZeroMemory(_mek);
                    _mek = null;
                }
            }
            finally { _transfer.Release(); }
        }
    }

    private bool TryValidateLocal(byte[] key)
    {
        RequireEncryptedFile(_databasePath);
        try
        {
            using var connection = OpenPrivate(_databasePath, Convert.ToHexString(key), SqliteOpenMode.ReadOnly);
            using var transaction = connection.BeginTransaction(deferred: true);
            Scalar(connection, "SELECT count(*) FROM main.sqlite_master", transaction);
            // Integrity errors become InvalidDataException: never retry a key after successful schema access.
            ValidateIntegrity(connection, "main", true, transaction);
            return true;
        }
        catch (SqliteException error) when (error.SqliteErrorCode is 26 or 11) { return false; }
    }

    private static void CreateEmptyDatabase(string path, string password)
    {
        using (var connection = OpenPrivate(path, password, SqliteOpenMode.ReadWriteCreate))
            Execute(connection, "VACUUM");
        ValidateFile(path, password);
    }

    private static SqliteConnection OpenPrivate(string path, string password, SqliteOpenMode mode)
    {
        SqlCipherConnectionFactory.EnsureProvider();
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = mode,
            Pooling = false
        }.ToString());
        try
        {
            connection.Open();
            // Own the native handle before key validation: Microsoft.Data.Sqlite's Password
            // constructor path can throw while its internal connection is still being constructed.
            if (password.Length != 0)
            {
                using var quote = Command(connection, "SELECT quote($password)");
                quote.Parameters.AddWithValue("$password", password);
                var literal = (string)quote.ExecuteScalar()!;
                Execute(connection, $"PRAGMA key={literal}");
            }
            SqlCipherConnectionFactory.Configure(connection);
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    private static void CopyDatabase(string sourcePath, string sourcePassword, string outputPath, string outputPassword,
        bool plaintextSource, Dictionary<string, Dictionary<string, Column>>? requiredSchema, CancellationToken cancellationToken)
    {
        if (!plaintextSource)
            RequireEncryptedFile(sourcePath);
        using (var output = OpenPrivate(outputPath, outputPassword, SqliteOpenMode.ReadWriteCreate))
        {
            Execute(output, "PRAGMA journal_mode=DELETE");
            using (var attach = Command(output, "ATTACH DATABASE $path AS source KEY $key"))
            {
                attach.Parameters.AddWithValue("$path", new Uri(Path.GetFullPath(sourcePath)).AbsoluteUri + "?mode=ro");
                attach.Parameters.AddWithValue("$key", sourcePassword);
                attach.ExecuteNonQuery();
            }
            using (var snapshot = output.BeginTransaction(deferred: true))
            {
                // This first read fixes the source snapshot for validation, metadata and the complete copy.
                Scalar(output, "SELECT count(*) FROM source.sqlite_master", snapshot);
                ValidateIntegrity(output, "source", !plaintextSource, snapshot);
                cancellationToken.ThrowIfCancellationRequested();
                if (requiredSchema is not null)
                    RequireCompatibleSchema(requiredSchema, ReadSchema(output, "source", snapshot));
                var userVersion = Scalar(output, "PRAGMA source.user_version", snapshot);
                var applicationId = Scalar(output, "PRAGMA source.application_id", snapshot);
                var autoVacuum = Scalar(output, "PRAGMA source.auto_vacuum", snapshot);
                SetPragma(output, "auto_vacuum", autoVacuum!, snapshot);
                Execute(output, "SELECT sqlcipher_export('main', 'source')", snapshot);
                // sqlcipher_export may reconstruct sequence values from current rows. Preserve deleted high IDs too.
                if (Convert.ToInt64(Scalar(output, "SELECT count(*) FROM main.sqlite_master WHERE name='sqlite_sequence'", snapshot), CultureInfo.InvariantCulture) != 0)
                {
                    Execute(output, "DELETE FROM main.sqlite_sequence", snapshot);
                    Execute(output, "INSERT INTO main.sqlite_sequence(name, seq) SELECT name, seq FROM source.sqlite_sequence", snapshot);
                }
                SetPragma(output, "user_version", userVersion!, snapshot);
                SetPragma(output, "application_id", applicationId!, snapshot);
                cancellationToken.ThrowIfCancellationRequested();
                snapshot.Commit();
            }
            Execute(output, "DETACH DATABASE source");
            if (!string.Equals(Scalar(output, "PRAGMA journal_mode=DELETE") as string, "delete", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The database snapshot could not be finalized.");
        }
        ValidateFile(outputPath, outputPassword);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static void ValidateFile(string path, string password)
    {
        var encrypted = password.Length != 0;
        if (encrypted)
            RequireEncryptedFile(path);
        using var connection = OpenPrivate(path, password, SqliteOpenMode.ReadOnly);
        using var snapshot = connection.BeginTransaction(deferred: true);
        Scalar(connection, "SELECT count(*) FROM main.sqlite_master", snapshot);
        ValidateIntegrity(connection, "main", encrypted, snapshot);
    }

    private static void ValidateIntegrity(SqliteConnection connection, string alias, bool encrypted, SqliteTransaction transaction)
    {
        try
        {
            if (encrypted)
            {
                using var cipher = Command(connection, $"PRAGMA {alias}.cipher_integrity_check", transaction);
                using var cipherResult = cipher.ExecuteReader();
                if (cipherResult.Read())
                    throw new InvalidDataException(InvalidDatabaseMessage);
            }
            using var structural = Command(connection, $"PRAGMA {alias}.integrity_check", transaction);
            using var result = structural.ExecuteReader();
            if (!result.Read() || !string.Equals(result.GetString(0), "ok", StringComparison.Ordinal) || result.Read())
                throw new InvalidDataException(InvalidDatabaseMessage);
        }
        catch (SqliteException) { throw new InvalidDataException(InvalidDatabaseMessage); }
    }

    private sealed record Column(string Type, long NotNull, long PrimaryKey);

    private static Dictionary<string, Dictionary<string, Column>> ReadSchema(SqliteConnection connection, string alias, SqliteTransaction transaction)
    {
        var result = new Dictionary<string, Dictionary<string, Column>>(StringComparer.OrdinalIgnoreCase);
        using (var tables = Command(connection, $"SELECT name FROM {alias}.sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite\\_%' ESCAPE '\\'", transaction))
        using (var reader = tables.ExecuteReader())
            while (reader.Read())
                result.Add(reader.GetString(0), new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase));
        foreach (var (table, columns) in result)
        {
            using var command = Command(connection, "SELECT name, type, \"notnull\", pk FROM pragma_table_info($table, $schema)", transaction);
            command.Parameters.AddWithValue("$table", table);
            command.Parameters.AddWithValue("$schema", alias);
            using var reader = command.ExecuteReader();
            while (reader.Read())
                columns.Add(reader.GetString(0), new Column(reader.GetString(1).Trim(), reader.GetInt64(2), reader.GetInt64(3)));
        }
        return result;
    }

    private static void RequireCompatibleSchema(Dictionary<string, Dictionary<string, Column>> expected,
        Dictionary<string, Dictionary<string, Column>> actual)
    {
        foreach (var (table, columns) in expected)
        {
            if (!actual.TryGetValue(table, out var sourceColumns))
                throw new InvalidDataException("The selected database has an incompatible application schema.");
            foreach (var (name, column) in columns)
                if (!sourceColumns.TryGetValue(name, out var source) || source.NotNull != column.NotNull ||
                    source.PrimaryKey != column.PrimaryKey || !string.Equals(source.Type, column.Type, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("The selected database has an incompatible application schema.");
        }
    }

    private static void SetPragma(SqliteConnection connection, string name, object value, SqliteTransaction transaction)
    {
        using var quote = Command(connection, "SELECT quote($value)", transaction);
        quote.Parameters.AddWithValue("$value", value);
        var literal = (string)quote.ExecuteScalar()!;
        Execute(connection, $"PRAGMA main.{name}={literal}", transaction);
    }

    private static SqliteCommand Command(SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        return command;
    }

    private static object? Scalar(SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
    {
        using var command = Command(connection, sql, transaction);
        return command.ExecuteScalar();
    }

    private static void Execute(SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
    {
        using var command = Command(connection, sql, transaction);
        command.ExecuteNonQuery();
    }

    private string ValidateSelectedPath(string selectedPath)
    {
        string path;
        try { path = Path.GetFullPath(selectedPath); }
        catch (Exception error) when (error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new IOException("The selected database location is invalid.");
        }
        var relativePath = path[Path.GetPathRoot(path)!.Length..];
        if (relativePath.Contains(':') || relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(part => part.EndsWith(' ') || part.EndsWith('.')))
            throw new IOException("Select a regular database filename, not an alternate data stream or ambiguous Windows filename.");
        var physical = ResolvePhysicalPath(path);
        foreach (var protectedPath in new[] { _databasePath, _legacyPath, _keyPath })
        {
            if (SamePathOrSidecar(path, protectedPath) || SamePathOrSidecar(physical, ResolvePhysicalPath(protectedPath)))
                throw new IOException("Select a backup location outside the application's live database, key and temporary files.");
        }
        lock (_stateLock)
            foreach (var staging in _ownedStaging)
                if (SamePathOrSidecar(path, staging) || SamePathOrSidecar(physical, ResolvePhysicalPath(staging)))
                    throw new IOException("Application staging files cannot be selected as database backups.");
        return path;
    }

    private static bool SamePathOrSidecar(string path, string protectedPath) =>
        string.Equals(path, protectedPath, StringComparison.OrdinalIgnoreCase) ||
        SidecarSuffixes.Any(suffix => string.Equals(path, protectedPath + suffix, StringComparison.OrdinalIgnoreCase));

    private static string ResolvePhysicalPath(string path)
    {
        // Opening metadata also resolves junctions, symbolic links and Windows short-name aliases.
        // Missing export files are resolved against their nearest existing parent directory.
        var nativePath = path.StartsWith(@"\\?\", StringComparison.Ordinal) ? path :
            path.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + path[2..] : @"\\?\" + path;
        using var handle = CreateFile(nativePath, 0, 7, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero);
        if (handle.IsInvalid)
        {
            var error = Marshal.GetLastWin32Error();
            var parent = Path.GetDirectoryName(path);
            if (error is 2 or 3 && !string.IsNullOrEmpty(parent) && !string.Equals(path, parent, StringComparison.OrdinalIgnoreCase))
                return Path.Combine(ResolvePhysicalPath(parent), Path.GetFileName(path));
            throw new IOException("The selected database location cannot be accessed or resolved safely.");
        }

        var buffer = new StringBuilder(512);
        var length = GetFinalPathNameByHandle(handle, buffer, (uint)buffer.Capacity, 0);
        if (length >= buffer.Capacity)
        {
            buffer.Capacity = checked((int)length + 1);
            length = GetFinalPathNameByHandle(handle, buffer, (uint)buffer.Capacity, 0);
        }
        if (length == 0 || length >= buffer.Capacity)
            throw new IOException("The selected database location cannot be resolved safely.");
        var resolved = buffer.ToString();
        if (resolved.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            return @"\\" + resolved[8..];
        return resolved.StartsWith(@"\\?\", StringComparison.Ordinal) ? resolved[4..] : resolved;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string path, uint access, uint share, IntPtr security,
        uint disposition, uint flags, IntPtr template);

    [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle, StringBuilder path, uint size, uint flags);

    private string TakePendingImport(string stagingPath)
    {
        lock (_stateLock)
        {
            if (_pendingImport is null || !string.Equals(stagingPath, _pendingImport, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only an import prepared by this database instance can be completed.");
            var result = _pendingImport;
            _pendingImport = null;
            return result;
        }
    }

    private void ThrowIfUnavailable()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connections is null || _mek is null || _installed)
            throw new InvalidOperationException("Encrypted database storage is not available.");
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Contains('\0'))
            throw new ArgumentException("Enter a nonempty password without NUL characters.", nameof(password));
    }

    private static bool Exists(string path)
    {
        try { return (File.GetAttributes(path) & FileAttributes.Directory) == 0; }
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
    }

    private static void ReserveFile(string path)
    {
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    }

    private static bool IsPlaintextFile(string path)
    {
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        Span<byte> header = stackalloc byte[16];
        return file.Read(header) == header.Length && header.SequenceEqual("SQLite format 3\0"u8);
    }

    private static void RequireEncryptedFile(string path)
    {
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        Span<byte> header = stackalloc byte[16];
        if (file.Length < 4096 || file.Length % 4096 != 0 || file.Read(header) != header.Length || header.SequenceEqual("SQLite format 3\0"u8))
            throw new InvalidDataException(InvalidDatabaseMessage);
    }

    private static void DeleteSidecars(string path)
    {
        foreach (var suffix in SidecarSuffixes)
            File.Delete(path + suffix);
    }

    private static void DeleteDatabaseFiles(string path)
    {
        try
        {
            File.Delete(path);
            DeleteSidecars(path);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new IOException("An obsolete database or temporary file could not be removed. Check storage permissions before restarting.");
        }
    }
}
