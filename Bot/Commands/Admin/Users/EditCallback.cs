using Bot.Commands.Abstractions;
using Bot.Commands.Markups;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Users;

[RegisterTransient(ServiceKey = "admin_users_edit " /*:{user_id}:{page}*/, ServiceType = typeof(ICommand))]
public class EditCallback(ITelegramBotClient bot, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override async Task Execute(Update update) {
        var data = update.CallbackQuery.Data.Split(':');
        var id = long.Parse(data[1]);
        var page = int.Parse(data[2]);

        var user = await users.FindAsync(id);
        
        await bot.EditMessageTextAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId,
            $"Edit {user.Username}",
            replyMarkup: AdminMarkups.EditMarkup(user.Id, page)
        );
    }
}