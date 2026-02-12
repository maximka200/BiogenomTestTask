namespace BiogenomTestTask.Models;

public class ImageRequest
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; }
    
    public Guid ImgId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<DetectedItem> Items { get; set; } = new List<DetectedItem>();
}