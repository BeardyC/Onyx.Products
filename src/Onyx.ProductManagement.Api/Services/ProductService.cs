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
            logger.LogInformation("CreateProductAsync started for product: {ProductName}", request.Name);
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
            
            logger.LogInformation("Product {ProductId} created successfully", product.Id);
            return product.Id;
        }
        catch (Exception e)
        {
            logger.LogCritical("An exception occured when fetching products. Message: {ExceptionMessage}", e.Message);
            
            // We should also publish a message on failure
            // We can consume the failure elsewhere and deal with them
            return new ApiError("An error occured on the server while fetching data.");

        }
    }

    public async Task<OneOf<IEnumerable<Product>, ApiError>>  GetAllProductsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("GetAllProductsAsync started with pagination: PageNumber={PageNumber}, PageSize={PageSize}",
            pageNumber, pageSize);
        try
        {
<<<<<<< Updated upstream
            var products = await dbContext.Products
                .AsNoTracking()
                .ToListAsync(cancellationToken);

=======
            var query = dbContext.Products.AsNoTracking();
            
            // We obviously wouldn't have this in a real system.
            // This is just for testing purposes in case we want to pull everything 
            if (pageNumber.HasValue && pageSize.HasValue && pageNumber.Value > 0 && pageSize.Value > 0)
            {
                query = query
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }
            
            var products = await query.ToListAsync(cancellationToken);
            
            logger.LogInformation("ProductService.GetAllProductsAsync retrieved {ProductCount} products", products.Count);
>>>>>>> Stashed changes
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
        logger.LogInformation("GetProductsByColourAsync started for colour: {Colour}", colour);
        try
        {
            var products = await dbContext.Products
                .AsNoTracking()
                .Where(p => p.Colour == colour)
                .ToListAsync(cancellationToken);
            
            logger.LogInformation("ProductService.GetProductsByColourAsync retrieved {ProductCount} products for colour {Colour}", products.Count(), colour);

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