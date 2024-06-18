using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_max_views", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_max_views_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsEditMaxViews(Models.Configs.Settings settings, ITelegramBotClient bot, StateManager state, IRepository<Models.User, long> users) 
    : AdminStateCommand(
        "admin_settings_edit_max_views_confirm", 
        "Edit maximum views", 
        "Maximum views was updated", 
        AdminMarkups.SettingsBackMarkup, 
        
        bot, 
        state, 
        users
    ) {
    private protected override Task<bool> _OnStateHandler(Update update) {
        if (!int.TryParse(update.Message.Text, out var views) && views >= settings.Views.Start.Value) {
            return Task.FromResult(false);
        }

        settings.Views = new(settings.Views.Start.Value, views);

        return Task.FromResult(true);
    }
}