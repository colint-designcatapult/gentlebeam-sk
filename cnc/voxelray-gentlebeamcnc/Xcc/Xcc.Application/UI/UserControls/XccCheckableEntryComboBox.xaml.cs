using Prism.Mvvm;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.UserControls
{
    public class CheckableEntry : BindableBase
    {
        public required object Value { get; set; }
        public required string DisplayName { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    IsCheckedChanged?.Invoke(this, IsChecked);
                }
            }
        }

        public event EventHandler<bool>? IsCheckedChanged;
    }

    public class XccCheckableEntryComboBox : ComboBox
    {
        #region Contructors
        static XccCheckableEntryComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XccCheckableEntryComboBox), new FrameworkPropertyMetadata(typeof(XccCheckableEntryComboBox)));
        }
        #endregion Contructors


        #region Private methods
        /// <summary>
        /// Unchecks every checkable entry in the collection of checkable entries,
        /// then checks matching entries presented in the <b>CheckedItemsSource</b>.
        /// </summary>
        protected void OnCheckedItemsSourceChanged()
        {
            var allCheckableEntries = Items.Cast<CheckableEntry>().ToList();

            var checkedItems = CheckedItemsSource?.Cast<object>().ToList();

            // uncheck every checkable entry
            foreach (var checkableEntry in allCheckableEntries)
            {
                checkableEntry.IsChecked = false;
            }

            // find every entry with value presented in the CheckedItemsSource and check it
            if (checkedItems is not null)
            {
                var toCheck = allCheckableEntries.IntersectBy(checkedItems, x => x.Value);

                foreach (var checkableEntry in toCheck)
                {
                    checkableEntry.IsChecked = true;
                }
            }

            // subscribe to CheckedItemsSource, if it's an observable collection,
            // and set a IsChecked flag for the corresponding CheckableEntry depending on whether the element was deleted or added
            if (CheckedItemsSource is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged += (o, e) =>
                {
                    var checkableEntries = Items.Cast<CheckableEntry>().ToList();

                    if (e.OldItems is not null)
                    {
                        var toUncheck = checkableEntries.IntersectBy(e.OldItems.Cast<object>(), x => x.Value).ToList();

                        foreach (var checkableEntry in toUncheck)
                        {
                            checkableEntry.IsChecked = false;
                        }
                    }

                    if (e.NewItems is not null)
                    {
                        var toCheck = checkableEntries.IntersectBy(e.NewItems.Cast<object>(), x => x.Value).ToList();

                        foreach (var checkableEntry in toCheck)
                        {
                            checkableEntry.IsChecked = true;
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Generates collection of checkable entries from specified source.
        /// </summary>
        protected void OnCheckableEntriesSourceChanged()
        {
            if (CheckableEntriesSource is null)
                return;

            var checkableEntryList = new List<CheckableEntry>();

            foreach (var item in CheckableEntriesSource)
            {
                var checkableEntry = new CheckableEntry()
                {
                    Value = item,
                    DisplayName = GetDisplayName(item)
                };

                Items.Add(checkableEntry);

                checkableEntry.IsCheckedChanged += CheckableEntryIsCheckedChanged;
            }
        }

        /// <summary>
        /// Checks if the changed <b>CheckableEntry.Value</b> is in the resulting collection <b>CheckedItemsSource</b>
        /// and removes or adds the element to it depending on <b>CheckableEntry.IsChecked</b> state.
        /// </summary>
        private void CheckableEntryIsCheckedChanged(object? sender, bool e)
        {
            if (sender is not CheckableEntry entry)
                return;

            if (CheckedItemsSource is null)
                return;

            if (entry.IsChecked)
            {
                if (CheckedItemsSource.Contains(entry.Value) != true)
                {
                    CheckedItemsSource.Add(entry.Value);
                }
            }
            else
                CheckedItemsSource.Remove(entry.Value);
        }
        #endregion Private methods


        #region Dependecy properties
        public IEnumerable CheckableEntriesSource { get => (IEnumerable)GetValue(CheckableEntriesSourceProperty); set => SetValue(CheckableEntriesSourceProperty, value); }

        public static readonly DependencyProperty CheckableEntriesSourceProperty =
            DependencyProperty.Register(
                nameof(CheckableEntriesSource),
                typeof(IEnumerable),
                typeof(XccCheckableEntryComboBox),
                new PropertyMetadata((obj, e) =>
                {
                    if (obj is not XccCheckableEntryComboBox control)
                        return;
                    
                    control.OnCheckableEntriesSourceChanged();
                }));


        public IList? CheckedItemsSource 
        { 
            get => (IList?)GetValue(CheckedItemsSourceProperty); 
            set => SetValue(CheckedItemsSourceProperty, value); 
        }

        public static readonly DependencyProperty CheckedItemsSourceProperty =
            DependencyProperty.Register(
                nameof(CheckedItemsSource),
                typeof(IList),
                typeof(XccCheckableEntryComboBox),
                new PropertyMetadata((obj, e) =>
                {
                    if (obj is not XccCheckableEntryComboBox control)
                        return;

                    control.OnCheckedItemsSourceChanged();
                }));


        
        public DataTemplate? CheckedEntriesItemTemplate
        {
            get => (DataTemplate?)GetValue(CheckedEntriesItemTemplateProperty);
            set => SetValue(CheckedEntriesItemTemplateProperty, value);
        }

        public static readonly DependencyProperty CheckedEntriesItemTemplateProperty =
            DependencyProperty.Register(
                nameof(CheckedEntriesItemTemplate),
                typeof(DataTemplate),
                typeof(XccCheckableEntryComboBox),
                new PropertyMetadata(null));
        #endregion Dependecy properties


        #region Static methods
        private static string GetDisplayName(object item)
        {
            Type type = item.GetType();

            string? itemAsString = item.ToString();

            if (itemAsString is null)
                return type.Name;

            MemberInfo[] memberInfos = type.GetMember(itemAsString);

            if (memberInfos.Length == 0)
                return itemAsString;

            IEnumerable<DisplayAttribute> attributes = memberInfos
                .First()
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .Cast<DisplayAttribute>();

            return attributes.FirstOrDefault()?.Name ?? itemAsString;
        }
        #endregion Static methods
    }
}
