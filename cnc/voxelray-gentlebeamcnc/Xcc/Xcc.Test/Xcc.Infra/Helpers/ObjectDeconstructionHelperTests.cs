using System.Reflection;
using Xcc.Core.Helpers;

namespace Xcc.Test.Xcc.Infra.Helpers
{
    internal class ObjectDeconstructionHelperTests
    {
        internal class BaseTestClass
        {
            public long BasePublicProperty { get; set; } = 1;
            private long BasePrivateProperty { get; set; } = 2;
        }

        internal class DerivedTestClass : BaseTestClass
        {
            public long DerivedPublicProperty { get; set; } = 3;
            private long DerivedPrivateProperty { get; set; } = 4;
        }

        [Test]
        public void DefaultConstructorTest()
        {
            var helper = new ObjectDeconstructionHelper<BaseTestClass>();
            Assert.That(helper.PropertyList, Is.Not.Empty);
            Assert.That(helper.Properties, Is.Not.Empty);
            Assert.That(helper.Properties.Count, Is.EqualTo(1));
            Assert.That(helper.PropertyList.Count, Is.EqualTo(1));
            Assert.That(helper.PropertyList, Contains.Item(nameof(BaseTestClass.BasePublicProperty)));
        }

        [Test]
        public void CustomFlagsConstructorTest()
        {
            // Now it should reveal both public and private properties
            var helper = new ObjectDeconstructionHelper<BaseTestClass>(
                ObjectDeconstructionHelper<BaseTestClass>.DefaultBindingFlags | BindingFlags.NonPublic);
            Assert.That(helper.PropertyList, Is.Not.Empty);
            Assert.That(helper.PropertyList.Count, Is.EqualTo(2));
        }

        [Test]
        public void CustomConstructorPositiveTest()
        {
            string privatePropertyName = "BasePrivateProperty";
            // It should reveal both public and private properties, but we select only private one:
            var helper = new ObjectDeconstructionHelper<BaseTestClass>(
                new List<string> { privatePropertyName },
                ObjectDeconstructionHelper<BaseTestClass>.DefaultBindingFlags | BindingFlags.NonPublic);
            Assert.That(helper.PropertyList, Is.Not.Empty);
            Assert.That(helper.PropertyList.Count, Is.EqualTo(1));
            Assert.That(helper.PropertyList, Contains.Item(privatePropertyName));
        }

        [Test]
        public void CustomConstructorNegativeTest()
        {
            string privatePropertyName = "BasePrivateProperty";
            // Request private property without passing proper flags to access it:
            Assert.Throws<ArgumentException>(
                () => new ObjectDeconstructionHelper<BaseTestClass>(new List<string> { privatePropertyName }));
        }

        [Test]
        public void CustomConstructor_NullReferenceTest()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectDeconstructionHelper<BaseTestClass>(null));
        }

        [Test]
        public void GetPropertyValues_BaseTestClassTest()
        {
            var baseHelper = new ObjectDeconstructionHelper<BaseTestClass>();
            var baseObj = new BaseTestClass();
            ICollection<object?> values = null!;
            Assert.DoesNotThrow(() => values = baseHelper.GetPropertyValues(baseObj));
            Assert.That(values, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(values.Count, Is.EqualTo(1));
                Assert.That(Convert.ToInt64(values.First()), Is.EqualTo(baseObj.BasePublicProperty));
            });
        }

        [Test]
        public void GetPropertyValues_DerivedTestClassTest()
        {
#if !(NET7_0_OR_GREATER)
    // This approach relies on the GetPropertyValues returning the values in a particular order,
    // see https://learn.microsoft.com/en-us/dotnet/api/system.type.getproperties?view=net-8.0
#error Can't ensure the right property order in GetProperties() reflection method
#endif

            var derivedTypeHelper = new ObjectDeconstructionHelper<DerivedTestClass>();
            var derivedObj = new DerivedTestClass();
            ICollection<object?> values = null!;
            Assert.DoesNotThrow(() => values = derivedTypeHelper.GetPropertyValues(derivedObj));
            Assert.That(values, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(values.Count, Is.EqualTo(2));
                Assert.That(Convert.ToInt64(values.First()), Is.EqualTo(derivedObj.DerivedPublicProperty));
                Assert.That(Convert.ToInt64(values.Last()), Is.EqualTo(derivedObj.BasePublicProperty));
            });
        }
    }
}
