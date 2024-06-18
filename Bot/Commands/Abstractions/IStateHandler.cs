using Telegram.Bot.Types;

namespace Bot.Commands.Abstractions;

public interface IStateHandler : ICommand {
    Task OnStateHandler(Update update);
}