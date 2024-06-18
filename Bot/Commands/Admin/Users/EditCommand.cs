using Bot.Commands.Abstractions;
using Bot.Commands.Markups;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Users;

[RegisterTransient(ServiceKey = "/edit" /*<id>*/, ServiceType = typeof(ICommand))]
public class EditCommand(ITelegramBotClient bot, IRepository<Models.User, long> users) : AdminCommand(users) {
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
        
        await bot.SendTextMessageAsync(
            update.Message.From.Id,
            $"Edit {user.Username}",
            replyMarkup: AdminMarkups.EditMarkup(user.Id)
        );
    }
}