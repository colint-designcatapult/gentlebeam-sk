using Heracles.Core.Models.EMR;

using Prism.Commands;
using Prism.Regions;

using System;

using Xcc.Application.UI;
using Xcc.Application.UI.Mvvm;

namespace Heracles.Indoor.ViewModels
{
    [RegionMemberLifetime(KeepAlive = false)]
    public class ImageViewerViewModel : RegionViewModelBase
    {
        #region Constructors
        public ImageViewerViewModel() : base(null)
        {
        }

        public ImageViewerViewModel(IRegionManager regionManager) : base(regionManager)
        {
        }

        #endregion Constructors

        #region Properties
        private IPhoto? _currentPhoto;
        public IPhoto? CurrentPhoto
        {
            get => _currentPhoto;
            set
            {
                SetProperty(ref _currentPhoto, value);
            }
        }

        public string FormattedDate => CurrentPhoto?.CreationDate.ToString("MMM dd, yyyy HH:mm:ss") ?? "Unknown";

        public string LocationDisplay => CurrentPhoto?.Location ?? "Unknown";

        public string TypeDisplay => CurrentPhoto?.Type.ToString() ?? "Unknown";

        #endregion Properties

        #region Commands
        private DelegateCommand? _exitCommand;
        public DelegateCommand ExitCommand => _exitCommand ??= new DelegateCommand(
            () =>
            {
                RegionManager.RequestNavigate(Regions.Main.ClinicalDataRegion, "PlanView");
            });

        #endregion Commands

        #region Navigation
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                base.OnNavigatedTo(navigationContext);

                // Extract photo from navigation parameters
                if (navigationContext.Parameters.TryGetValue("photo", out object? photoObj))
                {
                    if (photoObj is IPhoto photo)
                    {
                        CurrentPhoto = photo;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion Navigation
    }
}
