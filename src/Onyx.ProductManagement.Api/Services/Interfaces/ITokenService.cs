namespace Onyx.ProductManagement.Api.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(string username);
}