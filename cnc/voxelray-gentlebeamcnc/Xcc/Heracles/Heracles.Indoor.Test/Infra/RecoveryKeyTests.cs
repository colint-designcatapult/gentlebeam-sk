extern alias SqliteServer;

using RecoveryKeyCodec = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.RecoveryKeyCodec;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
public sealed class RecoveryKeyTests
{
    private const string Vector = "000000-000011-000110-002805-002816-045045-360448-720885";

    [Test]
    public void FixedVectorPreservesBigEndianGroupsAndBoundaries()
    {
        var bytes = Convert.FromHexString("00000001000A00FF01000FFF8000FFFF");
        Assert.That(RecoveryKeyCodec.Format(bytes), Is.EqualTo(Vector));
        Assert.That(RecoveryKeyCodec.TryParse(" \r\n" + Vector + "\t ", out var decoded), Is.True);
        Assert.That(decoded, Is.EqualTo(bytes));
    }

    [TestCase("000001-000011-000110-002805-002816-045045-360448-720885")]
    [TestCase("720896-000011-000110-002805-002816-045045-360448-720885")]
    [TestCase("00000-000011-000110-002805-002816-045045-360448-720885")]
    [TestCase("000000_000011-000110-002805-002816-045045-360448-720885")]
    [TestCase("００００００-000011-000110-002805-002816-045045-360448-720885")]
    [TestCase("000000-000011-000110-002805-002816-045045-360448")]
    [TestCase("000000-000011-000110-002805-002816-045045-360448-720885-000000")]
    [TestCase("000000-000011-000110-002805-002816-045045-360448-72088\0")]
    [TestCase(null)]
    public void MalformedRecoveryKeyReturnsNoPartialKey(string? input)
    {
        Assert.That(RecoveryKeyCodec.TryParse(input!, out var decoded), Is.False);
        Assert.That(decoded, Is.Empty);
    }

    [TestCase(0)]
    [TestCase(15)]
    [TestCase(17)]
    public void FormatterRejectsWrongKeySize(int length)
    {
        Assert.Throws<ArgumentException>(() => RecoveryKeyCodec.Format(new byte[length]));
    }
}
