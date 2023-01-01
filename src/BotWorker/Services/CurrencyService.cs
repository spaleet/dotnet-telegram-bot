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
            new("USD", "🇺🇸"), new("EUR", "🇪🇺"),
            new("JPY", "🇯🇵"), new("GBP", "🇬🇧"),
            new("HKD", "🇭🇰"), new("INR", "🇮🇳"),
            new("CAD", "🇨🇦"), new("AED", "🇦🇪"),
            new("CNY", "🇨🇳"),  new("AUD", "🇦🇺")
        };

        return Task.FromResult(currencies);
    }
}
