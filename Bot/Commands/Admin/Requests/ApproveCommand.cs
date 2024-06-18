using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Extensions;
using Bot.Services;

using Models;
using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Requests;

[RegisterTransient(ServiceKey = "/approve" /*<id>*/, ServiceType = typeof(ICommand))]
public class ApproveCommand(ITelegramBotClient bot, NotifyService notify, IRepository<ApproveMessage, string> messages, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override async Task Execute(Update update) {
        var data = update.Message.Text.Split(' ');
        var id = long.Parse(data[1]);

        if (await users.FindAsync(id) is not { } user) {
            await bot.SendTextMessageAsync(
                update.Message.From.Id,
                $"User {id} not found",
                replyMarkup: GeneralMarkups.DeleteMessage
            );
            
            return;
        }
        
        user.IsApproved = true;

        if (!await users.UpdateAsync(user)) {
            return;
        }
        
        await notify.SendNotification(user.Id, "Your request was approved");

        await bot.DeleteApproveMessagesAsync(
            messages,
            user.Id
        );

        await bot.SendTextMessageAsync(
            update.Message.From.Id,
            $"({user.Id}) {user.Username}'s request was approved",
            replyMarkup: GeneralMarkups.DeleteMessage
        );
    }
}