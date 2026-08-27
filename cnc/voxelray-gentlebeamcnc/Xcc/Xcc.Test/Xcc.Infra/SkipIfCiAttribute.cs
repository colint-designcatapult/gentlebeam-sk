using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnitTest = NUnit.Framework.Internal.Test;

namespace Xcc.Test.Xcc.Infra
{
    /// <summary>
    /// Attribute to conditionally skip tests when running in CI environment.
    /// Tests marked with this attribute will be skipped when the CI_ENVIRONMENT variable is set.
    /// This is useful for infrastructure/integration tests that require hardware or external services.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class SkipIfCiAttribute : Attribute, IApplyToTest
    {
        private readonly string _reason;

        public SkipIfCiAttribute(string reason = "Infrastructure test - skipped in CI environment")
        {
            _reason = reason;
        }

        public void ApplyToTest(NUnitTest test)
        {
            if (Environment.GetEnvironmentVariable("CI_ENVIRONMENT") == "true")
            {
                test.RunState = RunState.Ignored;
                test.Properties.Set(PropertyNames.SkipReason, _reason);
            }
        }
    }
}
