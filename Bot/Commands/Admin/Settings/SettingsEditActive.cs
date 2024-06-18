using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_active", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_active_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsEditActive(Models.Configs.Settings settings, ITelegramBotClient bot, StateManager state, IRepository<Models.User, long> users)
    : AdminStateCommand(
        "admin_settings_edit_min_price_confirm", 
        "Edit count active", 
        "Count active was updated", 
        AdminMarkups.SettingsBackMarkup,
        
        bot, 
        state, 
        users
    ) {
    private protected override Task<bool> _OnStateHandler(Update update) {
        if (!uint.TryParse(update.Message.Text, out var active)) {
            return Task.FromResult(false);
        }

        settings.MaxActive = active;

        return Task.FromResult(true);
    }
}