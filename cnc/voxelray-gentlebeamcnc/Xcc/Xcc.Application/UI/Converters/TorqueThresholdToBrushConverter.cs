using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Xcc.Application.UI.Converters
{
    public sealed class TorqueThresholdToBrushConverter : IValueConverter
    {
        public double DefaultAbsThreshold { get; set; } = 20.0; // Nm
        public Brush ExceededBrush { get; set; } = Brushes.Orange;
        public Brush DefaultBrush { get; set; } = Brushes.DarkGray;

        /// <summary>
        /// If true, trigger when |v| >= threshold. If false (default), trigger when |v| > threshold.
        /// </summary>
        public bool Inclusive { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(targetType == typeof(Brush) || typeof(Brush).IsAssignableFrom(targetType)))
                return DefaultBrush;

            if (!TryToDouble(value, out var v) || double.IsNaN(v) || double.IsInfinity(v))
                return DefaultBrush;

            double threshold = DefaultAbsThreshold;
            if (TryParseParameter(parameter, out var paramThreshold))
                threshold = Math.Abs(paramThreshold);

            var abs = Math.Abs(v);
            var exceeded = Inclusive ? abs >= threshold : abs > threshold;

            return exceeded ? ExceededBrush : DefaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static bool TryToDouble(object value, out double result)
        {
            switch (value)
            {
                case double d: result = d; return true;
                case float f: result = f; return true;
                case IConvertible conv:
                    try { result = conv.ToDouble(CultureInfo.InvariantCulture); return true; }
                    catch { break; }
            }
            result = 0;
            return false;
        }

        // Accepts "25", "25.0", or even "25|ignored" (first number wins)
        private static bool TryParseParameter(object parameter, out double threshold)
        {
            threshold = 0;
            var s = parameter?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return false;

            var parts = s.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(p => p.Trim());

            foreach (var p in parts)
            {
                if (double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out var t))
                {
                    threshold = t;
                    return true;
                }
            }
            return false;
        }
    }
}
