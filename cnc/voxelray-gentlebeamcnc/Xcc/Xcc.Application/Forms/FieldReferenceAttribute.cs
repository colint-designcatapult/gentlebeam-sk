using System;

namespace Xcc.Application.Forms
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldReferenceAttribute : Attribute
    {
        public string Name { get; }
        public FieldReferenceAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
        }
    }
}
