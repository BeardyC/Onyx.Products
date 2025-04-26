namespace Onyx.ProductManagement.Api.Endpoints.v1.Products;

internal record CreateProductRequest(string Name, decimal Price, string Colour);