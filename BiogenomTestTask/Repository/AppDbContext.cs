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
    
    public async Task<CheckMaterialsResponse> CreateMaterialsResponseAsync(Guid requestId, Dictionary<string, string> itemMaterials)
    {
        var imageRequest = await ImageRequests.Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == requestId);

        if (imageRequest == null)
            throw new KeyNotFoundException("Image request not found.");

        foreach (var item in imageRequest.Items)
        {
            if (!itemMaterials.TryGetValue(item.Name, out var materialName)) continue;
            var material = await Materials.FirstOrDefaultAsync(m => m.Name == materialName) 
                           ?? new Material { Id = Guid.NewGuid(), Name = materialName };

            ItemMaterials.Add(new ItemMaterial
            {
                DetectedItemId = item.Id,
                MaterialId = material.Id
            });
        }

        await SaveChangesAsync();

        return new CheckMaterialsResponse
        {
            DetectedItems = imageRequest.Items.Select(i => i.Name).ToArray(),
            ItemMaterials = itemMaterials
        };
    }
}