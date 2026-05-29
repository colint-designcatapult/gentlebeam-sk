using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xcc.Application.Helpers;

namespace Xcc.Application.UI.UserControls
{
    /// <summary>
    /// Interaction logic for TaskOverlay.xaml
    /// </summary>
    public partial class TaskOverlay : UserControl
    {
        public TaskOverlay()
        {
            InitializeComponent();
        }

        #region Dependency properties
        public ObservableTask ObservedTask
        {
            get => (ObservableTask)GetValue(ObservedTaskProperty);
            set => SetValue(ObservedTaskProperty, value);
        }

        public static readonly DependencyProperty ObservedTaskProperty =
            DependencyProperty.Register(
                nameof(ObservedTask),
                typeof(ObservableTask),
                typeof(TaskOverlay),
                new FrameworkPropertyMetadata((s,e) => 
        {
        }));


        public ICommand RetryCommand 
        { 
            get => (ICommand)GetValue(RetryCommandProperty); 
            set => SetValue(RetryCommandProperty, value); 
        }

        public static readonly DependencyProperty RetryCommandProperty = 
            DependencyProperty.Register(
                nameof(RetryCommand),
                typeof(ICommand),
                typeof(TaskOverlay));


        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(
                nameof(CancelCommand),
                typeof(ICommand),
                typeof(TaskOverlay));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(TaskOverlay));


        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(TaskOverlay));


        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(TaskOverlay),
                new PropertyMetadata(Orientation.Vertical));
        #endregion Dependency properties
    }
}
