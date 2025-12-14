using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Features.Basket;

// • Remove an item from the basket
public static class RemoveItemFromBasketEndpoint
{
    public static IEndpointRouteBuilder MapRemoveItemFromBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/baskets/{basketId:guid}/items/{itemId:guid}", async (Guid basketId, Guid itemId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var cmd = new RemoveItemFromBasketCommand(basketId, itemId);
                    var result = await mediator.Send(cmd, cancellationToken);
                    if (result == null)
                    {
                        return Results.NotFound();
                    }
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
            .WithName("RemoveItemFromBasket")
            .WithDescription("Removes an item from the specified basket.")
            .WithTags("Baskets");

        return app;
    }
}

public class RemoveItemFromBasketCommandHandler(AppDbContext context) : IRequestHandler<RemoveItemFromBasketCommand, BasketDto>
{
    public async Task<BasketDto> Handle(RemoveItemFromBasketCommand cmd, CancellationToken cancellationToken)
    {
        // Check if basket exists
        var basketEntity = await context.Baskets
                               .Include(b => b.Items)
                               .ThenInclude(i => i.Product)
                               .Include(b => b.AppliedDiscount)
                               .FirstOrDefaultAsync(b => b.Id == cmd.BasketId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Basket with ID {cmd.BasketId} not found");

        basketEntity.RemoveItem(cmd.ItemId);
        await context.SaveChangesAsync(cancellationToken);

        return basketEntity.MapToDto();
    }
}

public record RemoveItemFromBasketCommand (Guid BasketId, Guid ItemId) : IRequest<BasketDto>;