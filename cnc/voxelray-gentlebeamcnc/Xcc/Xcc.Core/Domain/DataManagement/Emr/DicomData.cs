using System.IO;

namespace Xcc.Core.Domain.DataManagement.Emr
{
    public class DicomData
    {
        public required string Filename { get; set; }

        /// <summary>
        /// For next version in-memory implementation
        /// </summary>
        public Stream? FileStream { get; set; }
    }
}
