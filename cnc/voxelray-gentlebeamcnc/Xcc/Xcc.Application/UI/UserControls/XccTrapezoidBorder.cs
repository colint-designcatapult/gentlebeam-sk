using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xcc.Application.UI.UserControls
{
    public class XccTrapezoidBorder : ContentControl
    {
        private Path? BorderPath { get; set; }

        public XccTrapezoidBorder()
        {
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (BorderPath is not null)
            {
                //check path line thickness and apply to trapezoid points to avoid blurring
                Thickness round = new(
                    (StrokeThickness.Left % 2) / 2,
                    (StrokeThickness.Top % 2) / 2,
                    (StrokeThickness.Right % 2) / 2,
                    (StrokeThickness.Bottom % 2) / 2);


                //draws a trapezoid
                PathFigure trapezoid = new()
                {
                    StartPoint = new Point(0 + round.Left, arrangeBounds.Height - round.Bottom),
                };

                LineSegment leftSideLine = new()
                {
                    Point = new Point(Bevel + round.Left, 0 + round.Top)
                };

                LineSegment topBaseLine = new()
                {
                    Point = new Point(arrangeBounds.Width - Bevel - round.Right, 0 + round.Top)
                };

                LineSegment rightSideLine = new()
                {
                    Point = new Point(arrangeBounds.Width - round.Right, arrangeBounds.Height - round.Bottom)
                };

                trapezoid.IsClosed = IsClosed;
                trapezoid.IsFilled = true;
                trapezoid.Segments = [leftSideLine, topBaseLine, rightSideLine];

                PathGeometry geometry = new();
                geometry.Figures.Add(trapezoid);

                BorderPath.Data = geometry;
            }

            return base.ArrangeOverride(arrangeBounds);
        }


        static XccTrapezoidBorder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XccTrapezoidBorder), new FrameworkPropertyMetadata(typeof(XccTrapezoidBorder)));
        }

        public override void OnApplyTemplate()
        { 
            base.OnApplyTemplate();
            BorderPath = this.Template.FindName("PART_BorderPath", this) as Path;
        }

        public static readonly DependencyProperty IsClosedProperty =
            DependencyProperty.Register(
                nameof(IsClosed),
                typeof(bool),
                typeof(XccTrapezoidBorder),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public bool IsClosed
        {
            get => (bool)GetValue(IsClosedProperty);
            set => SetValue(IsClosedProperty, value);
        }


        public static readonly DependencyProperty BevelProperty =
            DependencyProperty.Register(
                nameof(Bevel),
                typeof(int),
                typeof(XccTrapezoidBorder),
                new FrameworkPropertyMetadata(
                    8,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public int Bevel
        {
            get => (int)GetValue(BevelProperty);
            set => SetValue(BevelProperty, value);
        }


        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(
                nameof(Stroke),
                typeof(Brush),
                typeof(XccTrapezoidBorder),
                new FrameworkPropertyMetadata(
                    Brushes.Black,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }


        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(Thickness),
                typeof(XccTrapezoidBorder),
                new FrameworkPropertyMetadata(
                    new Thickness(1),
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public Thickness StrokeThickness
        {
            get => (Thickness)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
    }
}
