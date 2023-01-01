namespace BotWorker.Models;

public record CurrencyDto
{
    public CurrencyDto(string name, string emoji)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Emoji = emoji;
    }

    public string Id { get; }
    public string Name { get; set; }
    public string Emoji { get; set; }

    public override string ToString()
    {
        return $"{Name} {Emoji}";
    }
}
