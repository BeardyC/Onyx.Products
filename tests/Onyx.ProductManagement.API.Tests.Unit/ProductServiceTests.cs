using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using Onyx.ProductManagement.Data.Context;

namespace Onyx.ProductManagement.API.Tests.Unit;

public class ProductServiceTests
{
    private readonly ProductsDbContext _dbContext;
    private readonly ILogger<ProductService> _logger;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProductsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProductsDbContext(options);
        _logger = A.Fake<ILogger<ProductService>>();

        _productService = new ProductService(_dbContext, _logger);
    }

    [Fact]
    public async Task ShouldSuccessfullyCreateProductIfRequestIsValid()
    {
        // Arrange
        var request = new CreateProductRequest(
            Name: "Test Product",
            Price: 9.99m,
            Colour: "Red"
        );

        // Act
        var result = await _productService.CreateProductAsync(request, CancellationToken.None);

        var productId = result.AsT0;
        
        // Assert
        productId.Should().BeGreaterThan(0);

        var productInDb = await _dbContext.Products.FindAsync(productId);
        productInDb.Should().NotBeNull();
        productInDb.Name.Should().Be("Test Product");
        productInDb.Colour.Should().Be("Red");
        productInDb.Price.Should().Be(9.99m);
    }

    [Fact]
    public async Task ShouldReturnAllProductsIfProductsExist()
    {
        // Arrange
        _dbContext.Products.AddRange(
            new Data.Models.Product { Name = "Product 1", Colour = "Blue", Price = 5 },
            new Data.Models.Product { Name = "Product 2", Colour = "Green", Price = 10 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var products = await _productService.GetAllProductsAsync(CancellationToken.None);

        // Assert
        var result = products.AsT0.ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Product 1");
        result.Should().Contain(p => p.Name == "Product 2");
    }

    [Fact]
    public async Task ShouldReturnProductsByColourIfMatchingProductsExist()
    {
        // Arrange
        _dbContext.Products.AddRange(
            new Data.Models.Product { Name = "Red Product 1", Colour = "Red", Price = 5 },
            new Data.Models.Product { Name = "Red Product 2", Colour = "Red", Price = 15 },
            new Data.Models.Product { Name = "Blue Product", Colour = "Blue", Price = 20 }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var redProducts = await _productService.GetProductsByColourAsync("Red", CancellationToken.None);

        // Assert
        var result = redProducts.AsT0.ToList();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Colour == "Red");
    }

    [Fact]
    public async Task ShouldReturnErrorAndLogCriticalIfCreateProductFails()
    {
        // Arrange
        var brokenService = new ProductService(null!, _logger);

        var request = new CreateProductRequest(
            Name: "Should Fail",
            Price: 0,
            Colour: "Black"
        );

        // Act
        var result = await brokenService.CreateProductAsync(request, CancellationToken.None);

        // Assert
        result.IsT1.Should().BeTrue();
        var errorResult = result.AsT1;
        errorResult.Message.Should().Contain("An error occured");

        A.CallTo(_logger)
            .Where(call => call.Method.Name == "Log" && (LogLevel)call.Arguments[0]! == LogLevel.Critical)
            .MustHaveHappened();
    }
}
