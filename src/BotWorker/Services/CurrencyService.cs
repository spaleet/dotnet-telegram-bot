using BotWorker.Models;

namespace BotWorker.Services;
public interface ICurrencyService
{
    Task<CurrencyDto[]> GetAllCurrencies();
}

public class CurrencyService : ICurrencyService
{
    public Task<CurrencyDto[]> GetAllCurrencies()
    {
        var currencies = new CurrencyDto[]
        {
            new("USD", "🇺🇸"), new("CAD", "🇨🇦"),
            new("USD", "🇺🇸"), new("CAD", "🇨🇦"),
            new("USD", "🇺🇸"), new("CAD", "🇨🇦"),
            new("USD", "🇺🇸"), new("CAD", "🇨🇦"),
            new("USD", "🇺🇸"), new("CAD", "🇨🇦"),
            new("USD", "🇺🇸"), new("CAD", "🇨🇦"),
        };

        return Task.FromResult(currencies);
    }
}
