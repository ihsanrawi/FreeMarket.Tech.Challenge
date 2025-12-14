namespace FreeMarket.Tech.Challenge.Api.Entities;

public class Discount
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; } = 0m; // e.g., 10 for 10%
    public bool IsActive { get; set; } = true;
    public DateTime ValidTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks if the discount is currently valid
    /// </summary>
    public bool IsValid() =>
        IsActive &&
        (ValidTo == default || ValidTo > DateTime.UtcNow);
}