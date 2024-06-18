using Newtonsoft.Json;

namespace Models.Converters;

public class JsonToCityConverter : JsonConverter<City.City> {
    public override void WriteJson(JsonWriter writer, City.City? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }

    public override City.City? ReadJson(JsonReader reader, Type objectType, City.City? existingValue, bool hasExistingValue,
        JsonSerializer serializer) {
        var json = serializer.Deserialize<dynamic>(reader)!;

        return new()
        {
            Name = json["address"]!,
            Latitude = json["location"]!["latitude"]!,
            Longitude = json["location"]!["longitude"]!
        };
    }
}