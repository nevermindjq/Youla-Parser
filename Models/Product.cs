using Models.Abstractions;

namespace Models;

public class Product : IEntity<string> {
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string Url { get; set; }
    public float Price { get; set; }
    public long DatePublished { get; set; }
    public int SubcategoryId { get; set; }
    public int Views { get; set; }
    public int FavoriteCounter { get; set; }

    public bool IsSold { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsArchived { get; set; }
    
    // Custom
    public bool IsUsed { get; set; }

    // Seller
    public Seller? Seller { get; set; }
    public string SellerId { get; set; } // owner.id
    
    // Category
    public Category? Category { get; set; }
    public int CategoryId { get; set; }
}