using Newtonsoft.Json;

namespace Models.Converters;

public class JsonGraphToProductConverter : JsonConverter<Product> {
    public override void WriteJson(JsonWriter writer, Product? value, JsonSerializer serializer) {
        throw new NotImplementedException();
    }

    public override Product? ReadJson(JsonReader reader, Type objectType, Product? existingValue, bool hasExistingValue,
        JsonSerializer serializer) {
        var json = serializer.Deserialize<dynamic>(reader)!;

        if (json.__typename is not "ProductItem" and not "PromotedProductItem") {
            return null;
        }

        var product = json["product"]!;
        var analytics = json["productAnalytics"]!;

        return new()
        {
            Id = product.id,
            SellerId = product.owner.id,
            Name = product.name,

            Url = $"https://youla.link/p/{product.id}",
            Price = (int)product.price.readPrice.price / 100.0f,
            DatePublished = product.datePublished,
            CategoryId = product.category,
            SubcategoryId = product.subcategory,

            IsSold = analytics.isSold,
            IsDeleted = analytics.isDeleted,
            IsBlocked = analytics.isBlocked,
            IsArchived = analytics.isArchived
        };
    }
}