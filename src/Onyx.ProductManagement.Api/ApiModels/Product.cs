namespace Onyx.ProductManagement.Api.ApiModels;

public record Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Colour { get; set; } = string.Empty;
}