using Bot.Extensions;

using Models;
using Models.Configs;
using Models.Data.Abstractions;
using Models.Net;

namespace Bot.Services.Hosted;

public class ApiParseService(
    Settings settings, 
    Youla.Youla parser, 
    IRepository<Product, string> products,
    //IRepository<Cookie, string> cookies, 
    IRepository<Proxy, long> proxies
    ) : BackgroundService {
    public override async Task StartAsync(CancellationToken cancellationToken) {
        if (!settings.IsParserWorking) {
            return;
        }

        if (/*!await cookies.AnyAsync(x => !x.IsShadowBanned) ||*/ !await proxies.AnyAsync()) {
            settings.IsParserWorking = false;
            return;
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        int page = 1;

        while (!stoppingToken.IsCancellationRequested) {
            var list = parser.GetProducts(page: page);
        
            await foreach (var product in list.WithCancellation(stoppingToken)) {
                if (!await products.AddAsync(product)) {
                    break;
                }
            }

            if (!await list.AnyAsync(cancellationToken: stoppingToken)) {
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                page = 1;
                continue;
            }

            page++;
        }
    }
}