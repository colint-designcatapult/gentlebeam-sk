using System.Text.Json.Serialization;

namespace Heracles.Application.DeepColor.DataTypes
{
    public class Acquisition
    {
        [JsonPropertyName("acqId")]
        public int Id { get; set; }
        
        [JsonPropertyName("acqName")]
        public required string Name { get; set; }

        [JsonPropertyName("acqDate")]
        public required long Date { get; set; }

        [JsonPropertyName("acqPreviewPath")]
        public string? PreviewPath { get; set; }
    }

    public class VersionInfo
    {
        [JsonPropertyName("version")]
        public required string Version { get; set; }
    }
}
