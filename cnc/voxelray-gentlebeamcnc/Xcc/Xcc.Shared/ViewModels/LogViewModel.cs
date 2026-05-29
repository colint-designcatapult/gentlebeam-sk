using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

using Xcc.Application.Models;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Shared.ViewModels
{
    public class LogViewModel : RegionViewModelBase
    {
        #region Properties
        public ILogReader LogReader { get; }

        private ObservableCollection<ILogRecord> _records = [];
        public ObservableCollection<ILogRecord> Records 
        { 
            get => _records;
            set
            {
                if (SetProperty(ref _records, value))
                {
                    LogRecordsViewSource.Source = _records;
                }
            }
        }

        private CollectionViewSource _logRecordsViewSource = new();
        public CollectionViewSource LogRecordsViewSource
        {
            get => _logRecordsViewSource;
            set => SetProperty(ref _logRecordsViewSource, value);
        }

        private DateTime _filterFromDate = DateTime.Today.AddDays(-1);
        public DateTime FilterFromDate
        {
            get => _filterFromDate;
            set
            {
                if (SetProperty(ref _filterFromDate, value))
                    LogRecordsViewSource.View?.Refresh();
            }
        }

        private DateTime _filterToDate = DateTime.Today.AddDays(1);
        public DateTime FilterToDate
        {
            get => _filterToDate;
            set
            {
                if (SetProperty(ref _filterToDate, value))
                    LogRecordsViewSource.View?.Refresh();
            }
        }

        private string _searchPhrase = string.Empty;
        public string SearchPhrase
        {
            get => _searchPhrase;
            set
            {
                if (SetProperty(ref _searchPhrase, value))
                    LogRecordsViewSource.View?.Refresh();
            }
        }

        private bool _isBusy;
        public bool IsBusy 
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
            }
        }
        #endregion


        #region Commands
        private DelegateCommand? _fetchLogCommand;
        public DelegateCommand FetchLogCommand
        {
            get => _fetchLogCommand ??= new DelegateCommand(
                async () =>
                {
                    IsBusy = true;

                    IList<ILogRecord> records = new List<ILogRecord>();

                    try
                    {
                        records = await LogReader.FetchAsync();
                    }
                    catch (Exception ex)
                    {
                        var report = new Xcc.Application.Models.Report(
                            Xcc.Core.Enums.ReportType.Error,
                            "Failed to load log",
                            ex.Message);

                        DialogParameters parameters = new() { { "Report", report } };

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            DialogService?.ShowDialog("ReportView", parameters, result => { });
                        });
                    }

                    if (records.Count > 0)
                        Records = new ObservableCollection<ILogRecord>(records);

                    IsBusy = false;
                },
                () => LogReader?.CanFetch() ?? false);
        }

        private DelegateCommand? _showDatePickerCommand;
        public DelegateCommand ShowDatePickerCommand => _showDatePickerCommand ??= new DelegateCommand(
            () =>
            {
                DialogParameters parameters = new()
                {
                    { "FromDate", FilterFromDate },
                    { "ToDate", FilterToDate }
                };

                DialogService?.ShowDialog("DatePickerDialogView", parameters, result =>
                {
                    if (result.Result == ButtonResult.OK)
                    {
                        if (result.Parameters.TryGetValue("FromDate", out DateTime fromDate))
                            FilterFromDate = fromDate;

                        if (result.Parameters.TryGetValue("ToDate", out DateTime toDate))
                            FilterToDate = toDate;
                    }
                });
            },
            canExecuteMethod: () => true);


        private DelegateCommand? _addFilterCommand;
        public DelegateCommand AddFilterCommand => _addFilterCommand ??= new DelegateCommand(
            () =>
            {
                DialogService.ShowDialog("CreateFilterDialogView", (result) =>
                {
                    if (result.Parameters.TryGetValue("Filter", out Filter filter))
                    {
                        if (Filters.Any(x => filter.Equals(x)))
                            return;

                        Filters.Add(filter);
                    }
                });

            },
            canExecuteMethod: () => true);

        public ObservableCollection<Filter> Filters { set; get; } = [];

        public DelegateCommand RemoveSearchPhraseCommand { get; }
        private void RemoveSearchPhrase()
        {
            SearchPhrase = string.Empty;
        }
        #endregion


        public LogViewModel(IRegionManager regionManager, ILogReader logReader, IEventAggregator eventAggregator, IDialogService dialogService) 
            : base(regionManager, dialogService: dialogService)
        {
            LogReader = logReader;
            RemoveSearchPhraseCommand = new DelegateCommand(RemoveSearchPhrase, () => true);
            eventAggregator.GetEvent<LogRecordAddedEvent>().Subscribe(OnLogRecordAdded);

            LogRecordsViewSource.Filter += (s, e) =>
            {
                if (e.Item is ILogRecord logRecord)
                    e.Accepted = FilterLogRecord(logRecord);
                else
                    e.Accepted = false;
            };

            LogRecordsViewSource.SortDescriptions.Add(new SortDescription("TimeStamp", ListSortDirection.Descending));

            Filters.CollectionChanged += (s, e) => LogRecordsViewSource.View?.Refresh();

            IsBusy = true;
            Task.Run(() =>
            {
                //Startup initialization
                ICollection<ILogRecord> records = [];

                try
                {
                    records = LogReader.Fetch();
                }
                catch(Exception ex)
                {
                    var report = new Xcc.Application.Models.Report(
                        Xcc.Core.Enums.ReportType.Error,
                        "Failed to load log",
                        ex.Message);

                    DialogParameters parameters = new() { { "Report", report } };

                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        DialogService?.ShowDialog("ReportView", parameters, result => { });
                    });
                }

                System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    Records = [.. records];
                    IsBusy = false;
                });
            });
        }

        #region Private methods
        private bool FilterLogRecord(ILogRecord logRecord)
        {
            bool searchPhraseFilterResult =
                logRecord.Message.Contains(SearchPhrase, StringComparison.CurrentCultureIgnoreCase) ||
                logRecord.Severity.ToString().Contains(SearchPhrase, StringComparison.CurrentCultureIgnoreCase);

            
            bool dateFilterResult = logRecord.TimeStamp >= FilterFromDate && logRecord.TimeStamp < FilterToDate;

            
            var severityFilters = Filters.Where(x => x.Field == typeof(LogRecordSeverity));
            bool severityFilterResult = !severityFilters.Any();
            foreach (var filter in severityFilters)
            {
                severityFilterResult |= (logRecord.Severity == (LogRecordSeverity)filter.Value);
            }

            
            var typeFilters = Filters.Where(x => x.Field == typeof(LogRecordType));
            bool typeFilterResult = !typeFilters.Any();
            foreach (var filter in typeFilters)
            {
                typeFilterResult |= (logRecord.Type == (LogRecordType)filter.Value);
            }

            return searchPhraseFilterResult && dateFilterResult && typeFilterResult && severityFilterResult;
        }

        private void OnLogRecordAdded(ILogRecord logRecord)
        {
            LogRecordsViewSource.Dispatcher.BeginInvoke(() =>
            {
                //using (LogRecordsViewSource.DeferRefresh()) // no need to use it for just one Add call
                //{
                    Records?.Add(logRecord);
                //}
            });
        }
        #endregion
    }
}
