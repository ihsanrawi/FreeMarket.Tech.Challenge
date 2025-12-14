namespace FreeMarket.Tech.Challenge.Api.DTOs;

public class DiscountDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Value { get; set; }
}