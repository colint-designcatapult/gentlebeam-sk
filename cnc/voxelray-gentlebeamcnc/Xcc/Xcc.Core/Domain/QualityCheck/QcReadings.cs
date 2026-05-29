using System;

namespace Xcc.Core.Domain.QualityCheck
{
    public class QcReadings
    {
        public QcReadings(float[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException("QCReadings error: no data");
            }
            Data = data;
        }
        public float[] Data { get; private set; }
    }
}
