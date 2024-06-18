using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Models.Data.Abstractions;
using Models.Net;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin;

// format: host:port@username:password
[RegisterTransient(ServiceKey = "admin_proxies", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_proxies_confirm", ServiceType = typeof(IStateHandler))]
public class Proxies(ITelegramBotClient bot, StateManager state, IRepository<Proxy, long> proxies, IRepository<Models.User, long> users)
    : AdminStateCommand(
        "admin_proxies_confirm",
        "Send .txt file with proxies",
        "Proxies was successfully added",
        AdminMarkups.AdminBackToStart,

        bot,
        state,
        users
    ) {
    private protected override async Task<bool> _OnStateHandler(Update update) {
        if (update.Message.Document is not {MimeType: "text/plain"}) {
            return false;
        }
        
        var file = await bot.GetFileAsync(update.Message.Document.FileId);

        using (var memory = new MemoryStream()) {

            await bot.DownloadFileAsync(file.FilePath, memory);
            
            memory.Position = 0;

            var proxiesList = new List<Proxy>();

            using (var reader = new StreamReader(memory)) {
                while (!reader.EndOfStream) {
                    var line = await reader.ReadLineAsync();
                    var data = line.Split('@')
                        .SelectMany(x => x.Split(':'))
                        .ToList();
                    var proxy = new Proxy
                    {
                        Host = data[0],
                        Port = int.Parse(data[1])
                    };

                    if (data.Count > 2) {
                        proxy.Username = data[2];
                        proxy.Password = data[3];
                    }

                    proxiesList.Add(proxy);
                }
            }

            await proxies.AddAsync(proxiesList);
        }

        return true;
    }
}