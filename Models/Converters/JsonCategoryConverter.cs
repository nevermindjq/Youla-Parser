using Newtonsoft.Json;

namespace Models.Converters;

public class JsonCategoryConverter : JsonConverter<Category> {
    public override void WriteJson(JsonWriter writer, Category? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }

    public override Category? ReadJson(
        JsonReader reader, 
        Type objectType, 
        Category? existingValue, 
        bool hasExistingValue,
        JsonSerializer serializer) {
        var json = serializer.Deserialize<dynamic>(reader)!;

        return new()
        {
            Id = json.id,
            Name = json.name,
            SlugId = json.slug_id,
            Caption = json.icon.url.web_rotator
        };
    }
}