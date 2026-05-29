using System;
using Xcc.Core.Domain.DataManagement.Common;

namespace Xcc.Core.Domain.DataManagement.System
{
    public interface ISystemPresetEntry : IEntry
    {
        long PresetConfigurationId { get; set; }
        DateTime CreationDate { get; set; }

    }
}
