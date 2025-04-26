using Microsoft.EntityFrameworkCore;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using Onyx.ProductManagement.Api.Services.Interfaces;
using Onyx.ProductManagement.Data.Context;
using Product = Onyx.ProductManagement.Api.ApiModels.Product;

namespace Onyx.ProductManagement.Api.Services;

internal class ProductService(
    ProductsDbContext dbContext,
    ILogger<ProductService> logger)
    : IProductService
{
    public async Task<int> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
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

            return product.Id;
        }
        catch (Exception e)
        {
            logger.LogCritical("An exception occured when fetching products. Message: {ExceptionMessage}", e.Message);

        }

        return 0; // TODO: Return OneOf<,>
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken)
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
            logger.LogCritical("An exception occured when fetching products. Message: {ExceptionMessage}", e.Message);
        }

        return [];
    }


    public async Task<IEnumerable<Product>> GetProductsByColourAsync(string colour, CancellationToken cancellationToken)
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
        }

        return [];
    }
}

public static class MappingExtension
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