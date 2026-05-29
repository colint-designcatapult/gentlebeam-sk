using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xcc.Application.UI.UserControls
{
    public class TagListControl : ListBox
    {
        #region Contructors
        public TagListControl()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteExecuted, DeleteCanExecute));
        }

        static TagListControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TagListControl), new FrameworkPropertyMetadata(typeof(TagListControl)));
        }
        #endregion Contructors


        #region Private methods
        private void DeleteExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var source = ItemsSource as IList;
            source?.Remove(e.Parameter);
        }

        private void DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
        #endregion Private methods


        #region Dependecy properties
        #endregion Dependecy properties
    }
}
