using Models;

using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Commands.Markups;

public static class GeneralMarkups {
    public static InlineKeyboardMarkup DeleteMessage { get; } = new(
        new InlineKeyboardButton("\u274c")
        {
            CallbackData = "message_delete"
        });

    public static string ProductDescription(Product product) {
        var created = DateTimeOffset.FromUnixTimeSeconds(product.DatePublished).DateTime + TimeSpan.FromHours(3);
        
        // ☎️ Телефон: {$"`{product.Seller.Phone}`" ?? "None"}{_GenerateWAUrl(product.Seller.Phone, product.Url)}{_GenerateTelegramUrl(product.Seller.Phone)}
        
        return $$"""
                 📌 {{product.Name}}
                 💸 Цена: {{product.Price}} ₽
                 ✏️ Линк: [Товар]({{product.Url}})
                 📅 Создано GMT +3: {{created.ToShortDateString()}} {{created.ToShortTimeString()}}
                 """;
    }

    private static string _GenerateWAUrl(string? phone, string url) {
        if (phone is null) {
            return "";
        }

        return $"\n🔰 [WhatsApp](https://wa.me/{phone}" +
               $"?text=Доброго+времени+суток,+я+по+поводу+объявления,+не+продали+всё+ещё+{url}+" +
               $"Подскажите+пожалуйста+все+как+на+фото?+Если+вам+внесу+всю+сумму,+удобно+будет+доставщика+юлы+встретить+и+передать+ему?+просто+сам+не+смогу+никак+забрать)";
    }
    
    private static string _GenerateTelegramUrl(string? phone) {
        return phone is not null ? $"\n🔰 [Telegram](https://t.me/{phone})" : "";
    }
}