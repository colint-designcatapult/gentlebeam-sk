using System.Windows.Controls;
using Heracles.Indoor.ViewModels;

namespace Heracles.Indoor.Views
{
    /// <summary>
    /// Interaction logic for PatientsView
    /// </summary>
    public partial class PatientsView : ContentControl
    {
        public PatientsView()
        {
            InitializeComponent();
            IsVisibleChanged += (_, _) =>
            {
                if (DataContext is PatientsViewModel viewModel)
                    viewModel.SetPatientListVisible(IsVisible);
            };
            Loaded += (_, _) =>
            {
                if (DataContext is PatientsViewModel viewModel)
                    viewModel.SetPatientListVisible(IsVisible);
            };
        }
    }
}
