using FreeMarket.Tech.Challenge.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Context;

public class AppDbContext : DbContext
{
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<BasketItem> BasketItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Discount> DiscountCodes { get; set; }
    public DbSet<Address> Addresses { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Basket
        modelBuilder.Entity<Basket>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.CustomerEmail).HasMaxLength(200);
            entity.HasOne(b => b.AppliedDiscount)
                .WithMany()
                .HasForeignKey("AppliedDiscountId")
                .IsRequired(false);
            entity.HasMany(b => b.Items)
                .WithOne(bi => bi.Basket)
                .HasForeignKey(i => i.BasketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(b => b.ShippingAddress)
                .WithMany()
                .HasForeignKey("ShippingAddressId")
                .IsRequired(false);
        });

        // Configure BasketItem
        modelBuilder.Entity<BasketItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.StockQuantity).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.DiscountedPrice).HasPrecision(18, 2);
        });
        
        // Configure DiscountCode
        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Code).IsRequired().HasMaxLength(50);
            entity.Property(d => d.DiscountPercentage).HasPrecision(5, 2);
        });

        // Configure Address
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.CustomerEmail).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Country).IsRequired().HasMaxLength(100);
        });
        
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Baskets - Only 1 basket with multiple items
        var basketId1 = Guid.Parse("D2F1C5E4-8F4B-4C3A-9D6A-1E2B3C4D5E6F");

        // Seed DiscountCodes - declare IDs first for use in basket relationships
        var discountCodeId1 = Guid.Parse("A1B2C3D4-E5F6-7A8B-9C0D-1E2F3A4B5C6D"); // Valid code
        var discountCodeId2 = Guid.Parse("B2C3D4E5-F6A7-8B9C-0D1E-2F3A4B5C6D7E"); // Expired code

        var createdAt = DateTime.UtcNow.AddDays(-7); // Baskets created a week ago
        var updatedAt = DateTime.UtcNow.AddDays(-1);  // Updated yesterday

        modelBuilder.Entity<Basket>().HasData(
            new Basket
            {
                Id = basketId1,
                CustomerEmail = "john.doe@example.com",
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            }
        );

        // Seed Products
        var productId1 = Guid.Parse("3BDA8B6A-A189-4D10-9B74-ED0BE50694D9"); // Laptop Pro X1 (non-discounted)
        var productId2 = Guid.Parse("FFC53C01-97FA-40A4-B1F4-54FE1BC3B0FA"); // Wireless Mouse (discounted)
        var productId3 = Guid.Parse("0343e51b-f134-49f5-97cc-d0cb4535644b"); // No stock
        var productId4 = Guid.Parse("6bf47d60-d900-41ce-95d8-968a3b34a968"); // Mechanical Keyboard (discounted)
        var productId5 = Guid.Parse("E8A7B5C3-9D2F-4E6A-8B1C-3F5E7D9A2B4C"); // Discounted product
        var productId6 = Guid.Parse("C4F8E2D1-7A9B-3C5E-9F2D-6B8A1C4E7F9A"); // Not discounted

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = productId1,
                Name = "Laptop Pro X1",
                Description = "High-performance laptop with 16GB RAM, 512GB SSD, and Intel Core i7 processor",
                Price = 1299.99m,
                IsDiscounted = false,
                DiscountedPrice = 0m,
                StockQuantity = 8,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Id = productId2,
                Name = "Wireless Ergonomic Mouse",
                Description = "Ergonomic wireless mouse with long battery life, precision tracking, and comfortable grip",
                Price = 34.99m,
                IsDiscounted = true,
                DiscountedPrice = 29.99m,
                StockQuantity = 45,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Product
            {
                Id = productId3,
                Name = "USB-C Hub Premium",
                Description = "7-in-1 USB-C hub with HDMI 4K, USB 3.0 ports, SD card reader, and power delivery",
                Price = 59.99m,
                IsDiscounted = false,
                DiscountedPrice = 0m,
                StockQuantity = 0,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Product
            {
                Id = productId4,
                Name = "Mechanical Gaming Keyboard",
                Description = "RGB mechanical keyboard with blue switches, programmable keys, and anti-ghosting",
                Price = 89.99m,
                IsDiscounted = true,
                DiscountedPrice = 69.99m,
                StockQuantity = 12,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Product
            {
                Id = productId5,
                Name = "4K Webcam Pro",
                Description = "Professional 4K webcam with auto-focus, noise cancellation, and wide-angle lens",
                Price = 149.99m,
                StockQuantity = 18,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Product
            {
                Id = productId6,
                Name = "Monitor Light Bar",
                Description = "LED monitor light bar with auto-dimming, eye-care technology, and USB powered",
                Price = 79.99m,
                StockQuantity = 35,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        );

        // Seed DiscountCodes - Only 2 codes (1 valid, 1 invalid)

        modelBuilder.Entity<Discount>().HasData(
            new Discount
            {
                Id = discountCodeId1,
                Code = "SAVE10",
                DiscountPercentage = 10.00m,
                IsActive = true,
                ValidTo = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Discount
            {
                Id = discountCodeId2,
                Code = "EXPIRED20",
                DiscountPercentage = 20.00m,
                IsActive = true,
                ValidTo = DateTime.UtcNow.AddDays(-5), // Expired (invalid)
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        );

        // Seed BasketItems - connecting the single basket with multiple products
        var basketItemId1 = Guid.Parse("B1C2D3E4-5F6A-7B8C-9D0E-1F2A3B4C5D6E");
        var basketItemId2 = Guid.Parse("C2D3E4F5-6A7B-8C9D-0E1F-2A3B4C5D6E7F");
        var basketItemId3 = Guid.Parse("D3E4F5A6-7B8C-9D0E-1F2A-3B4C5D6E7F8A");
        var basketItemId4 = Guid.Parse("E4F5A6B7-8C9D-0E1F-2A3B-4C5D6E7F8A9B");

        modelBuilder.Entity<BasketItem>().HasData(
            // Single basket with multiple items (both discounted and non-discounted products)
            new BasketItem
            {
                Id = basketItemId1,
                BasketId = basketId1,
                ProductId = productId1, // Laptop Pro X1 (non-discounted)
                Quantity = 1,
                UnitPrice = 1299.99m,
                AddedAt = createdAt.AddHours(2)
            },
            new BasketItem
            {
                Id = basketItemId2,
                BasketId = basketId1,
                ProductId = productId2, // Wireless Mouse (discounted)
                Quantity = 2,
                UnitPrice = 34.99m,
                AddedAt = createdAt.AddHours(3)
            },
            new BasketItem
            {
                Id = basketItemId3,
                BasketId = basketId1,
                ProductId = productId4, // Mechanical Keyboard (discounted)
                Quantity = 1,
                UnitPrice = 89.99m,
                AddedAt = updatedAt.AddHours(-1)
            }
        );
    }
}