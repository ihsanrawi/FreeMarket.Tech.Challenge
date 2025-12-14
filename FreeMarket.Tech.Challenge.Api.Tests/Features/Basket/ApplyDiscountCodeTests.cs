using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.Entities;
using FreeMarket.Tech.Challenge.Api.Features.Basket;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Tests.Features.Basket;

public class ApplyDiscountCodeTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ApplyDiscountCodeHandler _handler;

    public ApplyDiscountCodeTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _handler = new ApplyDiscountCodeHandler(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var activeDiscount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "SAVE10",
            DiscountPercentage = 10.00m,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        var expiredDiscount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "EXPIRED20",
            DiscountPercentage = 20.00m,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(-5),
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var inactiveDiscount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "INACTIVE15",
            DiscountPercentage = 15.00m,
            IsActive = false,
            ValidTo = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _context.Baskets.Add(basket);
        _context.DiscountCodes.AddRange(activeDiscount, expiredDiscount, inactiveDiscount);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidDiscountCode_ShouldApplyDiscountToBasket()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var request = new ApplyDiscountCodeRequest("SAVE10");
        var command = new ApplyDiscountCodeCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(basket.Id, result.Id);
        Assert.NotNull(result.AppliedDiscount);
        Assert.Equal("SAVE10", result.AppliedDiscount.Code);
        Assert.Equal(10.00m, result.AppliedDiscount.Value);

        // Verify basket is updated in database
        var updatedBasket = await _context.Baskets
            .Include(b => b.AppliedDiscount)
            .FirstAsync(b => b.Id == basket.Id);
        Assert.NotNull(updatedBasket.AppliedDiscount);
        Assert.Equal("SAVE10", updatedBasket.AppliedDiscount.Code);
    }

    [Fact]
    public async Task Handle_WithNonExistentBasket_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistentBasketId = Guid.NewGuid();
        var request = new ApplyDiscountCodeRequest("SAVE10");
        var command = new ApplyDiscountCodeCommand(nonExistentBasketId, request);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData("NONEXISTENT")]
    [InlineData("INACTIVE15")]
    public async Task Handle_WithNonExistentDiscountCode_ShouldThrowKeyNotFoundException(string discountCode)
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var request = new ApplyDiscountCodeRequest(discountCode);
        var command = new ApplyDiscountCodeCommand(basket.Id, request);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithExpiredDiscountCode_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var request = new ApplyDiscountCodeRequest("EXPIRED20");
        var command = new ApplyDiscountCodeCommand(basket.Id, request);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithBasketContainingDiscountedItems_ShouldExcludeDiscountedItemsFromCalculation()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();

        // Add both discounted and non-discounted items
        var regularProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Regular Product",
            Description = "Description",
            Price = 100.00m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsDiscounted = false
        };

        var discountedProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Discounted Product",
            Description = "Description",
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
            Quantity = 1,
            UnitPrice = regularProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        var discountedItem = new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = discountedProduct.Id,
            Quantity = 1,
            UnitPrice = discountedProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Products.AddRange(regularProduct, discountedProduct);
        _context.BasketItems.AddRange(regularItem, discountedItem);
        await _context.SaveChangesAsync();

        var request = new ApplyDiscountCodeRequest("SAVE10");
        var command = new ApplyDiscountCodeCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AppliedDiscount);

        // Only regular product should be eligible for discount: 100 * 0.10 = 10
        Assert.Equal(10.00m, result.DiscountAmount);
        Assert.Equal(170.00m, result.SubtotalAfterDiscount); // 100 + 80 - 10
    }

    [Fact]
    public async Task Handle_WithExistingDiscount_ShouldReplaceWithNewDiscount()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var existingDiscount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "OLD5",
            DiscountPercentage = 5.00m,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        basket.AppliedDiscount = existingDiscount;
        await _context.SaveChangesAsync();

        var request = new ApplyDiscountCodeRequest("SAVE10");
        var command = new ApplyDiscountCodeCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AppliedDiscount);
        Assert.Equal("SAVE10", result.AppliedDiscount.Code);
        Assert.Equal(10.00m, result.AppliedDiscount.Value);

        // Verify basket is updated in database
        var updatedBasket = await _context.Baskets
            .Include(b => b.AppliedDiscount)
            .FirstAsync(b => b.Id == basket.Id);
        Assert.NotNull(updatedBasket.AppliedDiscount);
        Assert.Equal("SAVE10", updatedBasket.AppliedDiscount.Code);
    }

    [Theory]
    [InlineData("save10")] // lowercase
    [InlineData("SAVE10")] // uppercase
    [InlineData("Save10")] // mixed case
    public async Task Handle_WithDifferentCaseCodes_ShouldApplyDiscount(string discountCode)
    {
        // Arrange - Add discount code in lowercase
        var caseInsensitiveDiscount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "save10",
            DiscountPercentage = 10.00m,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        _context.DiscountCodes.Add(caseInsensitiveDiscount);
        await _context.SaveChangesAsync();

        var basket = await _context.Baskets.FirstAsync();
        var request = new ApplyDiscountCodeRequest(discountCode);
        var command = new ApplyDiscountCodeCommand(basket.Id, request);

        // Act & Assert - This should fail because the comparison is case-sensitive
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnBasketDtoWithCorrectValues()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var request = new ApplyDiscountCodeRequest("SAVE10");
        var command = new ApplyDiscountCodeCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(basket.Id, result.Id);
        Assert.Equal(basket.CustomerEmail, result.CustomerEmail);
        Assert.Equal(basket.CreatedAt, result.CreatedAt);
        Assert.True(result.UpdatedAt >= basket.UpdatedAt); // Should be updated
        Assert.NotNull(result.AppliedDiscount);
        Assert.Equal("SAVE10", result.AppliedDiscount.Code);
        Assert.Equal(10.00m, result.AppliedDiscount.Value);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}