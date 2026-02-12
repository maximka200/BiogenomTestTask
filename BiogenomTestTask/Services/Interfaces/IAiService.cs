namespace BiogenomTestTask.Services.Interfaces;

public interface IAiService
{
    Task<string[]> AnalyzeImageAsync(Stream imgStream, Guid imgId);
    Task<Guid> UploadImageAsStreamAsync(Stream imgStream, string fileName);
}