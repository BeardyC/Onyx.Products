using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Onyx.ProductManagement.Api.Constants;
using Onyx.ProductManagement.Api.Services.Interfaces;

namespace Onyx.ProductManagement.Api.Services;

// This would not be used at all.
// Ideally we would be using AzureAD or similar.
internal class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;
    private static readonly IReadOnlyDictionary<string, string[]> UserRoles = new Dictionary<string, string[]>
    {
        { "readuser", [AppRoles.ProductReadAccess] },
        { "writeuser", [AppRoles.ProductReadAccess, AppRoles.ProductWriteAccess] },
        { "adminuser", [AppRoles.ProductReadAccess, AppRoles.ProductWriteAccess, AppRoles.ProductAdminAccess] }
    }.AsReadOnly();

    
    public string GenerateToken(string username)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (UserRoles.TryGetValue(username.ToLower(), out var roles))
        {
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, AppRoles.ProductReadAccess));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}