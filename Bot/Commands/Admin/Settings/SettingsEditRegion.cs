using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.City;
using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_region", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_region_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsEditRegion(
    Models.Configs.Settings settings,
    IRepository<City, string> cities,
    Youla.Youla parser,
    
    ITelegramBotClient bot, 
    StateManager state,
    IRepository<Models.User, long> users
    ) :  AdminStateCommand(
        "admin_settings_edit_region_confirm",
        "Send region name or 'All'",
        "Region was updated",
        AdminMarkups.SettingsBackMarkup,
        
        bot,
        state,
        users
    ) {
    private protected override async Task<bool> _OnStateHandler(Update update) {
        if (update.Message.Text == "All") {
            settings.Region = null;
            return true;
        }

        if (await cities.FindAsync(x => x.Name.ToLower() == update.Message.Text.ToLower()) is not { } city) {
            try {
                city = await parser.CityAsync(update.Message.Text);
            }
            catch {
                return false;
            }

            await cities.AddAsync(city);
        }

        settings.Region = city;

        return true;
    }
}