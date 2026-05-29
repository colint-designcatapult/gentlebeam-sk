using System;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.System
{
    public class ReferenceField : IReferenceField
    {
        public ReferenceField(IReferenceField? entry = null)
        {
            entry?.CopyProperties(this);
        }
        public MagnetometerType MagnetometerType { get; set; }
        public double Rf11 { get; set; }
        public double Rf21 { get; set; }
        public double Rf31 { get; set; }
    }

    public class ReferenceFieldEntry : ReferenceField, IReferenceFieldEntry
    {
        public ReferenceFieldEntry()
        { }

        public ReferenceFieldEntry(IReferenceFieldEntry entry)
        {
            entry?.CopyProperties(this);
        }

        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public long PresetConfigurationId { get; set; } = BaseEntry.NEW_ENTRY_ID;

        public void SetValues(IReferenceField field)
        {
            Rf11 = field.Rf11;
            Rf21 = field.Rf21;
            Rf31 = field.Rf31;
        }
    }
}
