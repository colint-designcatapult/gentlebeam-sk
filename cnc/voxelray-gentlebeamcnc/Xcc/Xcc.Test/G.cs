using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Xcc.Test
{
    public static class G
    {
        // Same as in App.xaml.cs, OnStartup()
        public static readonly CultureInfo Culture = new("en-US");

        public static void SetupCulture()
        {
            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = Culture;
            CultureInfo.DefaultThreadCurrentCulture = Culture;
            CultureInfo.DefaultThreadCurrentUICulture = Culture;
        }

        public static readonly double Precision = 0.000001;
        
        public static void AssertAllPublicPropertiesEqualTo<T>(this T actual, T expected)
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            
            Assert.Multiple(() =>
            {
                foreach (var prop in props)
                {
                    var expectedValue = prop.GetValue(expected);
                    var actualValue = prop.GetValue(actual);
                    var type = prop.PropertyType;

                    if (type == typeof(double) || type == typeof(float))
                    {
                        Assert.That(actualValue, Is.EqualTo(expectedValue).Within(G.Precision), $"Property {prop.Name} not equal");
                    }
                    else
                    {
                        Assert.That(actualValue, Is.EqualTo(expectedValue), $"Property {prop.Name} not equal");
                    }
                }
            });
        }
        
        public static void SetPropertyValue<T>(this T obj, string propertyName, object value)
        {
            var prop = typeof(T).GetProperty(propertyName);
            Assert.That(prop, Is.Not.Null, $"Property {propertyName} not found");
            
            prop.SetValue(obj, value);
        }
        
        public static bool SetNotifiedPropertyValue<T>(this T obj, string propertyName, object value)
            where T : INotifyPropertyChanged
        {
            var prop = typeof(T).GetProperty(propertyName);
            Assert.That(prop, Is.Not.Null, $"Property {propertyName} not found");
            
            bool isChanged = false;
            obj.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == propertyName)
                    isChanged = true;
            };

            prop.SetValue(obj, value);

            return isChanged;
        }
    }
}