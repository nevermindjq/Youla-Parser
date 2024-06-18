using Models;
using Models.Data.Abstractions;

using Telegram.Bot;

namespace Bot.Extensions;

public static class TelegramBotExtensions {
    public static async Task DeleteApproveMessagesAsync(this ITelegramBotClient bot, IRepository<ApproveMessage, string> messages, long userId, int? excludeMessageId = null) {
        foreach (var message in await messages.All(x => x.UserId == userId).ToListAsync()) {
            await messages.RemoveAsync(message.Id);
            
            if (message.MessageId == excludeMessageId) {
                continue;
            }
            
            await bot.DeleteMessageAsync(
                message.AdminId,
                message.MessageId
            );
        }
    }
}