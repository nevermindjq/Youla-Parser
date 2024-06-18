using Bot.Services;
using Bot.Services.Hosted;

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

using Youla.Services;

using User = Models.User;

var builder = Host.CreateApplicationBuilder(args);

// TODO add http client

builder.Services.AddHostedService<HostBootstrapper>();

builder.Services.AddSingleton<ApiParseService>();
builder.Services.AddHostedService<ApiParseService>(x => x.GetRequiredService<ApiParseService>());

// Bot
builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ => new(builder.Configuration["Bot:Token"]!));
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();

#region Database

builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlite(builder.Configuration["ConnectionStrings:Users"]));

// Bot
builder.Services.AddScoped<IRepository<User, long>, UsersRepository>();
builder.Services.AddScoped<IRepository<ApproveMessage, string>, ApproveMessagesRepository>();

// Youla
builder.Services.AddScoped<IRepository<Category, int>, CategoriesRepository>();
builder.Services.AddScoped<IRepository<City, string>, CitiesRepository>();
builder.Services.AddScoped<IRepository<Product, string>, ProductsRepository>();
builder.Services.AddScoped<IRepository<Seller, string>, SellersRepository>();

// Http
builder.Services.AddScoped<IRepository<Cookie, string>, CookiesRepository>();
builder.Services.AddScoped<IRepository<Proxy, long>, ProxiesRepository>();

#endregion

// Services
builder.Services.AddTransient<Youla.Youla>();
builder.Services.AddScoped<NotifyService>();
builder.Services.AddScoped<StateManager>();
builder.Services.AddScoped<AuthorizedHttpClientFactory>();
builder.Services.AddScoped<ProxyHttpClientFactory>();

// Configs
builder.Services.AddSingleton<Settings>(_ =>
{
    var settings = new Settings();

    if (File.Exists("settings")) {
        settings = MemoryPackSerializer.Deserialize<Settings>(File.ReadAllBytes("settings"))!;
    }

    return settings;
});

builder.Services.AddBot();

var app = builder.Build();

app.Run();