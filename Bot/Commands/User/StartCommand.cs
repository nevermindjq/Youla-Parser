using Bot.Commands.Abstractions;
using Bot.Services;

using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.User;

[RegisterTransient(ServiceKey = "/start", ServiceType = typeof(ICommand))]
public class StartCommand(ITelegramBotClient bot, NotifyService notify, IRepository<Models.User, long> users) : ICommand {
    public async Task Execute(Update update) {
        if (await users.FindAsync(update.Message.From.Id) is null) {
            var user = new Models.User
            {
                Id = update.Message.From.Id,
                Username = update.Message.From.Username,
                IsAdmin = false,
                IsApproved = false
            };
            
            await notify.SendAdminsApproveNotification(update.Message.From);
            
            await users.AddAsync(user);
        }

        await bot.SendTextMessageAsync(update.Message.From.Id, $"Hello, {update.Message.From.Username}!\nYour request is on moderation");
    }

    public Task<bool> CanExecute(Update update) {
        return Task.FromResult(true);
    }
}