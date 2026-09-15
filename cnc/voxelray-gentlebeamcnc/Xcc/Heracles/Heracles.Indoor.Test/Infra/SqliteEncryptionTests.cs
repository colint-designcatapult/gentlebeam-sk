extern alias SqliteServer;

using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Database = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqlCipherDatabase;
using KeyStore = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.DpapiKeyStore;
using RecoveryKeyCodec = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.RecoveryKeyCodec;
using ConnectionFactory = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqlCipherConnectionFactory;
using LogRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log>;
using Log = SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
[NonParallelizable]
public sealed class SqliteEncryptionTests
{
    private readonly List<Database> _databases = [];
    private string _root = null!;
    private string LivePath => Path.Combine(_root, "heracles.encrypted.db");
    private string LegacyPath => Path.Combine(_root, "heracles.db");
    private string KeyPath => Path.Combine(_root, "heracles.db.key");

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), $"heracles-cipher-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
        // Use the application's provider initialization, never an ordinary SQLite bundle.
        _ = new ConnectionFactory(Path.Combine(_root, "provider-only.db"), "provider-initialization");
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var database in _databases)
            database.Dispose();
        _databases.Clear();
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    [Test]
    public async Task EncryptedRepositoryPersistsAndUnkeyedAccessFails()
    {
        var database = Initialize();
        var repository = new LogRepository(database.Connections, "logs");
        var created = await repository.CreateAsync(new Log { Message = "distinctive encrypted clinical record" });
        var localPassword = LocalPassword(database);
        database.Dispose();

        Assert.That(File.ReadAllBytes(LivePath).AsSpan(0, 16).SequenceEqual("SQLite format 3\0"u8), Is.False);
        Assert.Throws<SqliteException>(() => ReadScalar(LivePath, "", "SELECT count(*) FROM sqlite_master"));
        Assert.Throws<SqliteException>(() => ReadScalar(LivePath, new string('0', 32), "SELECT count(*) FROM sqlite_master"));

        var reopened = InitializeWithoutPrompt();
        var persisted = await new LogRepository(reopened.Connections, "logs").ReadAsync(created.Id);
        Assert.That(persisted!.Message, Is.EqualTo("distinctive encrypted clinical record"));
        using var keyed = Open(LivePath, localPassword);
        Assert.That(Scalar(keyed, "PRAGMA cipher_version")!.ToString(), Does.StartWith("4."));
        Assert.That(Scalar(keyed, "PRAGMA cipher_use_hmac")!.ToString(), Is.EqualTo("1"));
        Assert.That(Scalar(keyed, "PRAGMA kdf_iter")!.ToString(), Is.EqualTo("256000"));
        Assert.That(Scalar(keyed, "PRAGMA cipher_page_size")!.ToString(), Is.EqualTo("4096"));
        using var check = keyed.CreateCommand();
        check.CommandText = "PRAGMA cipher_integrity_check";
        using var result = check.ExecuteReader();
        Assert.That(result.Read(), Is.False, "A successful cipher integrity check returns no rows.");
    }

    [Test]
    public void RuntimeConnectionsCannotRecreateDeletedDatabase()
    {
        var database = Initialize();
        var factory = database.Connections;
        SqliteConnection.ClearAllPools();
        File.Delete(LivePath);
        Assert.Throws<IOException>(() => factory.Open());
        Assert.That(File.Exists(LivePath), Is.False);
    }

    [Test]
    public async Task DevelopmentMigrationPreservesJsonHierarchySchemaMetadataAndSequence()
    {
        using (var plaintext = Open(LegacyPath, "", create: true))
        {
            Execute(plaintext, "PRAGMA auto_vacuum=FULL; PRAGMA user_version=42; PRAGMA application_id=1729;");
            Execute(plaintext, """
                CREATE TABLE logs(id INTEGER PRIMARY KEY AUTOINCREMENT, data TEXT NOT NULL);
                INSERT INTO logs(id, data) VALUES(7, '{"id":"7","message":"migrated protobuf log"}');
                INSERT INTO logs(id, data) VALUES(900, '{"id":"900","message":"deleted"}');
                DELETE FROM logs WHERE id=900;
                CREATE TABLE children(id INTEGER PRIMARY KEY AUTOINCREMENT, parent_id INTEGER NOT NULL, data TEXT NOT NULL);
                CREATE INDEX ix_children_parent ON children(parent_id);
                CREATE TABLE audit(message TEXT NOT NULL);
                CREATE TRIGGER child_added AFTER INSERT ON children BEGIN INSERT INTO audit VALUES('created'); END;
                INSERT INTO children(parent_id,data) VALUES(7,'child JSON');
                """);
        }

        var database = Initialize();
        Assert.That(File.Exists(LegacyPath), Is.False);
        var repository = new LogRepository(database.Connections, "logs");
        Assert.That((await repository.ReadAsync(7))!.Message, Is.EqualTo("migrated protobuf log"));
        Assert.That((await repository.CreateAsync(new Log { Message = "after migration" })).Id, Is.EqualTo(901));
        using var connection = database.Connections.Open();
        Assert.That(Scalar(connection, "PRAGMA user_version"), Is.EqualTo(42L));
        Assert.That(Scalar(connection, "PRAGMA application_id"), Is.EqualTo(1729L));
        Assert.That(Scalar(connection, "PRAGMA auto_vacuum"), Is.EqualTo(1L));
        Assert.That(Scalar(connection, "SELECT parent_id FROM children"), Is.EqualTo(7L));
        Assert.That(Scalar(connection, "SELECT name FROM sqlite_master WHERE type='index' AND tbl_name='children'"), Is.EqualTo("ix_children_parent"));
        Execute(connection, "INSERT INTO children(parent_id,data) VALUES(7,'another child')");
        Assert.That(Scalar(connection, "SELECT count(*) FROM audit WHERE message='created'"), Is.EqualTo(2L));
    }

    [Test]
    public void ProvisioningCancellationDoesNotCommitKeyOrTouchDevelopmentSource()
    {
        using (var source = Open(LegacyPath, "", create: true))
            Execute(source, "CREATE TABLE retained(value TEXT); INSERT INTO retained VALUES('untouched')");
        var before = Digest(LegacyPath);
        var database = NewDatabase();
        Assert.Throws<OperationCanceledException>(() => database.Initialize(_ => false, _ => throw new AssertionException("Unexpected recovery prompt.")));
        Assert.That(File.Exists(KeyPath), Is.False);
        Assert.That(File.Exists(LivePath), Is.False);
        Assert.That(Digest(LegacyPath), Is.EqualTo(before));
    }

    [Test]
    public void MigrationFailurePreservesSourceAndReusesProtectorOnRetry()
    {
        File.WriteAllBytes(LegacyPath, "not a SQLite database"u8.ToArray());
        var before = Digest(LegacyPath);
        var first = NewDatabase();
        string? savedKey = null;
        Assert.Throws<InvalidDataException>(() => first.Initialize(key => { savedKey = key; return true; }, _ => null));
        Assert.That(File.Exists(LivePath), Is.False);
        Assert.That(Digest(LegacyPath), Is.EqualTo(before));
        var protectedKey = File.ReadAllBytes(KeyPath);
        var second = NewDatabase();
        Assert.Throws<OperationCanceledException>(() => second.Initialize(key => { Assert.That(key, Is.EqualTo(savedKey)); return false; }, _ => null));
        Assert.That(File.ReadAllBytes(KeyPath), Is.EqualTo(protectedKey));
    }

    [Test]
    public void ValidEncryptedDatabaseTakesPrecedenceOverRemainingDevelopmentSource()
    {
        var database = Initialize();
        SeedRecords(database, "authoritative");
        database.Dispose();
        using (var legacy = Open(LegacyPath, "", create: true))
            Execute(legacy, "CREATE TABLE obsolete(value TEXT); INSERT INTO obsolete VALUES('do not merge')");
        var reopened = InitializeWithoutPrompt();
        using var connection = reopened.Connections.Open();
        Assert.That(Scalar(connection, "SELECT data FROM records"), Is.EqualTo("authoritative"));
        Assert.That(Scalar(connection, "SELECT count(*) FROM sqlite_master WHERE name='obsolete'"), Is.EqualTo(0L));
        Assert.That(File.Exists(LegacyPath), Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MissingOrCorruptProtectorAllowsRecoveryAndRestoresAutomaticUnlock(bool corrupt)
    {
        var database = Initialize();
        SeedRecords(database, "recover without data loss");
        var recoveryKey = database.GetRecoveryKey();
        database.Dispose();
        if (corrupt)
            File.WriteAllBytes(KeyPath, [1, 2, 3]);
        else
            File.Delete(KeyPath);
        var before = Digest(LivePath);
        var recovered = NewDatabase();
        var attempts = 0;
        recovered.Initialize(_ => throw new AssertionException("A present encrypted database must not be provisioned."), error =>
        {
            attempts++;
            if (attempts == 1)
            {
                Assert.That(error, Is.Null);
                return "000001-000000-000000-000000-000000-000000-000000-000000";
            }
            Assert.That(error, Is.Not.Null);
            return attempts == 2 ? RecoveryKeyCodec.Format(new byte[16]) : recoveryKey;
        });
        Assert.That(attempts, Is.EqualTo(3));
        Assert.That(Digest(LivePath), Is.EqualTo(before));
        recovered.Dispose();
        var reopened = InitializeWithoutPrompt();
        using var connection = reopened.Connections.Open();
        Assert.That(Scalar(connection, "SELECT data FROM records"), Is.EqualTo("recover without data loss"));
    }

    [Test]
    public void WrongThenCancelledRecoveryLeavesDatabaseAndLegacySourceUntouched()
    {
        var database = Initialize();
        SeedRecords(database, "keep encrypted data");
        database.Dispose();
        File.Delete(KeyPath);
        File.WriteAllBytes(LegacyPath, "development source must not replace encrypted data"u8.ToArray());
        var before = Digest(LivePath);
        var legacyBefore = Digest(LegacyPath);
        var recovered = NewDatabase();
        var attempts = 0;
        Assert.Throws<OperationCanceledException>(() => recovered.Initialize(_ => throw new AssertionException("Unexpected provisioning."), _ =>
            ++attempts == 1 ? RecoveryKeyCodec.Format(new byte[16]) : null));
        Assert.That(Digest(LivePath), Is.EqualTo(before));
        Assert.That(Digest(LegacyPath), Is.EqualTo(legacyBefore));
        Assert.That(File.Exists(KeyPath), Is.False);
    }

    [Test]
    public void CurrentUserDpapiReloadsRealProtectorAndRejectsWrongDecodedLength()
    {
        var key = RandomNumberGenerator.GetBytes(16);
        var store = new KeyStore(KeyPath);
        try
        {
            store.Save(key);
            Assert.That(store.TryLoad(out var loaded), Is.True);
            try { Assert.That(loaded, Is.EqualTo(key)); }
            finally { CryptographicOperations.ZeroMemory(loaded); }
            var malformed = ProtectedData.Protect(new byte[15], null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(KeyPath, malformed);
            CryptographicOperations.ZeroMemory(malformed);
            Assert.That(store.TryLoad(out loaded), Is.False);
            Assert.That(loaded, Is.Empty);
        }
        finally { CryptographicOperations.ZeroMemory(key); }
    }

    [Test]
    public void NonHeaderPageCorruptionFailsClosedWithoutRecoveryOrRecreation()
    {
        var database = Initialize();
        SeedRecords(database, new string('x', 14000));
        database.Dispose();
        CorruptDataPage(LivePath);
        var damaged = Digest(LivePath);
        var reopened = NewDatabase();
        Assert.Throws<InvalidDataException>(() => reopened.Initialize(_ => throw new AssertionException("Cannot recreate corruption."), _ => throw new AssertionException("Cannot recover corruption by changing key.")));
        Assert.That(Digest(LivePath), Is.EqualTo(damaged));
    }

    [Test]
    public void EmptyExistingEncryptedFileCannotBeTreatedAsFreshProvisioning()
    {
        File.WriteAllBytes(LivePath, []);
        var database = NewDatabase();
        Assert.Throws<InvalidDataException>(() => database.Initialize(
            _ => throw new AssertionException("An existing file must not be provisioned."),
            _ => throw new AssertionException("An empty file cannot be recovered with a key.")));
        Assert.That(new FileInfo(LivePath).Length, Is.Zero);
        Assert.That(File.Exists(KeyPath), Is.False);
    }

    [Test]
    public void ValidDpapiProtectorForDifferentKeyRequiresRecovery()
    {
        var database = Initialize();
        SeedRecords(database, "original encrypted data");
        var savedKey = database.GetRecoveryKey();
        database.Dispose();
        var wrongKey = new byte[16];
        new KeyStore(KeyPath).Save(wrongKey);
        var recovered = NewDatabase();
        var prompted = false;
        recovered.Initialize(_ => throw new AssertionException("Must not replace a present database."), _ =>
        {
            prompted = true;
            return savedKey;
        });
        Assert.That(prompted, Is.True);
        using var connection = recovered.Connections.Open();
        Assert.That(Scalar(connection, "SELECT data FROM records"), Is.EqualTo("original encrypted data"));
    }

    [Test]
    public async Task PasswordSnapshotRoundTripPreservesEverythingAndStableLocalKey()
    {
        const string transferPassword = "  pa'ss;密碼;雪  ";
        var database = Initialize();
        SeedRecords(database, "prior clinical record");
        using (var connection = database.Connections.Open())
        {
            Execute(connection, """
                CREATE TABLE users(id INTEGER PRIMARY KEY, data TEXT NOT NULL);
                CREATE TABLE settings(id INTEGER PRIMARY KEY, data TEXT NOT NULL);
                INSERT INTO users VALUES(17,'administrator and permissions');
                INSERT INTO settings VALUES(4,'prior settings');
                INSERT INTO records(id,parent_id,data) VALUES(800,0,'deleted record');
                DELETE FROM records WHERE id=800;
                """);
        }
        var recoveryKey = database.GetRecoveryKey();
        var protector = File.ReadAllBytes(KeyPath);
        var localPassword = LocalPassword(database);
        var backup = Path.Combine(_root, "patient's;備份.db");
        await database.ExportAsync(backup, transferPassword);
        Assert.Throws<SqliteException>(() => ReadScalar(backup, "", "SELECT count(*) FROM sqlite_master"));
        Assert.Throws<SqliteException>(() => ReadScalar(backup, localPassword, "SELECT count(*) FROM sqlite_master"));
        Assert.Throws<SqliteException>(() => ReadScalar(backup, transferPassword.Trim(), "SELECT count(*) FROM sqlite_master"));
        Assert.That(ReadScalar(backup, transferPassword, "SELECT data FROM records"), Is.EqualTo("prior clinical record"));
        using (var connection = database.Connections.Open())
        {
            Execute(connection, "UPDATE records SET data='later clinical record'; UPDATE users SET data='later users'; UPDATE settings SET data='later settings'");
        }
        var staged = await database.PrepareImportAsync(backup, transferPassword);
        try
        {
            Assert.That(ReadScalar(staged, localPassword, "SELECT data FROM records"), Is.EqualTo("prior clinical record"));
            Assert.Throws<SqliteException>(() => ReadScalar(staged, transferPassword, "SELECT count(*) FROM sqlite_master"));
            var prepared = staged;
            staged = null!;
            database.InstallPreparedImport(prepared);
        }
        finally { if (staged is not null) database.DiscardPreparedImport(staged); }
        var reopened = InitializeWithoutPrompt();
        Assert.That(reopened.GetRecoveryKey(), Is.EqualTo(recoveryKey));
        Assert.That(File.ReadAllBytes(KeyPath), Is.EqualTo(protector));
        using var imported = reopened.Connections.Open();
        Assert.That(Scalar(imported, "SELECT data FROM users WHERE id=17"), Is.EqualTo("administrator and permissions"));
        Assert.That(Scalar(imported, "SELECT data FROM settings WHERE id=4"), Is.EqualTo("prior settings"));
        Assert.That(Scalar(imported, "SELECT data FROM records"), Is.EqualTo("prior clinical record"));
        Assert.That(Scalar(imported, "INSERT INTO records(parent_id,data) VALUES(0,'next'); SELECT last_insert_rowid()"), Is.EqualTo(801L));
        Assert.That(Scalar(imported, "PRAGMA journal_mode"), Is.EqualTo("delete"));
    }

    [Test]
    public async Task EmptyExportPasswordCreatesUnkeyedSnapshotWithoutChangingLiveEncryption()
    {
        var database = Initialize();
        using (var connection = database.Connections.Open())
            Execute(connection, "PRAGMA auto_vacuum=FULL; VACUUM; PRAGMA user_version=42; PRAGMA application_id=1729;");
        SeedRecords(database, "clinical plaintext export");
        using (var connection = database.Connections.Open())
        {
            Execute(connection, """
                CREATE TABLE users(id INTEGER PRIMARY KEY, data TEXT NOT NULL);
                CREATE TABLE settings(id INTEGER PRIMARY KEY, data TEXT NOT NULL);
                INSERT INTO users VALUES(17,'administrator and permissions');
                INSERT INTO settings VALUES(4,'exported settings');
                INSERT INTO records(id,parent_id,data) VALUES(800,17,'deleted record');
                DELETE FROM records WHERE id=800;
                UPDATE records SET parent_id=17;
                CREATE INDEX ix_records_parent ON records(parent_id);
                CREATE TABLE audit(message TEXT NOT NULL);
                CREATE TRIGGER record_added AFTER INSERT ON records BEGIN INSERT INTO audit VALUES('created'); END;
                """);
        }
        var liveBefore = Digest(LivePath);
        var protectorBefore = File.ReadAllBytes(KeyPath);
        var backup = Path.Combine(_root, "unencrypted-export.db");
        await database.ExportAsync(backup, "");

        Assert.That(File.ReadAllBytes(backup).AsSpan(0, 16).SequenceEqual("SQLite format 3\0"u8), Is.True);
        using (var snapshot = Open(backup, "", write: true))
        {
            Assert.That(Scalar(snapshot, "SELECT data FROM records WHERE parent_id=17"), Is.EqualTo("clinical plaintext export"));
            Assert.That(Scalar(snapshot, "SELECT data FROM users WHERE id=17"), Is.EqualTo("administrator and permissions"));
            Assert.That(Scalar(snapshot, "SELECT data FROM settings WHERE id=4"), Is.EqualTo("exported settings"));
            Assert.That(Scalar(snapshot, "PRAGMA user_version"), Is.EqualTo(42L));
            Assert.That(Scalar(snapshot, "PRAGMA application_id"), Is.EqualTo(1729L));
            Assert.That(Scalar(snapshot, "PRAGMA auto_vacuum"), Is.EqualTo(1L));
            Assert.That(Scalar(snapshot, "SELECT name FROM sqlite_master WHERE type='index' AND tbl_name='records'"), Is.EqualTo("ix_records_parent"));
            Assert.That(Scalar(snapshot, "INSERT INTO records(parent_id,data) VALUES(17,'next'); SELECT last_insert_rowid()"), Is.EqualTo(801L));
            Assert.That(Scalar(snapshot, "SELECT message FROM audit"), Is.EqualTo("created"));
            Assert.That(Scalar(snapshot, "PRAGMA integrity_check"), Is.EqualTo("ok"));
            Assert.That(Scalar(snapshot, "PRAGMA journal_mode"), Is.EqualTo("delete"));
        }
        Assert.That(Digest(LivePath), Is.EqualTo(liveBefore));
        Assert.That(File.ReadAllBytes(KeyPath), Is.EqualTo(protectorBefore));
        Assert.Throws<SqliteException>(() => ReadScalar(LivePath, "", "SELECT data FROM records"));
        Assert.That(ReadScalar(LivePath, LocalPassword(database), "SELECT data FROM records"), Is.EqualTo("clinical plaintext export"));
    }

    [TestCase("", 1024)]
    [TestCase("not used for plaintext", 4096)]
    public async Task PlaintextImportRestoresDataUnderExistingLocalKey(string password, int pageSize)
    {
        var database = Initialize();
        SeedRecords(database, "archived clinical data");
        using (var connection = database.Connections.Open())
            Execute(connection, """
                CREATE TABLE users(id INTEGER PRIMARY KEY, data TEXT NOT NULL);
                CREATE TABLE settings(id INTEGER PRIMARY KEY, data TEXT NOT NULL);
                INSERT INTO users VALUES(17,'archived administrator');
                INSERT INTO settings VALUES(4,'archived settings');
                """);
        var recoveryKey = database.GetRecoveryKey();
        var localPassword = LocalPassword(database);
        var protector = File.ReadAllBytes(KeyPath);
        var source = Path.Combine(_root, "plaintext-import.db");
        await database.ExportAsync(source, "");
        using (var connection = Open(source, "", write: true))
            Execute(connection, $"PRAGMA page_size={pageSize}; VACUUM");
        var sourceBefore = Digest(source);
        using (var connection = database.Connections.Open())
            Execute(connection, "UPDATE records SET data='new clinical data'; DELETE FROM users; UPDATE settings SET data='new settings'");

        var staging = await database.PrepareImportAsync(source, password);
        Assert.Throws<SqliteException>(() => ReadScalar(staging, "", "SELECT data FROM records"));
        Assert.That(ReadScalar(staging, localPassword, "SELECT data FROM records"), Is.EqualTo("archived clinical data"));
        database.InstallPreparedImport(staging);

        var reopened = Initialize();
        Assert.That(reopened.GetRecoveryKey(), Is.EqualTo(recoveryKey));
        Assert.That(File.ReadAllBytes(KeyPath), Is.EqualTo(protector));
        Assert.That(ReadScalar(LivePath, localPassword, "SELECT data FROM users WHERE id=17"), Is.EqualTo("archived administrator"));
        Assert.That(ReadScalar(LivePath, localPassword, "SELECT data FROM settings WHERE id=4"), Is.EqualTo("archived settings"));
        Assert.Throws<SqliteException>(() => ReadScalar(LivePath, "", "SELECT data FROM records"));
        Assert.That(Digest(source), Is.EqualTo(sourceBefore));
    }

    [Test]
    public async Task StructurallyCorruptPlaintextImportPreservesCurrentDatabase()
    {
        var database = Initialize();
        SeedRecords(database, new string('p', 14000));
        var source = Path.Combine(_root, "truncated-plaintext.db");
        await database.ExportAsync(source, "");
        using (var file = new FileStream(source, FileMode.Open, FileAccess.Write, FileShare.None))
            file.SetLength(4096);
        var sourceBefore = Digest(source);
        var liveBefore = Digest(LivePath);
        Assert.ThrowsAsync<InvalidDataException>(() => database.PrepareImportAsync(source, ""));
        Assert.That(Digest(LivePath), Is.EqualTo(liveBefore));
        Assert.That(Digest(source), Is.EqualTo(sourceBefore));
    }

    [TestCase(" \t\r\n")]
    [TestCase("password\0suffix")]
    [TestCase(null)]
    public void InvalidExportPasswordsPreserveExistingDestination(string? password)
    {
        var database = Initialize();
        SeedRecords(database, "keep live data");
        var destination = Path.Combine(_root, "existing-backup.db");
        File.WriteAllBytes(destination, "existing destination must survive"u8.ToArray());
        var before = Digest(destination);
        Assert.ThrowsAsync<ArgumentException>(() => database.ExportAsync(destination, password!));
        Assert.That(Digest(destination), Is.EqualTo(before));
    }

    [Test]
    public async Task InvalidImportsLeaveCurrentDataAndSourceUntouched()
    {
        var database = Initialize();
        SeedRecords(database, new string('d', 14000));
        var backup = Path.Combine(_root, "valid.db");
        await database.ExportAsync(backup, "correct password");
        var before = Digest(LivePath);
        var backupBefore = Digest(backup);
        Assert.ThrowsAsync<InvalidDataException>(() => database.PrepareImportAsync(backup, "wrong password"));
        Assert.That(Digest(backup), Is.EqualTo(backupBefore));

        var tampered = Path.Combine(_root, "tampered.db");
        File.Copy(backup, tampered);
        CorruptDataPage(tampered);
        Assert.ThrowsAsync<InvalidDataException>(() => database.PrepareImportAsync(tampered, "correct password"));

        var plaintext = Path.Combine(_root, "plaintext.db");
        using (var connection = Open(plaintext, "", create: true))
            Execute(connection, "CREATE TABLE records(id INTEGER PRIMARY KEY AUTOINCREMENT,data TEXT NOT NULL)");
        Assert.ThrowsAsync<InvalidDataException>(() => database.PrepareImportAsync(plaintext, "any password"));

        var truncated = Path.Combine(_root, "truncated.db");
        File.WriteAllBytes(truncated, File.ReadAllBytes(backup)[..200]);
        Assert.ThrowsAsync<InvalidDataException>(() => database.PrepareImportAsync(truncated, "correct password"));
        Assert.That(Digest(LivePath), Is.EqualTo(before));
        Assert.That(File.Exists(plaintext), Is.True);
    }

    [Test]
    public async Task ImportSchemaRequiresLiveColumnsAndPreservesCompatibleExtras()
    {
        var database = Initialize();
        SeedRecords(database, "current data");
        var incompatible = Path.Combine(_root, "incompatible.db");
        using (var connection = Open(incompatible, "source password", create: true))
            Execute(connection, "CREATE TABLE records(id INTEGER PRIMARY KEY AUTOINCREMENT,parent_id INTEGER,data TEXT NOT NULL)");
        Assert.ThrowsAsync<InvalidDataException>(() => database.PrepareImportAsync(incompatible, "source password"));
        using (var connection = Open(incompatible, "source password", write: true))
        {
            Execute(connection, """
                DROP TABLE records;
                CREATE TABLE records(id INTEGER PRIMARY KEY AUTOINCREMENT,parent_id INTEGER NOT NULL,data TEXT NOT NULL,extra TEXT);
                INSERT INTO records(parent_id,data,extra) VALUES(6,'compatible record','extra column retained');
                CREATE TABLE additional(value TEXT);
                INSERT INTO additional VALUES('extra table retained');
                """);
        }
        var staged = await database.PrepareImportAsync(incompatible, "source password");
        try
        {
            Assert.That(ReadScalar(staged, LocalPassword(database), "SELECT extra FROM records"), Is.EqualTo("extra column retained"));
            Assert.That(ReadScalar(staged, LocalPassword(database), "SELECT value FROM additional"), Is.EqualTo("extra table retained"));
        }
        finally { database.DiscardPreparedImport(staged); }
        using var live = database.Connections.Open();
        Assert.That(Scalar(live, "SELECT data FROM records"), Is.EqualTo("current data"));
    }

    [TestCase("")]
    [TestCase("password")]
    public async Task FailedExportReplacementPreservesExistingDestination(string password)
    {
        var database = Initialize();
        SeedRecords(database, "export data");
        var destination = Path.Combine(_root, "existing-backup.db");
        File.WriteAllBytes(destination, "existing destination must survive"u8.ToArray());
        var before = Digest(destination);
        using (var lockedDestination = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.ThrowsAsync<IOException>(() => database.ExportAsync(destination, password));
        Assert.That(Digest(destination), Is.EqualTo(before));
        await database.ExportAsync(destination, password);
        Assert.That(ReadScalar(destination, password, "SELECT data FROM records"), Is.EqualTo("export data"));
    }

    [Test]
    public async Task PreparedImportReservesTransferUntilDiscardAndRejectsUnownedPaths()
    {
        var database = Initialize();
        SeedRecords(database, "current");
        var backup = Path.Combine(_root, "backup.db");
        await database.ExportAsync(backup, "password");
        var staged = await database.PrepareImportAsync(backup, "password");
        try
        {
            Assert.Throws<InvalidOperationException>(() => database.InstallPreparedImport(backup));
            using var cancellation = new CancellationTokenSource();
            var export = database.ExportAsync(Path.Combine(_root, "blocked.db"), "password", cancellation.Token);
            Assert.That(export.IsCompleted, Is.False);
            cancellation.Cancel();
            Assert.That(async () => await export, Throws.InstanceOf<OperationCanceledException>());
        }
        finally { database.DiscardPreparedImport(staged); }
        Assert.That(File.Exists(staged), Is.False);
        await database.ExportAsync(Path.Combine(_root, "after-discard.db"), "password");
        Assert.That(ReadScalar(backup, "password", "SELECT data FROM records"), Is.EqualTo("current"));
    }

    [Test]
    public async Task ProtectedDatabaseAndProtectorPathsCannotBeSelected()
    {
        var database = Initialize();
        SeedRecords(database, "protected");
        var before = Digest(LivePath);
        Assert.ThrowsAsync<IOException>(() => database.ExportAsync(LivePath.ToUpperInvariant(), "password"));
        Assert.ThrowsAsync<IOException>(() => database.ExportAsync(KeyPath, "password"));
        Assert.ThrowsAsync<IOException>(() => database.PrepareImportAsync(LivePath + "-wal", "password"));
        Assert.ThrowsAsync<IOException>(() => database.ExportAsync(LegacyPath, "password"));
        Assert.ThrowsAsync<IOException>(() => database.ExportAsync(LegacyPath + ".", "password"));
        Assert.ThrowsAsync<IOException>(() => database.ExportAsync(LivePath + ":backup", "password"));
        Assert.ThrowsAsync<IOException>(() => database.ExportAsync(@"\\?\" + LivePath, "password"));
        var backup = Path.Combine(_root, "backup.db");
        await database.ExportAsync(backup, "password");
        var staged = await database.PrepareImportAsync(backup, "password");
        database.DiscardPreparedImport(staged);
        Assert.ThrowsAsync<IOException>(() => database.ExportAsync(staged, "password"));
        Assert.That(Digest(LivePath), Is.EqualTo(before));
    }

    [Test]
    public async Task WalImportFinalizesSidecarsAndPreventsStaleFactoryUse()
    {
        var database = Initialize();
        SeedRecords(database, "backup state");
        var factory = database.Connections;
        var backup = Path.Combine(_root, "wal-backup.db");
        await database.ExportAsync(backup, "password");
        using (var connection = factory.Open())
        {
            Assert.That(Scalar(connection, "PRAGMA journal_mode=WAL"), Is.EqualTo("wal"));
            Execute(connection, "PRAGMA wal_autocheckpoint=0; UPDATE records SET data='live WAL state'");
        }
        Assert.That(File.Exists(LivePath + "-wal"), Is.True);
        var staged = await database.PrepareImportAsync(backup, "password");
        database.InstallPreparedImport(staged);
        Assert.Throws<ObjectDisposedException>(() => factory.Open());
        Assert.That(File.Exists(LivePath + "-wal"), Is.False);
        Assert.That(File.Exists(LivePath + "-shm"), Is.False);
        using var connectionAfter = InitializeWithoutPrompt().Connections.Open();
        Assert.That(Scalar(connectionAfter, "SELECT data FROM records"), Is.EqualTo("backup state"));
    }

    [Test]
    public async Task ReplacementFailurePreservesOriginalAndReleasesReservation()
    {
        var database = Initialize();
        SeedRecords(database, "backup state");
        var backup = Path.Combine(_root, "backup.db");
        await database.ExportAsync(backup, "password");
        using (var connection = database.Connections.Open())
            Execute(connection, "UPDATE records SET data='original must survive'");
        var staged = await database.PrepareImportAsync(backup, "password");
        var password = LocalPassword(database);
        SqliteConnection.ClearAllPools();
        using (var replacementBlocker = new FileStream(LivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            Assert.Throws<IOException>(() => database.InstallPreparedImport(staged));
        Assert.That(ReadScalar(LivePath, password, "SELECT data FROM records"), Is.EqualTo("original must survive"));
        Assert.That(File.Exists(staged), Is.False);
        database.Dispose();
    }

    [TestCase("")]
    [TestCase("snapshot password")]
    public async Task ExportReadSnapshotCannotMixCommittedWriterStates(string password)
    {
        var database = Initialize();
        using (var connection = database.Connections.Open())
        {
            Execute(connection, "PRAGMA journal_mode=WAL");
            Execute(connection, "CREATE TABLE first_state(value INTEGER); CREATE TABLE second_state(value INTEGER); INSERT INTO first_state VALUES(0); INSERT INTO second_state VALUES(0)");
        }
        using var stop = new CancellationTokenSource();
        var writing = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var writer = Task.Run(() =>
        {
            try
            {
                using var connection = database.Connections.Open();
                while (!stop.IsCancellationRequested)
                {
                    using var transaction = connection.BeginTransaction();
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = "UPDATE first_state SET value=value+1; UPDATE second_state SET value=value+1";
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    writing.TrySetResult();
                }
            }
            catch (Exception error)
            {
                writing.TrySetException(error);
                throw;
            }
        });
        var backup = Path.Combine(_root, "concurrent.db");
        try
        {
            await writing.Task;
            await database.ExportAsync(backup, password);
        }
        finally
        {
            stop.Cancel();
            await writer;
        }
        using var snapshot = Open(backup, password);
        var first = Scalar(snapshot, "SELECT value FROM first_state");
        var second = Scalar(snapshot, "SELECT value FROM second_state");
        Assert.That(first, Is.EqualTo(second));
        Assert.That((long)first!, Is.GreaterThan(0));
    }

    private Database NewDatabase()
    {
        var database = new Database(_root);
        _databases.Add(database);
        return database;
    }

    private Database Initialize()
    {
        var database = NewDatabase();
        database.Initialize(_ => true, _ => throw new AssertionException("Unexpected recovery request."));
        return database;
    }

    private Database InitializeWithoutPrompt()
    {
        var database = NewDatabase();
        database.Initialize(_ => throw new AssertionException("Unexpected provisioning request."), _ => throw new AssertionException("DPAPI should automatically unlock."));
        return database;
    }

    private static string LocalPassword(Database database)
    {
        if (!RecoveryKeyCodec.TryParse(database.GetRecoveryKey(), out var mek))
            throw new AssertionException("Fixture recovery key is invalid.");
        try { return Convert.ToHexString(mek); }
        finally { CryptographicOperations.ZeroMemory(mek); }
    }

    private static void SeedRecords(Database database, string value)
    {
        using var connection = database.Connections.Open();
        Execute(connection, "CREATE TABLE records(id INTEGER PRIMARY KEY AUTOINCREMENT,parent_id INTEGER NOT NULL,data TEXT NOT NULL)");
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO records(parent_id,data) VALUES(0,$data)";
        command.Parameters.AddWithValue("$data", value);
        command.ExecuteNonQuery();
    }

    private static SqliteConnection Open(string path, string password, bool create = false, bool write = false)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = create ? SqliteOpenMode.ReadWriteCreate : write ? SqliteOpenMode.ReadWrite : SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString());
        try
        {
            connection.Open();
            if (password.Length != 0)
            {
                using var quote = connection.CreateCommand();
                quote.CommandText = "SELECT quote($password)";
                quote.Parameters.AddWithValue("$password", password);
                var literal = (string)quote.ExecuteScalar()!;
                Execute(connection, $"PRAGMA key={literal}");
            }
            Execute(connection, "PRAGMA temp_store=MEMORY");
            return connection;
        }
        catch { connection.Dispose(); throw; }
    }

    private static object? ReadScalar(string path, string password, string sql)
    {
        using var connection = Open(path, password);
        return Scalar(connection, sql);
    }

    private static object? Scalar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static byte[] Digest(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        return SHA256.HashData(stream);
    }

    private static void CorruptDataPage(string path)
    {
        using var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        file.Position = 4096 + 200;
        var value = file.ReadByte();
        if (value < 0)
            throw new AssertionException("The fixture must contain more than one database page.");
        file.Position--;
        file.WriteByte((byte)(value ^ 0x40));
        file.Flush(flushToDisk: true);
    }
}
