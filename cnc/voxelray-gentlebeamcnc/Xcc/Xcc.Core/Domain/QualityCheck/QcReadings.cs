using System;

namespace Xcc.Core.Domain.QualityCheck
{
    public class QcReadings
    {
        public QcReadings(float[] data, uint? sampleCount = null)
        {
            if (data is null)
            {
                throw new ArgumentNullException("QCReadings error: no data");
            }
            Data = data;
            SampleCount = sampleCount;
        }
        public float[] Data { get; private set; }
        public uint? SampleCount { get; }
    }
}
