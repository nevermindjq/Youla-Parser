using Newtonsoft.Json;

namespace Models.Converters;

public class JsonApiToProductConverter : JsonConverter<Product> {
    public override void WriteJson(JsonWriter writer, Product? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }

    public override Product? ReadJson(JsonReader reader, Type objectType, Product? existing, bool hasExistingValue,
        JsonSerializer serializer) {
        var json = serializer.Deserialize<dynamic>(reader)!;

        if (json.data is not null) {
            json = json.data;
        }
        
        var product = new Product
        {
            Id = json.id,
            Name = json.name,
            Slug = json.slug,
            Description = json.description,
            Url = $"https://youla.ru{json.url}",
            Price = ((long?)json.price_with_discount_seller ?? (long)json.discounted_price) / 100.0f,
            DatePublished = json.date_published,
            CategoryId = json.category,
            SubcategoryId = json.subcategory,
            Views = json.views,
            FavoriteCounter = json.favorite_counter,

            IsSold = json.is_sold,
            IsDeleted = json.is_deleted,
            IsBlocked = json.is_blocked,
            IsArchived = json.is_archived,
            
            Seller = new()
            {
                Id = json.owner.id
            }
        };

        if (json.owner.settings is not null) {
            product.Seller.CanCall = json.owner.settings.call_settings.any_call_enabled ?? false;
        }
        
        return product;
    }
}