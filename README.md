# dotnet-telegram-bot

This is a Currency Exchange Telegram Bot built with **.net 7**

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

### Screenshots

<img src="/docs/sample.png" alt="bot" height="500" />