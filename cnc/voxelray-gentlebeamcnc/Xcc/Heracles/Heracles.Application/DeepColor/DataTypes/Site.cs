using System.Text.Json.Serialization;

namespace Heracles.Application.DeepColor.DataTypes;

public class Site
{
    [JsonPropertyName("siteId")]
    public required int Id { get; set; }

    [JsonPropertyName("siteName")]
    public required string Name { get; set; }
        
    [JsonPropertyName("acqs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Acquisition[]? Acquisitions { get; set; }
}