using Newtonsoft.Json;

namespace Models.Converters;

public class JsonToSellerConverter : JsonConverter<Seller> {
    public override void WriteJson(JsonWriter writer, Seller? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }

    public override Seller? ReadJson(JsonReader reader, Type objectType, Seller? existingValue, bool hasExistingValue,
        JsonSerializer serializer) {
        var json = serializer.Deserialize<dynamic>(reader)!["data"]!;
        
        return new()
        {
            Id = json.id,
            Name = json.name,
            Phone = json.phone,
            Active = json.prods_active_cnt,
            Sold = json.prods_sold_cnt,
        };
    }
}