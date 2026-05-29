using Xcc.Core.Enums;

namespace Xcc.Core.Domain.DataManagement.System
{

    public interface IReferenceField
    {
        MagnetometerType MagnetometerType { get; set; }
        public double Rf11 { get; set; }
        public double Rf21 { get; set; }
        public double Rf31 { get; set; }
    }

    public interface IReferenceFieldEntry : ISystemPresetEntry, IReferenceField
    {
        void SetValues(IReferenceField field);
    }
}
