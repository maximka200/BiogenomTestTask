using System.Text.Json.Serialization;

namespace BiogenomTestTask.Models.DTOs;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = default!;

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }
}