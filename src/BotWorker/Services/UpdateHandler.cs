using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotWorker.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string error = exception is ApiRequestException apiException
            ? $"Telegram API Error: \n[{apiException.ErrorCode}]\n{apiException.Message}"
            : exception.ToString();

        _logger.LogInformation("Handler Error: {error}", error);

        // 5 second cooldown incase of a network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
            case UpdateType.EditedMessage:
                await OnMessageReceived(update.Message, cancellationToken);
                break;

            case UpdateType.CallbackQuery:
                await OnCallbackQueryReceived(update.CallbackQuery, cancellationToken);
                break;

            default:
                await OnUnknownUpdate(update);
                break;
        }
    }

    private async Task OnMessageReceived(Message message, CancellationToken ct)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);

        string msgText = message.Text;

        string command = msgText.Split(' ')[0];

        var action = command switch
        {
            // welcome & display commands
            "/start" => Start(message, ct),
            "/exchange" => SelectFirstExchange(message, ct),

            // Use a command /help
            "/help" or _ => Help(message, ct)
        };

        Message sentMessage = await action;

        _logger.LogInformation("The message was sent with id: {msgId}", sentMessage.MessageId);
    }

    private Task OnUnknownUpdate(Update update)
    {
        _logger.LogInformation("Unknown update type: {type}", update.Type);
        return Task.CompletedTask;
    }

    // Inline Keyboard callback data
    private async Task OnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken ct)
    {
        _logger.LogInformation("Received inline keyboard callback from: {queryId} with data: {data}", callbackQuery.Id, callbackQuery.Data);

        if (callbackQuery.Data == "help")
            await Help(callbackQuery.Message, ct);

        if (callbackQuery.Data == "start-exchange")
            await SelectFirstExchange(callbackQuery.Message, ct);


    }

    #region OnMessage handlers

    private async Task<Message> Start(Message message, CancellationToken ct)
    {
        await _botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: ct);

        string commands = $"Welcome to @spaleet_bot 😀 ! \n\n These are all the available commands : \n" +
                             "/help - see available commands";

        InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Currency Exchange 💵", "start-exchange"),
                        InlineKeyboardButton.WithCallbackData("Help ℹ", "help")
                    },
                });

        return await _botClient.SendTextMessageAsync(message.Chat.Id,
                                                     commands,
                                                     replyMarkup: inlineKeyboard,
                                                     cancellationToken: ct);
    }

    private async Task<Message> Help(Message message, CancellationToken ct)
    {
        string commands = "These are all the available commands : \n" +
                             "/help - see available commands";

        return await _botClient.SendTextMessageAsync(message.Chat.Id,
                                                     commands,
                                                     replyMarkup: new ReplyKeyboardRemove(),
                                                     cancellationToken: ct);
    }

    private async Task<Message> SelectFirstExchange(Message message, CancellationToken ct)
    {
        string commands = "Select your first currency : \n";

        InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("USD 🇺🇸", "finish-exchange USD"),
                        InlineKeyboardButton.WithCallbackData("CAD 🇨🇦", "finish-exchange CAD")
                    },
                });

        return await _botClient.SendTextMessageAsync(message.Chat.Id,
                                                     commands,
                                                     replyMarkup: inlineKeyboard,
                                                     cancellationToken: ct);
    }

    #endregion
}
