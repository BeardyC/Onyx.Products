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

    [Fact]
    public void ShouldSuccessfullyGenerateTokenForReadUser()
    {
        // Arrange
        var username = "readUser";

        // Act
        var token = _tokenService.GenerateToken(username);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == AppRoles.ProductReadAccess);
    }

    [Fact]
    public void ShouldSuccessfullyGenerateTokenForWriteUser()
    {
        // Arrange
        var username = "writeUser";

        // Act
        var token = _tokenService.GenerateToken(username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == AppRoles.ProductReadAccess);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == AppRoles.ProductWriteAccess);
    }

    [Fact]
    public void ShouldSuccessfullyGenerateTokenForAdminUser()
    {
        // Arrange
        var username = "adminUser";

        // Act
        var token = _tokenService.GenerateToken(username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == AppRoles.ProductReadAccess);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == AppRoles.ProductWriteAccess);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == AppRoles.ProductAdminAccess);
    }

    [Fact]
    public void ShouldAssignDefaultRoleIfUsernameIsUnknown()
    {
        // Arrange
        var username = "unknownUser";

        // Act
        var token = _tokenService.GenerateToken(username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == AppRoles.ProductReadAccess);
    }
}
