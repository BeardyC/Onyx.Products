using Onyx.ProductManagement.Api.ApiModels;
using Onyx.ProductManagement.Api.Constants;
using Onyx.ProductManagement.Api.Services.Interfaces;

namespace Onyx.ProductManagement.Api.Endpoints.v1.Products;

internal static class ProductsEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateProductAsync)
            .RequireAuthorization(AppRoles.ProductWriteAccess, AppRoles.ProductReadAccess);
        group.MapGet("/", GetAllProductsAsync)
            .RequireAuthorization(AppRoles.ProductReadAccess)
            .Produces<IEnumerable<Product>>();
        group.MapGet("/colour/{colour}", GetProductsByColourAsync)
            .Produces<IEnumerable<Product>>()
            .RequireAuthorization(AppRoles.ProductReadAccess)
            .WithName("GetProductsByColour")
            .WithDescription("Gets products by colour");

        return group;
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        // IValidator<CreateProductRequest> validator,
        IProductService productService, CancellationToken cancellationToken)
    {
        // var validationResult = await validator.ValidateAsync(request);
        // if (!validationResult.IsValid)
        //     return validationResult.ToProblemDetails();

        var productId = await productService.CreateProductAsync(request, cancellationToken);

        return Results.Created($"/products/{productId}", productId);
    }

    private static async Task<IResult> GetAllProductsAsync(IProductService productService, CancellationToken cancellationToken)
    {
        var products = await productService.GetAllProductsAsync(cancellationToken);
        return Results.Ok(products);
    }

    private static async Task<IResult> GetProductsByColourAsync(string colour, IProductService productService, CancellationToken cancellationToken)
    {
        var products = await productService.GetProductsByColourAsync(colour, cancellationToken);
        return Results.Ok(products);
    }
}