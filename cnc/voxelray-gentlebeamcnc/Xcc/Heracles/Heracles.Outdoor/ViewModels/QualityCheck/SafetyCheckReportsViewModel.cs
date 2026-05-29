using System.Windows.Data;
using Heracles.Application.Common;
using Heracles.External.Models;
using Prism.Mvvm;
using Xcc.Core.Services;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class SafetyCheckReportsViewModel : BindableBase
    {
        #region Contructors
        public SafetyCheckReportsViewModel(ISafetyCheckModel safetyCheckModel, IPopUpService popUpService)
        {
            SafetyCheckModel = safetyCheckModel;
            PopUpService = popUpService;
            SafetyCheckModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Models.ISafetyCheckModel.SafetyChecks))
                {
                    CollectionViewSource.Source = SafetyCheckModel.SafetyChecks;
                }
            };

            if (SafetyCheckModel.SafetyChecks == null || SafetyCheckModel.SafetyChecks.Count == 0)
            {
                _ = FetchSafetyChecksAsync();
            }
            else
            {
                CollectionViewSource.Source = SafetyCheckModel.SafetyChecks;
            }
        }

        public ISafetyCheckModel SafetyCheckModel { get; }
        public IPopUpService PopUpService { get; }

        private CollectionViewSource _collectionViewSourceViewSource = new();
        public CollectionViewSource CollectionViewSource
        {
            get => _collectionViewSourceViewSource;
            set => SetProperty(ref _collectionViewSourceViewSource, value);
        }
        #endregion Contructors

        #region Properties
        #endregion Properties

        #region Commands
        #endregion Commands

        #region Private methods
        private async Task FetchSafetyChecksAsync()
        {
            try
            {
                await SafetyCheckModel.FetchSafetyCheckListAsync();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    StringConstants.TreatmentConsole.SafetyCheck.ListLoadError,
                    ex);
            }
        }
        #endregion
    }
}
