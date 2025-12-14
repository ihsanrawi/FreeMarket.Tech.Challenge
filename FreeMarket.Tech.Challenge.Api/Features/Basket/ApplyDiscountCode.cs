using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreeMarket.Tech.Challenge.Api.Features.Basket;

// • Add a discount code to the basket (excluding discounted items)
public static class ApplyDiscountCodeEndpoint
{
    public static IEndpointRouteBuilder MapApplyDiscountCodeEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/baskets/{basketId:guid}/apply-discount", async (Guid basketId,
                ApplyDiscountCodeRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var command = new ApplyDiscountCodeCommand(basketId, request);
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
            .WithName("ApplyDiscountCode")
            .WithDescription("Applies a discount code to the specified basket, excluding discounted items.")
            .WithTags("Baskets");

        return routes;
    }
}

public  class ApplyDiscountCodeHandler(AppDbContext context) : IRequestHandler<ApplyDiscountCodeCommand, BasketDto>
{
    public async Task<BasketDto> Handle(ApplyDiscountCodeCommand cmd, CancellationToken cancellationToken)
    {
        // Check if basket exists
        var basketEntity = await context.Baskets
                               .Include(b => b.Items)
                               .ThenInclude(i => i.Product)
                               .Include(b => b.AppliedDiscount)
                               .FirstOrDefaultAsync(b => b.Id == cmd.BasketId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Basket with ID {cmd.BasketId} not found");
        
        var discountCode = await context.DiscountCodes
            .FirstOrDefaultAsync(d => d.Code == cmd.Request.DiscountCode && d.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Discount code '{cmd.Request.DiscountCode}' not found or is inactive");
        
        // validate the discount code and apply it to the basket.
        basketEntity.ApplyDiscount(discountCode);
        await context.SaveChangesAsync(cancellationToken);
        
        return basketEntity.MapToDto();
    }
}

public record ApplyDiscountCodeRequest(string DiscountCode);

public record ApplyDiscountCodeCommand(Guid BasketId, ApplyDiscountCodeRequest Request) : IRequest<BasketDto>;