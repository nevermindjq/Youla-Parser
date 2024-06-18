using Bot.Commands.Abstractions;
using Bot.Commands.Markups;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Users;

[RegisterTransient(ServiceKey = "admin_users", ServiceType = typeof(ICommand))]
public class UsersCallback(ITelegramBotClient bot, Models.Configs.Settings settings, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override async Task Execute(Update update) {
        var data = update.CallbackQuery.Data.Split(':');
        var page = int.Parse(data[1]);
        var approved = users.All(x => x is { IsApproved: true, IsAdmin: false });
        
        if (!await approved.AnyAsync()) {
            await bot.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                "Approved users not found",
                true
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