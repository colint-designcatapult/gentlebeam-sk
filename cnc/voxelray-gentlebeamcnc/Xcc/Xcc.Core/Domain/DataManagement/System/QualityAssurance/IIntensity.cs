using System;
using Xcc.Core.Domain.DataManagement.Common;

namespace Xcc.Core.Domain.DataManagement.System.QualityAssurance
{
    public interface IIntensity : IEntry
    {
        DateTime CreationDate { get; set; }
        string DiodeName { get; set; }
        double IntensityValue { get; set; }
        long QcSampleFieldId { get; set; }

        int GetDiodeIndex();
    }
}