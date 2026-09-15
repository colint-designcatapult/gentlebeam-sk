extern alias SqliteServer;

using Grpc.Core;
using SqlCipherDatabase = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqlCipherDatabase;
using Moq;
using CreateBatchTreatmentFieldsRequest = SqliteServer::Com.Empyreanmed.Heracles.TreatmentFields.V1.CreateBatchTreatmentFieldsRequest;
using CreateTreatmentFieldRequest = SqliteServer::Com.Empyreanmed.Heracles.TreatmentFields.V1.CreateTreatmentFieldRequest;
using FIELDNAME = SqliteServer::Com.Empyreanmed.Heracles.Enums.V1.FIELDNAME;
using TreatmentField = SqliteServer::Com.Empyreanmed.Heracles.TreatmentFields.V1.TreatmentField;
using TreatmentFieldServiceImpl = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services.TreatmentFieldServiceImpl;
using UpdateTreatmentFieldRequest = SqliteServer::Com.Empyreanmed.Heracles.TreatmentFields.V1.UpdateTreatmentFieldRequest;
using SqliteProtoRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.TreatmentFields.V1.TreatmentField>;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
public sealed class SqliteTreatmentFieldPersistenceTests
{
    private string _directory = null!;
    private SqlCipherDatabase _database = null!;
    private ServerCallContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"treatment-fields-{Guid.NewGuid():N}");
        _database = new SqlCipherDatabase(_directory);
        _database.Initialize(_ => true, _ => null);
        _context = new Mock<ServerCallContext>().Object;
    }

    [TearDown]
    public void TearDown()
    {
        _database.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public async Task CreatePlusC_PersistsOneField()
    {
        var repository = CreateRepository();
        var service = new TreatmentFieldServiceImpl(repository);

        var response = await service.CreateTreatmentField(
            new CreateTreatmentFieldRequest { TreatmentField = CreateField(42) },
            _context);
        var stored = await repository.ReadByParentIdAsync(42);

        Assert.Multiple(() =>
        {
            Assert.That(response.TreatmentField.Id, Is.GreaterThan(0));
            Assert.That(response.TreatmentField.FieldName, Is.EqualTo(FIELDNAME.PlusC));
            Assert.That(stored, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void CreateWrongName_IsInvalidArgument()
    {
        var service = new TreatmentFieldServiceImpl(CreateRepository());
        var request = new CreateTreatmentFieldRequest
        {
            TreatmentField = CreateField(42, FIELDNAME.Plus1L1)
        };

        AssertStatus(
            StatusCode.InvalidArgument,
            () => service.CreateTreatmentField(request, _context));
    }

    [Test]
    public async Task CreateSecondFieldForPlan_IsAlreadyExists()
    {
        var service = new TreatmentFieldServiceImpl(CreateRepository());
        await service.CreateTreatmentField(
            new CreateTreatmentFieldRequest { TreatmentField = CreateField(42) },
            _context);

        AssertStatus(
            StatusCode.AlreadyExists,
            () => service.CreateTreatmentField(
                new CreateTreatmentFieldRequest { TreatmentField = CreateField(42) },
                _context));
    }

    [TestCase(0)]
    [TestCase(2)]
    public void BatchWithInvalidCount_IsInvalidArgument(int count)
    {
        var service = new TreatmentFieldServiceImpl(CreateRepository());
        var request = new CreateBatchTreatmentFieldsRequest();
        for (var index = 0; index < count; index++)
        {
            request.TreatmentFields.Add(CreateField(42));
        }

        AssertStatus(
            StatusCode.InvalidArgument,
            () => service.CreateBatchTreatmentFields(request, _context));
    }

    [Test]
    public async Task OneItemBatch_PersistsPlusC()
    {
        var repository = CreateRepository();
        var service = new TreatmentFieldServiceImpl(repository);
        var request = new CreateBatchTreatmentFieldsRequest();
        request.TreatmentFields.Add(CreateField(42));

        var response = await service.CreateBatchTreatmentFields(request, _context);
        var stored = await repository.ReadByParentIdAsync(42);

        Assert.Multiple(() =>
        {
            Assert.That(response.TreatmentFields, Has.Count.EqualTo(1));
            Assert.That(response.TreatmentFields[0].FieldName, Is.EqualTo(FIELDNAME.PlusC));
            Assert.That(stored, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task UpdateAwayFromPlusC_IsInvalidArgument()
    {
        var service = new TreatmentFieldServiceImpl(CreateRepository());
        var created = (await service.CreateTreatmentField(
            new CreateTreatmentFieldRequest { TreatmentField = CreateField(42) },
            _context)).TreatmentField;
        var changed = created.Clone();
        changed.FieldName = FIELDNAME.Plus1L1;

        AssertStatus(
            StatusCode.InvalidArgument,
            () => service.UpdateTreatmentField(
                new UpdateTreatmentFieldRequest { TreatmentField = changed },
                _context));
    }

    [Test]
    public async Task UpdateToAnotherPlan_IsInvalidArgument()
    {
        var service = new TreatmentFieldServiceImpl(CreateRepository());
        var created = (await service.CreateTreatmentField(
            new CreateTreatmentFieldRequest { TreatmentField = CreateField(42) },
            _context)).TreatmentField;
        var moved = created.Clone();
        moved.PlanId = 43;

        AssertStatus(
            StatusCode.InvalidArgument,
            () => service.UpdateTreatmentField(
                new UpdateTreatmentFieldRequest { TreatmentField = moved },
                _context));
    }

    private SqliteProtoRepository CreateRepository()
    {
        return new SqliteProtoRepository(
            _database.Connections,
            "treatment_fields",
            hasParentId: true,
            parentIdJsonField: "planId");
    }

    private static TreatmentField CreateField(
        long planId,
        FIELDNAME name = FIELDNAME.PlusC)
    {
        return new TreatmentField
        {
            PlanId = planId,
            FieldName = name,
            DwellTime = 3
        };
    }

    private static void AssertStatus(StatusCode expected, Func<Task> action)
    {
        var exception = Assert.ThrowsAsync<RpcException>(async () => await action());
        Assert.That(exception!.StatusCode, Is.EqualTo(expected));
    }
}
