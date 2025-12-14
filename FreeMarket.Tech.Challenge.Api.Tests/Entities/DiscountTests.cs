using FreeMarket.Tech.Challenge.Api.Entities;

namespace FreeMarket.Tech.Challenge.Api.Tests.Entities;

public class DiscountTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var discount = new Discount();

        // Assert
        Assert.Equal(default(Guid), discount.Id);
        Assert.Equal(string.Empty, discount.Code);
        Assert.Equal(0m, discount.DiscountPercentage);
        Assert.True(discount.IsActive);
        Assert.Equal(default(DateTime), discount.ValidTo);
        Assert.True(discount.CreatedAt > DateTime.MinValue);
        Assert.True(discount.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Properties_ShouldSetAndGetCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var code = "SAVE10";
        var discountPercentage = 0.10m;
        var isActive = true;
        var validTo = DateTime.UtcNow.AddDays(30);
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var discount = new Discount
        {
            Id = id,
            Code = code,
            DiscountPercentage = discountPercentage,
            IsActive = isActive,
            ValidTo = validTo,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(id, discount.Id);
        Assert.Equal(code, discount.Code);
        Assert.Equal(discountPercentage, discount.DiscountPercentage);
        Assert.Equal(isActive, discount.IsActive);
        Assert.Equal(validTo, discount.ValidTo);
        Assert.Equal(createdAt, discount.CreatedAt);
    }

    [Fact]
    public void IsValid_WithActiveDiscountAndNoValidTo_ShouldReturnTrue()
    {
        // Arrange
        var discount = new Discount
        {
            IsActive = true,
            ValidTo = default
        };

        // Act
        var result = discount.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithActiveDiscountAndFutureValidTo_ShouldReturnTrue()
    {
        // Arrange
        var discount = new Discount
        {
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = discount.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithInactiveDiscount_ShouldReturnFalse()
    {
        // Arrange
        var discount = new Discount
        {
            IsActive = false,
            ValidTo = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = discount.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithExpiredDiscount_ShouldReturnFalse()
    {
        // Arrange
        var discount = new Discount
        {
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = discount.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithInactiveAndExpiredDiscount_ShouldReturnFalse()
    {
        // Arrange
        var discount = new Discount
        {
            IsActive = false,
            ValidTo = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = discount.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(0.01)]
    [InlineData(0.10)]
    [InlineData(0.25)]
    [InlineData(0.50)]
    [InlineData(0.75)]
    [InlineData(0.99)]
    [InlineData(1.00)]
    public void DiscountPercentage_WithValidValues_ShouldAccept(decimal discountPercentage)
    {
        // Arrange
        var discount = new Discount();

        // Act
        discount.DiscountPercentage = discountPercentage;

        // Assert
        Assert.Equal(discountPercentage, discount.DiscountPercentage);
    }

    [Theory]
    [InlineData(-0.10)]
    [InlineData(-1.00)]
    [InlineData(-10.00)]
    [InlineData(-100.00)]
    public void DiscountPercentage_WithNegativeValues_ShouldAccept(decimal discountPercentage)
    {
        // Arrange
        var discount = new Discount();

        // Act
        discount.DiscountPercentage = discountPercentage;

        // Assert
        Assert.Equal(discountPercentage, discount.DiscountPercentage);
    }

    [Theory]
    [InlineData(1.50)]
    [InlineData(2.00)]
    [InlineData(10.00)]
    [InlineData(100.00)]
    public void DiscountPercentage_WithValuesOverOne_ShouldAccept(decimal discountPercentage)
    {
        // Arrange
        var discount = new Discount();

        // Act
        discount.DiscountPercentage = discountPercentage;

        // Assert
        Assert.Equal(discountPercentage, discount.DiscountPercentage);
    }

    [Theory]
    [InlineData("SAVE10")]
    [InlineData("PROMO2024")]
    [InlineData("WELCOME")]
    [InlineData("SPECIAL_OFFER")]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("VERY_LONG_DISCOUNT_CODE_THAT_EXCEEDS_NORMAL_LENGTHS")]
    public void Code_WithVariousValues_ShouldAccept(string code)
    {
        // Arrange
        var discount = new Discount();

        // Act
        discount.Code = code;

        // Assert
        Assert.Equal(code, discount.Code);
    }

    [Fact]
    public void Discount_CanBeInstantiatedWithObjectInitializer()
    {
        // Act
        var discount = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "SUMMER25",
            DiscountPercentage = 0.25m,
            IsActive = true,
            ValidTo = new DateTime(2024, 8, 31)
        };

        // Assert
        Assert.NotEqual(default(Guid), discount.Id);
        Assert.Equal("SUMMER25", discount.Code);
        Assert.Equal(0.25m, discount.DiscountPercentage);
        Assert.True(discount.IsActive);
        Assert.Equal(new DateTime(2024, 8, 31), discount.ValidTo);
        Assert.True(discount.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public void MultipleDiscounts_CanHaveIndependentProperties()
    {
        // Arrange
        var discount1 = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "SAVE10",
            DiscountPercentage = 0.10m,
            IsActive = true
        };

        var discount2 = new Discount
        {
            Id = Guid.NewGuid(),
            Code = "SAVE20",
            DiscountPercentage = 0.20m,
            IsActive = false
        };

        // Act & Assert
        Assert.NotEqual(discount1.Id, discount2.Id);
        Assert.NotEqual(discount1.Code, discount2.Code);
        Assert.NotEqual(discount1.DiscountPercentage, discount2.DiscountPercentage);
        Assert.NotEqual(discount1.IsActive, discount2.IsActive);
    }

    [Fact]
    public void IsValid_ExactlyAtExpirationTime_ShouldReturnFalse()
    {
        // Arrange
        var discount = new Discount
        {
            IsActive = true,
            ValidTo = DateTime.UtcNow.AddMilliseconds(-1) // Just expired
        };

        // Act
        var result = discount.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidTo_CanBeSetToMinDate()
    {
        // Arrange
        var discount = new Discount();

        // Act
        discount.ValidTo = DateTime.MinValue;

        // Assert
        Assert.Equal(DateTime.MinValue, discount.ValidTo);
        Assert.True(discount.IsValid()); // Should be valid when ValidTo is min date
    }

    [Fact]
    public void ValidTo_CanBeSetToMaxDate()
    {
        // Arrange
        var discount = new Discount();

        // Act
        discount.ValidTo = DateTime.MaxValue;

        // Assert
        Assert.Equal(DateTime.MaxValue, discount.ValidTo);
        Assert.True(discount.IsValid()); // Should be valid when ValidTo is far in future
    }
}