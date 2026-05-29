using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using Xcc.Application.Models;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Enums;

namespace Xcc.Shared.ViewModels
{
    public class CreateFilterDialogViewModel : DialogViewModelBase
    {
        public List<Tuple<System.Type, string>> Fields { get; set; } = [Tuple.Create(typeof(LogRecordSeverity), "Status"), Tuple.Create(typeof(LogRecordType), "Type")];

        private Tuple<System.Type, string>? selectedField;
        public Tuple<System.Type, string>? SelectedField
        {
            get => selectedField;
            set
            {
                SetProperty(ref selectedField, value);
                CreateCommand?.RaiseCanExecuteChanged();
            }
        }

        private List<object>? values;
        public List<object>? Values
        {
            get => values;
            set => SetProperty(ref values, value);
        }

        private object? _selectedValue;
        public object? SelectedValue
        {
            get => _selectedValue;
            set
            {
                SetProperty(ref _selectedValue, value);
                CreateCommand?.RaiseCanExecuteChanged();
            }
        }

        private DelegateCommand? _createCommand;
        public DelegateCommand CreateCommand => _createCommand ??= new DelegateCommand(
            () =>
            {
                if(SelectedField is null || SelectedValue is null)
                    return;

                DialogParameters parameters = new()
                {
                    { "Filter", new Filter(SelectedField.Item1, SelectedField.Item2, SelectedValue) }
                };

                CloseDialog(parameters);
            },
            () => SelectedField is not null && SelectedValue is not null);

        private DelegateCommand? _cancelCommand;
        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(CancelDialog);
    }
}
