using FreeMarket.Tech.Challenge.Api.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FreeMarket.Tech.Challenge.Api.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateDbContext()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<AppDbContext>();
    }
}