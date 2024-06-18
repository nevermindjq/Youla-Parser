using Models.Abstractions;

namespace Models;

public class Seller : IEntity<string> {
    public string Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public int Active { get; set; } // prods_active_cnt
    public int Sold { get; set; } // prods_sold_cnt

    public bool IsStore { get; set; }
    public bool IsVerified { get; set; }

    // Custom
    public bool CanCall { get; set; }
    public List<Product> Products { get; set; }
}