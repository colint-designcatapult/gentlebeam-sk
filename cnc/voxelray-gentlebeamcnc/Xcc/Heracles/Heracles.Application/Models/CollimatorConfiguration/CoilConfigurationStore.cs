using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Enums;
using Heracles.Core.Models.RDBMS;
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
        Task SubmitCollimatorConfigurationAsync();
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

            foreach(var coilConfiguration in coilConfigurationsFromDB)
            {
                CoilConfigurationForm? configurationToSetup = null;
                coilConfigurationsToSetup.TryGetValue(coilConfiguration.FieldName, out configurationToSetup);
                if (configurationToSetup != null)
                {
                    configurationToSetup.SetupFormValue(coilConfiguration);
                }
            }
            // Re-evaluate entire configuration dirty flag
            Configuration.IsModified = currentConfiguration.Any(p => p.IsModified);
        }

        public async Task SubmitCollimatorConfigurationAsync()
        {
            foreach (var coilConfiguration in Configuration.GetConfiguration())
            {
                ICoilConfigurationEntry entry = new CoilConfigurationEntry(coilConfiguration.GetValue());

                // Ensure proper preset id in the data:
                entry.PresetConfigurationId = CollimatorConfiguration.DefaultPreset.Id;

                var storedData = entry;
                if (BaseEntry.IsBlankEntry(entry))
                {
                    storedData = await CoilConfigurationCommands.CreateAsync(entry);                    
                }
                else if (coilConfiguration.IsModified)
                {
                    storedData = await CoilConfigurationCommands.UpdateAsync(null, entry);
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
