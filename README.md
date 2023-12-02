# Currency Exchange Telegram Bot

This is a Telegram bot built with .NET 7 that allows users to find the current exchange rate between two currencies.

## Features

- Simple and intuitive command-based user interface
- Currently supports 10 different world currencies
- Real-time exchange rates using [Currency Exchange API](https://rapidapi.com/fyhao/api/currency-exchange/)
- Easy integration with the Telegram Bot API
  
### Requirements

1. Install the latest [.NET SDK](https://dotnet.microsoft.com/download/dotnet/7.0)

### Configuration

Provide your [Telegram Bot token](https://core.telegram.org/bots/api) & [Currency Exchange](https://rapidapi.com/fyhao/api/currency-exchange/) in `appsettings*.json`.

```json
"BotConfiguration": {
    "BotToken": "{ Token }",
    "CurrencyExchangeApiKey": "{ Api Key }"
}
```

### Installation

Clone the project source:
```bash
git clone https://github.com/spaleet/dotnet-telegram-bot
cd src/
```

Start the worker service:
```bash
cd /BotWorker
dotnet run
```

### Usage

<br />

![](/docs/sample.png)

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT). Feel free to use it in any way you like, but I take no responsibility for any consequences of that use
