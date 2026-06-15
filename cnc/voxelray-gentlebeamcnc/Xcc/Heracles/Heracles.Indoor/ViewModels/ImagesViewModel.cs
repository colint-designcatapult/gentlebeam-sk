using Heracles.Application.Models;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Models;

using Prism.Commands;
using Prism.Regions;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using Xcc.Application.UI;
using Xcc.Application.UI.Mvvm;

namespace Heracles.Indoor.ViewModels;

public class ImagesViewModel : RegionViewModelBase
{
    #region Contructors
    public ImagesViewModel(IRegionManager regionManager, ISeriesModel seriesModel, IHeraclesMainSettings heraclesMainSettings, ITreatmentInfoStore treatmentInfoStore) : base(regionManager)
    {
        Images = [];
        SeriesModel = seriesModel;
        HeraclesMainSettings = heraclesMainSettings;
        TreatmentInfoStore = treatmentInfoStore;

        SeriesModel.PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(ISeriesModel.SeriesList))
            {
                PopulateImagesList();                    
            }
        };

        PopulateImagesList();
    }
    #endregion Contructors

        
    #region Properties

    private ISeriesModel SeriesModel { get; }
    private IHeraclesMainSettings HeraclesMainSettings { get; }
    public ITreatmentInfoStore TreatmentInfoStore { get; }

    private ObservableCollection<ImageEntry> _images;
    public ObservableCollection<ImageEntry> Images
    {
        get => _images;
        set
        {
            SetProperty(ref _images, value);
            ImagesViewSource.Source = _images;
            SortDescription = new SortDescription(nameof(ImageEntry.CreationDate), ListSortDirection.Descending);
        }
    }

    private CollectionViewSource _imagesViewSource = new();
    public CollectionViewSource ImagesViewSource
    {
        get => _imagesViewSource;
        set => SetProperty(ref _imagesViewSource, value);
    }

    private SortDescription _sortDescription;
    public SortDescription SortDescription
    {
        get => _sortDescription;
        set
        {
            var oldSortDescription = _sortDescription;
            if (SetProperty(ref _sortDescription, value))
            {
                ImagesViewSource.SortDescriptions.Remove(oldSortDescription);
                ImagesViewSource.SortDescriptions.Add(value);
            }
        }
    }
    #endregion Properties


    #region Commands
    private DelegateCommand<ImageEntry>? _goToViewerCommand;
    public DelegateCommand<ImageEntry> GoToViewerCommand => _goToViewerCommand ??= new DelegateCommand<ImageEntry>(
        series =>
        {
            var parameters = new NavigationParameters
            {
                { "Type", ImagingViewType.Viewer },
                { "AcquisitionId", series.NumberOfInstances },
            };

            RegionManager?.RequestNavigate(Regions.Main.ClinicalDataRegion, "ImagingView", parameters);
        });
    #endregion Commands


    #region Private methods
    private void PopulateImagesList()
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            Images.Clear();

            if (SeriesModel.SeriesList == null)
                return;

            foreach (var e in SeriesModel.SeriesList)
            {
                Images.Add(new ImageEntry(e, TreatmentInfoStore.Diagnosis.SiteName));
            }

            ImagesViewSource.Source = Images;
        });
    }
    #endregion Private methods


    #region Private methods
    protected override void OnExit()
    {
        RegionManager.RequestNavigate(Regions.MainRegion, "MainTabsView");
    }
    #endregion Private methods
}