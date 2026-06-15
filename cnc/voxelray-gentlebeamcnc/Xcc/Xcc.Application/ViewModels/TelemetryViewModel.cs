using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using Prism.Events;
using Prism.Mvvm;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;

namespace Xcc.Application.ViewModels
{
    public class TelemetryViewModel : BindableBase
    {
        private readonly string TelemetryForegroundKey = "TelemetryLCDForeground";

        private readonly string TelemetryForegroundDisabledKey = "TelemetryLCDForegroundDisabled";

        private readonly List<(float, float, float)> KvFeedbackMap =
        [
            (0,   0f,    2f),
            (50,  48f,   51f),
            (70,  68f,   71f),
            (100, 98f,   101f)
        ];

        public TelemetryViewModel(IGCBDataStore gcbDataStore, IEventAggregator eventAggregator)
        {
            GCBDataStore = gcbDataStore;
            LoadColorResources();

            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(SystemTelemetryChanged, ThreadOption.UIThread);
        }

        #region Properties
        public IGCBDataStore GCBDataStore { get; }

        private ISystemTelemetry? _systemTelemetry;
        public ISystemTelemetry? SystemTelemetry { get => _systemTelemetry; set => SetProperty(ref _systemTelemetry, value); }

        private Brush TelemetryForegroundBrush { set; get; }
        private Brush TelemetryForegroundBrushDisabled { set; get; }

        private Brush? _telemetryForeground;
        public Brush? TelemetryForeground { get => _telemetryForeground; set => SetProperty(ref _telemetryForeground, value); }

        private float? _mappedKvFeedback;
        public float? MappedKvFeedback { get => _mappedKvFeedback; set => SetProperty(ref _mappedKvFeedback, value); }

        private int _controlBoardState = -1;
        public int ControlBoardState { get => _controlBoardState; set => SetProperty(ref _controlBoardState, value); }
        #endregion Properties


        #region Private methods
        private void SystemTelemetryChanged(ISystemTelemetry? systemTelemetry)
        {
            if (systemTelemetry is null)
            {
                TelemetryForeground = TelemetryForegroundBrushDisabled;
                ControlBoardState = -1; //mapped as No Comm
            }
            else
            {
                TelemetryForeground = TelemetryForegroundBrush;
                SystemTelemetry = systemTelemetry;
                ControlBoardState = (int)systemTelemetry.ControlBoardState;

                //kvFeedback called Energy across the application
                MappedKvFeedback = GetMappedKvFeedback(systemTelemetry.KvFeedback);
            }
        }

        private float GetMappedKvFeedback(float kvFeedback)
        {
            foreach (var range in KvFeedbackMap)
            {
                if (kvFeedback > range.Item2 && kvFeedback < range.Item3)
                    return range.Item1;
            }

            return kvFeedback;
        }

        [MemberNotNull(nameof(TelemetryForegroundBrushDisabled), nameof(TelemetryForegroundBrush))]
        private void LoadColorResources()
        {
            ResourceDictionary colorResources = new()
            {
                Source = new Uri("pack://application:,,,/Xcc.Styles;Component/Styles/BaseStyle.xaml", UriKind.RelativeOrAbsolute)
            };

            if (colorResources.Contains(TelemetryForegroundKey))
                TelemetryForegroundBrush = (Brush)colorResources[TelemetryForegroundKey];
            else
                throw new Exception($"Required resource is missing. Resource key {TelemetryForegroundKey}");


            if (colorResources.Contains(TelemetryForegroundDisabledKey))
                TelemetryForegroundBrushDisabled = (Brush)colorResources[TelemetryForegroundDisabledKey];
            else
                throw new Exception($"Required resource is missing. Resource key {TelemetryForegroundDisabledKey}");

            TelemetryForeground = TelemetryForegroundBrushDisabled;
        }
        #endregion Private methods
    }
}
