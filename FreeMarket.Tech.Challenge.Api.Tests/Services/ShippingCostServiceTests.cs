using FreeMarket.Tech.Challenge.Api.Entities;
using FreeMarket.Tech.Challenge.Api.Services;

namespace FreeMarket.Tech.Challenge.Api.Tests.Services;

public class ShippingCostServiceTests
{
    [Fact]
    public void CalculateShippingCost_WithNullAddress_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ShippingCostService.CalculateShippingCost(null!));
    }

    [Theory]
    [InlineData(5.99, "UK")]
    [InlineData(8.99, "US")]
    [InlineData(5.99, "United Kingdom")]
    [InlineData(8.99, "France")]
    public void CalculateShippingCost_ExpectedRatesShouldMatchSpecification(decimal expectedRate, string country)
    {
        // Arrange
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Country = country
        };

        // Act
        var result = ShippingCostService.CalculateShippingCost(address);

        // Assert
        Assert.Equal(expectedRate, result);
        Assert.IsType<decimal>(result);
    }
}