using BotWorker.Models;
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
    private readonly ICurrencyService _currencyClient;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, ICurrencyService currencyClient)
    {
        _botClient = botClient;
        _logger = logger;
        _currencyClient = currencyClient;
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
            "/exchange" => SelectFirstCurrency(message, ct),

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

        if (callbackQuery.Data == "select-first-currency")
            await SelectFirstCurrency(callbackQuery.Message, ct);

        if (callbackQuery.Data.Split(" ")[0] == "select-second-currency")
            await SelectSecondCurrency(callbackQuery.Message, callbackQuery.Data.Split(" ")[1], ct);

        if (callbackQuery.Data.Split(" ")[0] == "finish-exchange")
            await GetExchangeRate(callbackQuery.Message, callbackQuery.Data.Split(" ")[1], callbackQuery.Data.Split(" ")[2], ct);
    }

    #region OnMessage handlers

    private async Task<Message> Start(Message message, CancellationToken ct)
    {
        await _botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: ct);

        string commands = $"Welcome to @spaleet_bot 😀 ! \n\n These are all the available commands : \n" +
                             "/exchange - currency exchange \n" +
                             "/help - see available commands";

        InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Currency Exchange 💵", "select-first-currency"),
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
                             "/exchange - currency exchange \n" +
                             "/help - see available commands";

        return await _botClient.SendTextMessageAsync(message.Chat.Id,
                                                     commands,
                                                     replyMarkup: new ReplyKeyboardRemove(),
                                                     cancellationToken: ct);
    }

    private async Task<Message> SelectFirstCurrency(Message message, CancellationToken ct)
    {
        string text = "Select your first currency : \n";

        var currencies = await _currencyClient.GetAllCurrencies();

        int currenciesRange = currencies.Count() - 1;
        bool listCountIsOdd = currencies.Count() % 2 != 0;

        // Rows containing two currencies
        int rows = currencies.Count() / 2;

        // Round up rows
        if (listCountIsOdd)
            rows += 1;

        List<IEnumerable<InlineKeyboardButton>> keyboardValues = new();

        // Fill inline kyboard rows
        for (int i = 0; i < rows; i++)
        {
            int firstCurrency = i + (i + 1) - 1;
            int secondCurrency = i + (i + 2) - 1;

            // Check for index out of range
            if (firstCurrency > currenciesRange)
                break;

            // Check if there's only one item for row
            if (secondCurrency > currenciesRange)
            {
                keyboardValues.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(currencies[firstCurrency].ToString(), $"select-second-currency {currencies[firstCurrency].Name}")
                });

                break;
            }

            // Add two items to row
            keyboardValues.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(currencies[firstCurrency].ToString(), $"select-second-currency {currencies[firstCurrency].Name}"),
                InlineKeyboardButton.WithCallbackData(currencies[secondCurrency].ToString(), $"select-second-currency {currencies[secondCurrency].Name}"),
            });
        }

        var inlineKeyboard = new InlineKeyboardMarkup(keyboardValues);

        return await _botClient.SendTextMessageAsync(message.Chat.Id,
                                                     text,
                                                     replyMarkup: inlineKeyboard,
                                                     cancellationToken: ct);
    }

    private async Task<Message> SelectSecondCurrency(Message message, string firstId, CancellationToken ct)
    {
        string text = "Select your second currency : \n";

        var currencies = await _currencyClient.GetAllCurrencies();

        var firstSelected = currencies.Where(x => x.Name == firstId).FirstOrDefault();
        int firstSelectedIndex = Array.FindIndex(currencies, x => x.Name == firstId);

        // Remove selected currency from array
        // And re-order array
        var currenciesReordered = new List<CurrencyDto>(currencies);
        currenciesReordered.RemoveAt(firstSelectedIndex);
        currencies = currenciesReordered.ToArray();

        int currenciesRange = currencies.Count() - 1;
        bool listCountIsOdd = currencies.Count() % 2 != 0;

        // Rows containing two currencies
        int rows = currencies.Count() / 2;

        // Round up rows
        if (listCountIsOdd)
            rows += 1;

        List<IEnumerable<InlineKeyboardButton>> keyboardValues = new();

        // Fill inline kyboard rows
        for (int i = 0; i < rows; i++)
        {
            int firstCurrency = i + (i + 1) - 1;
            int secondCurrency = i + (i + 2) - 1;

            // Check for index out of range
            if (firstCurrency > currenciesRange)
                break;

            // Check if there's only one item for row
            if (secondCurrency > currenciesRange)
            {
                keyboardValues.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(currencies[firstCurrency].ToString(), $"finish-exchange {firstId} {currencies[firstCurrency].Name}")
                });

                break;
            }

            // Add two items to row
            keyboardValues.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(currencies[firstCurrency].ToString(), $"finish-exchange {firstId} {currencies[firstCurrency].Name}"),
                InlineKeyboardButton.WithCallbackData(currencies[secondCurrency].ToString(), $"finish-exchange {firstId} {currencies[secondCurrency].Name}"),
            });
        }

        var inlineKeyboard = new InlineKeyboardMarkup(keyboardValues);

        return await _botClient.SendTextMessageAsync(message.Chat.Id,
                                                     text,
                                                     replyMarkup: inlineKeyboard,
                                                     cancellationToken: ct);
    }

    private async Task<Message> GetExchangeRate(Message message, string firstId, string secondId, CancellationToken ct)
    {
        var currencies = await _currencyClient.GetAllCurrencies();

        var firstSelected = currencies.Where(x => x.Name == firstId).FirstOrDefault();
        var secondSelected = currencies.Where(x => x.Name == secondId).FirstOrDefault();

        decimal exchangeRate = await _currencyClient.GetExchangeRate(firstSelected.Name, secondSelected.Name);


        string text = $"Exchange Rate from {firstSelected} to {secondSelected} is *{exchangeRate}*";

        return await _botClient.SendTextMessageAsync(message.Chat.Id,
                                                     text,
                                                     replyMarkup: new ReplyKeyboardRemove(),
                                                     parseMode: ParseMode.Markdown,
                                                     cancellationToken: ct);
    }

    #endregion
}
