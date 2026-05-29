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

public class ReferenceFieldForm(IReferenceFieldEntry? field = null) : Form
{
    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.ReferenceFieldsMin, PhysicsValueRange.ReferenceFieldsMax)]
    public object? Rf11
    {
        get => GetFieldValue(_rf11);
        set => SetFieldValue(value, _rf11);
    }

    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.ReferenceFieldsMin, PhysicsValueRange.ReferenceFieldsMax)]
    public object? Rf21
    {
        get => GetFieldValue(_rf21);
        set => SetFieldValue(value, _rf21);
    }

    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [Double]
    [NumericRange(PhysicsValueRange.ReferenceFieldsMin, PhysicsValueRange.ReferenceFieldsMax)]
    public object? Rf31
    {
        get => GetFieldValue(_rf31);
        set => SetFieldValue(value, _rf31);
    }

    public long PresetConfigurationId { get; set; } = field?.PresetConfigurationId ?? BaseEntry.NewEntryId;
    public required MagnetometerType MagnetometerType { get; set; } = field?.MagnetometerType ?? 0;
    public long Id { get; set; } = BaseEntry.NewEntryId;

    public IReferenceFieldEntry ToReferenceFieldEntry(long presetId)
    {
        return new ReferenceFieldEntry
        {
            Id = this.Id,
            PresetConfigurationId = presetId,
            MagnetometerType = this.MagnetometerType,
            Rf11 = _rf11.ValidValue,
            Rf21 = _rf21.ValidValue,
            Rf31 = _rf31.ValidValue,
        };
    }

    public void Set(
        double? rf11 = null,
        double? rf21 = null, 
        double? rf31 = null)
    {
        Rf11 = rf11;
        Rf21 = rf21;
        Rf31 = rf31;

        AcceptChanges();
    }

    private FormField<double> _rf11 = new(field?.Rf11);
    private FormField<double> _rf21 = new(field?.Rf21);
    private FormField<double> _rf31 = new(field?.Rf31);
}