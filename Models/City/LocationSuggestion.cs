using Newtonsoft.Json;

namespace Models.City;

public class LocationSuggestion
{
    [JsonProperty("narrow_address")] public string NarrowAddress { get; set; }
    [JsonProperty("broad_address")] public string BroadAddress { get; set; }
    [JsonProperty("reference")] public string Reference { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
}