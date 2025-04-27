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
        
        SeedBaseData();
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
        // Act
        var products = await _productService.GetAllProductsAsync(null, null,CancellationToken.None);

        // Assert
        var result = products.AsT0.ToList();
        result.Should().HaveCount(5);
        result.Should().Contain(p => p.Name == "Product 1");
        result.Should().Contain(p => p.Name == "Product 2");
    }

    [Fact]
    public async Task ShouldReturnProductsByColourIfMatchingProductsExist()
    {
        // Act
        var redProducts = await _productService.GetProductsByColourAsync("Red", CancellationToken.None);

        // Assert
        var result = redProducts.AsT0.ToList();
        result.Should().HaveCount(1);
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
    
    [Theory] 
    [InlineData(1, 2, 2)] 
    [InlineData(2, 2, 2)] 
    [InlineData(3, 2, 1)] 
    [InlineData(1, 5, 5)] 
    [InlineData(2, 5, 0)] 
    [InlineData(1, 10, 5)]
    public async Task ShouldReturnCorrectNumberOfProductsWhenValidPaginationParametersAreProvided(int pageNumber, int pageSize, int expectedCount)
    {
        // Act
        var products = await _productService.GetAllProductsAsync(pageNumber, pageSize, CancellationToken.None);

        // Assert
        var result = products.AsT0.ToList();
        result.Should().HaveCount(expectedCount);
    }
    
    [Fact]
    public async Task ShouldReturnDuplicateProductErrorWhenProductWithSameNameExists()
    {
        // Arrange
        var existingProductName = "Product 1";
        var request = new CreateProductRequest(
            Name: existingProductName,
            Price: 9.99m,
            Colour: "Orange"
        );

        // Act
        var result = await _productService.CreateProductAsync(request, CancellationToken.None);

        // Assert
        result.IsT2.Should().BeTrue(); // Should be the DuplicateProductError type
        var errorResult = result.AsT2;
        errorResult.Message.Should().Contain($"Product with name '{existingProductName}' already exists.");

        var productCount = await _dbContext.Products.CountAsync();
        productCount.Should().Be(5);
    }
    
    private void SeedBaseData()
    {
        _dbContext.Products.AddRange(
            new Data.Models.Product { Id = 1, Name = "Product 1", Colour = "Blue", Price = 5, CreatedAt = DateTime.UtcNow.AddMinutes(1) },
            new Data.Models.Product { Id = 2, Name = "Product 2", Colour = "Green", Price = 10, CreatedAt = DateTime.UtcNow.AddMinutes(2) },
            new Data.Models.Product { Id = 3, Name = "Product 3", Colour = "Red", Price = 15, CreatedAt = DateTime.UtcNow.AddMinutes(3) },
            new Data.Models.Product { Id = 4, Name = "Product 4", Colour = "Yellow", Price = 20, CreatedAt = DateTime.UtcNow.AddMinutes(4) },
            new Data.Models.Product { Id = 5, Name = "Product 5", Colour = "Orange", Price = 25, CreatedAt = DateTime.UtcNow.AddMinutes(5) }
        );

        _dbContext.SaveChanges();
    }
}
