using System.Net.Http.Headers;
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

    public async Task<Guid> UploadImageAsStreamAsync(Stream imageStream, string fileName)
    {
        var accessToken = tokenProvider.GetToken()
                          ?? throw new InvalidOperationException("Access token is not available");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "files"); 

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


    public async Task<string[]> AnalyzeImageAsync(Stream imgStream, Guid imgId)
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
