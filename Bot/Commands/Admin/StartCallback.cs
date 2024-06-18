using Bot.Commands.Abstractions;
using Bot.Commands.Markups;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin;

[RegisterTransient(ServiceKey = "admin_start", ServiceType = typeof(ICommand))]
public class StartCallback(ITelegramBotClient bot, Models.Configs.Settings settings, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override Task Execute(Update update) {
        return bot.EditMessageTextAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId,
            AdminMarkups.StartText(update.CallbackQuery.From.Username),
            replyMarkup: AdminMarkups.Start(settings)
        );
    }
}