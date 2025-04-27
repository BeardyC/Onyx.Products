using FluentValidation;
using Onyx.ProductManagement.Api.ApiModels;
using Onyx.ProductManagement.Api.Common;
using Onyx.ProductManagement.Api.Constants;
using Onyx.ProductManagement.Api.Services.Interfaces;

namespace Onyx.ProductManagement.Api.Endpoints.v1.Products;

internal static class ProductsEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateProductAsync)
            .RequireAuthorization(AppRoles.ProductWriteAccess)    
            .Produces<int>(StatusCodes.Status201Created) 
            .Produces<IEnumerable<ValidationFailureResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<DuplicateProductError>(StatusCodes.Status500InternalServerError);
        
        group.MapGet("/", GetAllProductsAsync)
            .RequireAuthorization(AppRoles.ProductReadAccess)
            .Produces<IEnumerable<Product>>() 
            .Produces<ApiError>(StatusCodes.Status401Unauthorized) 
            .Produces<ApiError>(StatusCodes.Status500InternalServerError);

        group.MapGet("/colour/{colour}", GetProductsByColourAsync)
            .Produces<IEnumerable<Product>>()
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AppRoles.ProductReadAccess)
            .WithName("GetProductsByColour")
            .WithDescription("Gets products by colour");

        return group;
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request, IValidator<CreateProductRequest> validator,
        IProductService productService, ILogger<IApiMarker> logger, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received CreateProduct request for product: {ProductName}", request.Name);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Validation failed for CreateProduct request: {ValidationErrors}", validationResult.Errors);
            var errors = validationResult.Errors.Select(error => new ValidationFailureResponse
            {
                PropertyName = error.PropertyName,
                ErrorMessage = error.ErrorMessage
            });
            return Results.BadRequest(errors);
        }
            
        var result = await productService.CreateProductAsync(request, cancellationToken);
        
        return result.Match<IResult>(
            productId =>
                Results.Created($"/v1/products/{productId}", productId),
            (ApiError error) =>
            {
                logger.LogError("Product creation failed: {ErrorMessage}", error.Message);
                return Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Product Creation Failed",
                    detail: error.Message
                );
            },
            (DuplicateProductError error) =>
            {
                logger.LogWarning("Product creation failed due to duplicate name: {ErrorMessage}", error.Message);
                return Results.Conflict(new ApiError(error.Message));
            }
        );
    }

    private static async Task<IResult> GetAllProductsAsync(
        [AsParameters] PaginationParameters paginationParameters, IProductService productService,
        ILogger<IApiMarker> logger, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received GetAllProducts request with pagination: PageNumber={PageNumber}, PageSize={PageSize}",
            paginationParameters.PageNumber, paginationParameters.PageSize);
        var result = await productService.GetAllProductsAsync(paginationParameters.PageNumber, paginationParameters.PageSize, cancellationToken);

        return result.Match<IResult>(
            products =>
                Results.Ok(products),
            error => 
                Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Failed to fetch products.",
                    detail: error.Message
                ));
    }

    private static async Task<IResult> GetProductsByColourAsync(string colour, IProductService productService, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(colour))
        {
            return Results.BadRequest();
        }
        
        var result = await productService.GetProductsByColourAsync(colour, cancellationToken);
        
        return result.Match<IResult>(
            products =>
                Results.Ok(products),
            error => 
                Results.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Failed to fetch products by colour.",
                    detail: error.Message
                ));
    }
}