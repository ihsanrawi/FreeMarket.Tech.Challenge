using System.Reflection;
using FreeMarket.Tech.Challenge.Api.Context;
using FreeMarket.Tech.Challenge.Api.Features.Basket;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("ShoppingBasketDb"));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Shopping Basket API", Version = "v1" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Basket API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Register endpoints
app.MapCreateBasketEndpoint();
app.MapGetBasketEndpoint();
app.MapAddItemToBasketEndpoints();
app.MapRemoveItemFromBasketEndpoint();
app.MapUpdateItemQuantityEndpoint();
app.MapGetTotalCostEndpoints();
app.MapApplyDiscountCodeEndpoint();
app.MapAddShippingAddressEndpoint();

app.Run();