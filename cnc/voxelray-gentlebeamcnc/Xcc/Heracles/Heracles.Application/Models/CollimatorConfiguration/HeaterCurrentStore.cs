using System.Linq;
using System.Threading.Tasks;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Xcc.Application.Domain.System;
using Xcc.Application.Helpers;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Models;

namespace Heracles.Application.Models.CollimatorConfiguration
{

    public interface IHeaterCurrentStore : IDirtyFlaggedBindableBase
    {
        ICollimatorConfiguration CollimatorConfiguration { get; set; }
        HeaterCurrentBindable HeaterCurrent { get; }

        Task FetchHeaterCurrentAsync();
        Task SubmitHeaterCurrentAsync();
    }

    /// <summary>
    /// The class is intended for dependency injection of magnetometer corrections
    /// </summary>
    public class HeaterCurrentStore : DirtyFlaggedBindableBase, IHeaterCurrentStore
    {
        private HeaterCurrentBindable _heaterCurrent;
        private ICollimatorConfiguration _collimatorConfiguration;

        #region Properties
        public HeaterCurrentBindable HeaterCurrent {
            get => _heaterCurrent;
            set
            {
                SetPropertyWithDirtyFlag(ref _heaterCurrent, value);
                IsModified = HeaterCurrent?.IsModified ?? false;
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
                    HeaterCurrent = (CollimatorConfiguration == null) ? null : new HeaterCurrentBindable();
                }
            }
        }
        public IHeaterCurrentConfigCommands HeaterCurrentCommands { get; }
        public ICollimatorModel CollimatorModel { get; }
        #endregion Properties

        public HeaterCurrentStore()
        {
        }

        public HeaterCurrentStore(
            IHeaterCurrentConfigCommands heaterCurrentCommands,
            ICollimatorModel collimatorModel)
        {
            HeaterCurrentCommands = heaterCurrentCommands;
            CollimatorModel = collimatorModel;
        }

        public async Task FetchHeaterCurrentAsync()
        {
            if (CollimatorConfiguration?.DefaultPreset == null)
                return;

            var allConfigs = await HeaterCurrentCommands.ReadListAsync(CollimatorConfiguration.DefaultPreset.Id);
            var configForPreset = allConfigs.LastOrDefault();

            HeaterCurrent = new HeaterCurrentBindable(configForPreset);
        }

        public async Task SubmitHeaterCurrentAsync()
        {
            if (!IsModified && !BaseEntry.IsBlankId(HeaterCurrent.Id))
                return;

            // Ensure proper preset id:
            HeaterCurrent.PresetConfigurationId = CollimatorConfiguration.DefaultPreset.Id;

            var storedData = (BaseEntry.IsBlankEntry(HeaterCurrent))
                ? await HeaterCurrentCommands.CreateAsync(HeaterCurrent)
                : await HeaterCurrentCommands.UpdateAsync(null, HeaterCurrent);

            HeaterCurrent = new HeaterCurrentBindable(storedData);
            AcceptChanges();
        }

        private void HeaterCurrent_IsModifiedChanged(object sender, bool isModified)
        {
            IsModified = isModified;
        }
    };
}
