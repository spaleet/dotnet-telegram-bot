using BotWorker;
using BotWorker.Extensions;
using BotWorker.Models;
using BotWorker.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));

        services.ConfigureTelegramClient();
        services.AddScoped<UpdateHandler>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IReceiverService, ReceiverService>();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();