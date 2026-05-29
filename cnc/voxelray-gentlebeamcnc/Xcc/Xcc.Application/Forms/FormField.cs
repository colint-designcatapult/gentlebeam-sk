using Prism.Mvvm;
using System;
using System.Globalization;

namespace Xcc.Application.Forms
{
    public class FormFieldBase<FieldType>(
        FieldType? value,
        bool isValid = true,
        bool isModified = false) : BindableBase, IFormField
        where FieldType : class?
    {
        public event EventHandler<object?>? ValueChanged;

        private FieldType? _value = value;
        public FieldType? Value
        {
            get => _value;
            private set => SetProperty(ref _value, value);
        }

        private bool _isValid = isValid;
        public bool IsValid
        {
            get => _isValid;
            private set => SetProperty(ref _isValid, value);
        }

        private bool _isModified = isModified;
        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value);
        }

        public void SetValue(FieldType? value, bool isValid)
        {
            if (value == _value)
                return;
            if (value is not null && value.Equals(_value))
                return;
            // Invalidate before assignment:
            if (!isValid)
            {
                IsValid = false;
            }
            Value = value;
            // Validate after assignment:
            if (isValid)
            {
                IsValid = true;
            }
            IsModified = true;
            ValueChanged?.Invoke(this, value);
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }

        public void AcceptChanges()
        {
            IsModified = false;
        }
    }

    public class FormField(
        object? value,
        bool isValid = true,
        bool isModified = false) 
        : FormFieldBase<object>(value, isValid, isModified)
    {        
    }

    public interface ITypedFormField<ValueType> : IFormField
    {
        ValueType ValidValue { get; }
    }

    public class FormField<ValueType>(object? value, bool isValid = true, bool isModified = false) 
        : FormField(value, value != null && isValid, isModified), ITypedFormField<ValueType>
        where ValueType : IParsable<ValueType>
    {
        public ValueType ValidValue => IsValid
            ? ValueType.Parse(ToString(), CultureInfo.CurrentCulture)
            : throw new InvalidCastException("FormField cast error: value is not valid");
    }

    public class EnumFormField<EnumType>(EnumType? value, bool isValid = true, bool isModified = true)
        : FormField(value, value != null && isValid, isModified), ITypedFormField<EnumType>
        where EnumType : struct, Enum
    {
        public EnumType ValidValue => IsValid
            ? Enum.Parse<EnumType>(ToString() ?? throw new InvalidCastException("EnumFormField cast error: value is null"))
            : throw new InvalidCastException("EnumFormField cast error: value is not valid");
    }
}
