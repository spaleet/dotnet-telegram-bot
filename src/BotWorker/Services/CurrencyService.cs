using BotWorker.Models;

namespace BotWorker.Services;
public interface ICurrencyService
{
    Task<CurrencyDto[]> GetAllCurrencies();
}

public class CurrencyService : ICurrencyService
{
    private readonly CurrencyDto[] Currencies = new CurrencyDto[]
    {
        new("USD", "🇺🇸"), new("EUR", "🇪🇺"),
        new("JPY", "🇯🇵"), new("GBP", "🇬🇧"),
        new("HKD", "🇭🇰"), new("INR", "🇮🇳"),
        new("CAD", "🇨🇦"), new("AED", "🇦🇪"),
        new("CNY", "🇨🇳"),  new("AUD", "🇦🇺")
    };

    public Task<CurrencyDto[]> GetAllCurrencies()
    {
        return Task.FromResult(Currencies);
    }
}
