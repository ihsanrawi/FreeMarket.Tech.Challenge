using FreeMarket.Tech.Challenge.Api.Entities;

namespace FreeMarket.Tech.Challenge.Api.Tests.Entities;

public class BasketTests
{
    // Test Factory Methods for isolated test data creation
    private static Basket CreateBasket(string? customerEmail = null)
    {
        return new Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = customerEmail ?? "test@example.com"
        };
    }

    private static Product CreateProduct(decimal price = 10.00m, bool isDiscounted = false, decimal? discountedPrice = null, string? description = null)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = description ?? "Test Description",
            Price = price,
            IsDiscounted = isDiscounted,
            DiscountedPrice = discountedPrice ?? 0m,
            StockQuantity = 100
        };
    }

    private static Discount CreateValidDiscount(decimal percentage = 10m)
    {
        return new Discount
        {
            Id = Guid.NewGuid(),
            Code = "TEST10",
            DiscountPercentage = percentage,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(30)
        };
    }

    [Fact]
    public void RemoveItem_WithNonExistentItemId_ShouldNotThrowException()
    {
        // Arrange
        var basket = CreateBasket();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - Should not throw
        basket.RemoveItem(nonExistentId);
        Assert.Empty(basket.Items);
    }

    [Fact]
    public void ApplyDiscount_WithValidDiscount_ShouldSetAppliedDiscount()
    {
        // Arrange
        var basket = CreateBasket();
        var discount = CreateValidDiscount();

        // Act
        basket.ApplyDiscount(discount);

        // Assert
        Assert.Equal(discount, basket.AppliedDiscount);
    }

    [Fact]
    public void ApplyDiscount_WithInactiveDiscount_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var basket = CreateBasket();
        var discount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "INACTIVE",
            DiscountPercentage = 10m,
            IsActive = false
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => basket.ApplyDiscount(discount));
    }

    [Fact]
    public void ApplyDiscount_WithExpiredDiscount_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var basket = CreateBasket();
        var discount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "EXPIRED",
            DiscountPercentage = 10m,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => basket.ApplyDiscount(discount));
    }

    [Fact]
    public void GetSubtotal_WithNoItems_ShouldReturnZero()
    {
        // Arrange
        var basket = CreateBasket();

        // Act
        var result = basket.GetSubtotal();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetDiscountAmount_WithNoDiscount_ShouldReturnZero()
    {
        // Arrange
        var basket = CreateBasket();

        // Act
        var result = basket.GetDiscountAmount();

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void SetShippingAddress_WithValidAddress_ShouldUpdateShippingDetails()
    {
        // Arrange
        var basket = CreateBasket();
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "customer@example.com",
            Country = "US"
        };
        var shippingCost = 8.99m;

        // Act
        basket.SetShippingAddress(address, shippingCost);

        // Assert
        Assert.Equal(address, basket.ShippingAddress);
        Assert.Equal(shippingCost, basket.ShippingCost);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var basket = new Basket();

        // Assert
        Assert.NotNull(basket.Items);
        Assert.Empty(basket.Items);
        Assert.Equal(string.Empty, basket.CustomerEmail);
        Assert.Null(basket.ShippingAddress);
        Assert.Equal(0m, basket.ShippingCost);
        Assert.Null(basket.AppliedDiscount);
        Assert.True(basket.CreatedAt > DateTime.MinValue);
        Assert.True(basket.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public void ApplyDiscount_ShouldUpdateUpdatedAtTimestamp()
    {
        // Arrange
        var basket = CreateBasket();
        var discount = CreateValidDiscount();
        var testTime = new DateTime(2023, 1, 1, 12, 0, 1);

        // Act
        basket.ApplyDiscount(discount, testTime);

        // Assert
        Assert.Equal(testTime, basket.UpdatedAt);
    }

    [Fact]
    public void SetShippingAddress_ShouldUpdateUpdatedAtTimestamp()
    {
        // Arrange
        var basket = CreateBasket();
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "customer@example.com",
            Country = "US"
        };
        var testTime = new DateTime(2023, 1, 1, 12, 0, 1);

        // Act
        basket.SetShippingAddress(address, 8.99m, testTime);

        // Assert
        Assert.Equal(testTime, basket.UpdatedAt);
    }

    [Fact]
    public void GetSubtotal_WithItems_ShouldCalculateCorrectly()
    {
        // Arrange
        var basket = CreateBasket();
        var product1 = CreateProduct(10.00m, false, null, "Product 1");
        product1.Name = "Product 1";
        var product2 = CreateProduct(20.00m, false, null, "Product 2");
        product2.Name = "Product 2";

        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product1.Id,
            Product = product1,
            Quantity = 2,
            UnitPrice = product1.Price
        }); // 2 * 10 = 20

        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product2.Id,
            Product = product2,
            Quantity = 1,
            UnitPrice = product2.Price
        }); // 1 * 20 = 20

        // Act
        var result = basket.GetSubtotal();

        // Assert
        Assert.Equal(40m, result);
    }

    [Fact]
    public void GetSubtotal_WithDiscountedProducts_ShouldUseDiscountedPrices()
    {
        // Arrange
        var basket = CreateBasket();
        var normalProduct = CreateProduct(10.00m, false, null, "Normal Product");
        normalProduct.Name = "Normal Product";
        var discountedProduct = CreateProduct(20.00m, true, 15.00m, "Discounted Product");
        discountedProduct.Name = "Discounted Product";

        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = normalProduct.Id,
            Product = normalProduct,
            Quantity = 1,
            UnitPrice = normalProduct.Price
        }); // 10

        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = discountedProduct.Id,
            Product = discountedProduct,
            Quantity = 2,
            UnitPrice = discountedProduct.Price
        }); // 2 * 15 = 30

        // Act
        var result = basket.GetSubtotal();

        // Assert
        Assert.Equal(40m, result);
    }

    [Fact]
    public void GetDiscountAmount_WithMixedDiscountedProducts_ShouldExcludeAlreadyDiscountedItems()
    {
        // Arrange
        var basket = CreateBasket();
        var discount = CreateValidDiscount(10m); // 10% discount
        basket.ApplyDiscount(discount);

        var normalProduct = CreateProduct(10.00m, false, null, "Normal Product"); // Eligible for discount
        normalProduct.Name = "Normal Product";
        var discountedProduct = CreateProduct(20.00m, true, 15.00m, "Discounted Product"); // NOT eligible
        discountedProduct.Name = "Discounted Product";

        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = normalProduct.Id,
            Product = normalProduct,
            Quantity = 2,
            UnitPrice = normalProduct.Price
        }); // 2 * 10 = 20 (eligible)

        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = discountedProduct.Id,
            Product = discountedProduct,
            Quantity = 1,
            UnitPrice = discountedProduct.Price
        }); // 15 (not eligible)

        // Act
        var result = basket.GetDiscountAmount();

        // Assert - Should be 10% of 20 (eligible amount only)
        Assert.Equal(2m, result);
    }

    [Fact]
    public void GetTotal_WithItemsAndShipping_ShouldCalculateCorrectly()
    {
        // Arrange
        var basket = CreateBasket();
        var product = CreateProduct(10.00m, false, null, "Test Product");
        product.Name = "Test Product";
        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            UnitPrice = product.Price
        }); // 20 subtotal

        basket.SetShippingAddress(new Address { Id = Guid.NewGuid(), Country = "US", CustomerEmail = "test@example.com" }, 8.99m);

        // Act
        var result = basket.GetTotal();

        // Assert - 20 subtotal + 8.99 shipping + VAT (20% of 28.99 = 5.80) = 34.788
        Assert.Equal(34.788m, result);
    }

    [Fact]
    public void GetTotalWithVatRate_ShouldUseCustomRate()
    {
        // Arrange
        var basket = CreateBasket();
        var product = CreateProduct(100.00m, false, null, "Test Product");
        product.Name = "Test Product";
        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            UnitPrice = product.Price
        }); // 100 subtotal

        // Act
        var result = basket.GetTotal(10m); // 10% VAT

        // Assert - 100 subtotal + 0 shipping + VAT (10% of 100 = 10) = 110
        Assert.Equal(110m, result);
    }

    [Fact]
    public void GetSubtotalAfterDiscount_ShouldReturnCorrectValue()
    {
        // Arrange
        var basket = CreateBasket();
        var discount = CreateValidDiscount(20m); // 20% discount
        basket.ApplyDiscount(discount);

        var product = CreateProduct(10.00m, false, null, "Test Product");
        product.Name = "Test Product";
        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            UnitPrice = product.Price
        }); // 20 subtotal

        // Act
        var result = basket.GetSubtotalAfterDiscount();

        // Assert - 20 subtotal - 4 discount (20% of 20) = 16
        Assert.Equal(16m, result);
    }

    [Fact]
    public void GetTotalWithoutVat_ShouldReturnSubtotalPlusShipping()
    {
        // Arrange
        var basket = CreateBasket();
        var product = CreateProduct(10.00m, false, null, "Test Product");
        product.Name = "Test Product";
        basket.Items.Add(new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            UnitPrice = product.Price
        }); // 10 subtotal

        basket.SetShippingAddress(new Address { Id = Guid.NewGuid(), Country = "UK", CustomerEmail = "test@example.com" }, 5.99m);

        // Act
        var result = basket.GetTotalWithoutVat();

        // Assert - 10 subtotal + 5.99 shipping = 15.99
        Assert.Equal(15.99m, result);
    }
}