using Bot.Commands.Abstractions;
using Bot.Extensions;

using Models;
using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Requests;

[RegisterTransient(ServiceKey = "admin_requests_decline" /*:{user_id}*/, ServiceType = typeof(ICommand))]
public class DeclineCallback(ITelegramBotClient bot, IRepository<ApproveMessage, string> messages, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override Task Execute(Update update) {
        var data = update.CallbackQuery.Data.Split(':');
        var id = long.Parse(data[1]);

        return bot.DeleteApproveMessagesAsync(messages, id);
    }
}