using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Features.Basket;

// • Get the total cost for the basket (including 20% VAT)
// • Get the total cost without VAT
public static class GetTotalCost
{
    public static IEndpointRouteBuilder MapGetTotalCostEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/baskets/{basketId:guid}/total", async (Guid basketId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var query = new GetTotalCostQuery(basketId, IsTotalWithVat: true);
                    var result = await mediator.Send(query, cancellationToken);
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
            .WithName("GetBasketTotalWithVat")
            .WithDescription("Gets the total cost of the specified basket, including 20% VAT.")
            .WithTags("Baskets");

        app.MapGet("/api/baskets/{basketId:guid}/total/exluding-vat", async (Guid basketId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var query = new GetTotalCostQuery(basketId, IsTotalWithVat: false);
                    var result = await mediator.Send(query, cancellationToken);
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
            .WithName("GetBasketTotalWithoutVat")
            .WithDescription("Gets the total cost of the specified basket, excluding VAT.")
            .WithTags("Baskets");

        return app;
    }
}

public class GetTotalCostHandler(AppDbContext context) : IRequestHandler<GetTotalCostQuery, decimal>
{
    public async Task<decimal> Handle(GetTotalCostQuery query, CancellationToken cancellationToken)
    {
        // Check if basket exists
        var basketEntity = await context.Baskets
                               .Include(b => b.Items)
                               .ThenInclude(i => i.Product)
                               .FirstOrDefaultAsync(b => b.Id == query.BasketId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Basket with ID {query.BasketId} not found");
        
        var basketDto = basketEntity.MapToDto();
        
        return query.IsTotalWithVat ? basketDto.Total : basketDto.TotalWithoutVat;
    }
}

public record GetTotalCostQuery(Guid BasketId, bool IsTotalWithVat) : IRequest<decimal>;