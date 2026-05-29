using AxMSTSCLib;
using MSTSCLib;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KukaRdpHost
{
    public partial class MainWindow : Window
    {
        private AxMsRdpClient9NotSafeForScripting rdpClient;
        private DispatcherTimer spinnerTimer;
        private int currentDotIndex = 0;
        private readonly string[] dotNames = { "Dot0", "Dot1", "Dot2", "Dot3", "Dot4", "Dot5", "Dot6", "Dot7" };

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                StartSpinnerAnimation();
                string ip = GetTargetFromArgs();
                LaunchRdpSession(ip);
            };
        }

        private string GetTargetFromArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            return args.Length > 1 ? args[1] : "172.31.1.147";
        }

        private async Task LaunchRdpSession(string hostname)
        {
            rdpClient = new AxMsRdpClient9NotSafeForScripting();
            rdpClient.BeginInit();
            RdpHost.Child = rdpClient;
            rdpClient.EndInit();

            rdpClient.Server = hostname;
            rdpClient.UserName = "kukauser";
            ((IMsTscNonScriptable)rdpClient.GetOcx()).ClearTextPassword = "68kuka1secpw59";

            rdpClient.DesktopWidth = (int)this.ActualWidth;
            rdpClient.DesktopHeight = (int)this.ActualHeight;
            rdpClient.AdvancedSettings9.SmartSizing = true;
            rdpClient.AdvancedSettings9.EnableCredSspSupport = true;

            rdpClient.OnConnected += (sender, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    StopSpinnerAnimationAndShowRdp();
                });
            };

            try
            {
                await Task.Delay(800);
                rdpClient.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect: {ex.Message}", "RDP Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StartSpinnerAnimation()
        {
            spinnerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50) // 3x faster than 100ms
            };
            spinnerTimer.Tick += (s, e) =>
            {
                for (int i = 0; i < dotNames.Length; i++)
                {
                    var dot = (System.Windows.Shapes.Ellipse)this.FindName(dotNames[i]);
                    dot.Fill = i == currentDotIndex ? Brushes.White : Brushes.Gray;
                }

                currentDotIndex = (currentDotIndex + 1) % dotNames.Length;
            };
            spinnerTimer.Start();
        }

        private void StopSpinnerAnimationAndShowRdp()
        {
            spinnerTimer.Stop();
            DotSpinner.Visibility = Visibility.Collapsed;
            RdpHost.Visibility = Visibility.Visible;

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            RdpHost.BeginAnimation(UIElement.OpacityProperty, fade);
        }
    }
}
