namespace BiogenomTestTask.Services.Interfaces;

public interface IAiService
{
    Task<string[]> AnalyzeImageItemsAsync(Guid imgId);
    Task<Dictionary<string, string>> AnalyzeImageMaterialsAsync(Guid imgId, string[] detectedItems);
    Task<Guid> UploadImageAsStreamAsync(Stream imgStream, string fileName);
}