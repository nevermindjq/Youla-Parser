using MemoryPack;

namespace Models.Configs;

[MemoryPackable]
public partial class Settings {
    public List<Category> Categories { get; set; } = new();
    public City.City? Region { get; set; }
    public Range? Price { get; set; }
    public uint MaxActive { get; set; } = 10;
    public uint MaxSold { get; set; } = 10;
    public Range Views { get; set; } = new(0, 10);
    public bool IsParserWorking { get; set; }
    public double PhoneDelay { get; set; } = 5;
}