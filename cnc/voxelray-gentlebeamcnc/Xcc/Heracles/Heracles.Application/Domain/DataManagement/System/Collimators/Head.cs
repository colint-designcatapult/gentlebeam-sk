using Empyrean.Common.Core.Domain.DataManagement.Common;
using System;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.System;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public class Head : BaseEntry, IHead
    {
        public DateTime CreationDate { get; set; }
        public string Serial { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public Head()
        { }

        public Head(IHead head)
        {
            head?.CopyProperties(this);
        }
    }
}