using BotWorker.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace BotWorker.Services;

public interface ICurrencyService
{
    Task<CurrencyDto[]> GetAllCurrencies();
    Task<decimal> GetExchangeRate(string from, string to);
}

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _client;

    private readonly CurrencyDto[] Currencies =
    {
        new("USD", "🇺🇸"), new("EUR", "🇪🇺"),
        new("JPY", "🇯🇵"), new("GBP", "🇬🇧"),
        new("HKD", "🇭🇰"), new("INR", "🇮🇳"),
        new("CAD", "🇨🇦"), new("AED", "🇦🇪"),
        new("CNY", "🇨🇳"), new("AUD", "🇦🇺")
    };

    public CurrencyService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("currency_exchange_client");
    }

    public Task<CurrencyDto[]> GetAllCurrencies()
    {
        return Task.FromResult(Currencies);
    }

    public async Task<decimal> GetExchangeRate(string from, string to)
    {
        if (string.IsNullOrEmpty(from))
            throw new ArgumentNullException(nameof(from));

        if (string.IsNullOrEmpty(to))
            throw new ArgumentNullException(nameof(to));

        var query = new Dictionary<string, string>
        {
            ["from"] = from,
            ["to"] = to,
            ["q"] = "1.0"
        };

        string uri = QueryHelpers.AddQueryString("exchange", query);

        var res = await _client.GetAsync(uri);
        res.EnsureSuccessStatusCode();

        string content = await res.Content.ReadAsStringAsync();

        return Convert.ToDecimal(content);
    }
}