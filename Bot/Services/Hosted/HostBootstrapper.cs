using MemoryPack;

using Microsoft.EntityFrameworkCore;

using Models;
using Models.City;
using Models.Configs;
using Models.Data;
using Models.Data.Abstractions;
using Models.Net;

using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Bot.Services.Hosted;

public class HostBootstrapper(
    Youla.Youla parser,
    IRepository<Category, int> categories,
    
    AppDbContext context,
    IRepository<User, long> users,
    IConfiguration configuration,
    
    ITelegramBotClient bot, 
    IUpdateHandler handler, 
    Settings settings
    ) : IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken) {
        bot.StartReceiving(handler, cancellationToken: cancellationToken);

        await _InitializeUsers();
        
        try {
            await _UpdateCookies();
        
            if (!await categories.AnyAsync()) {
                await categories.AddAsync(await parser.ParseCategoriesAsync().ToListAsync(cancellationToken: cancellationToken));
            }
        }
        catch {
            // ignore
        }
    }
    
    // Work on CTRL + C
    public Task StopAsync(CancellationToken cancellationToken) {
        return File.WriteAllBytesAsync("settings", MemoryPackSerializer.Serialize(settings));
    }

    private async Task _InitializeUsers() {
        var config = configuration.GetSection("Bot").Get<BotConfig>()!;

        await context.Database.EnsureCreatedAsync();
        
        if (!await users.AnyAsync()) {
            await users.AddAsync(config.Admins.Select(x => new User
            {
                Id = x,
                Username = "Admin",
                IsAdmin = true,
                IsApproved = true
            }));
        }
    }

    private async Task _UpdateCookies() {
        var cookies = context.Set<Cookie>();

        foreach (var cookie in cookies) {
            cookie.IsShadowBanned = false;
        }
        
        context.UpdateRange(cookies);
        await context.SaveChangesAsync();
    }
}