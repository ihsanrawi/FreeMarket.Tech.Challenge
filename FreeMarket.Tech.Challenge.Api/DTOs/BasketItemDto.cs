namespace FreeMarket.Tech.Challenge.Api.DTOs;

public class BasketItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } = 0m;
    public decimal TotalPrice { get; set; } = 0m;
    public bool IsDiscounted { get; set; }
    public decimal DiscountedPrice { get; set; } = 0m;
    public decimal ItemTotal { get; set; } = 0m; // Final price after discount
    public DateTime AddedAt { get; set; }
}