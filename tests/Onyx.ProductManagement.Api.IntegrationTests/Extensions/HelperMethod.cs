using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Onyx.ProductManagement.Api.IntegrationTests.Extensions;

public static class HelperMethod
{
    public static IServiceCollection RemoveDbContextOptions<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<TDbContext>));

        if (descriptor != null)
            services.Remove(descriptor);

        services.RemoveAll(typeof(TDbContext));
        return services;
    }
}