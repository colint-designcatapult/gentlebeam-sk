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
    public class AcknowledgePrescriptionViewModel(ILogRepository logger) : BindableBase, IDialogAware
    {
        public const string SelectedOptionsParameterKey = "SelectedOptions";

        private bool _depthChanged;
        public bool DepthChanged
        {
            get => _depthChanged;
            set
            {
                if (SetProperty(ref _depthChanged, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private bool _boost;
        public bool Boost
        {
            get => _boost;
            set
            {
                if (SetProperty(ref _boost, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private bool _doseDecay;
        public bool DoseDecay
        {
            get => _doseDecay;
            set
            {
                if (SetProperty(ref _doseDecay, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private bool _providerRequest;
        public bool ProviderRequest
        {
            get => _providerRequest;
            set
            {
                if (SetProperty(ref _providerRequest, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private bool _trogProtocol;
        public bool TrogProtocol
        {
            get => _trogProtocol;
            set
            {
                if (SetProperty(ref _trogProtocol, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private bool _other;
        public bool Other
        {
            get => _other;
            set
            {
                if (SetProperty(ref _other, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }

        private string _comment;
        public string Comment
        {
            get => _comment;
            set
            {
                if (SetProperty(ref _comment, value))
                    RaisePropertyChanged(nameof(CanAccept));
            }
        }


        private DelegateCommand _acceptCommand;
        public DelegateCommand AcceptCommand => _acceptCommand ??= new DelegateCommand
        (() =>
        {
            string message = $"{StringConstants.EMR.AcknowledgePrescriptionMessage}: {GetAcknowledgeString()}";

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, 
                new DialogParameters
                {
                    { SelectedOptionsParameterKey , message }
                })
            );
        }).ObservesCanExecute(() => CanAccept);

        private bool CanAccept =>
            DepthChanged ||
            Boost ||
            DoseDecay ||
            ProviderRequest ||
            TrogProtocol ||
            (Other && !string.IsNullOrWhiteSpace(Comment));


        private string GetAcknowledgeString()
        {
            var items = new[]
            {
                DepthChanged ? "Depth changed" : null,
                Boost ? "Boost" : null,
                DoseDecay ? "Dose decay" : null,
                ProviderRequest ? "Provider request" : null,
                TrogProtocol ? "TROG, Protocol" : null,
                Other ? $"Other ({Comment})" : null
            };

            var filtered = items.Where(s => !string.IsNullOrEmpty(s));

            return string.Join("; ", filtered);
        }



        #region IDialogAware
        public event Action<IDialogResult> RequestClose;

        public string Title => StringConstants.EMR.AcknowledgePrescriptionUiMessage;
        
        public bool CanCloseDialog() => true;
        
        public void OnDialogClosed() { }
        
        public void OnDialogOpened(IDialogParameters parameters) { }
        #endregion IDialogAware
    }

}
