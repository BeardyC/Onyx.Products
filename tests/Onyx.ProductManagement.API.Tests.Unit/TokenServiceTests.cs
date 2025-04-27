using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Onyx.ProductManagement.Api.Constants;

namespace Onyx.ProductManagement.API.Tests.Unit;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var jwtSettings = new JwtSettings
        {
            Key = "ThisIsASecretKeyForJwt1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        };

        var jwtOptions = A.Fake<IOptions<JwtSettings>>();
        A.CallTo(() => jwtOptions.Value).Returns(jwtSettings);

        _tokenService = new TokenService(jwtOptions);
    }

    [Theory]
    [InlineData("readUser", new[] { AppRoles.ProductReadAccess })]
    [InlineData("writeUser", new[] { AppRoles.ProductReadAccess, AppRoles.ProductWriteAccess })]
    [InlineData("adminUser", new[] { AppRoles.ProductReadAccess, AppRoles.ProductWriteAccess, AppRoles.ProductAdminAccess })]
    [InlineData("unknownUser", new[] { AppRoles.ProductReadAccess })]
    public void ShouldGenerateTokenWithCorrectRoles(string username, string[] expectedRoles)
    {
        // Act
        var token = _tokenService.GenerateToken(username);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        roleClaims.Should().BeEquivalentTo(expectedRoles);
    }
}
