using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Xcc.Core.Domain.QualityCheck;

namespace Heracles.Application.Domain.DataManagement.System.QualityCheck
{
    public interface IQcSampleFieldEntry : IFieldEntryBase
    {
        QcReadings Intensities { get; set; }
        ICollimatorConfiguration Configuration { get; }
        string CollimatorTypeLabel { get; }
        TargetType CollimatorType { get; }
        double FilamentSetpoint { get; }
        bool IsDone { get; set; }
    }
}
