namespace FreeMarket.Tech.Challenge.Api.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0m;
    public bool IsDiscounted { get; set; }
    public decimal DiscountedPrice { get; set; } = 0m;
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}