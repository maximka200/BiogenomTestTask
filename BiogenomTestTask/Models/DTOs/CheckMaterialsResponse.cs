namespace BiogenomTestTask.Models.DTOs;

public class CheckMaterialsResponse
{
    public string[] DetectedItems { get; set; }
    public Dictionary<string, string> ItemMaterials { get; set; }
}