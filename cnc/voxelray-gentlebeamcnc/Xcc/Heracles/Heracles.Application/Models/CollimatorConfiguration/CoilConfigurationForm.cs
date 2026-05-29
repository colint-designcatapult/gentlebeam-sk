using System;
using System.ComponentModel.DataAnnotations;
using Empyrean.Common.Application.Common;
using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Core.Enums;
using Heracles.Core.Models.RDBMS;
using Xcc.Application.Forms;

namespace Heracles.Application.Models.CollimatorConfiguration;

/// <summary>
/// Form class for CoilConfiguration physics data element
/// </summary>
/// <param name="configurationEntry"></param>
public class CoilConfigurationForm(ICoilConfigurationEntry? configurationEntry = null) : Form
{
    public long Id { get => _id; set => SetProperty(ref _id, value); }
    public long PresetConfigurationId { get => _presetId; set => SetProperty(ref _presetId, value); }
    public TreatmentFieldName FieldName { get => _fieldName; set => SetPropertyWithDirtyFlag(ref _fieldName, value); }
    public string DisplayName { get => _displayName; set => SetPropertyWithDirtyFlag(ref _displayName, value); }
    public DateTime CreationDate { get; set; }

    [Required(ErrorMessage = Xcc.Core.Constants.StringConstants.Physics.Validation.XCoilCurrentRequired)]
    [NumericRange(Xcc.Core.Constants.PhysicsValueRange.XDeflectionCurrentMin, Xcc.Core.Constants.PhysicsValueRange.XDeflectionCurrentMax)]
    public object? XDeflectionCurrent
    {
        get => GetFieldValue(_xDeflectionCurrent);
        set => SetFieldValue(value, _xDeflectionCurrent);
    }

    [Required(ErrorMessage = Xcc.Core.Constants.StringConstants.Physics.Validation.YCoilCurrentRequired)]
    [NumericRange(Xcc.Core.Constants.PhysicsValueRange.YDeflectionCurrentMin, Xcc.Core.Constants.PhysicsValueRange.YDeflectionCurrentMax)]
    public object? YDeflectionCurrent
    {
        get => GetFieldValue(_yDeflectionCurrent);
        set => SetFieldValue(value, _yDeflectionCurrent);
    }

    [Required(ErrorMessage = Xcc.Core.Constants.StringConstants.Physics.Validation.FocusCurrentRequired)]
    [NumericRange(Xcc.Core.Constants.PhysicsValueRange.FocusCurrentMin, Xcc.Core.Constants.PhysicsValueRange.FocusCurrentMax)]
    public object? FocusCurrent
    {
        get => GetFieldValue(_focusCurrent);
        set => SetFieldValue(value, _focusCurrent);
    }

    public ICoilConfigurationEntry GetValue()
    {
        return new CoilConfigurationEntry()
        {
            Id = Id,
            XDeflectionCurrent = _xDeflectionCurrent.ValidValue,
            YDeflectionCurrent = _yDeflectionCurrent.ValidValue,
            FocusCurrent = _focusCurrent.ValidValue,
            FieldName = FieldName,
            PresetConfigurationId = PresetConfigurationId,
        };
    }

    public void ResetValues()
    {
        XDeflectionCurrent = null;
        YDeflectionCurrent = null;
        FocusCurrent = null;

        AcceptChanges();
    }

    public void SetupFormValue(ICoilConfigurationEntry coilConfiguration)
    {
        Id = coilConfiguration.Id;
        FieldName = coilConfiguration.FieldName;
        PresetConfigurationId = coilConfiguration.PresetConfigurationId;
        XDeflectionCurrent = coilConfiguration.XDeflectionCurrent;
        YDeflectionCurrent = coilConfiguration.YDeflectionCurrent;
        FocusCurrent = coilConfiguration.FocusCurrent;
        AcceptChanges();
    }

    private long _id = configurationEntry?.Id ?? BaseEntry.NewEntryId;
    private long _presetId = configurationEntry?.PresetConfigurationId ?? BaseEntry.NewEntryId;
    private TreatmentFieldName _fieldName = configurationEntry?.FieldName ?? 0;
    private string _displayName = "";
    private FormField<double> _xDeflectionCurrent = new(configurationEntry?.XDeflectionCurrent);
    private FormField<double> _yDeflectionCurrent = new(configurationEntry?.YDeflectionCurrent);
    private FormField<double> _focusCurrent = new(configurationEntry?.FocusCurrent);
}