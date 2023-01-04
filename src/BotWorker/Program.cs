using BotWorker;
using BotWorker.Extensions;
using BotWorker.Models;
using BotWorker.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));

        services.ConfigureTelegramClient();

        // Currency Exchange Api by https://rapidapi.com/fyhao/api/currency-exchange
        services.AddHttpClient("currency_exchange_client", options =>
        {
            options.BaseAddress = new Uri("https://currency-exchange.p.rapidapi.com");
            options.DefaultRequestHeaders.Add("X-RapidAPI-Key",
                context.Configuration["BotConfiguration:CurrencyExchangeApiKey"]);
            options.DefaultRequestHeaders.Add("X-RapidAPI-Host", "currency-exchange.p.rapidapi.com");
        });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IReceiverService, ReceiverService>();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();