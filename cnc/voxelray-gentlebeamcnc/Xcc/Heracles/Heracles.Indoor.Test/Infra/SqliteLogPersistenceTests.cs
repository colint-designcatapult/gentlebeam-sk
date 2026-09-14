extern alias SqliteServer;

using Google.Protobuf.WellKnownTypes;
using Microsoft.Data.Sqlite;
using Log = SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log;
using ListLogsRequest = SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.ListLogsRequest;
using LogService = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services.LogServiceImpl;
using LogRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log>;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
public sealed class SqliteLogPersistenceTests
{
    private string _dbPath = null!;

    [SetUp]
    public void SetUp()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"heracles-logs-{Guid.NewGuid():N}.db");
    }

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }

    [Test]
    public async Task ListLogs_UsesStableNewestFirstPaging()
    {
        var repository = new LogRepository(_dbPath, "logs");
        for (var i = 1; i <= 3; i++)
        {
            await repository.CreateAsync(new Log
            {
                Message = $"log-{i}",
                Timestamp = Timestamp.FromDateTime(DateTime.UnixEpoch.AddMinutes(i))
            });
        }

        var service = new LogService(repository);
        var first = await service.ListLogs(new ListLogsRequest { Get = 2, Skip = 0 }, null!);
        var second = await service.ListLogs(new ListLogsRequest { Get = 2, Skip = 2 }, null!);

        Assert.That(first.Logs.Select(x => x.Message), Is.EqualTo(new[] { "log-3", "log-2" }));
        Assert.That(first.NextPageToken, Is.EqualTo("2"));
        Assert.That(second.Logs.Select(x => x.Message), Is.EqualTo(new[] { "log-1" }));
        Assert.That(second.NextPageToken, Is.Empty);
    }
}
