namespace BiogenomTestTask.Models.DTOs;

public class CheckItemResponse
{
    public Guid ResponseId { get; set; }
    public string[]? DetectedItems { get; set; }
}