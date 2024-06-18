using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_min_price", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_min_price_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsEditMinPrice(Models.Configs.Settings settings, ITelegramBotClient bot, StateManager state, IRepository<Models.User, long> users)
    : AdminStateCommand(
        "admin_settings_edit_min_price_confirm", 
        "Edit minimal price", 
        "Minimal price was updated", 
        AdminMarkups.SettingsBackMarkup,
        
        bot, 
        state, 
        users
    ) {
    private protected override Task<bool> _OnStateHandler(Update update) {
        if (!int.TryParse(update.Message.Text, out var price) && price >= 0) {
            return Task.FromResult(false);
        }

        settings.Price = new(price, settings.Price?.End.Value ?? price);

        return Task.FromResult(true);
    }
}