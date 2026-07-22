﻿using Prism.Events;
using Prism.Mvvm;
using System.Collections.Generic;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;

namespace Heracles.Application.UI.ViewModels
{
    public class DetailedTelemetryViewModel : BindableBase
    {
        private IReadOnlyList<FaultEntry> _faults;

        public DetailedTelemetryViewModel(IGCBDataStore gcbDataStore, IEventAggregator eventAggregator)
        {
            GCBDataStore = gcbDataStore;
            _faults = gcbDataStore.ActiveFaults;
            eventAggregator.GetEvent<FaultsChangedEvent>().Subscribe(OnFaultsChanged, ThreadOption.UIThread);
        }

        public IGCBDataStore GCBDataStore { get; }
        public IReadOnlyList<FaultEntry> Faults
        {
            get => _faults;
            private set => SetProperty(ref _faults, value);
        }

        private void OnFaultsChanged(IReadOnlyList<FaultEntry> faults)
        {
            Faults = faults;
        }
    }
}
