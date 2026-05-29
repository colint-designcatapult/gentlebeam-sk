using Empyrean.Common.Core.Domain.DataManagement.Common;
using System;
using Xcc.Core.Common;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public class Collimator : BaseEntry, ICollimator
    {
        public DateTime CreationDate { get; set; }
        public string Serial { get; set; }
        public bool IsActive { get; set; } = true;
        public long CollimatorConfigurationId { get; set; } = NewEntryId;
        public long HeadId { get; set; }
        public ICollimatorConfiguration Configuration { get; set; }

        public Collimator() { }
        public Collimator(ICollimator collimator = null, ICollimatorConfiguration configuration = null)
        {
            collimator?.CopyProperties(this);
            Configuration = configuration;
            CollimatorConfigurationId = configuration?.Id ?? CollimatorConfigurationId;
        }
    }
}