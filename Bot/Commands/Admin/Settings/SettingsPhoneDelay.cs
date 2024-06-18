using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_phone_delay", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_phone_delay_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsPhoneDelay(Models.Configs.Settings settings, ITelegramBotClient bot, StateManager state, IRepository<Models.User, long> users) 
    : AdminStateCommand(
    "admin_settings_edit_phone_delay_confirm", 
    "Edit delay for phone parsing request (in minutes)", 
    "Phone delay was updated",
    AdminMarkups.SettingsBackMarkup,
    
    bot, 
    state, 
    users
    ) {
    private protected override Task<bool> _OnStateHandler(Update update) {
        if (!double.TryParse(update.Message.Text, out var delay)) {
            return Task.FromResult(false);
        }

        settings.PhoneDelay = delay;

        return Task.FromResult(true);
    }
}