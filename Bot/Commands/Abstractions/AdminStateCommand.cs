using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Commands.Abstractions;

public abstract class AdminStateCommand(
    string stateKey,
    string callbackMessage,
    string stateMessage,
    InlineKeyboardMarkup stateMarkup,
    
    ITelegramBotClient bot,
    StateManager state,
    IRepository<Models.User, long> users,
    
    bool sendError = true) : AdminCommand(users), IStateHandler {
    public override Task Execute(Update update) {
        state.Set(update.CallbackQuery.From.Id, stateKey);

        return bot.EditMessageTextAsync(
            update.CallbackQuery.From.Id,
            update.CallbackQuery.Message.MessageId,
            callbackMessage
        );
    }

    public async Task OnStateHandler(Update update) {
        if (!await _OnStateHandler(update)) {
            state.Set(update.Message.From.Id, stateKey);

            if (sendError) {
                await bot.SendTextMessageAsync(
                    update.Message.From.Id,
                    """
                    Error while updating data
                    Please try again
                    """,
                    replyMarkup: stateMarkup
                );
            }
            
            return;
        }

        await bot.SendTextMessageAsync(
            update.Message.From.Id,
            stateMessage,
            replyMarkup: stateMarkup
        );
    }

    private protected abstract Task<bool> _OnStateHandler(Update update);
}