using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Features.Basket;

// • Add multiple of the same item to the basket
public static class UpdateItemQuantityEndpoint
{
    public static IEndpointRouteBuilder MapUpdateItemQuantityEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPut("/api/baskets/{basketId:guid}/items/{itemId:guid}/quantity/{quantity:int}", async (Guid basketId, Guid itemId, int quantity, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new UpdateItemQuantityCommand(basketId, itemId, quantity);

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
        .WithName("UpdateItemQuantity")
        .WithDescription("Update the quantity of an item in the specified basket.")
        .WithTags("Baskets");
        
        return routes;
    }
}

public class UpdateItemQuantityCommandHandler(AppDbContext context) : IRequestHandler<UpdateItemQuantityCommand, BasketDto>
{
    public async Task<BasketDto> Handle(UpdateItemQuantityCommand cmd, CancellationToken cancellationToken)
    {
        // Check if basket exists
        var basket = await context.Baskets
                         .Include(b => b.Items)
                         .ThenInclude(i => i.Product)
                         .Include(b => b.AppliedDiscount)
                         .FirstOrDefaultAsync(b => b.Id == cmd.BasketId, cancellationToken)
                     ?? throw new KeyNotFoundException($"Basket with ID {cmd.BasketId} not found");

        // Find the item in the basket
        var item = basket.Items.FirstOrDefault(i => i.Id == cmd.ItemId)
                   ?? throw new KeyNotFoundException($"Item with ID {cmd.ItemId} not found in basket {cmd.BasketId}");

        if (cmd.Quantity <= 0)
        {
            context.BasketItems.Remove(item);
        }
        else
        {
            item.UpdateQuantity(item.Quantity + item.Quantity);
        }
        
        await context.SaveChangesAsync(cancellationToken);

        return basket.MapToDto();
    }
}

public record UpdateItemQuantityCommand(Guid BasketId, Guid ItemId, int Quantity) : IRequest<BasketDto>;