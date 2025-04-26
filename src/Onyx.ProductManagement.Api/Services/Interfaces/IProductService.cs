using Onyx.ProductManagement.Api.ApiModels;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;

namespace Onyx.ProductManagement.Api.Services.Interfaces;

internal interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken ct);
    Task<IEnumerable<Product>> GetProductsByColourAsync(string color, CancellationToken ct);
    Task<int> CreateProductAsync(CreateProductRequest product, CancellationToken ct);
}