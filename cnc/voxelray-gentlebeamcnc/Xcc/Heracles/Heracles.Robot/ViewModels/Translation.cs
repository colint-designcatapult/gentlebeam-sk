using System;

namespace Heracles.Robot.ViewModels
{
    public class Translation
    {
        public float dXmm;
        public float dYmm;
        public float dZmm;
        public Translation(Action canExecuteChanged)
        {
            CanExecuteChanged = canExecuteChanged;
        }
        Action CanExecuteChanged { get; }
        public float DXmm
        {
            get => dXmm; set
            {
                dXmm = value;
                CanExecuteChanged?.Invoke();
            }
        }
        public float DYmm
        {
            get => dYmm; set
            {
                dYmm = value;
                CanExecuteChanged?.Invoke();
            }
        }
        public float DZmm
        {
            get => dZmm; set
            {
                dZmm = value;
                CanExecuteChanged?.Invoke();
            }
        }
    };
}