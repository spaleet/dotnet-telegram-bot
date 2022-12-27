using BotWorker.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace BotWorker.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureTelegramClient(this IServiceCollection services)
    {
        services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfig = sp.GetService<IOptions<BotConfiguration>>().Value;

                    TelegramBotClientOptions options = new(botConfig?.BotToken);

                    return new TelegramBotClient(options, httpClient);
                });
    }
}
