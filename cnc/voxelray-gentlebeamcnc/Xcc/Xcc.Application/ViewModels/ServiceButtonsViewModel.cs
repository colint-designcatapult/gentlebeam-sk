using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace Xcc.Application.ViewModels;

public class ServiceButtonsViewModel(IDialogService dialogService) : BindableBase
{
    private DelegateCommand? _getFaultsCommand;
    public DelegateCommand GetFaultsCommand => _getFaultsCommand ??= new DelegateCommand(
        () =>
        {
            dialogService.ShowDialog("FaultsView");
        });

    private DelegateCommand? _showInterlocks;
    public DelegateCommand ShowInterlocksCommand => _showInterlocks ??= new DelegateCommand(
        () =>
        {
            dialogService.ShowDialog("InterlocksDialogView");
        });

    private DelegateCommand? _showDetailedTelemetryCommand;
    public DelegateCommand ShowDetailedTelemetryCommand => _showDetailedTelemetryCommand ??= new DelegateCommand(
        () =>
        {
            dialogService.ShowDialog("TelemetryDialogView");
        });
}