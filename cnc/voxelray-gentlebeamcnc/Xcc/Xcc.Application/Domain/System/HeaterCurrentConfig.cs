using System;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System;

namespace Xcc.Application.Domain.System
{
    public class HeaterCurrentConfig : BaseEntry, IHeaterCurrentConfig
    {
        public HeaterCurrentConfig()
        {            
        }
        public HeaterCurrentConfig(IHeaterCurrentConfig entry)
        {
            entry?.CopyProperties(this);
        }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public long PresetConfigurationId { get; set; }
        public double? HeaterCurrent { get; set; }
        public bool IsSet => HeaterCurrent is not null;
    }
}
