using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Heracles.Ucsi.Models;
using Heracles.Ucsi.ViewModels;

namespace Heracles.Ucsi.Views;

public partial class UnifiedCalibrationServiceView : System.Windows.Controls.UserControl
{
    private readonly DispatcherTimer _refreshTimer;
    private long _lastGraphSequence = -1;
    private bool _refreshing;
    private CancellationTokenSource? _refreshCancellation;

    // Throttle timers for arrow key command sends (250ms minimum between sends)
    private DateTime _lastHvCommandSend = DateTime.MinValue;
    private DateTime _lastPowerCommandSend = DateTime.MinValue;
    private DateTime _lastGridCommandSend = DateTime.MinValue;
    private DateTime _lastHeatCommandSend = DateTime.MinValue;
    private const int CommandThrottleMs = 200;

    public UnifiedCalibrationServiceView()
    {
        InitializeComponent();
        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1d / 30d),
        };
        _refreshTimer.Tick += OnRefreshTick;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
            viewModel.Coordinator.Start();
        _refreshCancellation = new CancellationTokenSource();
        _refreshTimer.Start();
        _ = RefreshAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
    {
        _refreshTimer.Stop();
        _refreshCancellation?.Cancel();
        _refreshCancellation?.Dispose();
        _refreshCancellation = null;
    }

    private void OnRefreshTick(object? sender, EventArgs eventArgs) => _ = RefreshAsync();

    private async Task RefreshAsync()
    {
        if (_refreshing || DataContext is not UnifiedCalibrationServiceViewModel viewModel)
            return;
        _refreshing = true;
        try
        {
            CancellationToken cancellationToken = _refreshCancellation?.Token ?? CancellationToken.None;
            await viewModel.TickAsync();
            IReadOnlyList<UcsiTelemetrySample> liveBatch = viewModel.Mode == UcsiMode.Live
                ? viewModel.Coordinator.LiveHistory.GetAfter(_lastGraphSequence)
                : Array.Empty<UcsiTelemetrySample>();
            if (liveBatch.Count > 0)
                _lastGraphSequence = liveBatch[^1].LiveSequence;

            foreach (GraphPaneView graph in FindVisualChildren<GraphPaneView>(this))
                await graph.RefreshAsync(viewModel.Coordinator, liveBatch, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _refreshing = false;
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int index = 0; index < count; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
                yield return match;
            foreach (T descendant in FindVisualChildren<T>(child))
                yield return descendant;
        }
    }

    private void OnCommandTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox textBox)
            return;

        if (e.Key == Key.Return)
        {
            BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
            
            // Send the appropriate command based on which textbox
            if (textBox.Name == "HvCommandTextBox")
            {
                _ = (DataContext as UnifiedCalibrationServiceViewModel)?.SendHvpsKvAsync();
            }
            else if (textBox.Name == "PowerCommandTextBox")
            {
                // Power sends via property setter when binding updates
            }
            else if (textBox.Name == "GridCommandTextBox")
            {
                _ = (DataContext as UnifiedCalibrationServiceViewModel)?.SendHvpsGridAsync();
            }
            else if (textBox.Name == "HeatCommandTextBox")
            {
                _ = (DataContext as UnifiedCalibrationServiceViewModel)?.SendHvpsFilamentAsync();
            }
            else if (textBox.Name == "CoilsXCommandTextBox" || textBox.Name == "CoilsYCommandTextBox" || textBox.Name == "CoilsFocusCommandTextBox")
            {
                _ = (DataContext as UnifiedCalibrationServiceViewModel)?.SendCoilsAsync();
            }
            
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles PreviewKeyDown for arrow keys to enable numeric increment/decrement.
    /// This must be PreviewKeyDown (not KeyDown) because TextBox intercepts arrow keys
    /// for cursor navigation before KeyDown fires.
    /// </summary>
    private void OnCommandTextBoxPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox textBox)
            return;

        if (e.Key == Key.Up || e.Key == Key.Down)
        {
            bool handled = false;
            
            // Handle arrow key increment/decrement for HVPS command boxes
            if (textBox.Name == "HvCommandTextBox")
            {
                if (HandleArrowKeyNumericEdit(textBox, e.Key == Key.Up, 0, 100))
                {
                    if (CanSendCommand(ref _lastHvCommandSend))
                    {
                        _ = (DataContext as UnifiedCalibrationServiceViewModel)?.SendHvpsKvAsync();
                    }
                    handled = true;
                }
            }
            else if (textBox.Name == "PowerCommandTextBox")
            {
                if (HandleArrowKeyNumericEdit(textBox, e.Key == Key.Up, 0, 400))
                {
                    if (CanSendCommand(ref _lastPowerCommandSend))
                    {
                        // Power sends via property setter when binding updates
                    }
                    handled = true;
                }
            }
            else if (textBox.Name == "GridCommandTextBox")
            {
                if (HandleArrowKeyNumericEdit(textBox, e.Key == Key.Up, 0, 600))
                {
                    if (CanSendCommand(ref _lastGridCommandSend))
                    {
                        _ = (DataContext as UnifiedCalibrationServiceViewModel)?.SendHvpsGridAsync();
                    }
                    handled = true;
                }
            }
            else if (textBox.Name == "HeatCommandTextBox")
            {
                if (HandleArrowKeyNumericEdit(textBox, e.Key == Key.Up, 0, 4000))
                {
                    if (CanSendCommand(ref _lastHeatCommandSend))
                    {
                        _ = (DataContext as UnifiedCalibrationServiceViewModel)?.SendHvpsFilamentAsync();
                    }
                    handled = true;
                }
            }
            
            e.Handled = handled;
        }
    }

    /// <summary>
    /// Checks if enough time has passed since the last command send.
    /// Updates the timestamp if sufficient time has elapsed.
    /// Returns true if command should be sent, false if throttled.
    /// </summary>
    private bool CanSendCommand(ref DateTime lastSendTime)
    {
        DateTime now = DateTime.UtcNow;
        if ((now - lastSendTime).TotalMilliseconds >= CommandThrottleMs)
        {
            lastSendTime = now;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Handles arrow key increment/decrement for numeric textbox.
    /// Modifies the digit to the LEFT of cursor (if one exists).
    /// Returns true if a digit was modified, false if cursor is at leftmost position (no digit to left).
    /// </summary>
    private bool HandleArrowKeyNumericEdit(System.Windows.Controls.TextBox textBox, bool isIncrement, double minValue, double maxValue)
    {
        string text = textBox.Text;
        
        // Parse the actual text value (handles decimals correctly)
        if (!double.TryParse(text, out double currentValue))
            currentValue = 0;

        int cursorPos = textBox.CaretIndex;

        // Find decimal point position (if any)
        int decimalPos = text.IndexOf('.');

        // Count digits before cursor in original text
        int digitsBeforeCursor = 0;
        for (int i = 0; i < Math.Min(cursorPos, text.Length); i++)
        {
            if (char.IsDigit(text[i]))
                digitsBeforeCursor++;
        }

        // If no digits to the left of cursor, do nothing
        if (digitsBeforeCursor == 0)
            return false;

        // Target the digit to the LEFT of cursor (digitsBeforeCursor - 1)
        int targetDigitPosition = digitsBeforeCursor - 1;

        // Count total digits
        int totalDigits = text.Count(char.IsDigit);

        // Count how many digits are to the left of decimal point
        int digitsBeforeDecimal = decimalPos >= 0
            ? text.Substring(0, decimalPos).Count(char.IsDigit)
            : totalDigits;

        // Calculate place value: 
        // - If digit is before decimal: placeValue = 10^(digitsBeforeDecimal - 1 - digitIndex)
        // - If digit is after decimal: placeValue = 10^(-(digitIndex - digitsBeforeDecimal + 1))
        int placeExponent;
        if (targetDigitPosition < digitsBeforeDecimal)
        {
            // Digit is before decimal point
            placeExponent = digitsBeforeDecimal - 1 - targetDigitPosition;
        }
        else
        {
            // Digit is after decimal point
            placeExponent = -(targetDigitPosition - digitsBeforeDecimal + 1);
        }

        double placeValue = Math.Pow(10, placeExponent);

        // Increment or decrement by the place value
        double newValue = currentValue + (isIncrement ? placeValue : -placeValue);

        // Clamp to valid range
        newValue = Math.Max(minValue, Math.Min(maxValue, newValue));

        // Determine format string based on original format (preserve decimal places)
        int decimalPlaces = decimalPos >= 0 ? text.Length - decimalPos - 1 : 0;
        string format = decimalPlaces > 0 ? $"F{decimalPlaces}" : "0";

        // Convert to string
        string newText = newValue.ToString(format);

        // Find which digit position in the NEW text corresponds to the same place value we modified
        // This ensures cursor stays with the same place value even when digit count changes
        int newDecimalPos = newText.IndexOf('.');
        int newDigitsBeforeDecimal = newDecimalPos >= 0
            ? newDecimalPos
            : newText.Count(char.IsDigit);

        // Find the digit position in the new text that has this same exponent
        int newTargetDigitPosition = -1;
        int newTotalDigits = newText.Count(char.IsDigit);
        
        for (int d = 0; d < newTotalDigits; d++)
        {
            int exponent;
            if (d < newDigitsBeforeDecimal)
            {
                // Digit before decimal
                exponent = newDigitsBeforeDecimal - 1 - d;
            }
            else
            {
                // Digit after decimal
                exponent = -(d - newDigitsBeforeDecimal + 1);
            }

            if (exponent == placeExponent)
            {
                newTargetDigitPosition = d;
                break;
            }
        }

        // Position cursor to the right of the modified digit (same place value context)
        int newCursorPos = newTargetDigitPosition >= 0 ? newTargetDigitPosition : 0;

        // Translate digit position to actual cursor position in text
        int actualCursorPos = 0;
        int digitsCount = 0;
        for (int i = 0; i < newText.Length; i++)
        {
            if (char.IsDigit(newText[i]))
            {
                if (digitsCount == newCursorPos)
                {
                    actualCursorPos = i;
                    break;
                }
                digitsCount++;
            }
            if (i == newText.Length - 1)
                actualCursorPos = newText.Length;
        }

        // Update textbox
        textBox.Text = newText;
        textBox.CaretIndex = Math.Min(actualCursorPos + 1, textBox.Text.Length);

        // Update binding
        BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
        binding?.UpdateSource();

        return true;
    }

    private void OnApplyMonitoredParametersClick(object sender, RoutedEventArgs e)
    {
        // Close the popup by toggling the ParameterButton's IsChecked state
        ParameterButton.IsChecked = false;
    }

    private void OnEyeButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string parameterId)
        {
            if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
            {
                viewModel.AddToMonitoredParameters(parameterId);
            }
        }
    }

    private void OnHvCommandLostFocus(object sender, RoutedEventArgs e)
    {
        // Update binding first, then send command with the new value
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
        }
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendHvpsKvAsync();
        }
    }

    private void OnPowerCommandLostFocus(object sender, RoutedEventArgs e)
    {
        // Binding update to LostFocus triggers property setter which handles all logic:
        // clamping, updating dependent UI properties, and sending command if HV > 0
    }

    private void OnGridCommandLostFocus(object sender, RoutedEventArgs e)
    {
        // Update binding first, then send command with the new value
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
        }
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendHvpsGridAsync();
        }
    }

    private void OnHeatCommandLostFocus(object sender, RoutedEventArgs e)
    {
        // Update binding first, then send command with the new value
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
        }
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendHvpsFilamentAsync();
        }
    }

    private void OnHvCommandSliderReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // When user releases the HV slider, send HV+mA command to the board
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendHvpsKvAsync();
        }
    }

    private void OnPowerCommandSliderReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // When user releases the Power slider, send HV+mA command to the board (power affects HV calculation)
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendHvpsKvAsync();
        }
    }

    private void OnGridCommandSliderReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // When user releases the Grid slider, send Grid command to the board
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendHvpsGridAsync();
        }
    }

    private void OnHeatCommandSliderReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // When user releases the Heat/Filament slider, send Filament command to the board
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendHvpsFilamentAsync();
        }
    }

    private void OnXCoilCommandSliderReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // When user releases the X Coil slider, send coils command to the board
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendCoilsAsync();
        }
    }

    private void OnYCoilCommandSliderReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // When user releases the Y Coil slider, send coils command to the board
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendCoilsAsync();
        }
    }

    private void OnFocusCommandSliderReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // When user releases the Focus slider, send coils command to the board
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendCoilsAsync();
        }
    }

    private void OnXCoilCommandLostFocus(object sender, RoutedEventArgs e)
    {
        // Update binding first, then send command with the new value
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
        }
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendCoilsAsync();
        }
    }

    private void OnYCoilCommandLostFocus(object sender, RoutedEventArgs e)
    {
        // Update binding first, then send command with the new value
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
        }
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendCoilsAsync();
        }
    }

    private void OnFocusCommandLostFocus(object sender, RoutedEventArgs e)
    {
        // Update binding first, then send command with the new value
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
        }
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendCoilsAsync();
        }
    }

    private void OnSetMaLimitClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendMaLimitAsync();
        }
    }

    private void OnEmissionButtonClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
        {
            _ = viewModel.SendEmissionCommandAsync();
        }
    }

    /// <summary>
    /// Handles Enter key press in System Config TextBox inputs.
    /// When Enter is pressed, executes the SetCommand if it can execute (connection established and buttons enabled).
    /// Silently does nothing if the button is disabled (connection lost, polling active, etc).
    /// </summary>
    private void OnSystemConfigTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Return)
            return;

        if (sender is not System.Windows.Controls.TextBox textBox)
            return;

        // Get the data context (SystemConfigItem)
        if (textBox.DataContext is not SystemConfigItem configItem)
            return;

        // Check if the SetCommand can execute
        ICommand setCommand = configItem.SetCommand;
        if (!setCommand.CanExecute(null))
        {
            // Button is disabled (no connection, polling active, etc) - silently fail
            return;
        }

        // Update the binding to push InputValue back to the ViewModel
        BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
        binding?.UpdateSource();

        // Execute the command
        setCommand.Execute(null);
        e.Handled = true;
    }
}
