using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Xcc.Application.Helpers
{
    public sealed class ObservableTask : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Action? ContinueWith { get; set; }

        public ObservableTask(Task task, string? errorMessage = null, Action? continueWith = null) 
        {
            Task = task;
            ContinueWith = continueWith;
            ErrorMessage = errorMessage;

            if (task.IsCompleted)
            {
                ContinueWith?.Invoke();
                return;
            }

            _ = WatchTaskAsync(task);
        }

        public ObservableTask(Task task, string? errorMessage) : this(task, errorMessage, null)
        {
        }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task;
                ContinueWith?.Invoke();
            }
            catch
            {

            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));

            if (task.IsCanceled)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompletedWithoutFault)));
            }

            else if (task.IsFaulted)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exception)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InnerException)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExceptionMessage)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompletedWithoutFault)));
            }
        }

        public void SetErrorMessage(string errorMsg)
        {
            ErrorMessage = errorMsg;
        }

        public Task Task { get; }

        private string? errorMessage;
        public string? ErrorMessage { 
            get => errorMessage;
            private set
            {
                errorMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
        }
        public TaskStatus Status => Task.Status;
        public bool IsCompleted => Task.IsCompleted;
        public bool IsNotCompleted => !Task.IsCompleted;
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsFaulted => Task.IsFaulted;
        public bool IsCompletedWithoutFault => IsCanceled || IsSuccessfullyCompleted;
        public AggregateException? Exception => Task.Exception;
        public Exception? InnerException => Exception?.InnerException;
        public string? ExceptionMessage => InnerException?.Message;
    }
}
