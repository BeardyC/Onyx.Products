using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Onyx.ProductManagement.Api.ApiModels;
using Onyx.ProductManagement.Api.Common;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using Onyx.ProductManagement.Api.Services.Interfaces;
using Onyx.ProductManagement.Data.Context;

namespace Onyx.ProductManagement.Api.IntegrationTests;

[Collection("ProductsApiTests")]
public class ProductsApiTests : IClassFixture<ApiWebFactory>
{
    private readonly HttpClient _client;
    private readonly ITokenService _tokenService;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductsApiTests(ApiWebFactory factory)
    {
        _client = factory.CreateClient();

        var sp = factory.Services.CreateScope().ServiceProvider;
        _tokenService = sp.GetRequiredService<ITokenService>();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

    }

    private void AuthenticateClient(string username)
    {
        var token = _tokenService.GenerateToken(username);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AsT0);
    }
    
    [Fact]
    public async Task ShouldCreateProductSuccessfully()
    {
        // Arrange
        AuthenticateClient("writeUser");
        var request = new CreateProductRequest(
            Name: "Test Product",
            Price: 99.99m,
            Colour: "Red"
        );

        // Act
        var createResponse = await _client.PostAsJsonAsync("/v1/products", request);

        if (createResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"Server returned 500. Content: {errorContent}");
        }
        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location;
        location.Should().NotBeNull();

        var productId = await createResponse.Content.ReadFromJsonAsync<int>();
        productId.Should().BeGreaterThan(0);

        location.ToString().Should().Be($"/v1/products/{productId}");

        // Fetch products
        var getResponse = await _client.GetAsync("/v1/products");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        var products = result!.ToList();
        products.Should().NotBeNull();

        var createdProduct = products.FirstOrDefault(p => p.Id == productId);
        createdProduct.Should().NotBeNull();
        createdProduct.Name.Should().Be(request.Name);
        createdProduct.Price.Should().Be(request.Price);
        createdProduct.Colour.Should().Be(request.Colour);
    }

    [Fact]
    public async Task ShouldReturnAllProductsIfProductsExist()
    {
        // Arrange
        AuthenticateClient("readUser");
        var productsToSeed = new[]
        {
            new Data.Models.Product { Name = "Product A", Colour = "Black", Price = 10.00m, CreatedAt = DateTime.UtcNow },
            new Data.Models.Product { Name = "Product B", Colour = "White", Price = 20.00m, CreatedAt = DateTime.UtcNow.AddMinutes(1) },
            new Data.Models.Product { Name = "Product C", Colour = "Green", Price = 30.00m, CreatedAt = DateTime.UtcNow.AddMinutes(2) }
        };
        await SeedTestDataAsync(productsToSeed);

        // Act
        var response = await _client.GetAsync("/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        products.Should().NotBeNull();
        products.Should().ContainSingle(p => p.Name == "Product A" && p.Colour == "Black");
    }

    [Fact]
    public async Task ShouldReturnProductsByColourIfProductsExist()
    {
        // Arrange
        AuthenticateClient("writeUser");
        var createRequest = new CreateProductRequest(
            Name: "Product A",
            Colour: "Green",
            Price: 29.99m
        );

        await _client.PostAsJsonAsync("/v1/products", createRequest);

        // Act
        var response = await _client.GetAsync("/v1/products/colour/Green");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        products.Should().NotBeNull();
        products.Should().AllSatisfy(p => p.Colour.Should().Be("Green"));
    }

    [Fact]
    public async Task ShouldReturnEmptyListWhenNoProductsMatchColour()
    {
        // Arrange
        AuthenticateClient("readUser");
        
        var createRequest = new CreateProductRequest(
            Name: "Yellow Product",
            Colour: "Yellow",
            Price: 59.99m
        );

        await _client.PostAsJsonAsync("/v1/products", createRequest);

        // Act
        var response = await _client.GetAsync("/v1/products/colour/Purple");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        products.Should().NotBeNull();
        products.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ShouldReturnUnauthorisedWhenUserIsNotAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task ShouldReturnBadRequestWhenCreateProductRequestIsInvalid()
    {
        // Arrange
        AuthenticateClient("adminUser");
        var invalidRequest = new CreateProductRequest(
            Name: "", 
            Price: -10,
            Colour: ""
        );

        // Act
        var response = await _client.PostAsJsonAsync("/v1/products", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorList = await response.Content.ReadFromJsonAsync<IEnumerable<ValidationFailureResponse>>();
        var validationFailureResponses = (errorList ?? Array.Empty<ValidationFailureResponse>()).ToList();
        validationFailureResponses.Should().NotBeNull();
        validationFailureResponses.Should().Contain(e => e.PropertyName == "Name");
        validationFailureResponses.Should().Contain(e => e.PropertyName == "Price");
        validationFailureResponses.Should().Contain(e => e.PropertyName == "Colour");
    }
    
    [Theory]
    [InlineData(1, 2, 2)]
    [InlineData(2, 2, 1)]
    [InlineData(1, 5, 3)]
    [InlineData(2, 5, 0)]
    public async Task ShouldReturnPaginatedProductsWhenPaginationParametersAreProvided(int pageNumber, int pageSize, int expectedCount)
    {
        // Arrange
        var productsToSeed = new[]
        {
            new Data.Models.Product { Name = "Product A", Colour = "Black", Price = 10.00m, CreatedAt = DateTime.UtcNow },
            new Data.Models.Product { Name = "Product B", Colour = "White", Price = 20.00m, CreatedAt = DateTime.UtcNow.AddMinutes(1) },
            new Data.Models.Product { Name = "Product C", Colour = "Green", Price = 30.00m, CreatedAt = DateTime.UtcNow.AddMinutes(2) }
        };
        await SeedTestDataAsync(productsToSeed);
        AuthenticateClient("readUser");

        // Act
        var response = await _client.GetAsync($"/v1/products?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        products.Should().NotBeNull();
        products.Should().HaveCount(expectedCount);
    }
    
    [Fact]
    public async Task ShouldReturnConflictWhenCreatingProductWithDuplicateName()
    {
        // Arrange
        AuthenticateClient("writeUser");
        var request = new CreateProductRequest(
            Name: "Duplicate Product Test",
            Price: 10.00m,
            Colour: "Green"
        );

        // Act
        var createResponse1 = await _client.PostAsJsonAsync("/v1/products", request);
        createResponse1.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResponse2 = await _client.PostAsJsonAsync("/v1/products", request);

        // Assert
        createResponse2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await createResponse2.Content.ReadFromJsonAsync<ApiError>();
        error.Should().NotBeNull();
        error.Message.Should().Contain($"Product with name '{request.Name}' already exists.");
    }
    
    private async Task SeedTestDataAsync(params Data.Models.Product[] productsToSeed)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

        dbContext.Products.RemoveRange(dbContext.Products);
        await dbContext.SaveChangesAsync();

        if (productsToSeed.Any())
        {
            await dbContext.Products.AddRangeAsync(productsToSeed);
            await dbContext.SaveChangesAsync();
        }
    }
}
