using Bot.Commands.Abstractions;
using Bot.Commands.Markups;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings", ServiceType = typeof(ICommand))]
public class SettingsCallback(ITelegramBotClient bot, Models.Configs.Settings settings, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override async Task Execute(Update update) {
        await bot.EditMessageTextAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId,
            AdminMarkups.SettingsText(settings),
            replyMarkup: AdminMarkups.SettingsMarkup
        );
    }
}