using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.Entities;
using FreeMarket.Tech.Challenge.Api.Features.Basket;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Tests.Features.Basket;

public class AddItemToBasketTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AddItemsToBasketCommandHandler _handler;

    public AddItemToBasketTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _handler = new AddItemsToBasketCommandHandler(_context);

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        _context.Baskets.Add(basket);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidBasketAndProduct_ShouldAddItemToBasket()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var product = await _context.Products.FirstAsync();

        var command = new AddItemsToBasketCommand(basket.Id, new AddMultipleItemsRequest(
            [new AddItemsToBasketRequest(product.Id, 2)]));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(basket.Id, result.Id);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items.First().Quantity);
        Assert.Equal(product.Id, result.Items.First().ProductId);
    }

    [Fact]
    public async Task Handle_WithNonExistentBasket_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistentBasketId = Guid.NewGuid();
        var command = new AddItemsToBasketCommand(nonExistentBasketId, new AddMultipleItemsRequest(
            [new AddItemsToBasketRequest(Guid.NewGuid(), 1)]));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var nonExistentProductId = Guid.NewGuid();

        var command = new AddItemsToBasketCommand(basket.Id, new AddMultipleItemsRequest(
            [new AddItemsToBasketRequest(nonExistentProductId, 1)]));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithExistingItemInBasket_ShouldUpdateQuantity()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var product = await _context.Products.FirstAsync();

        // Add initial item
        var initialCommand = new AddItemsToBasketCommand(basket.Id, new AddMultipleItemsRequest(
            [new AddItemsToBasketRequest(product.Id, 2)]));
        await _handler.Handle(initialCommand, CancellationToken.None);

        // Add more of the same item
        var additionalCommand = new AddItemsToBasketCommand(basket.Id, new AddMultipleItemsRequest(
            [new AddItemsToBasketRequest(product.Id, 3)]));

        // Act
        var result = await _handler.Handle(additionalCommand, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(5, result.Items.First().Quantity); // 2 + 3
    }

    [Fact]
    public async Task Handle_WithMultipleDifferentItems_ShouldAddAllItems()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();

        // Add additional product
        var secondProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Second Product",
            Description = "Second Description",
            Price = 20.00m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow
        };
        _context.Products.Add(secondProduct);
        await _context.SaveChangesAsync();

        var firstProduct = await _context.Products.FirstAsync();

        var command = new AddItemsToBasketCommand(basket.Id, new AddMultipleItemsRequest([
            new AddItemsToBasketRequest(firstProduct.Id, 2),
            new AddItemsToBasketRequest(secondProduct.Id, 1)
            ]));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, i => i.ProductId == firstProduct.Id && i.Quantity == 2);
        Assert.Contains(result.Items, i => i.ProductId == secondProduct.Id && i.Quantity == 1);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}