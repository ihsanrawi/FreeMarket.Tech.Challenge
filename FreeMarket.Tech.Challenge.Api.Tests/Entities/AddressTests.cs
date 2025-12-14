using FreeMarket.Tech.Challenge.Api.Entities;

namespace FreeMarket.Tech.Challenge.Api.Tests.Entities;

public class AddressTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithProvidedValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerEmail = "test@example.com";
        var country = "UK";

        // Act
        var address = new Address
        {
            Id = id,
            CustomerEmail = customerEmail,
            Country = country
        };

        // Assert
        Assert.Equal(id, address.Id);
        Assert.Equal(customerEmail, address.CustomerEmail);
        Assert.Equal(country, address.Country);
    }

    [Theory]
    [InlineData("UK", true)]
    [InlineData("uk", true)]
    [InlineData("Uk", true)]
    [InlineData("uK", true)]
    [InlineData("United Kingdom", true)]
    [InlineData("united kingdom", true)]
    [InlineData("UNITED KINGDOM", true)]
    [InlineData("US", false)]
    [InlineData("USA", false)]
    [InlineData("Canada", false)]
    [InlineData("France", false)]
    [InlineData("Germany", false)]
    [InlineData("Australia", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    public void IsUkAddress_WithVariousCountryValues_ShouldReturnCorrectResult(string country, bool expected)
    {
        // Arrange
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Country = country
        };

        // Act
        var result = address.IsUkAddress();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Address_ShouldBeRecordType()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerEmail = "test@example.com";
        var country = "UK";

        // Act
        var address1 = new Address
        {
            Id = id,
            CustomerEmail = customerEmail,
            Country = country
        };
        var address2 = new Address
        {
            Id = id,
            CustomerEmail = customerEmail,
            Country = country
        };

        // Assert
        Assert.Equal(address1, address2); // Records have value-based equality
        Assert.Equal(address1.GetHashCode(), address2.GetHashCode());
    }

    [Fact]
    public void Address_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var address1 = new Address
        {
            Id = id,
            CustomerEmail = "test@example.com",
            Country = "UK"
        };
        var address2 = new Address
        {
            Id = id,
            CustomerEmail = "different@example.com",
            Country = "UK"
        };

        // Assert
        Assert.NotEqual(address1, address2);
    }

    [Fact]
    public void Address_CanHandleNullEmail()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = null!,
            Country = "UK"
        };

        // Assert
        Assert.Null(address.CustomerEmail);
        Assert.Equal("UK", address.Country);
    }

    [Fact]
    public void Address_CanHandleNullCountry()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Country = null!
        };

        // Assert
        Assert.Equal("test@example.com", address.CustomerEmail);
        Assert.Null(address.Country);
    }

    [Theory]
    [InlineData("U.K.")]
    [InlineData("U K")]
    [InlineData("UnitedKingdom")]
    [InlineData("Britain")]
    [InlineData("England")]
    [InlineData("Scotland")]
    [InlineData("Wales")]
    [InlineData("Northern Ireland")]
    public void IsUkAddress_WithNonStandardUkNames_ShouldReturnFalse(string country)
    {
        // Arrange
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Country = country
        };

        // Act
        var result = address.IsUkAddress();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Address_ShouldSupportToString()
    {
        // Arrange
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Country = "UK"
        };

        // Act
        var result = address.ToString();

        // Assert
        Assert.NotNull(result);
        // Classes inherit default ToString behavior
    }

    [Fact]
    public void Address_WithEmptyCountry_ShouldReturnFalseForIsUkAddress()
    {
        // Arrange
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Country = ""
        };

        // Act
        var result = address.IsUkAddress();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Address_WithWhitespaceCountry_ShouldReturnFalseForIsUkAddress()
    {
        // Arrange
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Country = "   "
        };

        // Act
        var result = address.IsUkAddress();

        // Assert
        Assert.False(result);
    }
}