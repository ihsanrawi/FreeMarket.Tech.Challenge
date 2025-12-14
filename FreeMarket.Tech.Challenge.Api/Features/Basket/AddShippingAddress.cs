using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Entities;
using FreeMarket.Tech.Challenge.Api.Extensions;
using FreeMarket.Tech.Challenge.Api.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Features.Basket;

// • Add shipping cost to the UK
// • Add shipping cost to other countries
public static class AddShippingAddressEndpoint
{
    public static IEndpointRouteBuilder MapAddShippingAddressEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/baskets/{basketId:guid}/shipping", async (Guid basketId, AddShippingRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var command = new AddShippingCommand(basketId, request);
                    var result = await mediator.Send(command, cancellationToken);
                    return Results.Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("AddShippingAddress")
            .WithDescription("Adds a shipping address to the specified basket and calculates the shipping cost.")
            .WithTags("Baskets");

        return routes;
    }
}

public class AddShippingHandler(AppDbContext context) : IRequestHandler<AddShippingCommand, BasketDto>
{
    public async Task<BasketDto> Handle(AddShippingCommand cmd, CancellationToken cancellationToken)
    {
        // Check if basket exists
        var basketEntity = await context.Baskets
                               .Include(b => b.Items)
                               .ThenInclude(i => i.Product)
                               .FirstOrDefaultAsync(b => b.Id == cmd.BasketId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Basket with ID {cmd.BasketId} not found");

        var shippingAddress = new Address
        {
            Id = Guid.NewGuid(),
            CustomerEmail = basketEntity.CustomerEmail,
            Country = cmd.Request.ShippingAddress.Country
        };
        var shippingCost = ShippingCostService.CalculateShippingCost(shippingAddress);

        // Add the new address to the context
        context.Addresses.Add(shippingAddress);

        // Add or update shipping address
        basketEntity.SetShippingAddress(shippingAddress, shippingCost);
        await context.SaveChangesAsync(cancellationToken);

        return basketEntity.MapToDto();
    }
}

public record AddShippingRequest
{
    public AddressDto ShippingAddress { get; set; } = new();
}

public record AddShippingCommand(Guid BasketId, AddShippingRequest Request) : IRequest<BasketDto>;
