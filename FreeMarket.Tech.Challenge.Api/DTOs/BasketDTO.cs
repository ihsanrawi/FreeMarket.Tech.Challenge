using FreeMarket.Tech.Challenge.Api.Extensions;

namespace FreeMarket.Tech.Challenge.Api.DTOs;

public class BasketDto
{
    public Guid Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
    public DiscountDto? AppliedDiscount { get; set; }
    public AddressDto? ShippingAddress { get; set; }
    public decimal ShippingCost { get; set; } = 0m;
    public decimal Subtotal { get; set; } = 0m;
    public decimal DiscountAmount { get; set; } = 0m;
    public decimal SubtotalAfterDiscount { get; set; } = 0m;
    public decimal VatAmount { get; set; } = 0m;
    public decimal Total { get; set; } = 0m;
    public decimal TotalWithoutVat { get; set; } = 0m;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}