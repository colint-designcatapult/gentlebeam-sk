using System;
using System.Windows.Markup;

namespace Xcc.Application.Common
{
    [MarkupExtensionReturnType(typeof(int))]
    public class IntExtension : MarkupExtension
    {
        public IntExtension(int value)
        {
            Value = value;
        }

        [ConstructorArgument("value")]
        public int Value { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}
