using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onyx.ProductManagement.Api.IntegrationTests.Extensions;
using Onyx.ProductManagement.Data.Context;
using Onyx.ProductManagement.Data.Models;
using Testcontainers.MsSql;

namespace Onyx.ProductManagement.Api.IntegrationTests;

public class ApiWebFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _database = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContextOptions<ProductsDbContext>();
            services.AddDbContext<ProductsDbContext>(options =>
            {
                options.UseSqlServer(_database.GetConnectionString());
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    public Task InitializeAsync() => _database.StartAsync();

    public new Task DisposeAsync() => _database.StopAsync();
    
    private static void SeedTestData(ProductsDbContext dbContext)
    {
        if (dbContext.Products.Any()) 
            return;
        dbContext.Products.AddRange(
            new Product
            {
                Name = "Red Product",
                Colour = "Red",
                Price = 29.99M,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Blue Product",
                Colour = "Blue",
                Price = 59.99M,
                CreatedAt = DateTime.UtcNow
            }
        );

        dbContext.SaveChanges();
    }

}