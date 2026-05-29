using System;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public struct VersionInfo
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Level { get; set; }
        public int FirmwareChecksum { get; set; }
        public FirmwareMode Mode { get; set; }

        public override string ToString()
        {
            return $"Version: {Major}.{Minor}{Environment.NewLine}Level: {Level}{Environment.NewLine}FirmwareChecksum: {FirmwareChecksum}{Environment.NewLine}Mode: {Mode.ToString()}";
        }
    }
}
