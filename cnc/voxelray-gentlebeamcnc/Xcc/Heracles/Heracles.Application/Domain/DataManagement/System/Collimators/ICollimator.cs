using System;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public interface ICollimator : IEntry
    {
        DateTime CreationDate { get; set; }
        /// <summary>
        /// expected to be long 
        /// </summary>
        string Serial { get; set; }
        bool IsActive { get; set; }
        long CollimatorConfigurationId { get; set; } // todo: remove it from converter
        long HeadId { get; set; }

        ICollimatorConfiguration Configuration { get; set; } 
    }
}
