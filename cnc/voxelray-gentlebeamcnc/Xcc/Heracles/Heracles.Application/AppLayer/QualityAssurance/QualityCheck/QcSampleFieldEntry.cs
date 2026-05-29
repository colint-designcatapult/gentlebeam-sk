using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Helpers;
using Heracles.Application.Models;
using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Xcc.Application.Common;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;
using Xcc.Core.Domain.QualityCheck;

namespace Heracles.Application.AppLayer.QualityAssurance.QualityCheck
{
    public class QcSampleFieldEntry : FieldEntryBase, IQcSampleFieldEntry
    {
        private bool isDone = false;

        public QcReadings Intensities { get; set; }

        public string CollimatorTypeLabel { get; }
        public TargetType CollimatorType { get; }
        public ICollimatorConfiguration Configuration { get; }

        public double FilamentSetpoint { get; }

        public bool IsDone { get => isDone; set => SetProperty(ref isDone, value); }

        public QcSampleFieldEntry(ICollimatorConfiguration configuration, double filamentSetpoint)
        {
            Configuration = configuration;
            CollimatorType = configuration.Type;
            Energy = configuration.Energy;
            Current = Convert.ToSingle(CurrentCalculator.CalculateCurrent(configuration.Energy));
            FilamentSetpoint = filamentSetpoint;
            CollimatorTypeLabel = ExtractCollimatorName(configuration.Type);
        }

        [Obsolete]
        public QcSampleFieldEntry(TargetType collimatorType, double filamentSetpoint, ICollection<IIntensityEntry> intensities = null)
        {
            CollimatorType = collimatorType;
            FilamentSetpoint = filamentSetpoint;
            CollimatorTypeLabel = ExtractCollimatorName(collimatorType);

        }        

        private string ExtractCollimatorName(TargetType type)
        {
            string collimatorName = type.GetAttribute<DisplayAttribute>().Name;
            Match match = Regex.Match(collimatorName, @"\d+_\w+\b");
            if (match.Success)
                collimatorName = match.Value;

            return collimatorName;
        }

    }
}
