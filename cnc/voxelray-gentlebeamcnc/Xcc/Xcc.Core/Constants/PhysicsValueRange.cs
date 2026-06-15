namespace Xcc.Core.Constants
{
    public class PhysicsValueRange
    {
        public const double XDeflectionCurrentMin = -1500; // mA 
        public const double XDeflectionCurrentMax = 1500;  // mA

        public const double YDeflectionCurrentMin = -1500; // mA
        public const double YDeflectionCurrentMax = 1500;  // mA

        public const double FocusCurrentMin = 0;     // mA
        public const double FocusCurrentMax = 3000;  // mA

        public const double HeaterCurrentMin = 1000; // mA
        public const double HeaterCurrentMax = 4000; // mA

        public const double OutputFactorMin = 0.8d;  
        public const double OutputFactorMax = 1.2d;

        public const double OutputDoseRateMin = 0d;    // cGy/min
        public const double OutputDoseRateMax = 1200d; // cGy/min

        // Defined in M2SG-267:
        public const double CorrectionMatrixMin = -5d; // mA/muT
        public const double CorrectionMatrixMax = 5d; // mA/muT

        public const double ReferenceFieldsMin = -200d; // muT
        public const double ReferenceFieldsMax = 200d; // muT
    }
}
