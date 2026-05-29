using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Xcc.Application.Models;
using Xcc.Core.Enums;

namespace Heracles.Application.UI.UserControls
{

    /// <summary>
    /// Interaction logic for TaskWatcherOverlay.xaml
    /// </summary>
    public partial class ReportOverlay : UserControl
    {
        public Report Report
        {
            get => (Report)GetValue(ReportProperty);
            set => SetValue(ReportProperty, value);
        }

        public static readonly DependencyProperty ReportProperty =
            DependencyProperty.Register(
                "Report",
                typeof(Report),
                typeof(ReportOverlay),
                new FrameworkPropertyMetadata(default(Report)));


        public ICommand CloseCommand 
        { 
            get => (ICommand)GetValue(CloseCommandProperty); 
            set => SetValue(CloseCommandProperty, value); 
        }

        public static readonly DependencyProperty CloseCommandProperty = 
            DependencyProperty.Register(
                "CloseCommand",
                typeof(ICommand),
                typeof(ReportOverlay));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(ReportOverlay));

        public ReportOverlay() => InitializeComponent();

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseCommand?.Execute(Report);
            SetCurrentValue(ReportProperty, null);
        }
    }

    #region Converters
    [ValueConversion(typeof(ReportType), typeof(Brush))]
    public class ReportTypeToBorderColor : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ReportType messageType = default(ReportType);

            if (value is ReportType)
                messageType = (ReportType)value;

            return messageType == ReportType.Error ? Brushes.Red : Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    [ValueConversion(typeof(ReportType), typeof(Visibility))]
    public class ReportTypeToVisiblity : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ReportType messageType = default(ReportType);

            if (value is ReportType)
                messageType = (ReportType)value;

            return (messageType == ReportType.Progress) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    [ValueConversion(typeof(ReportType), typeof(Visibility))]
    public class ReportToVisiblity : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Report)value is null) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    #endregion Converters
}
