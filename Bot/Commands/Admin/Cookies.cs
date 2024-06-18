using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Extensions;
using Bot.Services;

using Models.Data.Abstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SharpCompress.Archives;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Commands.Admin;

[RegisterTransient(ServiceKey = "admin_cookies", ServiceType = typeof(ICommand))]
[RegisterTransient(ServiceKey = "admin_cookies_confirm", ServiceType = typeof(IStateHandler))]
public class Cookies(ITelegramBotClient bot, StateManager state, IRepository<Models.Net.Cookie, string> cookies, IRepository<Models.User, long> users) 
    : AdminStateCommand(
    "admin_cookies_confirm",
    "Send .zip archive or .txt file with cookies",
    "Cookies was successfully added",
    AdminMarkups.AdminBackToStart,
    
    bot,
    state,
    users
    ) {
    private protected override async Task<bool> _OnStateHandler(Update update) {
        if (update.Message.Document is not {MimeType: "application/zip" or "text/plain" } document) {
            return false;
        }
        
        var file = await bot.GetFileAsync(document.FileId);

        using (var memory = new MemoryStream()) {

            await bot.DownloadFileAsync(file.FilePath, memory);
            
            memory.Position = 0;
            
            await cookies.AddAsync(document.MimeType switch
            {
                "application/zip" => await ParseFromZip(memory).ToListAsync(),
                "text/plain" => [await _ParseFromTxt(memory) ?? throw new Exception()],
                _ => throw new Exception()
            });
        }

        return true;
    }

    private async IAsyncEnumerable<Models.Net.Cookie> ParseFromZip(Stream stream) {
        var archive = ArchiveFactory.Open(stream);

        foreach (var entry in archive.Entries) {
            Models.Net.Cookie? cookie;
                
            using (var entryStream = entry.OpenEntryStream()) {
                cookie = await _ParseFromTxt(entryStream);
            }

            if (cookie== null) {
                continue;
            }

            yield return cookie;
        }
    }

    private async Task<Models.Net.Cookie?> _ParseFromTxt(Stream stream) {
        Models.Net.Cookie? cookie = null;
        
        using (var reader = new StreamReader(stream)) {
            var type = await _GetTypeOfCookie(reader);

            stream.Position = 0;
            
            switch (type) {
                case "json":
                    cookie = await _ParseFromJson(reader);
                    break;
                case "netscape":
                    cookie = await _ParseFromNetscape(reader);
                    break;
            }
        }

        return cookie is not { YoulaAuth: not null, YoulaAuthRefresh: not null, Uid: not null } ? null : cookie;
    }

    private async Task<string> _GetTypeOfCookie(StreamReader reader) {
        var line = await reader.ReadLineAsync();
        
        if (line.StartsWith("[{")) {
            return "json";
        }

        return "netscape";
    }

    private async Task<Models.Net.Cookie> _ParseFromNetscape(StreamReader reader) {
        var cookie = new Models.Net.Cookie();
        
        while (cookie is not { YoulaAuth: not null, YoulaAuthRefresh: not null } || !reader.EndOfStream) {
            string? line = await reader.ReadLineAsync();

            if (line is null) {
                continue;
            }

            if (line.Contains("youla_auth")) {
                cookie.YoulaAuth = line.NetscapeValue();
            }

            if (line.Contains("youla_auth_refresh")) {
                cookie.YoulaAuthRefresh = line.NetscapeValue();
            }

            if (line.Contains("youla_auth_refresh_switch_user")) {
                cookie.YoulaAuthRefreshSwitchUser = line.NetscapeValue();
            }

            if (line.Contains("_youla_uid")) {
                cookie.Uid = line.NetscapeValue();
            }

            if (line.Contains("cto_bundle")) {
                cookie.CtoBundle = line.NetscapeValue();
            }

            if (line.Contains("domain_sid")) {
                cookie.DomainSid = line.NetscapeValue();
            }

            if (line.Contains("sessid")) {
                cookie.SessId = line.NetscapeValue();
            }
        }

        return cookie;
    }

    private async Task<Models.Net.Cookie> _ParseFromJson(StreamReader reader) {
        var content = await reader.ReadToEndAsync();
        var array = JsonConvert.DeserializeObject<JArray>(content);
        var cookie = new Models.Net.Cookie();

        foreach (var token in array) {
            if (token["name"].Value<string>() == "youla_auth") {
                cookie.YoulaAuth = token["value"].Value<string>();
            }

            if (token["name"].Value<string>() == ("youla_auth_refresh")) {
                cookie.YoulaAuthRefresh = token["value"].Value<string>();
            }

            if (token["name"].Value<string>() == ("youla_auth_refresh_switch_user")) {
                cookie.YoulaAuthRefreshSwitchUser = token["value"].Value<string>();
            }

            if (token["name"].Value<string>() == ("_youla_uid")) {
                cookie.Uid = token["value"].Value<string>();
            }

            if (token["name"].Value<string>() == ("cto_bundle")) {
                cookie.CtoBundle = token["value"].Value<string>();
            }

            if (token["name"].Value<string>() == ("domain_sid")) {
                cookie.DomainSid = token["value"].Value<string>();
            }

            if (token["name"].Value<string>() == ("sessid")) {
                cookie.SessId = token["value"].Value<string>();
            }
        }

        return cookie;
    }
}
