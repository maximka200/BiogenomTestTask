using Microsoft.EntityFrameworkCore;
using BiogenomTestTask.Models;
using BiogenomTestTask.Models.DTOs;

namespace BiogenomTestTask.Repository;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ImageRequest> ImageRequests => Set<ImageRequest>();
    public DbSet<DetectedItem> DetectedItems => Set<DetectedItem>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<ItemMaterial> ItemMaterials => Set<ItemMaterial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemMaterial>()
            .HasKey(x => new { x.DetectedItemId, x.MaterialId });

        modelBuilder.Entity<ItemMaterial>()
            .HasOne(x => x.DetectedItem)
            .WithMany(x => x.Materials)
            .HasForeignKey(x => x.DetectedItemId);

        modelBuilder.Entity<ItemMaterial>()
            .HasOne(x => x.Material)
            .WithMany()
            .HasForeignKey(x => x.MaterialId);
    }
    
    public async Task<CheckItemResponse> CreateImageRequestAsync(string imageUrl, Guid imgId, string[] detectedItems)
    {
        var imageRequest = new ImageRequest
        {
            Id = Guid.NewGuid(),
            ImageUrl = imageUrl,
            ImgId = imgId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in detectedItems)
        {
            imageRequest.Items.Add(new DetectedItem
            {
                Id = Guid.NewGuid(),
                Name = item
            });
        }

        ImageRequests.Add(imageRequest);
        await SaveChangesAsync();

        return new CheckItemResponse
        {
            ResponseId = imageRequest.Id,
            DetectedItems = detectedItems
        };
    }
}