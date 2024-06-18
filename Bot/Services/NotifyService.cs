using Bot.Commands.Markups;

using Models;
using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using User = Telegram.Bot.Types.User;

namespace Bot.Services;

public class NotifyService(ITelegramBotClient bot, IRepository<Models.User, long> users, IRepository<ApproveMessage, string> messages) {
    public Task SendNotification(long userId, string text) => bot.SendTextMessageAsync(userId, text);

    public Task SendAdminsApproveNotification(User user) {
        return SendAdminsNotification(user.Id, admin => bot.SendTextMessageAsync(
            admin,
            $"New request from {user.Username}",
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                new InlineKeyboardButton("\u2705")
                {
                    CallbackData = $"admin_requests_approve:{user.Id}"
                },
                new InlineKeyboardButton("\u274c")
                {
                    CallbackData = $"admin_requests_decline:{user.Id}"
                }
            })
        ));
    }

    public Task SendAdminsGetProductNotification(User user, Product product) {
        if (product.Seller is null) {
            throw new ArgumentException("Seller is null", nameof(product));
        }
        
        return SendAdminsNotification(user.Id, admin => bot.SendTextMessageAsync(
            admin,
            $$"""
            Воркер @{{user.Username}} (#{{user.Id}}) получил строку:
            
            {{GeneralMarkups.ProductDescription(product)}}
            """,
            parseMode: ParseMode.Markdown,
            disableWebPagePreview: true
        ), isApproveMessage: true);
    }

    private async Task SendAdminsNotification(long userId, Func<long, Task<Message>> func, bool isApproveMessage = false) {
        await foreach (var admin in users.All(x => x.IsAdmin)) {
            var message = await func.Invoke(admin.Id);

            if (isApproveMessage) {
                await messages.AddAsync(new ApproveMessage
                {
                    MessageId = message.MessageId,
                    AdminId = admin.Id,
                    UserId = userId
                });
            }
        }
    }
}