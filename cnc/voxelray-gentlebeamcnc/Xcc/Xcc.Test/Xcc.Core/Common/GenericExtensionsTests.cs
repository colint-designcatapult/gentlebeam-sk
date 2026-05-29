using Xcc.Core.Common;

namespace Xcc.Test.Xcc.Core.Common
{
    public class Source
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Optional { get; set; }

        public string _descWriteOnly;
        public string Desc
        {
            set { _descWriteOnly = value; }
        }
    }

    public class Destination
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Optional { get; set; }

        public string Desc => "readonly";
    }

    public class GenericExtensionsTests
    {
        [Test]
        public void CopyProperties_WithNullSource()
        {
            Source src = null;
            var dst = new Destination { Name = "Original", Age = 42, Optional = "OriginalValue" };

            src.CopyProperties(dst);

            Assert.That(dst.Name, Is.EqualTo("Original"));
            Assert.That(dst.Age, Is.EqualTo(42));
            Assert.That(dst.Optional, Is.EqualTo("OriginalValue"));
            Assert.That(dst.Desc, Is.EqualTo("readonly"));
        }
        
        [Test]
        public void CopyProperties_WithNullDestination()
        {
            var src = new Source { Name = "XCC", Age = 30, Optional = "Value", Desc = "writeonly" };
            Destination dst = null;

            src.CopyProperties(dst);

            Assert.That(dst, Is.Null);
        }
        
        [Test]
        public void CopyProperties_WithNullSourceAndDestination()
        {
            Source src = null;
            Destination dst = null;

            src.CopyProperties(dst);

            Assert.That(dst, Is.Null);
        }
        
        [Test]
        public void CopyProperties_All()
        {
            var src = new Source { Name = "XCC", Age = 30, Optional = "Value" };
            var dst = new Destination();

            src.CopyProperties(dst);

            Assert.That(dst.Name, Is.EqualTo(src.Name));
            Assert.That(dst.Age, Is.EqualTo(src.Age));
            Assert.That(dst.Optional, Is.EqualTo(src.Optional));
            Assert.That(dst.Desc, Is.EqualTo("readonly"));
        }

        [Test]
        public void CopyProperties_WithIgnore()
        {
            var src = new Source { Name = "XCC", Age = 30, Optional = "Value" };
            var dst = new Destination { Name = "Original", Age = 0, Optional = null };
            
            src.CopyProperties(dst, ignoreList: new List<string> { "Name" });
            
            Assert.That(dst.Name, Is.EqualTo("Original"));
            Assert.That(dst.Age, Is.EqualTo(src.Age));
            Assert.That(dst.Optional, Is.EqualTo(src.Optional));
            Assert.That(dst.Desc, Is.EqualTo("readonly"));
        }

        [Test]
        public void CopyProperties_OverwriteWithNull_Disabled()
        {
            var src = new Source { Name = null, Age = 25, Optional = null };
            var dst = new Destination { Name = "Default", Age = 10, Optional = "DefaultOptional" };

            src.CopyProperties(dst, overwriteWithNull: false);

            Assert.That(dst.Name, Is.EqualTo("Default"));
            Assert.That(dst.Age, Is.EqualTo(src.Age));
            Assert.That(dst.Optional, Is.EqualTo("DefaultOptional"));
            Assert.That(dst.Desc, Is.EqualTo("readonly"));
        }

        [Test]
        public void CopyProperties_OverwriteWithNull_Enabled()
        {
            var src = new Source { Name = null, Age = 25, Optional = null };
            var dst = new Destination { Name = "Default", Age = 10, Optional = "DefaultOptional" };

            src.CopyProperties(dst, overwriteWithNull: true);

            Assert.That(dst.Name, Is.Null);
            Assert.That(dst.Age, Is.EqualTo(src.Age));
            Assert.That(dst.Optional, Is.Null);
            Assert.That(dst.Desc, Is.EqualTo("readonly"));
        }

        [Test]
        public void GetProperties()
        {
            var source = new Source();
            
            var properties = GenericExtensions.GetProperties(source);
            
            Assert.That(properties, Is.Not.Null);
            
            var listProperties = properties.ToList();
            Assert.That(listProperties, Does.Contain("Name"));
            Assert.That(listProperties, Does.Contain("Age"));
            Assert.That(listProperties, Does.Contain("Optional"));
            Assert.That(listProperties, Does.Contain("Desc"));
        }
        
        [Test]
        public void GetProperties_WithNull()
        {
            Source source = null;
            
            var result = GenericExtensions.GetProperties(source);
        
            Assert.That(result, Is.Null);
        }
    }
}