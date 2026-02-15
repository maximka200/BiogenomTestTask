using BiogenomTestTask.Models.DTOs;

namespace BiogenomTestTask.Models;

public class DetectedItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public Guid ImageRequestId { get; set; }
    
    public ImageRequest ImageRequest { get; set; }

    public ICollection<ItemMaterial> Materials { get; set; } = new List<ItemMaterial>();
}