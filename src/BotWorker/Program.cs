using BotWorker;
using BotWorker.Extensions;
using BotWorker.Settings;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));

        services.ConfigureTelegramClient();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();