using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Onyx.ProductManagement.Api.ApiModels;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using Onyx.ProductManagement.Api.Services.Interfaces;

namespace Onyx.ProductManagement.Api.IntegrationTests;

public class ProductsApiTests : IClassFixture<ApiWebFactory>
{
    private readonly HttpClient _client;
    private readonly ITokenService _tokenService;

    public ProductsApiTests(ApiWebFactory factory)
    {
        _client = factory.CreateClient();

        var sp = factory.Services.CreateScope().ServiceProvider;
        _tokenService = sp.GetRequiredService<ITokenService>();
    }

    private void AuthenticateClient(string username = "admin")
    {
        var token = _tokenService.GenerateToken(username);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        var createResponse = await _client.PostAsJsonAsync("/products", request);

        if (createResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"Server returned 500. Content: {errorContent}");
        }
        // Assert creation
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location;
        location.Should().NotBeNull();

        var productId = await createResponse.Content.ReadFromJsonAsync<int>();
        productId.Should().BeGreaterThan(0);

        location!.ToString().Should().Be($"/products/{productId}");

        // Fetch products
        var getResponse = await _client.GetAsync("/products");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        var products = result.ToList();
        products.Should().NotBeNull();

        var createdProduct = products!.FirstOrDefault(p => p.Id == productId);
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be(request.Name);
        createdProduct.Price.Should().Be(request.Price);
        createdProduct.Colour.Should().Be(request.Colour);
    }

    [Fact]
    public async Task ShouldReturnAllProductsIfProductsExist()
    {
        // Arrange
        AuthenticateClient("readUser");
        var request = new CreateProductRequest(
        
            Name: "Test Product",
            Colour: "Blue",
            Price: 49.99m
        );

        await _client.PostAsJsonAsync("/products", request);

        // Act
        var response = await _client.GetAsync("/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        products.Should().NotBeNull();
        products!.Should().ContainSingle(p => p.Name == "Blue Product" && p.Colour == "Blue");
    }

    [Fact]
    public async Task ShouldReturnProductsByColourIfProductsExist()
    {
        // Arrange
        AuthenticateClient("writeUser");
        var createRequest = new CreateProductRequest(
            Name: "Green Product",
            Colour: "Green",
            Price: 29.99m
        );

        await _client.PostAsJsonAsync("/products", createRequest);

        // Act
        var response = await _client.GetAsync("/products/colour/Green");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        products.Should().NotBeNull();
        products!.Should().AllSatisfy(p => p.Colour.Should().Be("Green"));
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

        await _client.PostAsJsonAsync("/products", createRequest);

        // Act
        var response = await _client.GetAsync("/products/colour/Purple"); // No product with "Purple"

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
        var response = await _client.GetAsync("/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}