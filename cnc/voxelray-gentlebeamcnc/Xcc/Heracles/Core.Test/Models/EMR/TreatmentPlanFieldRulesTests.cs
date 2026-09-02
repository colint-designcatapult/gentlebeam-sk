using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

namespace Heracles.Core.Test.Models.EMR;

[TestFixture]
public sealed class TreatmentPlanFieldRulesTests
{
    [Test]
    public void OnePlusCField_IsValid()
    {
        var fields = new ITreatmentField[] { new TestTreatmentField(TreatmentFieldName.PlusC) };

        Assert.Multiple(() =>
        {
            Assert.That(TreatmentPlanFieldRules.IsValid(fields), Is.True);
            Assert.DoesNotThrow(() => TreatmentPlanFieldRules.EnsureValid(fields));
        });
    }

    [TestCaseSource(nameof(InvalidCollections))]
    public void InvalidShape_IsRejected(IEnumerable<ITreatmentField>? fields)
    {
        Assert.That(TreatmentPlanFieldRules.IsValid(fields), Is.False);

        var exception = Assert.Throws<InvalidOperationException>(
            () => TreatmentPlanFieldRules.EnsureValid(fields));
        Assert.That(exception!.Message, Is.EqualTo(TreatmentPlanFieldRules.InvalidShapeMessage));
    }

    private static IEnumerable<TestCaseData> InvalidCollections()
    {
        yield return new TestCaseData(new object?[] { null });
        yield return new TestCaseData(new List<ITreatmentField>());
        yield return new TestCaseData(new List<ITreatmentField>
        {
            new TestTreatmentField(TreatmentFieldName.Plus1L1)
        });
        yield return new TestCaseData(new List<ITreatmentField>
        {
            new TestTreatmentField(TreatmentFieldName.PlusC),
            new TestTreatmentField(TreatmentFieldName.Plus1L1)
        });
    }

    private sealed class TestTreatmentField(TreatmentFieldName name) : ITreatmentField
    {
        public long Id { get; set; }
        public DateTime CreationDate { get; set; }
        public IPlan Plan { get; set; } = null!;
        public long PlanId { get; set; }
        public double CalculatedDose { get; set; }
        public double Current { get; set; }
        public bool IsActive { get; set; }
        public int DisplayValue { get; set; }
        public double DwellTime { get; set; }
        public Energy Energy { get; set; }
        public TreatmentFieldName Name { get; set; } = name;
    }
}
