using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_min_views", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_min_views_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsEditMinViews(Models.Configs.Settings settings, ITelegramBotClient bot, StateManager state, IRepository<Models.User, long> users) 
    : AdminStateCommand(
        "admin_settings_edit_min_views_confirm", 
        "Edit minimal views", 
        "Minimal views was updated", 
        AdminMarkups.SettingsBackMarkup, 
        
        bot, 
        state, 
        users
    ) {
    private protected override Task<bool> _OnStateHandler(Update update) {
        if (!int.TryParse(update.Message.Text, out var views) && views >= 0) {
            return Task.FromResult(false);
        }

        settings.Views = new(views, settings.Views.End.Value);

        return Task.FromResult(true);
    }
}