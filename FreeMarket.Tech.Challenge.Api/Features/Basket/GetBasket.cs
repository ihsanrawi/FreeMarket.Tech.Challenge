using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Features.Basket;

public static class GetBasketEndpoint
{

    public static IEndpointRouteBuilder MapGetBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/baskets/{basketId:guid}", async (Guid basketId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var query = new GetBasketQuery(basketId);
                var result = await mediator.Send(query, cancellationToken);
                if (result == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetBasket")
        .WithTags("Baskets");

        return app;
    }
}

public class GetBasketQueryHandler(AppDbContext context) : IRequestHandler<GetBasketQuery, BasketDto?>
{
    public async Task<BasketDto?> Handle(GetBasketQuery query, CancellationToken cancellationToken)
    {
        var basketEntity = await context.Baskets
            .Include(b => b.Items)
            .ThenInclude(i => i.Product)
            .Include(b => b.AppliedDiscount)
            .FirstOrDefaultAsync(b => b.Id == query.BasketId, cancellationToken);

        if (basketEntity == null)
        {
            return null;
        }

        return basketEntity.MapToDto();
    }
}

public record GetBasketQuery (Guid BasketId) : IRequest<BasketDto?>;