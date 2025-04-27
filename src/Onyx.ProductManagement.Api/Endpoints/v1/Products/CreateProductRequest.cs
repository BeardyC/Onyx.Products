namespace Onyx.ProductManagement.Api.Endpoints.v1.Products;

internal record CreateProductRequest(string Name, decimal Price, string Colour);
internal record PaginationParameters(int? PageNumber, int? PageSize);
