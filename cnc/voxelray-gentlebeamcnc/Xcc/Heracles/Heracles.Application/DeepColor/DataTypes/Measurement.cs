using System.Text.Json.Serialization;

namespace Heracles.Application.DeepColor.DataTypes;

public class Measurement
{
    [JsonPropertyName("acqId")]
    public int AcquisitionId { get; set; }
        
    [JsonPropertyName("measurementValue")]
    public float Value { get; set; }
}