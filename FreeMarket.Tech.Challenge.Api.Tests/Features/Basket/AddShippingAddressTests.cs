using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Entities;
using FreeMarket.Tech.Challenge.Api.Features.Basket;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Tests.Features.Basket;

public class AddShippingAddressTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AddShippingHandler _handler;

    public AddShippingAddressTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _handler = new AddShippingHandler(_context);

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

        _context.Baskets.Add(basket);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidUkAddress_ShouldAddShippingAddressWithCorrectCost()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var request = new AddShippingRequest
        {
            ShippingAddress = new AddressDto
            {
                Country = "UK"
            }
        };
        var command = new AddShippingCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(basket.Id, result.Id);
        Assert.Equal(5.99m, result.ShippingCost);

        // Verify basket is updated in database
        var updatedBasket = await _context.Baskets
            .Include(b => b.ShippingAddress)
            .FirstAsync(b => b.Id == basket.Id);
        Assert.NotNull(updatedBasket.ShippingAddress);
        Assert.Equal("UK", updatedBasket.ShippingAddress.Country);
        Assert.Equal(5.99m, updatedBasket.ShippingCost);
    }

    [Fact]
    public async Task Handle_WithNonUkAddress_ShouldAddShippingAddressWithInternationalCost()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var request = new AddShippingRequest
        {
            ShippingAddress = new AddressDto
            {
                Country = "USA"
            }
        };
        var command = new AddShippingCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8.99m, result.ShippingCost);

        // Verify basket is updated in database
        var updatedBasket = await _context.Baskets
            .Include(b => b.ShippingAddress)
            .FirstAsync(b => b.Id == basket.Id);
        Assert.NotNull(updatedBasket.ShippingAddress);
        Assert.Equal("USA", updatedBasket.ShippingAddress.Country);
        Assert.Equal(8.99m, updatedBasket.ShippingCost);
    }

    [Fact]
    public async Task Handle_WithNonExistentBasket_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistentBasketId = Guid.NewGuid();
        var request = new AddShippingRequest
        {
            ShippingAddress = new AddressDto
            {
                Country = "UK"
            }
        };
        var command = new AddShippingCommand(nonExistentBasketId, request);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData("UK", 5.99)]
    [InlineData("United Kingdom", 5.99)]
    [InlineData("uk", 5.99)]
    [InlineData("UNITED KINGDOM", 5.99)]
    [InlineData("USA", 8.99)]
    [InlineData("Canada", 8.99)]
    [InlineData("France", 8.99)]
    [InlineData("Germany", 8.99)]
    [InlineData("Australia", 8.99)]
    public async Task Handle_WithDifferentCountries_ShouldApplyCorrectShippingCost(string country, decimal expectedCost)
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var request = new AddShippingRequest
        {
            ShippingAddress = new AddressDto
            {
                Country = country
            }
        };
        var command = new AddShippingCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCost, result.ShippingCost);
    }

    [Fact]
    public async Task Handle_WithExistingShippingAddress_ShouldUpdateShippingAddress()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();

        // Add initial shipping address
        var initialAddress = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = basket.CustomerEmail,
            Country = "USA"
        };
        basket.SetShippingAddress(initialAddress, 8.99m);
        await _context.SaveChangesAsync();

        var request = new AddShippingRequest
        {
            ShippingAddress = new AddressDto
            {
                Country = "UK"
            }
        };
        var command = new AddShippingCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5.99m, result.ShippingCost);

        // Verify shipping address is updated in database
        var updatedBasket = await _context.Baskets
            .Include(b => b.ShippingAddress)
            .FirstAsync(b => b.Id == basket.Id);
        Assert.NotNull(updatedBasket.ShippingAddress);
        Assert.Equal("UK", updatedBasket.ShippingAddress.Country);
        Assert.Equal(5.99m, updatedBasket.ShippingCost);
    }

    [Fact]
    public async Task Handle_ShouldReturnBasketDtoWithCorrectValues()
    {
        // Arrange
        var basket = await _context.Baskets.FirstAsync();
        var request = new AddShippingRequest
        {
            ShippingAddress = new AddressDto
            {
                Country = "UK"
            }
        };
        var command = new AddShippingCommand(basket.Id, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(basket.Id, result.Id);
        Assert.Equal(basket.CustomerEmail, result.CustomerEmail);
        Assert.Equal(basket.CreatedAt, result.CreatedAt);
        Assert.True(result.UpdatedAt > basket.UpdatedAt); // Should be updated
        Assert.Equal(5.99m, result.ShippingCost);
        Assert.NotNull(result.ShippingAddress);
        Assert.Equal("UK", result.ShippingAddress.Country);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}