using System;

namespace Heracles.Robot.ViewModels
{
    public class Rotation
    {
        private float rXDeg;
        private float rYDeg;
        private float rZDeg;

        public Rotation(Action canExecuteChanged) 
        { 
            CanExecuteChanged = canExecuteChanged;
        }
        Action CanExecuteChanged { get; }

        public float RXDeg 
        {
            get => rXDeg; set 
            { 
                rXDeg = value;
                CanExecuteChanged?.Invoke();
            }  
        }
        public float RYDeg
        {
            get => rYDeg; set
            {
                rYDeg = value;
                CanExecuteChanged?.Invoke();
            }
        }
        public float RZDeg
        {
            get => rZDeg; set
            {
                rZDeg = value;
                CanExecuteChanged?.Invoke();
            }
        }
    };
}