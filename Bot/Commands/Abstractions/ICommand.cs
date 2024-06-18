using Telegram.Bot.Types;

namespace Bot.Commands.Abstractions;

public interface ICommand {
    Task Execute(Update update);
    Task<bool> CanExecute(Update update);
}