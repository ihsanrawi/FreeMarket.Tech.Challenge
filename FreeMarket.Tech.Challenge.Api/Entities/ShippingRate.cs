namespace FreeMarket.Tech.Challenge.Api.Entities;

public class ShippingRate
{
    public Guid Id { get; set; }
    public string Country { get; set; } = string.Empty;
    public decimal Cost { get; set; } = 0m;
}