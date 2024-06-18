using MemoryPack;

using Models.Abstractions;

namespace Models.City;

[MemoryPackable]
public partial class City : IEntity<string> {
    public string? Id { get; set; }
    public string Name { get; set; }
    public string? Slug { get; set; }
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}