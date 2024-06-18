using Bot.Commands.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Bot.Services;

public class UpdateHandler(IServiceProvider provider, StateManager state, ILogger<UpdateHandler> logger) : IUpdateHandler {
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        try {
            List<ICommand> commands = new();
            long userId;

            switch (update) {
                case { Message: { Text: var text, From.Id: var id } }:
                    userId = id;

                    if (text is null) {
                        break;
                    }
                    
                    commands = provider.GetKeyedServices<ICommand>(KeyFromMessage(text)).ToList();
                    break;
                case { CallbackQuery: { Data: { } data, From.Id: var id } }:
                    userId = id;
                    commands = provider.GetKeyedServices<ICommand>(KeyFromCallback(data)).ToList();
                    break;
                default:
                    logger.LogWarning("Unknown command type: {0}", update.Type.ToString());
                    return;
            }

        
            if (state.TryPop(userId) is { } handler && commands.Count == 0) {
                await handler.OnStateHandler(update);
                return;
            }
        
            if (await ExecuteCommandsAsync(commands, update, x => x is AdminCommand)) {
                return;
            }
        
            await ExecuteCommandsAsync(commands, update);
        }
        catch (Exception e) {
            logger.LogError(e, null);
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
        logger.LogError(exception, "Exception has occured while executing commands");
        
        return Task.CompletedTask;
    }

    private string KeyFromMessage(string text) {
        var space = text.IndexOf(' ');
        return text[..(space is -1 ? text.Length : space)];
    }

    private string KeyFromCallback(string data) {
        var delimiter = data.IndexOf(':');
        return data[..(delimiter is -1 ? data.Length : delimiter)];
    }

    private async Task<bool> ExecuteCommandsAsync(List<ICommand> commands, Update update, Func<ICommand, bool>? predicate = null) {
        var filtered = commands.Where(predicate ?? (_ => true)).ToList();
        var executed = false;

        foreach (var command in filtered) {
            if (await command.CanExecute(update)) {
                await command.Execute(update);

                executed = true;
            }

            commands.Remove(command);
        }

        return executed;
    }
}