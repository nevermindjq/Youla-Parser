using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_sold", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_sold_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsEditSold(Models.Configs.Settings settings, ITelegramBotClient bot, StateManager state, IRepository<Models.User, long> users) 
    : AdminStateCommand(
        "admin_settings_edit_sold_confirm", 
        "Edit count sold", 
        "Count sold was updated", 
        AdminMarkups.SettingsBackMarkup, 
        
        bot, 
        state, 
        users
    ) {
    private protected override Task<bool> _OnStateHandler(Update update) {
        if (!uint.TryParse(update.Message.Text, out var sold)) {
            return Task.FromResult(false);
        }

        settings.MaxSold = sold;

        return Task.FromResult(true);
    }
}