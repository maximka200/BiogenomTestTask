using BiogenomTestTask.Models.DTOs;

namespace BiogenomTestTask.Controllers.Interfaces;

public interface IImageAnalysisService
{
    public Task<CheckItemResponse> AnaliseItemsAsync(string link);
    public Task<CheckMaterialsResponse> AnaliseMaterialsAsync(CheckMaterialsRequest request);
}