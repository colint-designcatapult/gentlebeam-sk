using System;
using System.Globalization;

namespace Xcc.Application.Forms
{
    public class FormFieldAdapter<ValueType>(FormField field)
        where ValueType : IParsable<ValueType>
    {
        protected event EventHandler<ValueType>? ValueChanged;

        public void Subscribe(EventHandler<ValueType> subscriber)
        {
            ArgumentNullException.ThrowIfNull(subscriber);
            if (ValueChanged is null)
            {
                field.ValueChanged += OnFieldChanged;
            }
            ValueChanged += subscriber;
        }

        ValueType? _lastValue = default;
        public ValueType? Value => _lastValue;

        private void OnFieldChanged(object sender, object? value)
        {
            if (field.IsValid)
            {
                var valueString = value?.ToString() 
                    ?? throw new NullReferenceException("FormFieldAdapter.OnFieldChanged: value string is null");
                ValueType typedValue = ValueType.Parse(valueString, CultureInfo.CurrentCulture);
                if (!typedValue.Equals(_lastValue))
                {
                    _lastValue = typedValue;
                    ValueChanged?.Invoke(field, typedValue);
                }
            }
        }
    }
}
