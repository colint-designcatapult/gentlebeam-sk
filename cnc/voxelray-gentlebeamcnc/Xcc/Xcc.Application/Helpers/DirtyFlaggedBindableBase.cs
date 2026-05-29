using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Prism.Mvvm;
using Xcc.Core.Models;

namespace Xcc.Application.Helpers
{
    /// <summary>
    /// Deprecated. Use from Empyrean.Common.Application.Models
    /// </summary>
    [Obsolete]
    public class DirtyFlaggedBindableBase : BindableBase, IDirtyFlaggedBindableBase
    {
        public DirtyFlaggedBindableBase(bool validate = true)
        {
            // This code causes changes to the IsModified flag in properties, declared in the derived type
            // to also affect IsModified property of the derived class instance itself.
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                // skip read-only properties
                if (property.CanWrite == false)
                    break;

                // checks if property value can be assigned to INotifyIsModified
                if (property.PropertyType.IsAssignableTo(typeof(IDirtyFlagged)))
                {
                    PropagateIsModified(property.GetValue(this, null));
                }

                // checks if property value can be assigned to IEnumerable<INotifyIsModified>
                if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<IDirtyFlagged>)))
                {
                    PropagateIsModified(property.GetValue(this, null));
                }
                
                // checks if property value can be assigned to IPropagateIsValid
                if (property.PropertyType.IsAssignableTo(typeof(IPropagateIsValid)))
                {
                    PropagateIsValid(property.GetValue(this, null));
                }

                // checks if property value can be assigned to IEnumerable<IPropagateIsValid>
                if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<IPropagateIsValid>)))
                {
                    PropagateIsValid(property.GetValue(this, null));
                }
            }

            // To prevent premature validation before we do ancestor's initialization, we set validate to false
            if (validate)
                IsValid = Validator.TryValidateObject(this, new ValidationContext(this), null);
            else
                IsValid = false;
        }

        private bool _isModified;
        public bool IsModified
        {
            get => _isModified;
            set
            {
                //if (this.GetType().Name == "CalibrationDataStore") 
                //    ; 

                if (base.SetProperty(ref _isModified, value))
                {
                    OnIsModifiedChanged(this, value);
                }
                
                if (_isModified)
                {
                    RaiseChangedEvent();
                }
            }
        }

        /// <summary>
        /// Set new property value and the set IsModified flag to true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected virtual bool SetPropertyWithDirtyFlag<T>(ref T storage, T value, [CallerMemberName] string propertyName = null!)
        {
            var oldValue = storage;

            bool isSet = base.SetProperty(ref storage, value, propertyName);

            if (isSet)
            {
                PropagateIsModified(value, oldValue);
                PropagateIsValid(value, oldValue);
                SetIsModified();
                IsValid = ValidateSubProperties();
            }
            return isSet;
        }

        private void PropagateIsModified(object? propertyValue, object? oldPropertyValue = null)
        {
            // tries to cast property value to INotifyIsModified and subscribes to its IsModifiedChanged event.
            if (propertyValue is IDirtyFlagged dirtyProperty)
            {
                dirtyProperty.IsModifiedChanged += OnSubPropertyModified;
                _dirtyProperties.Add(dirtyProperty);
            }
            //same for old property value to unsubscribe
            if (oldPropertyValue is IDirtyFlagged oldDirtyProperty)
            {
                oldDirtyProperty.IsModifiedChanged -= OnSubPropertyModified;
                _dirtyProperties.Remove(oldDirtyProperty);
            }


            // tries to cast property value to IEnumerable<INotifyIsModified> and subscribes to IsModifiedChanged event of every item.
            if (propertyValue is IEnumerable<IDirtyFlagged> dirtyEntries)
            {
                _dirtyEntryLists.Add(dirtyEntries);

                foreach (var entry in dirtyEntries)
                {
                    entry.IsModifiedChanged += OnSubPropertyModified;
                }

                // check if property value implements INotifyCollectionChanged. 
                if (propertyValue is INotifyCollectionChanged dirtyObservableCollection)
                {
                    dirtyObservableCollection.CollectionChanged += (s, e) =>
                    {
                        //subscribes added items to IsModifiedChanged event.
                        if (e.NewItems is not null)
                        {
                            foreach (IDirtyFlagged item in e.NewItems)
                            {
                                item.IsModifiedChanged += OnSubPropertyModified;
                            }
                        }

                        //unsubscribe removed items from IsModifiedChanged event.
                        if (e.OldItems is not null)
                        {
                            foreach (IDirtyFlagged item in e.OldItems)
                                item.IsModifiedChanged -= OnSubPropertyModified;
                        }
                    };
                }
            }

            // same for old property value to unsubscribe
            if (oldPropertyValue is IEnumerable<IDirtyFlagged> oldDirtyEntries)
            {
                _dirtyEntryLists.Add(oldDirtyEntries);

                foreach (var entry in oldDirtyEntries)
                {
                    entry.IsModifiedChanged -= OnSubPropertyModified;
                }
            }
        }


        private void PropagateIsValid(object? propertyValue, object? oldPropertyValue = null)
        {
            // tries to cast property value to IPropagateIsValid and subscribes to its IsValidChanged event.
            if (propertyValue is IPropagateIsValid validableProperty)
            {
                validableProperty.IsValidChanged += OnSubPropertyValidationChanged;
                _validableProperties.Add(validableProperty);
            }
            //same for old property value to unsubscribe
            if (oldPropertyValue is IPropagateIsValid oldValidableProperty)
            {
                oldValidableProperty.IsValidChanged -= OnSubPropertyValidationChanged;
                _validableProperties.Remove(oldValidableProperty);
            }


            // tries to cast property value to IEnumerable<IPropagateIsValid> and subscribes to IsValidChanged event of every item.
            if (propertyValue is IEnumerable<IPropagateIsValid> validablePropertyEntries)
            {
                _validableEntryLists.Add(validablePropertyEntries);

                foreach (var entry in validablePropertyEntries)
                {
                    entry.IsValidChanged += OnSubPropertyValidationChanged;
                }

                // check if property value implements INotifyCollectionChanged. 
                if (propertyValue is INotifyCollectionChanged observableCollection)
                {
                    observableCollection.CollectionChanged += (s, e) =>
                    {
                        //subscribes added items to IsValidChanged event.
                        if (e.NewItems is not null)
                        {
                            foreach (IPropagateIsValid item in e.NewItems)
                                item.IsValidChanged += OnSubPropertyValidationChanged;
                        }

                        //unsubscribe removed items from IsValidChanged event.
                        if (e.OldItems is not null)
                        {
                            foreach (IPropagateIsValid item in e.OldItems)
                                item.IsValidChanged -= OnSubPropertyValidationChanged;
                        }
                    };
                }
            }

            // same for old property value to unsubscribe
            if (oldPropertyValue is IEnumerable<IPropagateIsValid> oldValidablePropertyEntries)
            {
                _validableEntryLists.Remove(oldValidablePropertyEntries);

                foreach (var entry in oldValidablePropertyEntries)
                {
                    entry.IsValidChanged -= OnSubPropertyValidationChanged;
                }
            }
        }


        #region IDirtyFlagged
        public event EventHandler? Changed;
        public event EventHandler<bool>? IsModifiedChanged;

        private readonly List<IEnumerable<IDirtyFlagged>> _dirtyEntryLists = [];

        private readonly List<IDirtyFlagged> _dirtyProperties = [];

        public virtual void SetIsModified()
        {
            IsModified = true;
        }
        
        protected virtual void OnIsModifiedChanged(object sender, bool isModified)
        {
            IsModifiedChanged?.Invoke(sender, isModified);
        }

        protected virtual void OnSubPropertyModified(object? sender, bool isSubPropertyModified)
        {
            if(_disableOnSubPropertyModified) return;

            IsModified |= CheckIsModifiedRecursive();
        }

        public virtual void AcceptChanges()
        {
            IsModified = false;
        }

        private bool _disableOnSubPropertyModified;

        public virtual void AcceptChangesRecursive()
        {
            _disableOnSubPropertyModified = true;

            foreach (var dirtyEntries in _dirtyEntryLists)
            {
                foreach (IDirtyFlagged dirtyEntry in dirtyEntries)
                {
                    dirtyEntry.IsModified = false;
                }
            }

            foreach (var dirtyProperty in _dirtyProperties)
            {
                dirtyProperty.IsModified = false;
            }


            IsModified = false;

            _disableOnSubPropertyModified = false;
        }

        private bool CheckIsModifiedRecursive()
        {
            var isModified = false;

            foreach (var dirtyEntries in _dirtyEntryLists)
            {
                foreach (IDirtyFlagged dirtyEntry in dirtyEntries)
                {
                    isModified |= dirtyEntry.IsModified;
                }
            }

            foreach (var dirtyProperties in _dirtyProperties)
            {
                isModified |= dirtyProperties.IsModified;
            }

            return isModified;
        }
        #endregion IDirtyFlagged


        #region IPropagateIsValid
        private bool _isValid = true;
        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (SetProperty(ref _isValid, value))
                {
                    IsValidChanged?.Invoke(this, _isValid);
                }
            }
        }

        public event EventHandler<bool>? IsValidChanged;

        /// <summary>
        /// Stores any lists of objects, which implements IPropagateIsValid.
        /// </summary>
        private readonly List<IEnumerable<IPropagateIsValid>> _validableEntryLists = [];

        private readonly List<IPropagateIsValid> _validableProperties = [];

        protected virtual void OnSubPropertyValidationChanged(object? sender, bool isSubPropertyValid)
        {
            var isValid = !HasErrors && isSubPropertyValid;

            if (isValid) //if isValid == false - it means object already is not valid. No need to check another validable properties and lists (performance optimization).
            {
                isValid &= ValidateSubProperties();
            }
            
            IsValid = isValid;
        }

        private bool ValidateSubProperties()
        {
            var isValid = true;

            foreach (var listOfValidableEntries in _validableEntryLists)
            {
                foreach (IPropagateIsValid validableEntry in listOfValidableEntries)
                {
                    isValid &= validableEntry.IsValid;
                }
            }

            foreach (var validableProperty in _validableProperties)
            {
                isValid &= validableProperty.IsValid;
            }

            return isValid;
        }
        #endregion IPropagateIsValid


        #region INotifyDataErrorInfo
        readonly Dictionary<string, List<string?>> _errors = [];

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool HasErrors => _errors.Count > 0;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName is null)
                return Enumerable.Empty<string>();

            if (_errors.TryGetValue(propertyName, out var value))
            {
                return value;
            }

            return Enumerable.Empty<string>();
        }

        public bool Validate(object? propertyValue, [CallerMemberName] string propertyName = null!)
        {
            var results = new List<ValidationResult>();

            bool propertyValidationResult = Validator.TryValidateProperty(propertyValue, new ValidationContext(this)
            {
                MemberName = propertyName,
                DisplayName = Regex.Replace(propertyName, @"(\p{Lu})", " $1").TrimStart(' '),
            }, results);

            if (results.Count != 0)
            {
                _errors[propertyName] = results.Select(r => r.ErrorMessage).Take(1).ToList();
                OnErrorsChanged(propertyName);
                return false;
            }
            else
            {
                _errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
                return true;
            }
        }

        protected virtual void OnErrorsChanged(string propertyName)
        {
            RaiseErrorsChanged(propertyName);
            RaisePropertyChanged(nameof(HasErrors)); // This is needed because there is a bug with 

            var results = new List<ValidationResult>();
            IsValid = Validator.TryValidateObject(this, new ValidationContext(this), results, validateAllProperties: true);
        }

        protected void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected virtual void OnIsValidChanged(object sender, bool isValid)
        {
            IsValidChanged?.Invoke(this, isValid);
        }
        #endregion INotifyDataErrorInfo

        protected void RaiseChangedEvent()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

    }
}
