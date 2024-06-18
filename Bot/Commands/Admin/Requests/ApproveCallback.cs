using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Extensions;
using Bot.Services;

using Models;
using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Requests;

[RegisterTransient(ServiceKey = "admin_requests_approve" /*:{userid}*/, ServiceType = typeof(ICommand))]
public class ApproveCallback(ITelegramBotClient bot, NotifyService notify, Models.Configs.Settings settings, IRepository<ApproveMessage, string> messages, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override async Task Execute(Update update) {
        var data = update.CallbackQuery.Data.Split(':');
        var id = long.Parse(data[1]);
        
        if (await users.FindAsync(id) is not { } user) {
            await bot.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                $"User {id} was not found",
                true
            );
            
            return;
        }

        user.IsApproved = true;

        if (await users.UpdateAsync(user)) {
            await bot.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                $"({user.Id}) {user.Username}'s request was approved",
                true
            );

            await notify.SendNotification(user.Id, "Your request was approved");
        }

        await bot.DeleteApproveMessagesAsync(
            messages,
            user.Id,
            update.CallbackQuery.Message.MessageId
        );

        await bot.EditMessageTextAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId,
            AdminMarkups.StartText(update.CallbackQuery.From.Username),
            replyMarkup: AdminMarkups.Start(settings)
        );
    }
}