using Models.Abstractions;

namespace Models;

public class Category : IEntity<int> {
    public int Id { get; set; }
    public string Name { get; set; }
    public string SlugId { get; set; }
    public string Caption { get; set; }
    
    //
    public List<Product> Products { get; set; }
}