using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.UserControls
{
    public class SortableColumnHeader : RadioButton
    {
        public static readonly DependencyProperty SortDescriptionProperty =
            DependencyProperty.RegisterAttached(
                "SortDescription",
                typeof(SortDescription),
                typeof(SortableColumnHeader),
                new FrameworkPropertyMetadata(
                    default(SortDescription),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (obj, e) => 
                    {
                        if (obj is not SortableColumnHeader x)
                            return;

                        x.IsChecked = ((SortDescription)e.NewValue).PropertyName == GetSortPropertyName(obj);
                    }));

        public static SortDescription GetSortDescription(DependencyObject obj) 
            => (SortDescription)obj.GetValue(SortDescriptionProperty);

        public static void SetSortDescription(DependencyObject obj, SortDescription value) 
            => obj.SetValue(SortDescriptionProperty, value);


        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "SortPropertyName",
                typeof(string),
                typeof(SortableColumnHeader),
                new FrameworkPropertyMetadata(default(string)));

        public static string GetSortPropertyName(DependencyObject obj)
            => (string)obj.GetValue(SortPropertyNameProperty);

        public static void SetSortPropertyName(DependencyObject obj, string value)
            => obj.SetValue(SortPropertyNameProperty, value);

        protected override void OnClick()
        {
            base.OnClick();

            if (this.IsChecked is null || this.IsChecked.Value == false)
                return;

            var propertyName = GetSortPropertyName(this);

            if (GetSortDescription(this).PropertyName == propertyName)
            {
                if (GetSortDescription(this).Direction == ListSortDirection.Ascending)
                {
                    SetSortDescription(this, new SortDescription(propertyName, ListSortDirection.Descending));
                }
                else

                {
                    SetSortDescription(this, new SortDescription(propertyName, ListSortDirection.Ascending));
                }
            }
            else
            {
                SetSortDescription(this, new SortDescription(propertyName, ListSortDirection.Ascending));
            }
        }
    }
}
