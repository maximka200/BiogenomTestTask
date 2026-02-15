using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BiogenomTestTask.Models;
using BiogenomTestTask.Models.DTOs;
using BiogenomTestTask.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BiogenomTestTask.Services.GigaChatServices;

public class AiServiceGigaChatService(HttpClient httpClient, IGigaChatTokenProvider tokenProvider,
    IOptions<AiSettings> settings) : IAiService
{
    private string? accessToken = "";
    private readonly string baseUrl = settings.Value.BaseUrl;
    private const string Model = "GigaChat-2-Max";

    private const string UploadFilePath = "files";

    private const string AnalyzeImagePath = "chat/completions";

    private const string FindItemsPromt = "Найди главные обьекты на изображении и верни мне ответ в виде json массива";
    private const string AnalyseMaterialsPromt = "Определи из каких материалов состоят обьекты на изображении и верни мне ответ в виде json массива";
    public async Task<Guid> UploadImageAsStreamAsync(Stream imageStream, string fileName)
    {
        var accessToken = tokenProvider.GetToken()
                          ?? throw new InvalidOperationException("Access token is not available");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            UploadFilePath); 

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        
        var content = new MultipartFormDataContent();
        
        var fileContent = new StreamContent(imageStream);
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(GetImageFormatFromUrl(fileName));

        content.Add(fileContent, "file", fileName);
        
        content.Add(new StringContent("general"), "purpose");

        request.Content = content;

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        
        var uploadResponse = JsonSerializer.Deserialize<UploadFileResponse>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (uploadResponse?.Id is null)
            throw new InvalidOperationException("File upload failed: no ID returned");

        return Guid.Parse(uploadResponse.Id);
    }
    
    public async Task<string[]> AnalyzeImageItemsAsync(Guid imgId)
    {
        var accessToken = tokenProvider.GetToken()
                          ?? throw new InvalidOperationException("Access token is not available");

        using var request = new HttpRequestMessage(HttpMethod.Post, AnalyzeImagePath);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var body = new
        {
            model = Model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = FindItemsPromt,
                    attachments = new[] { imgId.ToString() }
                }
            },
            temperature = 0.7
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);

        var choices = doc.RootElement.GetProperty("choices");

        var result = new List<string>();

        foreach (var parsed in (from choice in choices.EnumerateArray() select choice
                     .GetProperty("message")
                     .GetProperty("content")
                     .GetString() into content where !string.IsNullOrWhiteSpace(content) select JsonSerializer.Deserialize<string[]>(content)).OfType<string[]>())
        {
            result.AddRange(parsed);
        }

        return result.ToArray();
    }

    public async Task<Dictionary<string, string>> AnalyzeImageMaterialsAsync(Guid imgId, string[] detectedItems)
    {
        throw new NotImplementedException();
    }
    
    private string GetImageFormatFromUrl(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            _ => throw new InvalidDataException("Unsupported image format.")
        };
    }
}
