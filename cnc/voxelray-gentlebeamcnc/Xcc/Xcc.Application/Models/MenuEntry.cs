using System.Collections.Generic;
using Xcc.Core.Enums;

namespace Xcc.Application.Models
{
    public class MenuEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<AuthorizedUserLevel> AuthorizedLevels { get; set; } = new();
    }
}
