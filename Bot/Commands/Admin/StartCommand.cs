using Bot.Commands.Abstractions;
using Bot.Commands.Markups;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin;

[RegisterTransient(ServiceKey = "/start", ServiceType = typeof(ICommand))]
public class StartCommand(ITelegramBotClient bot, Models.Configs.Settings settings, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override Task Execute(Update update) {
        return bot.SendTextMessageAsync(
            update.Message.From.Id,
            AdminMarkups.StartText(update.Message.From.Username),
            replyMarkup: AdminMarkups.Start(settings)
        );
    }
}