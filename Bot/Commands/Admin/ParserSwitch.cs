using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services.Hosted;

using Models.Data.Abstractions;
using Models.Net;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin;

[RegisterTransient(ServiceKey = "admin_parser_switch", ServiceType = typeof(ICommand))]
public class ParserSwitch(
    ITelegramBotClient bot, 
    Models.Configs.Settings settings, 
    ApiParseService parser, 
    IRepository<Models.User, long> users,
    IRepository<Cookie, string> cookies, 
    IRepository<Proxy, long> proxies
    ) : AdminCommand(users) {
    public override async Task Execute(Update update) {
        if (/*!await cookies.AnyAsync(x => !x.IsShadowBanned) ||*/ !await proxies.AnyAsync()) {
            await bot.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                "No available proxies for parse", // cookies or
                true
            );
            
            return;
        }
        
        settings.IsParserWorking = !settings.IsParserWorking;
        
        switch (!settings.IsParserWorking) {
            case true:
                await parser.StopAsync(default);
                break;
            case false:
                await parser.StartAsync(default);
                break;
        }
        
        await bot.EditMessageTextAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId,
            $"Bot was {(settings.IsParserWorking ? "started" : "stopped")}",
            replyMarkup: AdminMarkups.Start(settings)
        );
    }
}