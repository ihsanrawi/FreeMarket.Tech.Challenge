using FreeMarket.Tech.Challenge.Api.Entities;

namespace FreeMarket.Tech.Challenge.Api.Services;

public static class ShippingCostService
{
    public static decimal CalculateShippingCost(Address shippingAddress)
    {
        if (shippingAddress == null)
        {
            throw new ArgumentNullException(nameof(shippingAddress));
        }

        return shippingAddress.IsUkAddress() ? 5.99m : 8.99m;
    }
}