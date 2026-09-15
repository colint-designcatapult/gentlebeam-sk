using Heracles.Core.Models.EMR;
using Heracles.Indoor.Models.UseCases;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace Heracles.Indoor.ViewModels
{
    public class PhotoViewerModalViewModel : BindableBase, IDialogAware
    {
        private readonly PatientRecordReadAudit _readAudit;

        public PhotoViewerModalViewModel(PatientRecordReadAudit readAudit)
        {
            _readAudit = readAudit;
        }

        private IPhoto? _currentPhoto;
        public IPhoto? CurrentPhoto
        {
            get => _currentPhoto;
            set => SetProperty(ref _currentPhoto, value);
        }

        private DelegateCommand? _closeCommand;
        public DelegateCommand CloseCommand => _closeCommand ??= new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

        public string Title => "Photo Viewer";

        public event Action<IDialogResult>? RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            try
            {
                if (parameters.TryGetValue("photo", out IPhoto? photo))
                {
                    CurrentPhoto = photo;
                    if (CurrentPhoto is not null)
                        _readAudit.Record(_readAudit.CreateRequest(CurrentPhoto.DiagnosisId), "photo", CurrentPhoto.Id);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
