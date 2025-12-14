using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.Entities;
using FreeMarket.Tech.Challenge.Api.Features.Basket;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Tests.Features.Basket;

public class GetTotalCostTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GetTotalCostHandler _handler;

    public GetTotalCostTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _handler = new GetTotalCostHandler(_context);
    }

    [Fact]
    public async Task Handle_WithEmptyBasket_ShouldReturnZero()
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Baskets.Add(basket);
        await _context.SaveChangesAsync();

        var queryWithVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: true);
        var queryWithoutVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: false);

        // Act
        var resultWithVat = await _handler.Handle(queryWithVat, CancellationToken.None);
        var resultWithoutVat = await _handler.Handle(queryWithoutVat, CancellationToken.None);

        // Assert
        Assert.Equal(0m, resultWithVat);
        Assert.Equal(0m, resultWithoutVat);
    }

    [Fact]
    public async Task Handle_WithNonExistentBasket_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistentBasketId = Guid.NewGuid();
        var query = new GetTotalCostQuery(nonExistentBasketId, IsTotalWithVat: true);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithBasketContainingRegularItems_ShouldCalculateCorrectTotalWithVat()
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = false
        };

        var basketItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            UnitPrice = product.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Baskets.Add(basket);
        _context.Products.Add(product);
        _context.BasketItems.Add(basketItem);
        await _context.SaveChangesAsync();

        var query = new GetTotalCostQuery(basket.Id, IsTotalWithVat: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Subtotal: 100 * 2 = 200
        // VAT (20%): 200 * 0.20 = 40
        // Total: 200 + 40 = 240
        Assert.Equal(240.00m, result);
    }

    [Fact]
    public async Task Handle_WithBasketContainingRegularItems_ShouldCalculateCorrectTotalWithoutVat()
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = false
        };

        var basketItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            UnitPrice = product.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Baskets.Add(basket);
        _context.Products.Add(product);
        _context.BasketItems.Add(basketItem);
        await _context.SaveChangesAsync();

        var query = new GetTotalCostQuery(basket.Id, IsTotalWithVat: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Subtotal: 100 * 2 = 200
        // Total without VAT: 200
        Assert.Equal(200.00m, result);
    }

    [Fact]
    public async Task Handle_WithBasketContainingDiscountedItems_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var discountedProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Discounted Product",
            Description = "Test Description",
            Price = 100.00m,
            DiscountedPrice = 80.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = true
        };

        var basketItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = discountedProduct.Id,
            Product = discountedProduct,
            Quantity = 2,
            UnitPrice = discountedProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Baskets.Add(basket);
        _context.Products.Add(discountedProduct);
        _context.BasketItems.Add(basketItem);
        await _context.SaveChangesAsync();

        var queryWithVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: true);
        var queryWithoutVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: false);

        // Act
        var resultWithVat = await _handler.Handle(queryWithVat, CancellationToken.None);
        var resultWithoutVat = await _handler.Handle(queryWithoutVat, CancellationToken.None);

        // Assert
        // Subtotal: 80 * 2 = 160 (using discounted price)
        // VAT (20%): 160 * 0.20 = 32
        // Total with VAT: 160 + 32 = 192
        // Total without VAT: 160
        Assert.Equal(192.00m, resultWithVat);
        Assert.Equal(160.00m, resultWithoutVat);
    }

    [Fact]
    public async Task Handle_WithBasketContainingAppliedDiscount_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var regularProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Regular Product",
            Description = "Test Description",
            Price = 100.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = false
        };

        var discountedProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Discounted Product",
            Description = "Test Description",
            Price = 100.00m,
            DiscountedPrice = 80.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = true
        };

        var regularItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = regularProduct.Id,
            Product = regularProduct,
            Quantity = 1,
            UnitPrice = regularProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        var discountedItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = discountedProduct.Id,
            Product = discountedProduct,
            Quantity = 1,
            UnitPrice = discountedProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        var discount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "SAVE10",
            DiscountPercentage = 10.00m,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        basket.AppliedDiscount = discount;

        _context.Baskets.Add(basket);
        _context.Products.AddRange(regularProduct, discountedProduct);
        _context.BasketItems.AddRange(regularItem, discountedItem);
        await _context.SaveChangesAsync();

        var queryWithVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: true);
        var queryWithoutVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: false);

        // Act
        var resultWithVat = await _handler.Handle(queryWithVat, CancellationToken.None);
        var resultWithoutVat = await _handler.Handle(queryWithoutVat, CancellationToken.None);

        // Assert
        // Subtotal: 100 (regular) + 80 (discounted) = 180
        // Discount amount: 100 * 0.10 = 10 (only on regular item)
        // Subtotal after discount: 180 - 10 = 170
        // VAT (20%): 170 * 0.20 = 34
        // Total with VAT: 170 + 34 = 204
        // Total without VAT: 170
        Assert.Equal(204.00m, resultWithVat);
        Assert.Equal(170.00m, resultWithoutVat);
    }

    [Fact]
    public async Task Handle_WithBasketContainingShippingCost_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ShippingCost = 5.99m
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = false
        };

        var basketItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            UnitPrice = product.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Baskets.Add(basket);
        _context.Products.Add(product);
        _context.BasketItems.Add(basketItem);
        await _context.SaveChangesAsync();

        var queryWithVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: true);
        var queryWithoutVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: false);

        // Act
        var resultWithVat = await _handler.Handle(queryWithVat, CancellationToken.None);
        var resultWithoutVat = await _handler.Handle(queryWithoutVat, CancellationToken.None);

        // Assert
        // Subtotal: 100
        // Subtotal + Shipping: 100 + 5.99 = 105.99
        // VAT (20%): 105.99 * 0.20 = 21.198
        // Total with VAT: 105.99 + 21.198 = 127.188 (rounded to 127.19)
        // Total without VAT: 105.99
        Assert.Equal(127.188m, resultWithVat);
        Assert.Equal(105.99m, resultWithoutVat);
    }

    [Fact]
    public async Task Handle_WithMixedItemsAndDiscountAndShipping_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ShippingCost = 5.99m
        };

        var regularProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Regular Product",
            Description = "Test Description",
            Price = 100.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = false
        };

        var discountedProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Discounted Product",
            Description = "Test Description",
            Price = 50.00m,
            DiscountedPrice = 40.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = true
        };

        var regularItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = regularProduct.Id,
            Product = regularProduct,
            Quantity = 2,
            UnitPrice = regularProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        var discountedItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = discountedProduct.Id,
            Product = discountedProduct,
            Quantity = 1,
            UnitPrice = discountedProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        var discount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "SAVE10",
            DiscountPercentage = 10.00m,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        basket.AppliedDiscount = discount;

        _context.Baskets.Add(basket);
        _context.Products.AddRange(regularProduct, discountedProduct);
        _context.BasketItems.AddRange(regularItem, discountedItem);
        await _context.SaveChangesAsync();

        var queryWithVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: true);
        var queryWithoutVat = new GetTotalCostQuery(basket.Id, IsTotalWithVat: false);

        // Act
        var resultWithVat = await _handler.Handle(queryWithVat, CancellationToken.None);
        var resultWithoutVat = await _handler.Handle(queryWithoutVat, CancellationToken.None);

        // Assert
        // Subtotal: (100 * 2) + (40 * 1) = 240
        // Discount amount: 200 * 0.10 = 20 (only on regular items)
        // Subtotal after discount: 240 - 20 = 220
        // Subtotal + Shipping: 220 + 5.99 = 225.99
        // VAT (20%): 225.99 * 0.20 = 45.198
        // Total with VAT: 225.99 + 45.198 = 271.188 (rounded to 271.19)
        // Total without VAT: 225.99
        Assert.Equal(271.188m, resultWithVat);
        Assert.Equal(225.99m, resultWithoutVat);
    }

    [Theory]
    [InlineData(true)] // With VAT
    [InlineData(false)] // Without VAT
    public async Task Handle_WithLargeQuantities_ShouldCalculateCorrectTotal(bool isTotalWithVat)
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            StockQuantity = 1000,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = false
        };

        var basketItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 50,
            UnitPrice = product.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Baskets.Add(basket);
        _context.Products.Add(product);
        _context.BasketItems.Add(basketItem);
        await _context.SaveChangesAsync();

        var query = new GetTotalCostQuery(basket.Id, isTotalWithVat);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Subtotal: 10 * 50 = 500
        // VAT (20%): 500 * 0.20 = 100
        // Total with VAT: 500 + 100 = 600
        // Total without VAT: 500
        var expected = isTotalWithVat ? 600.00m : 500.00m;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Handle_WithDecimalPrices_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 19.99m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = false
        };

        var basketItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 3,
            UnitPrice = product.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Baskets.Add(basket);
        _context.Products.Add(product);
        _context.BasketItems.Add(basketItem);
        await _context.SaveChangesAsync();

        var query = new GetTotalCostQuery(basket.Id, IsTotalWithVat: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Subtotal: 19.99 * 3 = 59.97
        // VAT (20%): 59.97 * 0.20 = 11.994
        // Total with VAT: 59.97 + 11.994 = 71.964 (rounded to 71.96)
        Assert.Equal(71.964m, result);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}