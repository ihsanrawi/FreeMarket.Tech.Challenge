using FreeMarket.Tech.Challenge.Api.Entities;

namespace FreeMarket.Tech.Challenge.Api.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var product = new Product();

        // Assert
        Assert.Equal(default(Guid), product.Id);
        Assert.Equal(string.Empty, product.Name);
        Assert.Equal(string.Empty, product.Description);
        Assert.Equal(0m, product.Price);
        Assert.Equal(0, product.StockQuantity);
        Assert.True(product.CreatedAt > DateTime.MinValue);
        Assert.True(product.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Properties_ShouldSetAndGetCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Product";
        var description = "Test Product Description";
        var price = 29.99m;
        var stockQuantity = 50;
        var createdAt = DateTime.UtcNow;

        // Act
        var product = new Product
        {
            Id = id,
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(id, product.Id);
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(stockQuantity, product.StockQuantity);
        Assert.Equal(createdAt, product.CreatedAt);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(99.99)]
    [InlineData(1000.00)]
    [InlineData(9999.99)]
    public void Price_WithValidValues_ShouldAccept(decimal price)
    {
        // Arrange
        var product = new Product();

        // Act
        product.Price = price;

        // Assert
        Assert.Equal(price, product.Price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public void StockQuantity_WithValidValues_ShouldAccept(int stockQuantity)
    {
        // Arrange
        var product = new Product();

        // Act
        product.StockQuantity = stockQuantity;

        // Assert
        Assert.Equal(stockQuantity, product.StockQuantity);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Product Name")]
    [InlineData("Very Long Product Name That Contains Many Words And Characters")]
    [InlineData("")]
    public void Name_WithVariousValues_ShouldAccept(string name)
    {
        // Arrange
        var product = new Product();

        // Act
        product.Name = name;

        // Assert
        Assert.Equal(name, product.Name);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Product Description")]
    [InlineData("Very Long Product Description That Contains Many Words And Characters And Lots Of Detail About The Product Being Sold")]
    [InlineData("")]
    public void Description_WithVariousValues_ShouldAccept(string description)
    {
        // Arrange
        var product = new Product();

        // Act
        product.Description = description;

        // Assert
        Assert.Equal(description, product.Description);
    }

    [Fact]
    public void Id_ShouldAcceptValidGuid()
    {
        // Arrange
        var product = new Product();
        var id = Guid.NewGuid();

        // Act
        product.Id = id;

        // Assert
        Assert.Equal(id, product.Id);
    }

    [Fact]
    public void CreatedAt_ShouldAcceptValidDateTime()
    {
        // Arrange
        var product = new Product();
        var createdAt = new DateTime(2023, 1, 1, 12, 30, 45);

        // Act
        product.CreatedAt = createdAt;

        // Assert
        Assert.Equal(createdAt, product.CreatedAt);
    }

    [Fact]
    public void Product_CanBeInstantiatedWithObjectInitializer()
    {
        // Act
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Smartphone",
            Description = "Latest model smartphone with advanced features",
            Price = 699.99m,
            StockQuantity = 25
        };

        // Assert
        Assert.NotEqual(default(Guid), product.Id);
        Assert.Equal("Smartphone", product.Name);
        Assert.Equal("Latest model smartphone with advanced features", product.Description);
        Assert.Equal(699.99m, product.Price);
        Assert.Equal(25, product.StockQuantity);
        Assert.True(product.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public void MultipleProducts_CanHaveIndependentProperties()
    {
        // Arrange
        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Product 1",
            Price = 10.00m,
            StockQuantity = 5
        };

        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Product 2",
            Price = 20.00m,
            StockQuantity = 10
        };

        // Act & Assert
        Assert.NotEqual(product1.Id, product2.Id);
        Assert.NotEqual(product1.Name, product2.Name);
        Assert.NotEqual(product1.Price, product2.Price);
        Assert.NotEqual(product1.StockQuantity, product2.StockQuantity);
    }

    [Fact]
    public void Product_CanHandleNegativePrice()
    {
        // Arrange
        var product = new Product();

        // Act
        product.Price = -10.00m;

        // Assert
        Assert.Equal(-10.00m, product.Price);
    }

    [Fact]
    public void Product_CanHandleNegativeStockQuantity()
    {
        // Arrange
        var product = new Product();

        // Act
        product.StockQuantity = -5;

        // Assert
        Assert.Equal(-5, product.StockQuantity);
    }
}