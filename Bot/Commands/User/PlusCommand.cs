using Bot.Commands.Abstractions;
using Bot.Commands.Markups;
using Bot.Services;

using Microsoft.EntityFrameworkCore;

using Models;
using Models.Configs;
using Models.Data;
using Models.Data.Abstractions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Commands.User;

[RegisterTransient(ServiceKey = "+", ServiceType = typeof(ICommand))]
public class PlusCommand(
    ITelegramBotClient bot, 
    Settings settings, 
    NotifyService notify, 
    AppDbContext context, 
    IRepository<Product, string> products, 
    ILogger<PlusCommand> logger
    ) : ICommand {
    public async Task Execute(Update update) {
        var list = context
            .Set<Product>()
            // Product filters
            .Where(x => !x.IsUsed && !x.IsSold && !x.IsBlocked && !x.IsArchived)
            .Where(x => x.Views >= settings.Views.Start.Value && x.Views <= settings.Views.End.Value)
            .Where(x => !settings.Price.HasValue || 
                        x.Price >= settings.Price.Value.Start.Value 
                        && x.Price <= settings.Price.Value.End.Value
            )
            // Seller filters
            .Include(x => x.Seller)
            .Where(x => x.Seller.CanCall)
            .Where(x => x.Seller.Active <= settings.MaxActive && x.Seller.Sold <= settings.MaxSold)
            //.Where(x => x.Seller.Phone != null)
            .OrderByDescending(x => x.DatePublished)
            .Take(10);

        if (await list.CountAsync() == 0) {
            await bot.SendTextMessageAsync(
                update.Message.From.Id,
                "No available products"
            );

            return;
        }

        using (var stream = new MemoryStream()) {
            using (var writer = new StreamWriter(stream, leaveOpen: true)) {
                await writer.WriteLineAsync("Product Name;Product Url;Seller Id");
                
                foreach (var product in list) {
                    await writer.WriteLineAsync($"{product.Name};{product.Url};{product.SellerId}");
                    
                    //await bot.SendTextMessageAsync(
                    //    update.Message.From.Id,
                    //    GeneralMarkups.ProductDescription(product),
                    //    parseMode: ParseMode.Markdown,
                    //    disableWebPagePreview: true
                    //);

                    //await notify.SendAdminsGetProductNotification(update.Message.From, product);

                    product.IsUsed = true;

                    if (!await products.UpdateAsync(product)) {
                        logger.LogError("Error while update product {id}", product.Id);
                    }
                }
            }

            stream.Position = 0;

            await bot.SendDocumentAsync(
                update.Message.From.Id,
                new InputFileStream(stream, $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.csv")
            );
        }
    }

    public async Task<bool> CanExecute(Update update) {
        return await context.FindAsync<Models.User>(update.Message.From.Id) is {IsApproved: true};
    }
}