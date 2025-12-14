using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Entities;
using FreeMarket.Tech.Challenge.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Features.Basket;

// • Add an item to the basket
// • Add multiple items to the basket
// • Add a discounted item to the basket
public static class AddItemToBasketEndpoint
{
    public static IEndpointRouteBuilder MapAddItemToBasketEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/baskets/{basketId:guid}/items/{productId:guid}", async (Guid basketId, Guid productId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                // TODO: Validate request items
                try
                {
                    var request = new AddMultipleItemsRequest([new AddItemsToBasketRequest(productId)]);
                    var command = new AddItemsToBasketCommand(basketId, request);

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
            .WithName("AddItemToBasket")
            .WithDescription("Adds a single item to the specified basket.")
            .WithTags("Baskets");

        routes.MapPost("/api/baskets/{basketId:guid}/items/multiple", async (Guid basketId, AddMultipleItemsRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                // TODO: Validate request items
                try
                {
                    var command = new AddItemsToBasketCommand(basketId, request);

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
            .WithName("AddMultipleItemToBasket")
            .WithDescription("Adds multiple items to the specified basket.")
            .WithTags("Baskets");

        return routes;
    }
}

public class AddItemsToBasketCommandHandler(AppDbContext context) : IRequestHandler<AddItemsToBasketCommand, BasketDto>
{
    public async Task<BasketDto> Handle(AddItemsToBasketCommand cmd, CancellationToken cancellationToken)
    {
        // Check if basket exists
        var basketEntity = await context.Baskets
                               .Include(b => b.Items)
                               .ThenInclude(i => i.Product)
                               .Include(b => b.AppliedDiscount)
                               .FirstOrDefaultAsync(b => b.Id == cmd.BasketId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Basket with ID {cmd.BasketId} not found");

        // Add all items to basket
        foreach (var item in cmd.Request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero");
            }

            // Check if product exists
            var product = await context.Products
                          .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken)
                          ?? throw new KeyNotFoundException($"Product with ID {item.ProductId} not found");

            // Check stock availability
            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity}");
            }

            // Check if item already exists in basket
            var existingItem = basketEntity.Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (existingItem != null)
            {
                // Update existing item quantity
                existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);

                // Update the entity in context
                context.BasketItems.Update(existingItem);
            }
            else
            {
                // Create new basket item
                var basketItem = new BasketItem
                {
                    Id = Guid.NewGuid(),
                    BasketId = basketEntity.Id,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    AddedAt = DateTime.UtcNow
                };

                // Add the new item to the database
                context.BasketItems.Add(basketItem);

                // Add to the basket's Items collection for proper tracking
                basketEntity.Items.Add(basketItem);
            }
        }

        basketEntity.UpdatedAt = DateTime.UtcNow;
        context.Baskets.Update(basketEntity);
        await context.SaveChangesAsync(cancellationToken);

        // Reload basket to get updated data
        var updatedBasket = await context.Baskets
                               .Include(b => b.Items)
                               .ThenInclude(i => i.Product)
                               .Include(b => b.AppliedDiscount)
                               .FirstOrDefaultAsync(b => b.Id == cmd.BasketId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Basket with ID {cmd.BasketId} not found");

        return updatedBasket.MapToDto();
    }
}

public record AddMultipleItemsRequest(List<AddItemsToBasketRequest> Items);
public record AddItemsToBasketRequest(Guid ProductId, int Quantity = 1);

public record AddItemsToBasketCommand(Guid BasketId, AddMultipleItemsRequest Request) : IRequest<BasketDto>;