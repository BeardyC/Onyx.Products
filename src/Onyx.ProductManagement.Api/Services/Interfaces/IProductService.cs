using Onyx.ProductManagement.Api.ApiModels;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using OneOf;
using Onyx.ProductManagement.Api.Common;

namespace Onyx.ProductManagement.Api.Services.Interfaces;

internal interface IProductService
{
    Task<OneOf<IEnumerable<Product>, ApiError>> GetAllProductsAsync(int? pageNumber, int? pageSize, CancellationToken ct);
    Task<OneOf<IEnumerable<Product>, ApiError>> GetProductsByColourAsync(string color, CancellationToken ct);
    Task<OneOf<int, ApiError>>  CreateProductAsync(CreateProductRequest product, CancellationToken ct);
}