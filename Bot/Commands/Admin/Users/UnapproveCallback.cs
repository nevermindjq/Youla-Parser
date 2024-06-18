using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Users;

[RegisterTransient(ServiceKey = "admin_users_unapprove " /*:{user_id}:{page}*/, ServiceType = typeof(ICommand))]
public class UnapproveCallback(ITelegramBotClient bot, NotifyService notify, Models.Configs.Settings settings, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override async Task Execute(Update update) {
        var data = update.CallbackQuery.Data.Split(':');
        var id = long.Parse(data[1]);
        var page = int.Parse(data[2]);

        var user = await users.FindAsync(id);
        user.IsApproved = false;

        if (await users.UpdateAsync(user)) {
            await bot.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                $"({user.Id}) {user.Username} was unapproved",
                true
            );

            await notify.SendNotification(user.Id, "You was unapproved");
        }
        
        // Approved
        var approved = users.All(x => x.IsApproved);
        
        if (!await approved.AnyAsync()) {
            await bot.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                "Approved users not found",
                true
            );

            await bot.EditMessageTextAsync(
                update.CallbackQuery.Id,
                update.CallbackQuery.Message.MessageId,
                AdminMarkups.StartText(update.CallbackQuery.From.Username),
                replyMarkup: AdminMarkups.Start(settings)
            );
            
            return;
        }

        await bot.EditMessageTextAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId,
            AdminMarkups.UsersText(page + 1),
            replyMarkup: await approved.UsersMarkupAsync(page: page)
        );
    }
}