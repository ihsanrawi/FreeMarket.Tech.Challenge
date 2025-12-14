using FreeMarket.Tech.Challenge.Api.Entities;

namespace FreeMarket.Tech.Challenge.Api.Tests.Entities;

public class BasketItemTests
{
    private readonly BasketItem _basketItem;
    private readonly Product _testProduct;

    public BasketItemTests()
    {
        _testProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            StockQuantity = 100
        };

        _basketItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = Guid.NewGuid(),
            ProductId = _testProduct.Id,
            Product = _testProduct,
            Quantity = 2,
            UnitPrice = _testProduct.Price,
            AddedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void TotalPrice_ShouldReturnCorrectValue()
    {
        // Arrange
        _basketItem.Quantity = 3;
        _basketItem.Product.Price = 5.50m;

        // Act
        var result = _basketItem.GetItemPrice();

        // Assert
        Assert.Equal(16.50m, result); // 3 * 5.50
    }

    [Fact]
    public void UpdateQuantity_WithValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var newQuantity = 5;

        // Act
        _basketItem.UpdateQuantity(newQuantity);

        // Assert
        Assert.Equal(newQuantity, _basketItem.Quantity);
    }

    [Fact]
    public void UpdateQuantity_WithZeroQuantity_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _basketItem.UpdateQuantity(0));
    }

    [Fact]
    public void UpdateQuantity_WithNegativeQuantity_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _basketItem.UpdateQuantity(-1));
    }

    [Fact]
    public void GetItemPrice_ShouldReturnCorrectValue()
    {
        // Arrange
        _basketItem.Quantity = 3;
        _basketItem.Product = new Product { Id = Guid.NewGuid(), Price = 7.25m };

        // Act
        var result = _basketItem.GetItemPrice();

        // Assert
        Assert.Equal(21.75m, result); // 3 * 7.25
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var basketItem = new BasketItem();

        // Assert
        Assert.Equal(0m, basketItem.UnitPrice);
        Assert.True(basketItem.AddedAt > DateTime.MinValue);
        Assert.True(basketItem.AddedAt <= DateTime.UtcNow);

        // GetItemPrice should throw NullReferenceException when Product is null
        Assert.Throws<NullReferenceException>(() => basketItem.GetItemPrice());
    }

    [Fact]
    public void TotalPrice_WithZeroQuantity_ShouldReturnZero()
    {
        // Arrange
        _basketItem.Quantity = 0;
        _basketItem.UnitPrice = 10.00m;

        // Act
        var result = _basketItem.GetItemPrice();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void TotalPrice_WithZeroUnitPrice_ShouldReturnZero()
    {
        // Arrange
        _basketItem.Quantity = 5;
        _basketItem.Product.Price = 0m;

        // Act
        var result = _basketItem.GetItemPrice();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetItemPrice_WithNullProduct_ShouldThrowNullReferenceException()
    {
        // Arrange
        _basketItem.Product = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _basketItem.GetItemPrice());
    }

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(5, 2.50, 12.50)]
    [InlineData(10, 0.99, 9.90)]
    [InlineData(100, 1.00, 100.00)]
    public void TotalPrice_WithVariousQuantitiesAndPrices_ShouldReturnCorrectValue(
        int quantity, decimal productPrice, decimal expectedTotal)
    {
        // Arrange
        _basketItem.Quantity = quantity;
        _basketItem.Product.Price = productPrice;

        // Act
        var result = _basketItem.GetItemPrice();

        // Assert
        Assert.Equal(expectedTotal, result);
    }

    [Theory]
    [InlineData(1, 15.50, 15.50)]
    [InlineData(3, 9.99, 29.97)]
    [InlineData(7, 2.00, 14.00)]
    [InlineData(12, 1.25, 15.00)]
    public void GetItemPrice_WithVariousQuantitiesAndProductPrices_ShouldReturnCorrectValue(
        int quantity, decimal productPrice, decimal expectedPrice)
    {
        // Arrange
        _basketItem.Quantity = quantity;
        _basketItem.Product = new Product { Id = Guid.NewGuid(), Price = productPrice };

        // Act
        var result = _basketItem.GetItemPrice();

        // Assert
        Assert.Equal(expectedPrice, result);
    }
}