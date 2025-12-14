namespace FreeMarket.Tech.Challenge.Api.Entities;

public class BasketItem
{
    public Guid Id { get; set; }
    public Guid BasketId { get; set; }
    public Basket Basket { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } = 0m;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updates the quantity of the basket item
    /// </summary>
    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        Quantity = quantity;
    }

    /// <summary>
    /// Gets the total price for this basket item, considering quantity and any discounts
    /// </summary>
    public decimal GetItemPrice()
    {
        return Product.IsDiscounted && Product.DiscountedPrice > 0
            ? Product.DiscountedPrice * Quantity
            : Product.Price * Quantity;
    }
}