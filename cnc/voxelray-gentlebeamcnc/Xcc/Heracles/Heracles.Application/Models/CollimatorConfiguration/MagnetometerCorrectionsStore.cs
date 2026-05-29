using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;

using System;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.AppLayer.Physics;
using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Models;

namespace Heracles.Application.Models.CollimatorConfiguration
{
    public interface IMagnetometerCorrectionsStore : IDirtyFlaggedBindableBase
    {
        ICollimatorConfiguration CollimatorConfiguration { get; set; }
        MagnetometerCorrections Corrections { get; }

        Task FetchMagnetometerParametersAsync();
        Task SubmitMagnetometerParametersAsync();
    }

    /// <summary>
    /// The class is intended for dependency injection of magnetometer corrections
    /// </summary>
    public class MagnetometerCorrectionsStore : DirtyFlaggedBindableBase, IMagnetometerCorrectionsStore
    {
        private MagnetometerCorrections _corrections;
        private ICollimatorConfiguration _collimatorConfiguration;

        #region Properties
        public MagnetometerCorrections Corrections { 
            get => _corrections;
            private set
            {
                SetPropertyWithDirtyFlag(ref _corrections, value);
                IsModified = Corrections?.IsModified ?? false;
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
                    Corrections = (CollimatorConfiguration == null) ? null : new MagnetometerCorrections();
                }
            }
        }

        public ICorrectionMatrixCommands CorrectionMatrixCommands { get; }
        public IReferenceFieldCommands ReferenceFieldCommands { get; }
        public ICollimatorModel CollimatorModel { get; }
        #endregion Properties

        public MagnetometerCorrectionsStore()
        {
        }

        public MagnetometerCorrectionsStore(
            ICorrectionMatrixCommands correctionMatrixCommands,
            IReferenceFieldCommands referenceFieldCommands,
            ICollimatorModel collimatorModel)
        {
            CorrectionMatrixCommands = correctionMatrixCommands;
            ReferenceFieldCommands = referenceFieldCommands;
            CollimatorModel = collimatorModel;
        }

        public async Task FetchMagnetometerParametersAsync()
        {
            if (CollimatorConfiguration?.DefaultPreset == null)
                return;

            await FetchCorrectionMatricesAsync();
            await FetchReferenceFieldsAsync();

            Corrections.AcceptChanges();
        }

        public async Task SubmitMagnetometerParametersAsync()
        {
            long presetId = CollimatorConfiguration.DefaultPreset.Id;

            await SubmitCorrectionMatrixAsync(Corrections.FrontMatrix, presetId);
            await SubmitCorrectionMatrixAsync(Corrections.BackMatrix, presetId);
            await SubmitReferenceFieldAsync(Corrections.FrontReferenceField, presetId);
            await SubmitReferenceFieldAsync(Corrections.BackReferenceField, presetId);

            Corrections.AcceptChanges();
        }

        private async Task SubmitCorrectionMatrixAsync(CorrectionMatrixForm matrix, long presetId)
        {
            if (!matrix.IsModified && !BaseEntry.IsBlankId(matrix.Id))
                return;
            
            await SubmitEntryAsync(CorrectionMatrixCommands, matrix.ToCorrectionMatrixEntry(presetId));
            matrix.AcceptChanges();
        }

        private async Task SubmitReferenceFieldAsync(ReferenceFieldForm field, long presetId)
        {
            if (!field.IsModified && !BaseEntry.IsBlankId(field.Id))
                return;

            await SubmitEntryAsync(ReferenceFieldCommands, field.ToReferenceFieldEntry(presetId));
            field.AcceptChanges();
        }

        private static async Task SubmitEntryAsync<TEntry, TEntrySubtype>(
            IAsyncСRUDCommands<TEntry> commands,            
            TEntrySubtype data)
            where TEntry : class, ISystemPresetEntry
            where TEntrySubtype : class, TEntry
        {
            TEntry storedData = data;
            if (BaseEntry.IsBlankEntry(data))
            {
                storedData = await commands.CreateAsync(data);
            }
            else
            {
                storedData = await commands.UpdateAsync(null, data);
            }
            
            storedData.CopyProperties(data);
        }

        private async Task FetchCorrectionMatricesAsync()
        {
            var currentPreset = CollimatorConfiguration.DefaultPreset;
            if (currentPreset == null) 
                return;

            var matrices = await CorrectionMatrixCommands.ReadListAsync(currentPreset.Id);
            foreach (var matrix in matrices)
            {
                switch (matrix.MagnetometerType)
                {
                    case MagnetometerType.Front:
                        matrix.CopyProperties(Corrections.FrontMatrix);
                        Corrections.FrontMatrix.Set(matrix);
                        break;

                    case MagnetometerType.Back:
                        matrix.CopyProperties(Corrections.BackMatrix);
                        Corrections.BackMatrix.Set(matrix);
                        break;

                    default:
                        throw new InvalidOperationException($"Wrong input magnetometer type {matrix.MagnetometerType}");
                }
            }
        }

        private async Task FetchReferenceFieldsAsync()
        {
            var currentPreset = CollimatorConfiguration.DefaultPreset;
            var fields = await ReferenceFieldCommands.ReadListAsync(currentPreset.Id);
            foreach (var field in fields)
            {
                switch (field.MagnetometerType)
                {
                    case MagnetometerType.Front:
                        field.CopyProperties(Corrections.FrontReferenceField);
                        Corrections.FrontReferenceField.Set(field.Rf11, field.Rf21, field.Rf31);
                        break;

                    case MagnetometerType.Back:
                        field.CopyProperties(Corrections.BackReferenceField);
                        Corrections.BackReferenceField.Set(field.Rf11, field.Rf21, field.Rf31);
                        break;

                    default:
                        throw new InvalidOperationException($"Wrong input magnetometer type {field.MagnetometerType}");

                }
            }
        }
    };
}