using Microsoft.EntityFrameworkCore;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using Onyx.ProductManagement.Api.Services.Interfaces;
using Onyx.ProductManagement.Data.Context;
using Product = Onyx.ProductManagement.Api.ApiModels.Product;
using OneOf;
using Onyx.ProductManagement.Api.Common;

namespace Onyx.ProductManagement.Api.Services;

internal class ProductService(
    ProductsDbContext dbContext,
    ILogger<ProductService> logger)
    : IProductService
{
    public async Task<OneOf<int, ApiError>>  CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var product = new Data.Models.Product
            {
                Name = request.Name,
                Colour = request.Colour,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            // For comms with other services we would publish a message here
            // RMBQ, AWS SNS, Azure Svc Bus etc etc 
            // await messageBus.PublishAsync(new ProductCreatedMessage
            // {
            //     ProductId = product.Id,
            //     Name = product.Name,
            //     Colour = product.Colour,
            //     Price = product.Price
            // });
            
            return product.Id;
        }
        catch (Exception e)
        {
            logger.LogCritical("An exception occured when fetching products. Message: {ExceptionMessage}", e.Message);
            return new ApiError("An error occured on the server while fetching data.");

        }
    }

    public async Task<OneOf<IEnumerable<Product>, ApiError>>  GetAllProductsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var products = await dbContext.Products
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return products.Select(p => p.ToDto()).ToList();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to fetch all products from the database.");

            return new ApiError("An unexpected error occurred while retrieving products.");
        }
    }


    public async Task<OneOf<IEnumerable<Product>, ApiError>> GetProductsByColourAsync(string colour, CancellationToken cancellationToken)
    {
        try
        {
            var products = await dbContext.Products
                .AsNoTracking()
                .Where(p => p.Colour == colour)
                .ToListAsync(cancellationToken);

            return products.Select(p => p.ToDto()).ToList();
        }
        catch (Exception e)
        {
            logger.LogCritical(
                "An exception occured when fetching products by colour GetProductsByColourAsync. Message: {ExceptionMessage}",
                e.Message);
            return new ApiError("An unexpected error occurred while retrieving products.");
        }
    }
}

internal static class MappingExtension
{
    public static Product ToDto(this Data.Models.Product p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Colour = p.Colour
        };
}