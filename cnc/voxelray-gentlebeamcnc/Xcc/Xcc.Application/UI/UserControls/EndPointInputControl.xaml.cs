using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Xcc.Core.Models;

namespace Xcc.Application.UI.UserControls
{
    public class EndPointInputControl : ContentControl
    {
        static EndPointInputControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EndPointInputControl), new FrameworkPropertyMetadata(typeof(EndPointInputControl)));
        }

        public EndPointInputControl()
        {
            SubscribeValidationEvents();
        }

        private TextBox? _ipPart1TextBox;
        private TextBox? _ipPart2TextBox;
        private TextBox? _ipPart3TextBox;
        private TextBox? _ipPart4TextBox;
        private TextBox? _portTextBox;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _ipPart1TextBox = Template.FindName("IpPart1TextBox", this) as TextBox;
            _ipPart2TextBox = Template.FindName("IpPart2TextBox", this) as TextBox;
            _ipPart3TextBox = Template.FindName("IpPart3TextBox", this) as TextBox;
            _ipPart4TextBox = Template.FindName("IpPart4TextBox", this) as TextBox;
            _portTextBox = Template.FindName("PortTextBox", this) as TextBox;

            SubscribeValidationEvents();
        }


        private void SubscribeValidationEvents()
        {
            _ipPart1TextBox?.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnValidationChanged));
            _ipPart2TextBox?.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnValidationChanged));
            _ipPart3TextBox?.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnValidationChanged));
            _ipPart4TextBox?.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnValidationChanged));
        }

        private void OnValidationChanged(object sender, RoutedEventArgs e)
        {
            Validate();
        }

        private void Validate()
        {
            if (_ipPart1TextBox == null || _ipPart2TextBox == null || _ipPart3TextBox == null || _ipPart4TextBox == null || _portTextBox == null)
            {
                HasErrors = false;
                return;
            }

            HasErrors =
                Validation.GetHasError(_ipPart1TextBox) ||
                Validation.GetHasError(_ipPart2TextBox) ||
                Validation.GetHasError(_ipPart3TextBox) ||
                Validation.GetHasError(_ipPart4TextBox);
        }


        private void textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char c = Convert.ToChar(e.Text);

            if (!char.IsDigit(c))
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
            }
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb)
            {
                return;
            }

            if (!string.IsNullOrEmpty(tb.Text) && Convert.ToInt32(tb.Text) > 255)
            {
                RestoreTextBoxText(tb.Name);

                e.Handled = true;
                SystemSounds.Beep.Play();

                return;
            }

            if (!string.IsNullOrEmpty(tb.Text))
                SaveTextBoxText(tb.Name);

            if (tb.Text.Length == 3)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);

                if (Keyboard.FocusedElement is UIElement keyboardFocus)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
            }
        }

        private void SaveTextBoxText(string name)
        {
            //int i;

            //switch (name)
            //{
            //    case "text1":
            //        text1Bkp = text1.Text;
            //        if (Int32.TryParse(text1.Text, out i))
            //            IP1 = i;
            //        break;
            //    case "text2":
            //        text2Bkp = text2.Text;
            //        if (Int32.TryParse(text2.Text, out i))
            //            IP2 = i;
            //        break;
            //    case "text3":
            //        text3Bkp = text3.Text;
            //        if (Int32.TryParse(text3.Text, out i))
            //            IP3 = i;
            //        break;
            //    case "text4":
            //        text4Bkp = text4.Text;
            //        if (Int32.TryParse(text4.Text, out i))
            //            EndPoint = i;
            //        break;
            //    default:
            //        break;
            //}
        }

        private void RestoreTextBoxText(string name)
        {
            //switch (name)
            //{
            //    case "text1":
            //        text1.Text = text1Bkp;
            //        if (text1.Text.Length > 0)
            //            text1.CaretIndex = text1.Text.Length;
            //        break;
            //    case "text2":
            //        text2.Text = text2Bkp;
            //        if (text2.Text.Length > 0)
            //            text2.CaretIndex = text2.Text.Length;
            //        break;
            //    case "text3":
            //        text3.Text = text3Bkp;
            //        if (text3.Text.Length > 0)
            //            text3.CaretIndex = text3.Text.Length;
            //        break;
            //    case "text4":
            //        text4.Text = text4Bkp;
            //        if (text4.Text.Length > 0)
            //            text4.CaretIndex = text4.Text.Length;
            //        break;
            //    default:
            //        break;
            //}
        }




        #region Dependency properties
        public ISystemEndPoint EndPoint
        {
            get => (ISystemEndPoint)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        public static readonly DependencyProperty EndPointProperty =
            DependencyProperty.Register(
                nameof(EndPoint),
                typeof(ISystemEndPoint),
                typeof(EndPointInputControl));


        public bool HasErrors
        {
            get => (bool)GetValue(HasErrorsProperty);
            set => SetValue(HasErrorsProperty, value);
        }

        public static readonly DependencyProperty HasErrorsProperty =
            DependencyProperty.Register(
                nameof(HasErrors), 
                typeof(bool), 
                typeof(EndPointInputControl));


        public bool IsPortInputEnabled
        {
            get => (bool)GetValue(IsPortInputEnabledProperty);
            set => SetValue(IsPortInputEnabledProperty, value);
        }

        public static readonly DependencyProperty IsPortInputEnabledProperty =
            DependencyProperty.Register(
                nameof(IsPortInputEnabled),
                typeof(bool),
                typeof(EndPointInputControl),
                new PropertyMetadata(true));


        public bool IsAddressInputEnabled
        {
            get => (bool)GetValue(IsAddressInputEnabledProperty);
            set => SetValue(IsAddressInputEnabledProperty, value);
        }

        public static readonly DependencyProperty IsAddressInputEnabledProperty =
            DependencyProperty.Register(
                nameof(IsAddressInputEnabled),
                typeof(bool),
                typeof(EndPointInputControl),
                new PropertyMetadata(true));    
        #endregion Dependency properties
    }
}
