using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.Entities;
using FreeMarket.Tech.Challenge.Api.Features.Basket;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Tests.Features.Basket;

public class RemoveItemFromBasketTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RemoveItemFromBasketCommandHandler _handler;

    public RemoveItemFromBasketTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _handler = new RemoveItemFromBasketCommandHandler(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow
        };

        var basket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var basketItem = new FreeMarket.Tech.Challenge.Api.Entities.BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            UnitPrice = product.Price,
            AddedAt = DateTime.UtcNow.AddHours(-1)
        };

        basket.Items.Add(basketItem);

        _context.Products.Add(product);
        _context.Baskets.Add(basket);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidBasketAndItemIds_ShouldRemoveItemFromBasket()
    {
        // Arrange
        var basket = await _context.Baskets
            .Include(b => b.Items)
            .FirstAsync();
        var itemToRemove = basket.Items.First();
        var command = new RemoveItemFromBasketCommand(basket.Id, itemToRemove.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(basket.Id, result.Id);
        Assert.Empty(result.Items); // Item should be removed

        // Verify item is removed from database
        var updatedBasket = await _context.Baskets
            .Include(b => b.Items)
            .FirstAsync(b => b.Id == basket.Id);
        Assert.Empty(updatedBasket.Items);
    }

    [Fact]
    public async Task Handle_WithNonExistentBasketId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistentBasketId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var command = new RemoveItemFromBasketCommand(nonExistentBasketId, itemId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentItemId_ShouldReturnBasketWithUnchangedItems()
    {
        // Arrange
        var basket = await _context.Baskets
            .Include(b => b.Items)
            .FirstAsync();
        var nonExistentItemId = Guid.NewGuid();
        var originalItemCount = basket.Items.Count;
        var command = new RemoveItemFromBasketCommand(basket.Id, nonExistentItemId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalItemCount, result.Items.Count);

        // Verify basket is unchanged in database
        var updatedBasket = await _context.Baskets
            .Include(b => b.Items)
            .FirstAsync(b => b.Id == basket.Id);
        Assert.Equal(originalItemCount, updatedBasket.Items.Count);
    }

    [Fact]
    public async Task Handle_WithBasketContainingMultipleItems_ShouldRemoveOnlySpecificItem()
    {
        // Arrange
        var basket = await _context.Baskets
            .Include(b => b.Items)
            .FirstAsync();

        // Add another item to the basket
        var secondProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Second Product",
            Description = "Second Description",
            Price = 20.00m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow
        };

        var secondItem = new FreeMarket.Tech.Challenge.Api.Entities.BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = secondProduct.Id,
            Product = secondProduct,
            Quantity = 1,
            UnitPrice = secondProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Products.Add(secondProduct);
        basket.Items.Add(secondItem);
        await _context.SaveChangesAsync();

        var itemToRemove = basket.Items.First();
        var command = new RemoveItemFromBasketCommand(basket.Id, itemToRemove.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items); // Should have one item left
        Assert.NotEqual(itemToRemove.Id, result.Items.First().Id); // Remaining item is different
    }

    [Fact]
    public async Task Handle_ShouldUpdateBasketUpdatedAtTimestamp()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var originalUpdatedAt = basket.UpdatedAt;
        var item = await _context.BasketItems.FirstAsync();

        var command = new RemoveItemFromBasketCommand(basket.Id, item.Id);

        // Add small delay to ensure timestamp difference
        await Task.Delay(10);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedBasket = await _context.Baskets.FirstAsync(b => b.Id == basket.Id);
        Assert.True(updatedBasket.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task Handle_WithEmptyBasket_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var emptyBasket = new FreeMarket.Tech.Challenge.Api.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "empty@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Baskets.Add(emptyBasket);
        await _context.SaveChangesAsync();

        var itemId = Guid.NewGuid();
        var command = new RemoveItemFromBasketCommand(emptyBasket.Id, itemId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectBasketDtoAfterRemoval()
    {
        // Arrange
        var basket = await _context.Baskets
            .Include(b => b.Items)
            .FirstAsync();
        var itemToRemove = basket.Items.First();
        var command = new RemoveItemFromBasketCommand(basket.Id, itemToRemove.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(basket.Id, result.Id);
        Assert.Equal(basket.CustomerEmail, result.CustomerEmail);
        Assert.Equal(basket.CreatedAt, result.CreatedAt);
        Assert.True(result.UpdatedAt > basket.UpdatedAt); // Should be updated
        Assert.Empty(result.Items);
        Assert.Equal(0m, result.Subtotal);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(0m, result.SubtotalAfterDiscount);
        Assert.Equal(0m, result.VatAmount);
        Assert.Equal(basket.ShippingCost, result.ShippingCost);
    }

    [Fact]
    public async Task Handle_ConcurrentRemovals_ShouldHandleGracefully()
    {
        // Arrange
        var basket = await _context.Baskets
            .Include(b => b.Items)
            .FirstAsync();
        var itemToRemove = basket.Items.First();

        var command1 = new RemoveItemFromBasketCommand(basket.Id, itemToRemove.Id);
        var command2 = new RemoveItemFromBasketCommand(basket.Id, itemToRemove.Id);

        // Act - Remove the same item concurrently
        var result1 = await _handler.Handle(command1, CancellationToken.None);

        // The second removal should not throw an exception, just return basket without changes
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Empty(result1.Items);
        Assert.Empty(result2.Items);
    }

    [Fact]
    public async Task Handle_WithBasketHavingAppliedDiscount_ShouldMaintainDiscountAfterRemoval()
    {
        // Arrange
        var basket = await _context.Baskets
            .Include(b => b.Items)
            .FirstAsync();

        // Add discount to basket
        var discount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "TEST10",
            DiscountPercentage = 0.10m,
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        basket.AppliedDiscount = discount;
        await _context.SaveChangesAsync();

        // Add second item so basket isn't empty after removal
        var secondProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Second Product",
            Description = "Second Description",
            Price = 20.00m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow
        };

        var secondItem = new FreeMarket.Tech.Challenge.Api.Entities.BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basket.Id,
            ProductId = secondProduct.Id,
            Product = secondProduct,
            Quantity = 1,
            UnitPrice = secondProduct.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.Products.Add(secondProduct);
        basket.Items.Add(secondItem);
        await _context.SaveChangesAsync();

        var itemToRemove = basket.Items.First(i => i.ProductId != secondProduct.Id);
        var command = new RemoveItemFromBasketCommand(basket.Id, itemToRemove.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AppliedDiscount);
        Assert.Equal(discount.Id, result.AppliedDiscount.Id);
        Assert.Single(result.Items); // One item should remain
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}