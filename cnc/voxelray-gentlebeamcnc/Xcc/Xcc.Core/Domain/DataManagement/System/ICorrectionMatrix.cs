using Xcc.Core.Enums;

namespace Xcc.Core.Domain.DataManagement.System
{
    public interface ICorrectionMatrix
    {
        MagnetometerType MagnetometerType { get; set; }
        public double Cm11 { get; set; }
        public double Cm12 { get; set; }
        public double Cm13 { get; set; }
        public double Cm21 { get; set; }
        public double Cm22 { get; set; }
        public double Cm23 { get; set; }
    }

    public interface ICorrectionMatrixEntry : ISystemPresetEntry, ICorrectionMatrix
    {
        void SetValues(ICorrectionMatrix matrix);
    }
}
