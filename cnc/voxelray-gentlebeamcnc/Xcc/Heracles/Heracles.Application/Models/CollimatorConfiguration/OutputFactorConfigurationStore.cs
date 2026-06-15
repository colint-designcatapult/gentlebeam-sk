using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Models;

namespace Heracles.Application.Models.CollimatorConfiguration
{
    public interface IOutputFactorConfigurationStore : IDirtyFlaggedBindableBase
    {
        ICollimatorConfiguration CollimatorConfiguration { get; set; }
        OutputFactorConfigurationBase Configuration { get; }
        bool HasValue { get; set; }

        double? DoseRate { get; set; }

        Task FetchOutputFactorsAsync();
        Task SubmitOutputFactorsAsync();
    }

    /// <summary>
    /// The class is intended for dependency injection of output factor configuration
    /// </summary>
    public class OutputFactorConfigurationStore : DirtyFlaggedBindableBase, IOutputFactorConfigurationStore
    {
        #region Contructors
        public OutputFactorConfigurationStore()
        {
        }

        public OutputFactorConfigurationStore(
            ICollimatorModel collimatorModel,
            CollimatorService collimatorService,
            IOutputFactorCommands outputFactorCommands)
        {
            CollimatorModel = collimatorModel;
            CollimatorService = collimatorService;
            OutputFactorCommands = outputFactorCommands;
        }
        #endregion Contructors

        private OutputFactorConfigurationBase _configuration;
        private ICollimatorConfiguration _collimatorConfiguration;
        private double? _doseRate = null;
        private bool _hasValue;

        #region Properties
        public OutputFactorConfigurationBase Configuration
        { 
            get => _configuration;
            private set
            {
                if (_configuration != null)
                {
                    _configuration.IsModifiedChanged -= OnConfigurationIsModifiedChanged;
                }

                if (SetPropertyWithDirtyFlag(ref _configuration, value))
                {
                    if (_configuration != null)
                    {
                        _configuration.IsModifiedChanged += OnConfigurationIsModifiedChanged;
                    }
                }

                HasValue = _configuration is not null;
            }
        }
        
        protected override void OnSubPropertyModified(object sender, bool isModified)
        {
            IsModified = isModified;
        }

        public ICollimatorConfiguration CollimatorConfiguration
        {
            get => _collimatorConfiguration;
            set
            {
                if (SetProperty(ref _collimatorConfiguration, value))
                {
                    Configuration = OutputFactorConfigurationBase.Create(
                        targetType: CollimatorConfiguration?.Type ?? TargetType.TargetType_None);
                    DoseRate = value?.ReferencedDoseRate;

                    IsModified = Configuration?.IsModified ?? false;
                }
            }
        }

        [Required(ErrorMessage = "Dose Rate is required")]
        [DeniedValues<double>(0d, ErrorMessage = "Dose Rate cannot be 0")]
        [NumericRange(PhysicsValueRange.OutputDoseRateMin, PhysicsValueRange.OutputDoseRateMax)]
        public double? DoseRate 
        {
            get => _doseRate;
            set
            {
                SetPropertyWithDirtyFlag(ref _doseRate, value);
                Validate(value);
            }
        }

        public bool HasValue 
        { 
            get => _hasValue;
            set => SetProperty(ref _hasValue, value);
        }

        public ICollimatorModel CollimatorModel { get; }
        public CollimatorService CollimatorService { get; }
        public IOutputFactorCommands OutputFactorCommands { get; }
        #endregion Properties


        #region Public methods
        public async Task FetchOutputFactorsAsync()
        {
            if (CollimatorConfiguration?.DefaultPreset == null)
                return;

            var outputFactorsFromDB = await OutputFactorCommands.ReadListAsync(CollimatorConfiguration.DefaultPreset.Id);
            var outputFactorsToSetup = Configuration.OutputFactors.ToDictionary(x => x.FieldName, x => x);

            foreach(var factorDB in outputFactorsFromDB)
            {
                IOutputFactorEntry factorToSetup = null;
                outputFactorsToSetup.TryGetValue(factorDB.FieldName, out factorToSetup);
                if (factorToSetup != null)
                {
                    factorDB.CopyProperties(factorToSetup);
                    factorToSetup.AcceptChanges();
                }
            }
            // Re-evaluate entire configuration dirty flag
            Configuration.IsModified = Configuration.OutputFactors.Any(p => p.IsModified);
        }

        public async Task SubmitOutputFactorsAsync()
        {
            // Update DoseRate:
            if (CollimatorConfiguration.ReferencedDoseRate != DoseRate)
            {
                CollimatorConfiguration = await CollimatorService.UpdateCollimatorConfigurationDoseRateAsync(CollimatorConfiguration.Id, DoseRate.Value);
                DoseRate = CollimatorConfiguration.ReferencedDoseRate; // just in case if there's any difference due to conversion in the repository
            }

            foreach (var outputFactor in Configuration.OutputFactors)
            {
                // Ensure that we save the data into the right preset:
                outputFactor.PresetConfigurationId = CollimatorConfiguration.DefaultPreset.Id;

                IOutputFactor storedData = outputFactor;
                if (BaseEntry.IsBlankEntry(outputFactor))
                {
                    storedData = await OutputFactorCommands.CreateAsync(outputFactor);
                }
                else if (outputFactor.IsModified)
                {
                    storedData = await OutputFactorCommands.UpdateAsync(null, outputFactor);
                }
                // Copy stored state and reset dirty flag
                storedData.CopyProperties(outputFactor);
                outputFactor.AcceptChanges();
            }
            // Everything is done, we may reset dirty flag for the entire configuration
            Configuration.AcceptChanges();
            AcceptChanges();
        }

        public override void AcceptChanges()
        {
            IsModified = false;
        }
        #endregion Public methods


        #region Private methods
        private void OnConfigurationIsModifiedChanged(object sender, bool isModified)
        {
            IsModified = isModified;
        }

        #endregion Private methods
    }
}
