using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models;
using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Settings;

[RegisterTransient(ServiceKey = "admin_settings_edit_categories", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_settings_edit_categories_confirm", ServiceType = typeof(IStateHandler))]
public class SettingsEditCategories(
    Models.Configs.Settings settings,
    IRepository<Category, int> categories,
    
    ITelegramBotClient bot, 
    StateManager state, 
    IRepository<Models.User, long> users
    ) : AdminStateCommand(
        "admin_settings_edit_categories_confirm",
        """
        Send category names or 'All'
        
        Example:
        Category 1
        Category 2
        Category 3
        ...
        """,
        "Categories was updated",
        AdminMarkups.SettingsBackMarkup,
        
        bot,
        state,
        users,
        
        false
    ) {
    private protected override async Task<bool> _OnStateHandler(Update update) {
        if (update.Message.Text == "All") {
            settings.Categories.Clear();
            return true;
        }

        var unknown = new List<string>();
        
        foreach (var categoryName in update.Message.Text.Split('\n')) {
            if (await categories.FindAsync(x => x.Name.ToLower() == categoryName.ToLower()) is not { } category) {
                unknown.Add(categoryName);
                
                continue;
            }

            settings.Categories.Add(category);
        }

        if (unknown.Count > 0) {
            settings.Categories.Clear();
                
            await bot.SendTextMessageAsync(
                update.Message.From.Id,
                $"Unknown categories:\n{string.Join('\n', unknown)}\nPlease try again",
                replyMarkup: AdminMarkups.SettingsBackMarkup
            );
            
            return false;
        }

        return true;
    }
}