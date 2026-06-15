using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xcc.Application.Helpers;

namespace Xcc.Application.Forms
{
    public interface IFormField
    {
        bool IsValid { get; }
        bool IsModified { get; set; }
    }

    /// <summary>
    /// Base form viewmodel class providing field binding and validation infrastructure
    /// </summary>
    public class Form : DirtyFlaggedBindableBase, IFormField
    {
        private readonly List<IFormField> _formFields = [];
        private readonly Dictionary<string, FormField> _formFieldMapping = [];

        public Form() : base(validate: false)
        {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach(var field in fields)
            {
                if (field.FieldType.IsAssignableTo(typeof(IFormField)))
                {
                    var value = field.GetValue(this) as IFormField ?? throw new NullReferenceException($"Form initialization error: {field.Name} property is null");
                    _formFields.Add(value);
                }
            }

            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<FieldReferenceAttribute>() is var value && value != null)
                {
                    var field = fields.FirstOrDefault(f => f.Name == value.Name && f.FieldType.IsAssignableTo(typeof(FormField)));
                    if (field is null)
                    {
                        throw new NullReferenceException($"ReferenceAttribute: invalid reference to {value.Name}");
                    }
                    var fieldValue = field.GetValue(this) as FormField;
                    if (fieldValue is null)
                    {
                        throw new NullReferenceException($"ReferenceAttribute: {value.Name} field value is missing");
                    }
                    _formFieldMapping[property.Name] = fieldValue;
                }
            }
            IsValid = Validator.TryValidateObject(this, new ValidationContext(this), null);
            IsModified = false;
        }

        #region Public methods
        public override void AcceptChanges()
        {
            foreach (var field in _formFields)
            {
                field.IsModified = false;
            }
            base.AcceptChanges();
        }

        public virtual FormFieldAdapter<Type> GetProperty<Type>(string propertyName)
            where Type : IParsable<Type>
        {
            return new FormFieldAdapter<Type>(_formFieldMapping[propertyName]);
        }
        #endregion Public methods


        #region Private methods
        protected object? GetFieldValue(FormField field)
        {
            return field.Value;
        }

        protected ValueType? GetFieldTypedValue<ValueType>(ITypedFormField<ValueType> field)
            where ValueType : struct
        {
            return field.IsValid ? field.ValidValue : null;
        }

        /// <summary>
        /// Gets field value ensuring non-zero return, otherwise exception will be raised
        /// </summary>
        /// <typeparam name="ValueType"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        protected ValueType GetFieldTypedValueStrict<ValueType>(ITypedFormField<ValueType> field)
        {
            return field.IsValid ? field.ValidValue : throw new InvalidCastException("Form.GetFieldTypedValueStrict: invalid value");
        }

        protected object? GetFieldValue([CallerMemberName] string propertyName = "")
        {
            return GetFieldValue(_formFieldMapping[propertyName]);
        }

        protected ValueType? GetFieldTypedValue<ValueType>([CallerMemberName] string propertyName = "")
            where ValueType : struct
        {
            var field = _formFieldMapping[propertyName] as ITypedFormField<ValueType>;
            return field is not null 
                ? GetFieldTypedValue<ValueType>(field)
                : throw new InvalidCastException($"Form.GetFieldValue: invalid target type parameter {nameof(ValueType)}");
        }

        protected bool SetFieldValue(object? value, FormField field, [CallerMemberName] string? propertyName = null)
        {
            ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

            if (value?.ToString() != field.Value?.ToString())
            {
                field.SetValue(value, Validate(value, propertyName));

                SetIsModified();

                OnErrorsChanged(propertyName); // we call this to re-evaluate form's IsValid
                RaisePropertyChanged(propertyName);
                RaiseChangedEvent();

                return true;
            }
            return false;
        }

        protected bool SetFieldValue(object? value, [CallerMemberName] string? propertyName = null)
        {
            ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

            return SetFieldValue(value, _formFieldMapping[propertyName], propertyName);
        }

        protected override void OnErrorsChanged(string propertyName)
        {
            RaiseErrorsChanged(propertyName);
            RaisePropertyChanged(nameof(HasErrors)); // This is needed because there is a bug with 

            IsValid = _formFields.All(f => f.IsValid);
        }
        #endregion Private methods
    }
}
