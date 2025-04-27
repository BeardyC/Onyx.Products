using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Onyx.ProductManagement.Api.Common;
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
    public void ShouldGenerateTokenWithCorrectRoles(string username, string[] expectedRoles)
    {
        // Act
        var result = _tokenService.GenerateToken(username);
        result.IsT0.Should().BeTrue("");
        var handler = new JwtSecurityTokenHandler();
        var token = result.AsT0;
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        roleClaims.Should().BeEquivalentTo(expectedRoles);
    }
    
    [Fact]
    public void ShouldReturnErrorForUnknownUser()
    {
        // Arrange
        var unknownUsername = "unknownUser";

        // Act
        var result = _tokenService.GenerateToken(unknownUsername);

        // Assert
        result.Should().NotBeNull();
        result.IsT1.Should().BeTrue("An error should be returned for unknown users");
        var error = result.AsT1;
        error.Should().BeOfType<ApiError>();
        error.Message.Should().Be("Unknown or invalid user specified.");
    }
}
