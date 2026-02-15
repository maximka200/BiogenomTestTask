using System.Text.Json.Serialization;

namespace BiogenomTestTask.Models.DTOs;

public class UploadFileResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}