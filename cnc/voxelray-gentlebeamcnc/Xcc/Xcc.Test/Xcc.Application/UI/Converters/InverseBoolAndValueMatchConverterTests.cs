using Xcc.Application.UI.Converters;

namespace Xcc.Test.Xcc.Application.UI.Converters;

internal class InverseBoolAndValueMatchConverterTests
{
    private readonly InverseBoolAndValueMatchConverter _converter = new();

    [TestCase(false, "Treatment", "Treatment", true)]
    [TestCase(true, "Treatment", "Treatment", true)]
    [TestCase(true, "Quality", "Treatment", false)]
    public void Convert_PreservesExistingTabLockBehavior(
        bool planLoaded,
        string selectedTab,
        string targetTab,
        bool expected)
    {
        var result = _converter.Convert(
            [planLoaded, selectedTab],
            typeof(bool),
            targetTab,
            null!);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public void Convert_RequiresOptionalOperationModeGate(bool operationAllowed, bool expected)
    {
        var result = _converter.Convert(
            [false, "Treatment", operationAllowed],
            typeof(bool),
            "Treatment",
            null!);

        Assert.That(result, Is.EqualTo(expected));
    }
}
