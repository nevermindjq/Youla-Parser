using Bot.Commands.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands;

[RegisterTransient(ServiceKey = "message_delete", ServiceType = typeof(ICommand))]
public class DeleteMessageCallback(ITelegramBotClient bot) : ICommand {
    public Task Execute(Update update) {
        return bot.DeleteMessageAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId
        );
    }

    public Task<bool> CanExecute(Update update) {
        return Task.FromResult(true);
    }
}