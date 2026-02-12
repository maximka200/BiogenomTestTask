namespace BiogenomTestTask.Models;

public class ItemMaterial
{
    public Guid DetectedItemId { get; set; }
    public DetectedItem DetectedItem { get; set; } = null!;

    public Guid MaterialId { get; set; }
    public Material Material { get; set; } = null!;
}