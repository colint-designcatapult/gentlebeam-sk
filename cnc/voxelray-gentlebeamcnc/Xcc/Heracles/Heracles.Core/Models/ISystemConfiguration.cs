using Heracles.Core.Enums;

namespace Heracles.Core.Models
{
    public interface ISystemConfiguration : Xcc.Core.Models.ISystemConfiguration
    {
        public XRayHeadConfigurationMode XRayHeadConfiguration { get; set; }
    }
}
