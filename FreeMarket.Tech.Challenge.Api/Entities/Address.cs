namespace FreeMarket.Tech.Challenge.Api.Entities;

public record Address
{
    public Guid Id { get; set; }
    public string Country { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public bool IsUkAddress() => Country.Equals("UK", StringComparison.OrdinalIgnoreCase) ||
                                 Country.Equals("United Kingdom", StringComparison.OrdinalIgnoreCase);
}