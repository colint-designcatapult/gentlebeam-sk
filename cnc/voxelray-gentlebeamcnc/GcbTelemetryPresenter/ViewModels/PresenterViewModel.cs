using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using GcbTelemetryPresenter.Domain;
using GcbTelemetryPresenter.Infra;

namespace GcbTelemetryPresenter.ViewModels
{
    internal class PresenterViewModel : BindableBase
    {
        private const int JumpSize = 4; // size of element to jump when clicking jump buttons

        public PresenterViewModel()
        {
            throw new Exception("Design time constructor");
        }

        public PresenterViewModel(TelemetryRepository telemetryRepository)
        {
            TelemetryRepository = telemetryRepository;

            GetFilenames();
        }

        public TelemetryRepository TelemetryRepository { get; }

        private DataMessage? _selectedTelemetryMessage;
        public DataMessage? SelectedTelemetryMessage
        {
            get => _selectedTelemetryMessage;
            set
            {
                if (SetProperty(ref _selectedTelemetryMessage, value))
                {
                    if (_selectedTelemetryMessage is not null)
                    {
                        MessageIndex = TelemetryMessages.IndexOf(_selectedTelemetryMessage.Value);
                    }
                }
            }
        }

        private ObservableCollection<DataMessage> _telemetryMessages = new ObservableCollection<DataMessage>();
        public ObservableCollection<DataMessage> TelemetryMessages
        {
            get => _telemetryMessages;
            set
            {
                if (SetProperty(ref _telemetryMessages, value))
                {
                    ValidateCanExecuteMessageCommands();
                }
            }
        }

        private ObservableCollection<string> _filenames = new ObservableCollection<string>();
        public ObservableCollection<string> Filenames
        {
            get => _filenames;
            set
            {
                if (SetProperty(ref _filenames, value))
                {
                    SelectedFilename = null;
                    SelectedTelemetryMessage = null;

                    ValidateCanExecuteCommands();
                }
            }
        }

        private string? _selectedFilename;
        public string? SelectedFilename
        {
            get => _selectedFilename;

            set
            {
                if (SetProperty(ref _selectedFilename, value))
                {
                    if (_selectedFilename is not null)
                    {
                        if (_cancellationTokenSource.Token.CanBeCanceled)
                            _cancellationTokenSource?.Cancel();

                        _cancellationTokenSource = new CancellationTokenSource();
                        
                        MessageIndex = 0;
                        FileIndex = Filenames.IndexOf(_selectedFilename);
                        _ = ReadTelemetry(_selectedFilename);
                    }
                }
            }
        }

        private int _messageIndex;
        public int MessageIndex
        {
            get => _messageIndex;
            set
            {
                if (SetProperty(ref _messageIndex, value))
                {
                    ValidateCanExecuteMessageCommands();
                }
            }
        }

        private int _fileIndex;
        public int FileIndex
        {
            get => _fileIndex;
            set
            {
                if (SetProperty(ref _fileIndex, value))
                {
                    ValidateCanExecuteFileCommands();
                }
            }
        }
        
        #region Commands

        private DelegateCommand _prevFileCommand;
        public DelegateCommand PrevFileCommand => _prevFileCommand ??= new DelegateCommand(
            () =>
            {
                MessageIndex = 0;
                FileIndex--;
                SelectedFilename = Filenames[FileIndex];
            },
            canExecuteMethod: () => FileIndex > 0);


        private DelegateCommand _nextFileCommand;
        public DelegateCommand NextFileCommand => _nextFileCommand ??= new DelegateCommand(
            () =>
            {
                MessageIndex = 0;
                FileIndex++;
                SelectedFilename = Filenames[FileIndex];
            },
            canExecuteMethod: () => FileIndex < Filenames.Count - 1);


        private DelegateCommand _prevMessageCommand;
        public DelegateCommand PrevMessageCommand => _prevMessageCommand ??= new DelegateCommand(
            () =>
            {
                MessageIndex--;
                SelectedTelemetryMessage = TelemetryMessages[MessageIndex];
            },
            canExecuteMethod: () => MessageIndex > 0);


