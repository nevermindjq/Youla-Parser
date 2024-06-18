using Bot.Commands.Abstractions;
using Bot.Commands.Markups;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin.Requests;

[RegisterTransient(ServiceKey = "admin_requests" /*:{page}*/, ServiceType = typeof(ICommand))]
public class RequestsCallback(ITelegramBotClient bot, IRepository<Models.User, long> users) : AdminCommand(users) {
    public override async Task Execute(Update update) {
        var data = update.CallbackQuery.Data.Split(':');
        var page = int.Parse(data[1]);
        var requests = users.All(x => !x.IsApproved);

        if (!await requests.AnyAsync()) {
            await bot.AnswerCallbackQueryAsync(
                update.CallbackQuery.Id,
                "Requests are empty now",
                true
            );
            
            return;
        }

        await bot.EditMessageTextAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId,
            $"Requests\nPage: {page + 1}",
            replyMarkup: await requests.RequestsMarkupAsync(page: page));
    }
}