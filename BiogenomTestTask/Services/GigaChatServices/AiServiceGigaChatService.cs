using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BiogenomTestTask.Models.DTOs;
using BiogenomTestTask.Services.Interfaces;

namespace BiogenomTestTask.Services.GigaChatServices;

public class AiServiceGigaChatService(HttpClient httpClient, IGigaChatTokenProvider tokenProvider) : IAiService
{
    private const string Model = "GigaChat-2-Pro";

    private const string UploadFilePath = "files";
    private const string AnalyzeImagePath = "chat/completions";

    private const string FindItemsPrompt = "Найди главные объекты на изображении и верни ответ в виде JSON массива в нижнем регистре, на русском языкe";

    private const string AnalyzeMaterialsPromptTemplate = """
                                            У тебя есть следующий список с объектами, обнаруженными на изображении: 
                                            {0}
                                            Определи, из каких материалов состоят перечисленные объекты, 
                                            и верни результат строго в формате JSON-словаря и в нижнем регистре, на русском языке:
                                            Пример:
                                            {{
                                              "рабочий стол": "дерево",
                                              "инструменты": "металл",
                                              "станок": "металл",
                                              "станок": "пластик"
                                            }}
                                            """;

    public async Task<Guid> UploadImageAsStreamAsync(Stream imageStream, string fileName)
    {
        var accessToken = tokenProvider.GetToken() 
                          ?? throw new InvalidOperationException("Access token is not available");

        using var request = new HttpRequestMessage(HttpMethod.Post, UploadFilePath);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(imageStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetImageFormatFromUrl(fileName));
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent("general"), "purpose");

        request.Content = content;

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var uploadResponse = JsonSerializer.Deserialize<UploadFileResponse>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (uploadResponse?.Id is null)
            throw new InvalidOperationException("File upload failed: no ID returned");

        return Guid.Parse(uploadResponse.Id);
    }
    
    public async Task<string[]> AnalyzeImageItemsAsync(Guid imgId)
    {
        var response = await AnalyzeImageAsync(imgId, FindItemsPrompt);
        return ParseArrayResponse(response);
    }
    
    public async Task<Dictionary<string, string>> AnalyzeImageMaterialsAsync(Guid imgId, string[] detectedItems)
    {
        var itemsList = string.Join(Environment.NewLine, detectedItems.Select(x => $"- {x}"));
        var prompt = string.Format(AnalyzeMaterialsPromptTemplate, itemsList);

        var response = await AnalyzeImageAsync(imgId, prompt);
        return ParseDictionaryResponse(response, detectedItems);
    }
    
    private async Task<JsonDocument> AnalyzeImageAsync(Guid imgId, string prompt)
    {
        var accessToken = tokenProvider.GetToken() 
                          ?? throw new InvalidOperationException("Access token is not available");

        using var request = new HttpRequestMessage(HttpMethod.Post, AnalyzeImagePath);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var body = new
        {
            model = Model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt,
                    attachments = new[] { imgId.ToString() }
                }
            },
            temperature = 0.7
        };

        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
    
    private static string[] ParseArrayResponse(JsonDocument response)
    {
        var result = new List<string>();

        var choices = response.RootElement.GetProperty("choices");
        foreach (var contentString in choices.EnumerateArray()
                     .Select(choice => choice.GetProperty("message").GetProperty("content").GetString())
                     .Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            try
            {
                var items = JsonSerializer.Deserialize<string[]>(contentString);
                if (items != null) result.AddRange(items);
            }
            catch
            {
                // ignored
            }
        }

        return result.ToArray();
    }
    
    private static Dictionary<string, string> ParseDictionaryResponse(JsonDocument response, string[] detectedItems)
    {
        var dict = new Dictionary<string, string>();

        var choices = response.RootElement.GetProperty("choices");
        foreach (var contentString in choices.EnumerateArray()
                     .Select(choice => choice.GetProperty("message").GetProperty("content").GetString())
                     .Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            try
            {
                var parsedDict = JsonSerializer.Deserialize<Dictionary<string, string>>(contentString);
                if (parsedDict == null) continue;
                foreach (var kvp in parsedDict)
                    dict[kvp.Key] = kvp.Value;
            }
            catch
            {
                // ignored
            }
        }
        
        foreach (var item in detectedItems)
            dict.TryAdd(item, "не определено");

        return dict;
    }

    private static string GetImageFormatFromUrl(string fileName)
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