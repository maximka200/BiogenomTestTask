using BiogenomTestTask.Controllers.Interfaces;
using BiogenomTestTask.Models.DTOs;
using BiogenomTestTask.Repository;
using BiogenomTestTask.Services.Interfaces;

namespace BiogenomTestTask.Services;

public class ImageAnalysisService(IAiService aiService, AppDbContext dbContext, HttpClient httpClient) : IImageAnalysisService
{
    public async Task<CheckItemResponse> AnaliseItemsAsync(string link)
    {
        var imageStream = await DownloadImageAsStreamAsync(link);
        
        var imgId = await aiService.UploadImageAsStreamAsync(imageStream, GetFileNameFromUrl(link));
        
        var detectedItems = await aiService.AnalyzeImageItemsAsync(imgId);

        return await dbContext.CreateImageRequestAsync(
            link,
            imgId,
            detectedItems);
    }

    public async Task<CheckMaterialsResponse> AnaliseMaterialsAsync(CheckMaterialsRequest request)
    {
        var imgId = await dbContext.GetImgIdByRequestIdAsync(request.Id);
        if (imgId is null)
            throw new InvalidDataException("Request ID not found.");
        var detectedMaterials = await aiService.AnalyzeImageMaterialsAsync(imgId.Value, request.DetectedItems);
        return await dbContext.CreateMaterialsResponseAsync(
            request.Id,
            detectedMaterials);
    }

    private async Task<Stream> DownloadImageAsStreamAsync(string link)
    {
        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
            throw new InvalidDataException("Invalid image link.");
        
        var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to download image. Status code: {response.StatusCode}");

        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (contentType is null || !contentType.StartsWith("image/"))
            throw new InvalidDataException("The provided link does not contain a valid image.");

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }
    
    private string GetFileNameFromUrl(string link)
    {
        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
            throw new InvalidDataException("Invalid link format.");

        var fileName = Path.GetFileName(uri.LocalPath);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidDataException("Unable to extract filename from link.");

        return fileName;
    }
}