        private DelegateCommand _jumpPrevMessageCommand;
        public DelegateCommand JumpPrevMessageCommand => _jumpPrevMessageCommand ??= new DelegateCommand(
            () =>
            {
                if (MessageIndex < JumpSize)
                {
                    MessageIndex = 0;
                }
                else
                {
                    MessageIndex -= JumpSize;
                }
                SelectedTelemetryMessage = TelemetryMessages[MessageIndex];
            },
            canExecuteMethod: () => MessageIndex > 0);


        private DelegateCommand _nextMessageCommand;

        public DelegateCommand NextMessageCommand => _nextMessageCommand ??= new DelegateCommand(
            () =>
            {
                MessageIndex++;
                SelectedTelemetryMessage = TelemetryMessages[MessageIndex];
            },
            canExecuteMethod: () => MessageIndex < TelemetryMessages.Count - 1);

        
        private DelegateCommand _jumpNextMessageCommand;
        public DelegateCommand JumpNextMessageCommand => _jumpNextMessageCommand ??= new DelegateCommand(
            () =>
            {
                if (MessageIndex > TelemetryMessages.Count - JumpSize)
                {
                    MessageIndex = TelemetryMessages.Count - 1;
                }
                else
                {
                    MessageIndex += JumpSize;
                }
                SelectedTelemetryMessage = TelemetryMessages[MessageIndex];
            },
            canExecuteMethod: () => MessageIndex < TelemetryMessages.Count - 1);

        #endregion

        private void GetFilenames()
        {
            Filenames = new ObservableCollection<string>();
            try
            {
                Filenames = new(TelemetryRepository.GetTelemetryFilenames());

                if (Filenames.Count > 0)
                {
                    SelectedFilename = Filenames[FileIndex];
                }

                Debug.WriteLine($"Filenames.Count = {Filenames.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ReadTelemetry exception", MessageBoxButton.OK);
            }
            finally
            {
                ValidateCanExecuteCommands();
            }
        }

        private async Task ReadTelemetry(string filename)
        {
            TelemetryMessages = new ObservableCollection<DataMessage>();

            try
            {
                await foreach (var message in TelemetryRepository.ReadTelemetryAsync(filename, _cancellationTokenSource.Token))
                {
                    // skip send messages
                    if (message.Command == Command.Send)
                        continue;

                    TelemetryMessages.Add(message);
                }

                Debug.WriteLine($"TelemetryMessages.Count = {TelemetryMessages.Count}");

                if (TelemetryMessages.Count > 0)
                    SelectedTelemetryMessage = TelemetryMessages[MessageIndex];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ReadTelemetry exception", MessageBoxButton.OK);
            }
            finally
            {
                ValidateCanExecuteMessageCommands();
            }
        }

        private async Task ReadTelemetry()
        {
            TelemetryMessages = new ObservableCollection<DataMessage>();

            try
            {
                foreach (var filename in Filenames)
                {
                    await foreach (var message in TelemetryRepository.ReadTelemetryAsync(filename, _cancellationTokenSource.Token))
                    {
                        // skip send messages for now
                        if (message.Command == Command.Send)
                            continue;

                        TelemetryMessages.Add(message);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ReadTelemetry exception", MessageBoxButton.OK);
            }
            finally
            {
                ValidateCanExecuteMessageCommands();
            }
        }

        private void ValidateCanExecuteMessageCommands()
        {
            JumpPrevMessageCommand.RaiseCanExecuteChanged();
            PrevMessageCommand.RaiseCanExecuteChanged();
            NextMessageCommand.RaiseCanExecuteChanged();
            JumpNextMessageCommand.RaiseCanExecuteChanged();
        }

        private void ValidateCanExecuteFileCommands()
        {
            PrevFileCommand.RaiseCanExecuteChanged();
            NextFileCommand.RaiseCanExecuteChanged();
        }

        private void ValidateCanExecuteCommands()
        {
            ValidateCanExecuteFileCommands();
            ValidateCanExecuteMessageCommands();
        }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    }
}
