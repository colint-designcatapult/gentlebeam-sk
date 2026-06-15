using Xcc.Application.Events;
using Prism.Events;
using Prism.Mvvm;
using System.Windows;

namespace Heracles.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Heracles Extern";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private WindowState _winState;
        public WindowState WinState
        {
            get { return _winState; }
            set { SetProperty(ref _winState, value); }
        }

        private WindowStyle _winStyle;
        public WindowStyle WinStyle
        {
            get { return _winStyle; }
            set { SetProperty(ref _winStyle, value); }
        }

        private ResizeMode _winResizeMode;
        public ResizeMode WinResizeMode
        {
            get { return _winResizeMode; }
            set { SetProperty(ref _winResizeMode, value); }
        }

        private readonly IEventAggregator _eventAggregator;

        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<ApplicationWindowStateChangeEvent>()
                .Subscribe(OnWindowStateChanged);

            WinState = WindowState.Normal;
            WinStyle = WindowStyle.ToolWindow;
            WinResizeMode = ResizeMode.CanResizeWithGrip;
        }

        private void OnWindowStateChanged()
        {
            //WinState = (WinState == WindowState.Maximized) ?
            //            WindowState.Normal : WindowState.Maximized; WinState = (WinState == WindowState.Maximized) ?

            WinState = WindowState.Maximized;

            WinStyle = (WinStyle == WindowStyle.None) ?
                        WindowStyle.SingleBorderWindow : WindowStyle.None;

            //WinResizeMode = (WinResizeMode == ResizeMode.NoResize) ?
            //                 ResizeMode.CanResize : ResizeMode.NoResize;
        }
    }
}
