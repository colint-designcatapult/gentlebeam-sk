using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.UserControls
{
    public partial class RadiationHazardControl : UserControl
    {
        public RadiationHazardControl()
        {
            InitializeComponent();
        }


        CancellationTokenSource? AlarmWatchDogCts { get; set; }


        private void StartAlarmWatchDog()
        {
            AlarmWatchDogCts?.Cancel();

            AlarmWatchDogCts = new CancellationTokenSource();

            Task alarmWatchDog = Task.Run(async () =>
            {
                while (AlarmWatchDogCts.IsCancellationRequested == false)
                {
                    Console.Beep(1200, 500);
                    await Task.Delay(500);
                }
            }, AlarmWatchDogCts.Token);
        }

        private void StopAlarmWatchDog() => AlarmWatchDogCts?.Cancel();

   
        #region Dependency properties
        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                "IsActive",
                typeof(bool),
                typeof(RadiationHazardControl),
                new PropertyMetadata(
                    false,
                    (o, e) =>
                    {
                        if (o is RadiationHazardControl control)
                        {
                            if (e.NewValue is bool isActive)
                            {
                                if (isActive)
                                    control.StartAlarmWatchDog();
                                else
                                    control.StopAlarmWatchDog();
                            }
                        }
                    }));
        #endregion Dependency properties
    }
}
