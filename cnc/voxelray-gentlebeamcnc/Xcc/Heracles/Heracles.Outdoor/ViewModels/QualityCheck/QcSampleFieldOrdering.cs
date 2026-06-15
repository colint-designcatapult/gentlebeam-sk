using Heracles.Application.Domain.DataManagement.System.QualityCheck;

namespace Heracles.External.ViewModels.QualityCheck;

public class QcSampleFieldOrdering : IComparer<IQcSampleFieldEntry>
{
    public int Compare(IQcSampleFieldEntry? x, IQcSampleFieldEntry? y)
    {
        if (x is null || y is null)
            throw new NullReferenceException($"{nameof(QcSampleFieldOrdering)}.{nameof(Compare)}: arguments cannot be null.");
            
        if (x.Energy.CompareTo(y.Energy) != 0)
        {
            return x.Energy.CompareTo(y.Energy);
        }
            
        return x.CollimatorType.CompareTo(y.CollimatorType);
    }
}