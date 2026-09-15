using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;

using System;
using System.Collections.Generic;
using Xcc.Application.AppLayer.Service;
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
        Task SubmitMagnetometerParametersAsync(IActionAuditService? audit = null);
    }

    /// <summary>
    /// The class is intended for dependency injection of magnetometer corrections
    /// </summary>
    public class MagnetometerCorrectionsStore : DirtyFlaggedBindableBase, IMagnetometerCorrectionsStore
    {
        private MagnetometerCorrections _corrections;
        private ICollimatorConfiguration _collimatorConfiguration;
        private readonly Dictionary<MagnetometerType, ICorrectionMatrixEntry> _savedMatrices = new();
        private readonly Dictionary<MagnetometerType, IReferenceFieldEntry> _savedFields = new();

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
                    _savedMatrices.Clear();
                    _savedFields.Clear();
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

        public async Task SubmitMagnetometerParametersAsync(IActionAuditService? audit = null)
        {
            long presetId = CollimatorConfiguration.DefaultPreset.Id;

            await SubmitCorrectionMatrixAsync(Corrections.FrontMatrix, presetId, audit);
            await SubmitCorrectionMatrixAsync(Corrections.BackMatrix, presetId, audit);
            await SubmitReferenceFieldAsync(Corrections.FrontReferenceField, presetId, audit);
            await SubmitReferenceFieldAsync(Corrections.BackReferenceField, presetId, audit);

            Corrections.AcceptChanges();
        }

        private async Task SubmitCorrectionMatrixAsync(CorrectionMatrixForm matrix, long presetId, IActionAuditService? audit)
        {
            var entry = matrix.ToCorrectionMatrixEntry(presetId);
            _savedMatrices.TryGetValue(matrix.MagnetometerType, out var saved);
            var fields = ChangedMatrixFields(entry, saved);
            if (fields.Count == 0 && !BaseEntry.IsBlankId(matrix.Id))
            {
                matrix.AcceptChanges();
                return;
            }

            var stored = await SubmitEntryAsync(CorrectionMatrixCommands, entry);
            _savedMatrices[matrix.MagnetometerType] = stored;
            fields = ChangedMatrixFields(stored, saved);
            if (fields.Count > 0)
                audit?.RegisterAction("Configuration saved",
                    $"Entity=CorrectionMatrix; Id={stored.Id}; PresetId={stored.PresetConfigurationId}; Fields={string.Join(",", fields)}");
            matrix.Id = stored.Id;
            matrix.Set(stored);
        }

        private async Task SubmitReferenceFieldAsync(ReferenceFieldForm field, long presetId, IActionAuditService? audit)
        {
            var entry = field.ToReferenceFieldEntry(presetId);
            _savedFields.TryGetValue(field.MagnetometerType, out var saved);
            var fields = ChangedReferenceFields(entry, saved);
            if (fields.Count == 0 && !BaseEntry.IsBlankId(field.Id))
            {
                field.AcceptChanges();
                return;
            }

            var stored = await SubmitEntryAsync(ReferenceFieldCommands, entry);
            _savedFields[field.MagnetometerType] = stored;
            fields = ChangedReferenceFields(stored, saved);
            if (fields.Count > 0)
                audit?.RegisterAction("Configuration saved",
                    $"Entity=ReferenceField; Id={stored.Id}; PresetId={stored.PresetConfigurationId}; Fields={string.Join(",", fields)}");
            field.Id = stored.Id;
            field.Set(stored.Rf11, stored.Rf21, stored.Rf31);
        }

        private static List<string> ChangedMatrixFields(ICorrectionMatrixEntry entry, ICorrectionMatrixEntry? saved)
        {
            var fields = new List<string>();
            if (saved is null || entry.Cm11 != saved.Cm11) fields.Add(nameof(entry.Cm11));
            if (saved is null || entry.Cm12 != saved.Cm12) fields.Add(nameof(entry.Cm12));
            if (saved is null || entry.Cm13 != saved.Cm13) fields.Add(nameof(entry.Cm13));
            if (saved is null || entry.Cm21 != saved.Cm21) fields.Add(nameof(entry.Cm21));
            if (saved is null || entry.Cm22 != saved.Cm22) fields.Add(nameof(entry.Cm22));
            if (saved is null || entry.Cm23 != saved.Cm23) fields.Add(nameof(entry.Cm23));
            return fields;
        }

        private static List<string> ChangedReferenceFields(IReferenceFieldEntry entry, IReferenceFieldEntry? saved)
        {
            var fields = new List<string>();
            if (saved is null || entry.Rf11 != saved.Rf11) fields.Add(nameof(entry.Rf11));
            if (saved is null || entry.Rf21 != saved.Rf21) fields.Add(nameof(entry.Rf21));
            if (saved is null || entry.Rf31 != saved.Rf31) fields.Add(nameof(entry.Rf31));
            return fields;
        }

        private static Task<TEntry> SubmitEntryAsync<TEntry>(
            IAsyncСRUDCommands<TEntry> commands,
            TEntry data)
            where TEntry : class, ISystemPresetEntry
        {
            return BaseEntry.IsBlankEntry(data)
                ? commands.CreateAsync(data)
                : commands.UpdateAsync(null, data);
        }

        private async Task FetchCorrectionMatricesAsync()
        {
            var currentPreset = CollimatorConfiguration.DefaultPreset;
            if (currentPreset == null) 
                return;

            var matrices = await CorrectionMatrixCommands.ReadListAsync(currentPreset.Id);
            _savedMatrices.Clear();
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
                _savedMatrices[matrix.MagnetometerType] = matrix;
            }
        }

        private async Task FetchReferenceFieldsAsync()
        {
            var currentPreset = CollimatorConfiguration.DefaultPreset;
            var fields = await ReferenceFieldCommands.ReadListAsync(currentPreset.Id);
            _savedFields.Clear();
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
                _savedFields[field.MagnetometerType] = field;
            }
        }
    };
}