using Models.Configs;

using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Commands.Markups;

public static class AdminMarkups {
    #region Start

    public static string StartText(string username) =>
        $"Hello, {username}!";
    
    public static InlineKeyboardMarkup Start(Settings settings) => new(new[]
    {
        new []
        {
            new InlineKeyboardButton("Users")
            {
                CallbackData = "admin_users:0"
            },
            new InlineKeyboardButton("Requests")
            {
                CallbackData = "admin_requests:0"
            }
        },
        [
            new("Settings")
            {
                CallbackData = "admin_settings"
            },
            new("Proxies")
            {
                CallbackData = "admin_proxies"
            }
        ],
        [
            new (settings.IsParserWorking ? "Stop" : "Start")
            {
                CallbackData = $"admin_parser_switch"
            }
        ]
    });

    #endregion

    // Requests
    public static async Task<InlineKeyboardMarkup> RequestsMarkupAsync(this IAsyncEnumerable<Models.User> enumerable,
        int max_requests = 5, int page = 0) {
        var requests = await enumerable
            ._InRange(max_requests, page)
            .Select(x => new[]
            {
                new InlineKeyboardButton(x.Username)
                {
                    CallbackData = $"admin_requests_approve:{x.Id}"
                }
            })
            .ToListAsync();

        requests.Add(await enumerable._Navigation(max_requests, page));

        return requests._WithBack();
    }

    #region Users

    public static string UsersText(int page) => $"Approved users\nPage: {page}";
    
    public static InlineKeyboardMarkup EditMarkup(long userId, int page = 0) =>
        new(new []
        {
            new InlineKeyboardButton("Delete")
            {
                CallbackData = $"admin_users_unapprove:{userId}:{page}"
            },
            new InlineKeyboardButton("Back")
            {
                CallbackData = $"admin_users:{page}"
            }
        });
    
    public static async Task<InlineKeyboardMarkup> UsersMarkupAsync(this IAsyncEnumerable<Models.User> enumerable,
        int max_users = 5, int page = 0) {
        var approved = await enumerable
            .Where(x => !x.IsAdmin)
            ._InRange(max_users, page)
            .Select(x => new[]
            {
                new InlineKeyboardButton(x.Username)
                {
                    CallbackData = $"admin_users_edit:{x.Id}:{page}"
                }
            })
            .ToListAsync();
        
        approved.Add(await enumerable._Navigation(max_users, page));

        return approved._WithBack();
    }

    #endregion

    #region Settings

    public static string SettingsText(Settings settings) {
        // ⌛️ Phone Request delay: {settings.PhoneDelay} minutes
        
        return $"""
               Settings
               🔎 Categories: {(settings.Categories.Count == 0 ? "All" : string.Join(", ", settings.Categories.Select(x => x.Name)))}
               ⛩ Region: {settings.Region?.Name ?? "All"}
               💸 Price: {(settings.Price is null ? "All" : $"{settings.Price.Value.Start.Value} - {settings.Price.Value.End.Value}")}
               💣 Count Active: up to {settings.MaxActive}
               🧨 Count Sold: up to {settings.MaxSold}
               📖 Count Views: {settings.Views.Start.Value} - {settings.Views.End.Value}
               """;
    }

    public static InlineKeyboardMarkup SettingsBackMarkup { get; } = new(
        new InlineKeyboardButton("Back")
        {
            CallbackData = "admin_settings"
        }
    );
    
    public static InlineKeyboardMarkup SettingsMarkup { get; } = new(new []
    {
        new []
        {
            new InlineKeyboardButton("Change Categories")
            {
                CallbackData = "admin_settings_edit_categories"
            },
            new InlineKeyboardButton("Change Region")
            {
                CallbackData = "admin_settings_edit_region"
            }
        },
        [
            new("Change Min Price")
            {
                CallbackData = "admin_settings_edit_min_price"
            },
            new("Change Max Price")
            {
                CallbackData = "admin_settings_edit_max_price"
            }
        ],
        [
            new("Change Count Active")
            {
                CallbackData = "admin_settings_edit_active"
            },
            new("Change Count Sold")
            {
                CallbackData = "admin_settings_edit_sold"
            }
        ],
        [
            new("Change Min Views")
            {
                CallbackData = "admin_settings_edit_min_views"
            },
            new("Change Max Views")
            {
                CallbackData = "admin_settings_edit_max_views"
            }
        ],
        //[
        //    new("Change Phone Request Delay")
        //    {
        //        CallbackData = "admin_settings_edit_phone_delay"
        //    }
        //],
        [
            new("Back")
            {
                CallbackData = "admin_start"
            }
        ]
    });

    #endregion

    public static InlineKeyboardMarkup AdminBackToStart { get; } = new([
        new InlineKeyboardButton("Back")
        {
            CallbackData = "admin_start"
        }
    ]);

    #region Components

    private static InlineKeyboardMarkup _WithBack(this IEnumerable<InlineKeyboardButton[]> enumerable) =>
        new(enumerable.Append([
            new("Back")
            {
                CallbackData = "admin_start"
            }
        ]));
    
    private static IAsyncEnumerable<T> _InRange<T>(this IAsyncEnumerable<T> enumerable, int max, int page) =>
        enumerable.Skip(page * max).Take(max);
    
    private static async Task<InlineKeyboardButton[]> _Navigation<T>(this IAsyncEnumerable<T> enumerable, int max_rows, int page = 0) {
        var navigation = new List<InlineKeyboardButton>();

        if (page > 0) {
            navigation.Add(new("\u25c0\ufe0f")
            {
                CallbackData = page - 1 == 0 ? "admin_requests" : $"admin_requests_swipe:{page - 1}"
            });
        }

        if (await enumerable.CountAsync() > max_rows) {
            navigation.Add(new("\u25b6\ufe0f")
            {
                CallbackData = $"admin_requests_swipe:{page + 1}"
            });
        }
        
        return navigation.ToArray();
    }

    #endregion
}