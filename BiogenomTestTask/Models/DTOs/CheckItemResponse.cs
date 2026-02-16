using System.Text.Json.Serialization;

namespace BiogenomTestTask.Models.DTOs;

public class CheckItemResponse
{
    [JsonPropertyName("responseId")]
    public Guid ResponseId { get; set; }
    [JsonPropertyName("detectedItems")]
    public string[]? DetectedItems { get; set; }
}