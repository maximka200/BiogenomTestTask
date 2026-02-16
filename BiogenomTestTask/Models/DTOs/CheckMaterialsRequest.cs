using System.Text.Json.Serialization;

namespace BiogenomTestTask.Models.DTOs;

public class CheckMaterialsRequest
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("detectedItems")]
    public string[] DetectedItems { get; set; }
}