using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Onyx.ProductManagement.Data.Models;

namespace Onyx.ProductManagement.Data.Context;

[ExcludeFromCodeCoverage]
public class ProductsDbContext : DbContext
{
    public ProductsDbContext()
    {
    }

    public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
        : base(options)
    {
        
    }

    public virtual DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Colour, "IX_Products_Colour");

            entity.Property(e => e.Colour).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

    }
}
