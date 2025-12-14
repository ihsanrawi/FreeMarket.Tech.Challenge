using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Entities;

namespace FreeMarket.Tech.Challenge.Api.Extensions;

// TODO: Can be replaced with AutoMapper in future
public static class MappingExtensions
{
    public static BasketDto MapToDto(this Basket basket)
    {
        var basketDto = new BasketDto
        {
            Id = basket.Id,
            CustomerEmail = basket.CustomerEmail,
            CreatedAt = basket.CreatedAt,
            UpdatedAt = basket.UpdatedAt,
            Items = basket.Items?.Select(item => item.MapToDto()).ToList() ?? new List<BasketItemDto>(),
            AppliedDiscount = basket.AppliedDiscount != null ? MapToDto(basket.AppliedDiscount) : null,
            ShippingAddress = basket.ShippingAddress != null ? MapToDto(basket.ShippingAddress) : null,
        };

        var items = basketDto.Items;
        basketDto.ShippingCost = basket.ShippingCost;
        basketDto.Subtotal = basket.GetSubtotal();
        basketDto.DiscountAmount = basket.GetDiscountAmount();
        basketDto.SubtotalAfterDiscount = basket.GetSubtotalAfterDiscount();
        basketDto.VatAmount = basket.GetVatAmount();
        basketDto.Total = basket.GetTotal();
        basketDto.TotalWithoutVat = basket.GetTotalWithoutVat();

        return basketDto;
    }

    public static BasketItemDto MapToDto(this BasketItem basketItem)
    {
        return new BasketItemDto
        {
            Id = basketItem.Id,
            ProductId = basketItem.ProductId,
            ProductName = basketItem.Product?.Name ?? string.Empty,
            ProductDescription = basketItem.Product?.Description ?? string.Empty,
            Quantity = basketItem.Quantity,
            UnitPrice = basketItem.UnitPrice,
            TotalPrice = basketItem.GetItemPrice(),
            AddedAt = basketItem.AddedAt
        };
    }

    private static DiscountDto MapToDto(Discount discount)
    {
        return new DiscountDto
        {
            Id = discount.Id,
            Code = discount.Code,
            Value = discount.DiscountPercentage,
        };
    }

    private static AddressDto MapToDto(Address address)
    {
        return new AddressDto
        {
            Country = address.Country,
        };
    }
}