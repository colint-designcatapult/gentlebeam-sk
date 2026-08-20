using System;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public struct VersionInfo
    {
        public string FirmwareVersion { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Level { get; set; }
        public uint FirmwareChecksum { get; set; }
        public FirmwareMode Mode { get; set; }
        public string HvpsFirmwareVersion { get; set; }
        public FirmwareMode HvpsMode { get; set; }

        public override string ToString()
        {
            return $"FirmwareVersion: {FirmwareVersion}{Environment.NewLine}FirmwareChecksum: {FirmwareChecksum}{Environment.NewLine}Mode: {Mode}{Environment.NewLine}HvpsFirmwareVersion: {HvpsFirmwareVersion}{Environment.NewLine}HvpsMode: {HvpsMode}";
        }
    }
}
