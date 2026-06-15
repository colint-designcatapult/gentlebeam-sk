using Empyrean.Common.Core.Domain.DataManagement.Common;
using System;
using System.ComponentModel.DataAnnotations;
using Xcc.Application.Common;
using Xcc.Application.Domain.System;
using Xcc.Application.Forms;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using NumericRangeAttribute = Empyrean.Common.Application.Common.NumericRangeAttribute;
using StringConstants = Empyrean.Common.Core.Constants.StringConstants;

namespace Xcc.Application.AppLayer.Physics;

public class CorrectionMatrixForm(ICorrectionMatrixEntry? matrix = null) : Form
{

    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax)]
    public object? Cm11
    {
        get => GetFieldValue(_cm11);
        set => SetFieldValue(value, _cm11);
    }

    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax)]
    public object? Cm12
    {
        get => GetFieldValue(_cm12);
        set => SetFieldValue(value, _cm12);
    }

    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax)]
    public object? Cm13
    {
        get => GetFieldValue(_cm13);
        set => SetFieldValue(value, _cm13);
    }


    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax)]
    public object? Cm21
    {
        get => GetFieldValue(_cm21);
        set => SetFieldValue(value, _cm21);
    }


    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax)]
    public object? Cm22
    {
        get => GetFieldValue(_cm22);
        set => SetFieldValue(value, _cm22);
    }

    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax)]
    public object? Cm23
    {
        get => GetFieldValue(_cm23);
        set => SetFieldValue(value, _cm23);
    }

    public long PresetConfigurationId { get; set; } = matrix?.PresetConfigurationId ?? BaseEntry.NewEntryId;
    public required MagnetometerType MagnetometerType { get; set; } = matrix?.MagnetometerType ?? 0;
    public long Id { get; set; } = matrix?.Id ?? BaseEntry.NewEntryId;


    public ICorrectionMatrixEntry ToCorrectionMatrixEntry(long presetId)
    {
        return new CorrectionMatrixEntry()
        {
            Id = this.Id,
            PresetConfigurationId = presetId,
            MagnetometerType = this.MagnetometerType,
            Cm11 = _cm11.ValidValue,
            Cm12 = _cm12.ValidValue,
            Cm13 = _cm13.ValidValue,
            Cm21 = _cm21.ValidValue,
            Cm22 = _cm22.ValidValue,
            Cm23 = _cm23.ValidValue,
        };
    }

    public void Set(ICorrectionMatrixEntry matrix)
    {
        Set(cm11: matrix.Cm11,
            cm12: matrix.Cm12,
            cm13: matrix.Cm13,
            cm21: matrix.Cm21,
            cm22: matrix.Cm22,
            cm23: matrix.Cm23);
    }

    public void Set(
        double? cm11 = null,
        double? cm12 = null,
        double? cm13 = null,
        double? cm21 = null,
        double? cm22 = null,
        double? cm23 = null)
    {
        Cm11 = cm11;
        Cm12 = cm12;
        Cm13 = cm13;
        Cm21 = cm21;
        Cm22 = cm22;
        Cm23 = cm23;

        AcceptChanges();
    }

    private FormField<double> _cm11 = new(matrix?.Cm11);
    private FormField<double> _cm12 = new(matrix?.Cm12);
    private FormField<double> _cm13 = new(matrix?.Cm13);
    private FormField<double> _cm21 = new(matrix?.Cm21);
    private FormField<double> _cm22 = new(matrix?.Cm22);
    private FormField<double> _cm23 = new(matrix?.Cm23);
}