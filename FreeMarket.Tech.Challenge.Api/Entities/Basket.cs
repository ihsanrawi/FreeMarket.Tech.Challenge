namespace FreeMarket.Tech.Challenge.Api.Entities;

public class Basket
{
    public Guid Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = new();
    public Discount? AppliedDiscount { get; set; }
    public Address? ShippingAddress { get; set; }
    public decimal ShippingCost { get; set; } = 0m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Removes an item from the basket by its ID
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Add discount to the basket
    /// </summary>
    /// <param name="discount"></param>
    /// <param name="currentTime"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ApplyDiscount(Discount discount, DateTime? currentTime = null)
    {
        if (!discount.IsValid())
        {
            throw new InvalidOperationException("Discount is not valid");
        }

        AppliedDiscount = discount;
        UpdatedAt = currentTime ?? DateTime.UtcNow;
    }

    
    /// <summary>
    /// Gets the subtotal of the basket before discounts and shipping
    /// </summary>
    public decimal GetSubtotal()
    {
        return Items.Sum(item => item.GetItemPrice());
    }

    /// <summary>
    /// Gets the discount amount applied to the basket
    /// </summary>
    public decimal GetDiscountAmount()
    {
        if (AppliedDiscount == null)
        {
            return 0m;
        }

        var eligibleItems = Items.Where(i => !i.Product.IsDiscounted);
        var eligibleAmount = eligibleItems.Sum(item => item.GetItemPrice());

        return eligibleAmount * (AppliedDiscount.DiscountPercentage / 100m);
    }

    /// <summary>
    /// Gets the subtotal after applying discounts
    /// </summary>
    public decimal GetSubtotalAfterDiscount()
    {
        return GetSubtotal() - GetDiscountAmount();
    }

    /// <summary>
    /// Gets the VAT amount for the basket
    /// </summary>
    public decimal GetVatAmount(decimal vatRate = 20m)
    {
        var subtotalWithShipping = GetSubtotalAfterDiscount() + ShippingCost;
        return subtotalWithShipping * (vatRate / 100m);
    }

    /// <summary>
    /// Gets the total amount of the basket including shipping and VAT
    /// </summary>
    public decimal GetTotal(decimal vatRate = 20m)
    {
        return GetSubtotalAfterDiscount()
               + ShippingCost
               + GetVatAmount(vatRate);
    }

    /// <summary>
    /// Gets the total amount of the basket excluding VAT
    /// </summary>
    public decimal GetTotalWithoutVat()
    {
        return GetSubtotalAfterDiscount()
               + ShippingCost;
    }

    /// <summary>
    /// Set shipping address and cost
    /// </summary>
    public void SetShippingAddress(Address address, decimal shippingCost, DateTime? currentTime = null)
    {
        ShippingAddress = address;
        ShippingCost = shippingCost;
        UpdatedAt = currentTime ?? DateTime.UtcNow;
    }
}