using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_max_price", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_max_price_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsEditMaxPrice(Models.Configs.Settings settings, ITelegramBotClient bot,StateManager state,IRepository<Models.User, long> users)
    : AdminStateCommand(
        "admin_settings_edit_max_price_confirm", 
        "Edit maximum price", 
        "Maximum price was updated", 
        AdminMarkups.SettingsBackMarkup, 
        
        bot, 
        state, 
        users
    ) {
    private protected override Task<bool> _OnStateHandler(Update update) {
        if (!int.TryParse(update.Message.Text, out var price) && price >= settings.Price?.Start.Value) {
            return Task.FromResult(false);
        }

        settings.Price = new(settings.Price?.Start.Value ?? price, price);

        return Task.FromResult(true);
    }
}