using System.Net.Http.Headers;
using System.Text.Json;
using BiogenomTestTask.Models;
using BiogenomTestTask.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BiogenomTestTask.Services;

public class AiServiceGigaChatService(HttpClient httpClient,
    IOptions<AiSettings> settings) : IAiService
{
    private readonly string apiKey = settings.Value.ApiKey;
    private readonly string baseUrl = settings.Value.BaseUrl;
    public async Task<string[]> AnalyzeImageAsync(Stream imgStream, Guid imgId)
    {
        throw new NotImplementedException();
    }

    public async Task<Guid> UploadImageAsStreamAsync(Stream imageStream, string fileName)
    {
        using var form = new MultipartFormDataContent();
        
        var imageContent = new StreamContent(imageStream);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); 
        
        form.Add(imageContent, "file", fileName);
        form.Add(new StringContent("general"), "purpose");
        
        var response = await httpClient.PostAsync("/files", form);

        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        
        var id = doc.RootElement.GetProperty("id").GetGuid();

        return id;
    }
}