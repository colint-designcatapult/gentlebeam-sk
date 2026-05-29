using System;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.System
{
    public class CorrectionMatrix : ICorrectionMatrix
    {
        public CorrectionMatrix(ICorrectionMatrix? entry = null)
        {
            entry?.CopyProperties(this);
        }
        public MagnetometerType MagnetometerType { get; set; }

        public double Cm11 { get; set; }
        public double Cm12 { get; set; }
        public double Cm13 { get; set; }
        public double Cm21 { get; set; }
        public double Cm22 { get; set; }
        public double Cm23 { get; set; }
    }

    public class CorrectionMatrixEntry : CorrectionMatrix, ICorrectionMatrixEntry
    {
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public long PresetConfigurationId { get; set; } = BaseEntry.NEW_ENTRY_ID;


        public CorrectionMatrixEntry()
        { }

        public CorrectionMatrixEntry(ICorrectionMatrixEntry correctionMatrixEntry)
        {
            correctionMatrixEntry.CopyProperties(this);
        }

        public void SetValues(ICorrectionMatrix matrix)
        {
            Cm11 = matrix.Cm11;
            Cm12 = matrix.Cm12;
            Cm13 = matrix.Cm13;
            Cm21 = matrix.Cm21;
            Cm22 = matrix.Cm22;
            Cm23 = matrix.Cm23;
        }
    }
}
