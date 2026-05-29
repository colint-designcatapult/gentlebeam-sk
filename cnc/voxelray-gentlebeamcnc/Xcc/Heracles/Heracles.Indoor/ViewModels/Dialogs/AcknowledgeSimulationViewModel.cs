using Heracles.Application.Common;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System;
using System.Linq;

using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels.Dialogs
{
    public class AcknowledgeSimulationViewModel(ILogRepository logger) : BindableBase, IDialogAware
    {
        private bool _lesionLocation;
        public bool LesionLocation
        {
            get => _lesionLocation;
            set
            {
                if (SetProperty(ref _lesionLocation, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private bool _lesionSkinCondition;
        public bool LesionSkinCondition
        {
            get => _lesionSkinCondition;
            set
            {
                if (SetProperty(ref _lesionSkinCondition, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private bool _noImagingOrders;
        public bool NoImagingOrders
        {
            get => _noImagingOrders;
            set
            {
                if (SetProperty(ref _noImagingOrders, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private bool _technicalIssue;
        public bool TechnicalIssue
        {
            get => _technicalIssue;
            set
            {
                if (SetProperty(ref _technicalIssue, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        public bool CanAccept =>
            LesionLocation ||
            LesionSkinCondition ||
            NoImagingOrders ||
            TechnicalIssue;

        private DelegateCommand _acceptCommand;
        public DelegateCommand AcceptCommand => _acceptCommand ??= new DelegateCommand(
            () =>
            {
                logger.LogAsync($"{StringConstants.EMR.AcknowledgeSimulationMessage}: {GetAcknowledgeString()}", LogRecordSeverity.Info, LogRecordType.User);

                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }).ObservesCanExecute(() => CanAccept);

        private string GetAcknowledgeString()
        {
            var items = new[]
            {
                LesionLocation ? "Lesion location" : null,
                LesionSkinCondition ? "Lesion skin condition" : null,
                NoImagingOrders ? "No imaging orders" : null,
                TechnicalIssue ? "Technical issue" : null
            };

            return string.Join("; ", items.Where(s => !string.IsNullOrWhiteSpace(s)));
        }


        #region IDialogAware
        public event Action<IDialogResult> RequestClose;

        public string Title => StringConstants.EMR.AcknowledgeSimulationUiMessage;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
        #endregion
    }

}
