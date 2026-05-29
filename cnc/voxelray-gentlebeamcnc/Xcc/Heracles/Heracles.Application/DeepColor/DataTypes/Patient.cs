using System.Text.Json.Serialization;

namespace Heracles.Application.DeepColor.DataTypes;

public class Patient
{
    [JsonPropertyName("patientName")]
    public required string Name { get; set; }
}