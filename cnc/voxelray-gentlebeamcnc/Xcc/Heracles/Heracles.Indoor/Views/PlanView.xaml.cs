using System.Windows.Controls;
using System.Windows.Input;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.ViewModels;

namespace Heracles.Indoor.Views
{
    /// <summary>
    /// Interaction logic for EmptyView.xaml
    /// </summary>
    public partial class PlanView : ContentControl
    {
        public PlanView()
        {
            InitializeComponent();
        }

        private void Photo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is IPhoto photo)
            {
                if (DataContext is PlanViewModel viewModel && viewModel.SelectPhotoCommand.CanExecute(photo))
                {
                    viewModel.SelectPhotoCommand.Execute(photo);
                }
            }
        }
    }
}
