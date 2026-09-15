extern alias SqliteServer;
using ConnectionFactory = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqlCipherConnectionFactory;

namespace Heracles.Indoor.Test.Infra;

/// <summary>File-backed SQLite for persistence tests; the fixture owns directory cleanup.</summary>
internal sealed class TestSqliteDatabase : IDisposable
{
    public TestSqliteDatabase(string directory)
    {
        Directory.CreateDirectory(directory);
        Connections = ConnectionFactory.CreatePlaintextForTests(Path.Combine(directory, "test.db"));
    }

    public ConnectionFactory Connections { get; }

    public void Dispose() => Connections.ClosePool();
}
