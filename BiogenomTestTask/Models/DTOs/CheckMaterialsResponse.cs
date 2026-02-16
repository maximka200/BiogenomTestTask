using System.Text.Json.Serialization;

namespace BiogenomTestTask.Models.DTOs;

public class CheckMaterialsResponse
{
    [JsonPropertyName("detectedItems")]
    public string[] DetectedItems { get; set; }
    [JsonPropertyName("itemMaterials")]
    public Dictionary<string, string> ItemMaterials { get; set; }
}