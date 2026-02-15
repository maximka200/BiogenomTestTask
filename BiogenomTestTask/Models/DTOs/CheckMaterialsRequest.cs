namespace BiogenomTestTask.Models.DTOs;

public class CheckMaterialsRequest
{
    public Guid Id { get; set; }
    public string[] DetectedItems { get; set; }
}