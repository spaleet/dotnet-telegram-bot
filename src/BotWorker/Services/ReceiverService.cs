using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace BotWorker.Services;

public interface IReceiverService
{
    Task ReceiveAsync(CancellationToken stoppingToken);
}

public class ReceiverService : IReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ReceiverService> _logger;
    private readonly IUpdateHandler _updateHandler;

    public ReceiverService(
        ITelegramBotClient botClient,
        UpdateHandler updateHandler,
        ILogger<ReceiverService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            ThrowPendingUpdates = true
        };

        _logger.LogInformation("Receiving successfully started");

        // start receiving
        await _botClient.ReceiveAsync(
            _updateHandler,
            receiverOptions,
            stoppingToken);
    }
}