using Models.Data.Abstractions;

using Telegram.Bot.Types;

namespace Bot.Commands.Abstractions;

public abstract class AdminCommand(IRepository<Models.User, long> users) : ICommand {
    public abstract Task Execute(Update update);

    public async Task<bool> CanExecute(Update update) {
        long userId = 0;
        
        switch (update) {
            case {Message: not null}:
                userId = update.Message.From.Id;
                break;
            case {CallbackQuery: not null}:
                userId = update.CallbackQuery.From.Id;
                break;
        }
            
        return await users.FindAsync(userId) is { IsAdmin: true };
    }
}