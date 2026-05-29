using System.Text.Json.Serialization;

namespace Heracles.Application.DeepColor.DataTypes;

public class Status
{
    [JsonPropertyName("patientName")]
    public required string PatientName { get; set; }
        
    [JsonPropertyName("sitesList")]
    public Site[]? Sites { get; set; }
        
    [JsonPropertyName("currentAcq")]
    public Acquisition? CurrentAcquisition { get; set; }

    [JsonPropertyName("currentSite")]
    public Site? CurrentSite { get; set; }
}