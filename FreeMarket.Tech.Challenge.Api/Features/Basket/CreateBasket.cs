using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.DTOs;
using FreeMarket.Tech.Challenge.Api.Extensions;
using MediatR;

namespace FreeMarket.Tech.Challenge.Api.Features.Basket;

public static class CreateBasketEndpoint
{
    public static IEndpointRouteBuilder MapCreateBasketEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/baskets", async (CreateBasketRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateBasketCommand
            {
                CustomerEmail = request.CustomerEmail
            };

            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/baskets/{result.Id}", result);
        })
        .WithName("CreateBasket")
        .WithTags("Baskets");

        return routes;
    }
}

public class CreateBasketCommandHandler(AppDbContext context) : IRequestHandler<CreateBasketCommand, BasketDto>
{
    public async Task<BasketDto> Handle(CreateBasketCommand cmd, CancellationToken cancellationToken)
    {
        // TODO: Check if a basket already exists for the customer email
        
        var basketEntity = new Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerEmail = cmd.CustomerEmail,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Baskets.Add(basketEntity);
        await context.SaveChangesAsync(cancellationToken);

        return basketEntity.MapToDto();
    }
}

public record CreateBasketRequest
{
    public string CustomerEmail { get; set; } = string.Empty;
}

public record CreateBasketCommand : IRequest<BasketDto>
{
    public string CustomerEmail { get; set; } = string.Empty;
}