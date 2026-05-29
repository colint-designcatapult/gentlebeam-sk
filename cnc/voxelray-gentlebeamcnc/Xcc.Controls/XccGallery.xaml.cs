using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Xcc.Controls
{
    public class XccGallery : Control
    {
        #region Template Initialization
        private const string PathScrollViewer = "PART_GalleryScrollViewer";
        private const string PathGalleryItems = "PART_GalleryItems";
        private const string PathScrollLeftButton = "PART_ScrollLeftButton";
        private const string PathScrollRightButton = "PART_ScrollRightButton";
        private const string PathLeftGlow = "PART_LeftGlow";
        private const string PathRightGlow = "PART_RightGlow";

        private ScrollViewer _scrollViewer = null!;
        private ItemsControl _galleryItems = null!;
        private Button _scrollLeftButton = null!;
        private Button _scrollRightButton = null!;
        private Grid _leftGlow = null!;
        private Grid _rightGlow = null!;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scrollViewer = GetTemplateChild(PathScrollViewer) as ScrollViewer
                ?? throw new Exception($"{PathScrollViewer} must be specified in the template");

            _scrollLeftButton = GetTemplateChild(PathScrollLeftButton) as Button
                ?? throw new Exception($"{PathScrollLeftButton} must be specified in the template");

            _scrollRightButton = GetTemplateChild(PathScrollRightButton) as Button
                ?? throw new Exception($"{PathScrollRightButton} must be specified in the template");

            _leftGlow = GetTemplateChild(PathLeftGlow) as Grid
                ?? throw new Exception($"{PathLeftGlow} must be specified in the template");

            _rightGlow = GetTemplateChild(PathRightGlow) as Grid
                ?? throw new Exception($"{PathRightGlow} must be specified in the template");

            _galleryItems = GetTemplateChild(PathGalleryItems) as ItemsControl
                ?? throw new Exception($"{PathGalleryItems} must be specified in the template");

            if (_galleryItems != null)
            {
                var style = new Style(typeof(ContentPresenter));
                style.Setters.Add(new EventSetter(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnItemClicked)));
                _galleryItems.ItemContainerStyle = style;
            }

            _scrollViewer.ScrollChanged += _scrollViewer_ScrollChanged;
            _scrollViewer.PreviewMouseLeftButtonDown += _scrollViewer_MouseDown;
            _scrollViewer.PreviewMouseMove += _scrollViewer_MouseMove;
            _scrollViewer.PreviewMouseLeftButtonUp += _scrollViewer_MouseUp;

            _scrollLeftButton.Click += ScrollLeft_Click;
            _scrollLeftButton.PreviewMouseLeftButtonDown += ScrollLeft_Hold;
            _scrollLeftButton.PreviewMouseLeftButtonUp += ScrollRelease;

            _scrollRightButton.Click += ScrollRight_Click;
            _scrollRightButton.PreviewMouseLeftButtonDown += ScrollRight_Hold;
            _scrollRightButton.PreviewMouseLeftButtonUp += ScrollRelease;

            CompositionTarget.Rendering += OnRendering;
        }
        #endregion Template Initialization

        private void OnItemClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is ContentPresenter presenter)
            {
                var item = presenter.Content;
                SelectedItem = item;

                //if (_galleryItems != null)
                //    SelectedIndex = _galleryItems.Items.IndexOf(item);
            }
        }


        #region Fields
        private double _targetOffset;
        private bool _isAnimating;
        private int _scrollDirection = 0;

        private DateTime _mouseDownTime;
        private bool _suppressClick;

        #endregion


        #region Handlers
        private void ScrollLeft_Hold(object sender, MouseButtonEventArgs e)
        {
            StopInertia();

            _scrollDirection = -1;
            _mouseDownTime = DateTime.Now;
            _suppressClick = false;
        }

        private void ScrollRight_Hold(object sender, MouseButtonEventArgs e)
        {
            StopInertia();

            _scrollDirection = 1;
            _mouseDownTime = DateTime.Now;
            _suppressClick = false;
        }

        private void ScrollRelease(object sender, MouseButtonEventArgs e)
        {
            _scrollDirection = 0;

            if ((DateTime.Now - _mouseDownTime).TotalMilliseconds > 150)
                _suppressClick = true;
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            StopInertia();

            if (_suppressClick) return;
            StartSmoothScroll(-100);
        }

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            StopInertia();

            if (_suppressClick) return;
            StartSmoothScroll(100);
        }

        private void _scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double offset = _scrollViewer.HorizontalOffset;
            double maxOffset = _scrollViewer.ScrollableWidth;

            // Обновляем состояние кнопок
            bool canScrollLeft = offset > 0;
            bool canScrollRight = offset < maxOffset;

            _scrollLeftButton.IsEnabled = canScrollLeft;
            _scrollRightButton.IsEnabled = canScrollRight;

            // Сброс направления, если кнопка отключилась
            if (!canScrollLeft && _scrollDirection < 0)
                _scrollDirection = 0;

            if (!canScrollRight && _scrollDirection > 0)
                _scrollDirection = 0;

            if (offset <= 0)
                AnimateGlow(_leftGlow);

            if (offset >= maxOffset)
                AnimateGlow(_rightGlow);
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (_scrollDirection != 0)
            {
                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + _scrollDirection * 5);
                return;
            }

            if (_isAnimating)
            {
                double current = _scrollViewer.HorizontalOffset;
                double delta = (_targetOffset - current) * 0.2;

                if (Math.Abs(delta) < 0.5)
                {
                    _scrollViewer.ScrollToHorizontalOffset(_targetOffset);
                    _isAnimating = false;
                }
                else
                {
                    _scrollViewer.ScrollToHorizontalOffset(current + delta);
                }
            }

            ScrollWithInertia();
        }
        #endregion



        #region Swipe scroll with inertia
        private Point _dragStartPoint;
        private double _dragStartOffset;
        private bool _isDraggingScroll;
        private Point _lastDragPoint;
        private DateTime _lastDragTime;
        private double _inertiaVelocity;
        private bool _isInertiaActive;

        private void _scrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //_dragStartPoint = e.GetPosition(_scrollViewer);
            //_dragStartOffset = _scrollViewer.HorizontalOffset;
            //_isDraggingScroll = true;
            //_isInertiaActive = false;
            //_inertiaVelocity = 0;
            //_lastDragPoint = _dragStartPoint;
            //_lastDragTime = DateTime.Now;
            //_scrollViewer.CaptureMouse();
        }
        private void _scrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingScroll || e.LeftButton != MouseButtonState.Pressed) return;

            Point currentPoint = e.GetPosition(_scrollViewer);
            double deltaX = _dragStartPoint.X - currentPoint.X;

            double newOffset = _dragStartOffset + deltaX;
            newOffset = Math.Max(0, Math.Min(newOffset, _scrollViewer.ScrollableWidth));
            _scrollViewer.ScrollToHorizontalOffset(newOffset);

            // вычисляем скорость
            var now = DateTime.Now;
            var timeDelta = (now - _lastDragTime).TotalMilliseconds;
            if (timeDelta > 0)
            {
                var distance = _lastDragPoint.X - currentPoint.X;
                _inertiaVelocity = distance / timeDelta * 16; // нормализуем к 60 FPS
            }

            _lastDragPoint = currentPoint;
            _lastDragTime = now;
        }

        private void _scrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingScroll) return;

            _isDraggingScroll = false;
            _scrollViewer.ReleaseMouseCapture();

            if (Math.Abs(_inertiaVelocity) > 0.5)
            {
                _isInertiaActive = true;
            }
        }

        private void ScrollWithInertia()
        {
            if (_isInertiaActive)
            {
                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + _inertiaVelocity / 20);

                double velocity = Math.Abs(_inertiaVelocity);

                // Линейная зависимость: чем выше скорость, тем меньше затухание
                double damping = Math.Clamp(velocity / 50.0, 0.01, 0.15);

                // Преобразуем в коэффициент затухания: от 0.85 до 0.98
                double decayFactor = Math.Clamp(0.85 + damping, 0.85, 0.985);

                //Debug.WriteLine($"[Inertia] Velocity: {velocity:F2}, Damping: {damping:F2}, DecayFactor: {decayFactor:F2}");

                _inertiaVelocity *= decayFactor;

                if (Math.Abs(_inertiaVelocity) < 0.5)
                    StopInertia();
            }
        }

        private void StopInertia()
        {
            _isInertiaActive = false;
            _inertiaVelocity = 0;
        }
        #endregion Swipe scroll with inertia



        #region Helpers
        private void StartSmoothScroll(double offsetDelta)
        {
            _targetOffset = _scrollViewer.HorizontalOffset + offsetDelta;
            _targetOffset = Math.Max(0, Math.Min(_targetOffset, _scrollViewer.ScrollableWidth));
            _isAnimating = true;
        }
        #endregion



        private void AnimateGlow(Grid glow)
        {
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(0.0, TimeSpan.FromMilliseconds(400))
            {
                BeginTime = TimeSpan.FromMilliseconds(200)
            };

            var storyboard = new System.Windows.Media.Animation.Storyboard();
            storyboard.Children.Add(fadeIn);
            storyboard.Children.Add(fadeOut);

            System.Windows.Media.Animation.Storyboard.SetTarget(fadeIn, glow);
            System.Windows.Media.Animation.Storyboard.SetTarget(fadeOut, glow);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

            storyboard.Begin();
        }





        #region Dependency Properties
        public Style? ButtonStyle
        {
            get => (Style?)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register(
                nameof(ButtonStyle), 
                typeof(Style), 
                typeof(XccGallery), 
                new PropertyMetadata(null));


        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource), 
                typeof(IEnumerable), 
                typeof(XccGallery), 
                new PropertyMetadata(null));


        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(XccGallery), 
                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(XccGallery),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        //public static readonly DependencyProperty SelectedIndexProperty =
        //    DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(XccGallery),
        //        new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public int SelectedIndex
        //{
        //    get => (int)GetValue(SelectedIndexProperty);
        //    set => SetValue(SelectedIndexProperty, value);
        //}
        #endregion


        static XccGallery()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XccGallery), new FrameworkPropertyMetadata(typeof(XccGallery)));
        }
    }
}
