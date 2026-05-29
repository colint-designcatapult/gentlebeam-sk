using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

public class ButtonCornerRadiusConverter : MarkupExtension, IMultiValueConverter
{
    public CornerRadius CornerRadius { get; set; }

    // Позиция кнопки в пределах Root
    public HorizontalAlignment Horizontal { get; set; }
    public VerticalAlignment Vertical { get; set; }

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is [CornerRadius rootCorner, HorizontalAlignment rootHorizontalAlignment, VerticalAlignment rootVerticalAlignment, ..])
        {
            return GetAdjustedCornerRadius(rootCorner, rootHorizontalAlignment, rootVerticalAlignment);
        }

        return CornerRadius;
    }

    private CornerRadius GetAdjustedCornerRadius(CornerRadius root, HorizontalAlignment rootHorizontalAlignment, VerticalAlignment rootVerticalAlignment)
    {
        bool isAtTargetCorner = Horizontal == rootHorizontalAlignment && Vertical == rootVerticalAlignment;

        return new CornerRadius(
            topLeft: UseAdjustedCorner(CornerRadius.TopLeft, root.TopLeft, isCorner: Horizontal == HorizontalAlignment.Left && Vertical == VerticalAlignment.Top, isAtTargetCorner),
            topRight: UseAdjustedCorner(CornerRadius.TopRight, root.TopRight, isCorner: Horizontal == HorizontalAlignment.Right && Vertical == VerticalAlignment.Top, isAtTargetCorner),
            bottomRight: UseAdjustedCorner(CornerRadius.BottomRight, root.BottomRight, isCorner: Horizontal == HorizontalAlignment.Right && Vertical == VerticalAlignment.Bottom, isAtTargetCorner),
            bottomLeft: UseAdjustedCorner(CornerRadius.BottomLeft, root.BottomLeft, isCorner: Horizontal == HorizontalAlignment.Left && Vertical == VerticalAlignment.Bottom, isAtTargetCorner)
        );
    }

    private double UseAdjustedCorner(double defaultValue, double rootValue, bool isCorner, bool isAtTargetCorner)
    {
        if (!isCorner)
            return defaultValue;

        return isAtTargetCorner ? 0 : Math.Max(rootValue - 1, 0);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(ButtonCornerRadiusConverter)}: ConvertBack is not supported for this converter.");

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}