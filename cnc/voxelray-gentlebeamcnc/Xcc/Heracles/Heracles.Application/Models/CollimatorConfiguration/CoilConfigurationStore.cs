using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Enums;
using Heracles.Core.Models.RDBMS;
using System.Collections.Generic;
using Xcc.Application.AppLayer.Service;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Models;

namespace Heracles.Application.Models.CollimatorConfiguration
{
    public interface ICoilConfigurationStore : IDirtyFlaggedBindableBase
    {
        ICollimatorConfiguration CollimatorConfiguration { get; set; }
        CoilConfigurationBase Configuration { get; }

        Task FetchCollimatorConfigurationAsync();
        Task SubmitCollimatorConfigurationAsync(IActionAuditService? audit = null);
    }

    /// <summary>
    /// The class is intended for dependency injection of magnetometer corrections
    /// </summary>
    public class CoilConfigurationStore : DirtyFlaggedBindableBase, ICoilConfigurationStore
    {
        public CoilConfigurationStore()
        {
        }

        public CoilConfigurationStore(
            ICoilConfigurationCommands coilConfigurationCommands,
            ICollimatorModel collimatorModel)
        {
            CoilConfigurationCommands = coilConfigurationCommands;
            CollimatorModel = collimatorModel;
        }

        private CoilConfigurationBase _configuration;
        private ICollimatorConfiguration _collimatorConfiguration;
        private readonly Dictionary<TreatmentFieldName, (double? X, double? Y, double? Focus)> _savedValues = new();

        public CoilConfigurationBase Configuration { 
            get => _configuration;
            private set
            {
                SetPropertyWithDirtyFlag(ref _configuration, value);
                IsModified = Configuration?.IsModified ?? false;
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
                    _savedValues.Clear();
                    Configuration = CoilConfigurationBase.CreateCoilConfiguration(
                        targetType: CollimatorConfiguration?.Type ?? TargetType.TargetType_None);
                }
            }
        }

        public ICoilConfigurationCommands CoilConfigurationCommands { get; }
        public ICollimatorModel CollimatorModel { get; }

        public async Task FetchCollimatorConfigurationAsync()
        {
            if (CollimatorConfiguration?.DefaultPreset == null)
                return;

            var coilConfigurationsFromDB = 
                await CoilConfigurationCommands.ReadListAsync(CollimatorConfiguration.DefaultPreset.Id);

            var currentConfiguration = Configuration.GetConfiguration();
            var coilConfigurationsToSetup = currentConfiguration.ToDictionary(x => x.FieldName, x => x);
            _savedValues.Clear();

            foreach(var coilConfiguration in coilConfigurationsFromDB)
            {
                CoilConfigurationForm? configurationToSetup = null;
                coilConfigurationsToSetup.TryGetValue(coilConfiguration.FieldName, out configurationToSetup);
                if (configurationToSetup != null)
                {
                    configurationToSetup.SetupFormValue(coilConfiguration);
                    _savedValues[coilConfiguration.FieldName] =
                        (coilConfiguration.XDeflectionCurrent, coilConfiguration.YDeflectionCurrent, coilConfiguration.FocusCurrent);
                }
            }
            // Re-evaluate entire configuration dirty flag
            Configuration.IsModified = currentConfiguration.Any(p => p.IsModified);
        }

        public async Task SubmitCollimatorConfigurationAsync(IActionAuditService? audit = null)
        {
            foreach (var coilConfiguration in Configuration.GetConfiguration())
            {
                ICoilConfigurationEntry entry = new CoilConfigurationEntry(coilConfiguration.GetValue());

                // Ensure proper preset id in the data:
                entry.PresetConfigurationId = CollimatorConfiguration.DefaultPreset.Id;
                var hasSavedValue = _savedValues.TryGetValue(entry.FieldName, out var saved);
                var fields = new List<string>();
                if (!hasSavedValue || entry.XDeflectionCurrent != saved.X) fields.Add(nameof(entry.XDeflectionCurrent));
                if (!hasSavedValue || entry.YDeflectionCurrent != saved.Y) fields.Add(nameof(entry.YDeflectionCurrent));
                if (!hasSavedValue || entry.FocusCurrent != saved.Focus) fields.Add(nameof(entry.FocusCurrent));

                var storedData = entry;
                bool isNew = BaseEntry.IsBlankEntry(entry);
                if (isNew)
                {
                    storedData = await CoilConfigurationCommands.CreateAsync(entry);                    
                }
                else if (fields.Count > 0)
                {
                    storedData = await CoilConfigurationCommands.UpdateAsync(null, entry);
                }
                if (isNew || fields.Count > 0)
                {
                    _savedValues[entry.FieldName] =
                        (storedData.XDeflectionCurrent, storedData.YDeflectionCurrent, storedData.FocusCurrent);
                    fields.Clear();
                    if (!hasSavedValue || storedData.XDeflectionCurrent != saved.X) fields.Add(nameof(entry.XDeflectionCurrent));
                    if (!hasSavedValue || storedData.YDeflectionCurrent != saved.Y) fields.Add(nameof(entry.YDeflectionCurrent));
                    if (!hasSavedValue || storedData.FocusCurrent != saved.Focus) fields.Add(nameof(entry.FocusCurrent));
                    if (isNew || fields.Count > 0)
                        audit?.RegisterAction("Configuration saved",
                            $"Entity=CoilConfiguration; Id={storedData.Id}; PresetId={storedData.PresetConfigurationId}; Fields={string.Join(",", fields)}");
                }
                // Copy stored state and reset dirty flag
                coilConfiguration.SetupFormValue(storedData);
            }

            // Everything is done, we may reset dirty flag for the entire configuration
            Configuration.AcceptChangesRecursive();
        }



        private void OnConfigurationIsModifiedChanged(object sender, bool isModified)
        {
            IsModified = isModified;
        }
    }
}
