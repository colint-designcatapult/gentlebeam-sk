using System;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;

namespace Xcc.Application.Domain.QualityAssurance
{
    public class Intensity : BaseEntry, IIntensity
    {
        public long QcSampleFieldId { get; set; }
        public DateTime CreationDate { get; set; }
        public string DiodeName { get; set; }
        public double IntensityValue { get; set; }

        // Now we name diodes just by their indices
        public static string GetDiodeName(int index)
        {
            return index.ToString();
        }
        public int GetDiodeIndex()
        {
            return int.Parse(DiodeName);
        }
    }
}
