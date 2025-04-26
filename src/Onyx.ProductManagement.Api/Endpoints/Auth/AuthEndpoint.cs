using Onyx.ProductManagement.Api.Services.Interfaces;

namespace Onyx.ProductManagement.Api.Endpoints.Auth;

internal static class AuthEndpoint
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/{user}", GenerateToken);

        return group;
    }
    
    private static IResult GenerateToken(
        string user,
        ITokenService tokenService)
    {
        if (string.IsNullOrEmpty(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Request",
                detail: "Username is required");
        }
        var token = tokenService.GenerateToken(user);
        return Results.Json(token);
    }
}