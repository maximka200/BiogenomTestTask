using BiogenomTestTask.Controllers.Interfaces;
using BiogenomTestTask.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BiogenomTestTask.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageAnalysisController(IImageAnalysisService analiseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CheckItems([FromBody] string link)
    {
        try
        {
            var result = await analiseService.AnaliseItemsAsync(link);
            return Ok(result);
        }
        catch (InvalidDataException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"An error occurred while processing the request. {e.Message}");
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> CheckMaterials([FromBody] CheckMaterialsRequest request)
    {
        try
        {
            var result = await analiseService.AnaliseMaterialsAsync(request);
            return Ok(result);
        }
        catch (InvalidDataException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"An error occurred while processing the request. {e.Message}");
        }
    }
}