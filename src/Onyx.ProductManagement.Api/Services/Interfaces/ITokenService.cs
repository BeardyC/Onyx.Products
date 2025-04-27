using Onyx.ProductManagement.Api.Common;
using OneOf;

namespace Onyx.ProductManagement.Api.Services.Interfaces;

public interface ITokenService
{
    OneOf<string, ApiError> GenerateToken(string username);
}