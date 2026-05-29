using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Xcc.Application.UI.Converters
{
    public class JointProximityToBrushConverter : IValueConverter
    {
        // Defaults if not supplied in XAML/parameter
        public double DefaultLimit { get; set; } = 120.0;      // degrees, symmetric ±limit
        public double TolerancePercent { get; set; } = 8.0;    // percent of 'limit' (0..100)
        public Brush NearBrush { get; set; } = Brushes.Yellow;
        public Brush DefaultBrush { get; set; } = Brushes.White;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(targetType == typeof(Brush) || targetType.IsSubclassOf(typeof(Brush))))
                return DefaultBrush;

            if (!TryToDouble(value, out var v) || double.IsNaN(v) || double.IsInfinity(v))
                return DefaultBrush;

            // Parse ConverterParameter as: "limit" or "limit,percent"
            // Examples: "120" or "120,5" (meaning 5% of 120)
            if (!TryParseParameter(parameter, out double limit, out double percent))
            {
                limit = Math.Abs(DefaultLimit);
                percent = Clamp01To100(TolerancePercent);
            }

            if (limit <= 0) return DefaultBrush;

            var tolPct = Clamp01To100(percent);
            var thresholdAbs = limit * (tolPct / 100.0);

            // "Near limit" when |v| is within 'thresholdAbs' of either bound (or beyond)
            if (Math.Abs(v) >= (limit - thresholdAbs))
                return NearBrush;

            return DefaultBrush;
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

        private static double Clamp01To100(double pct)
        {
            if (double.IsNaN(pct) || double.IsInfinity(pct)) return 0;
            if (pct < 0) return 0;
            if (pct > 100) return 100;
            return pct;
        }

        private bool TryParseParameter(object parameter, out double limit, out double percent)
        {
            limit = 0;
            percent = TolerancePercent;

            var s = parameter?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return false;

            var parts = s.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(p => p.Trim()).ToArray();

            if (parts.Length >= 1 &&
                double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lim))
            {
                limit = Math.Abs(lim); // symmetric ±limit

                if (parts.Length >= 2 &&
                    double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var pct))
                    percent = pct;

                return limit > 0;
            }
            return false;
        }
    }
}
