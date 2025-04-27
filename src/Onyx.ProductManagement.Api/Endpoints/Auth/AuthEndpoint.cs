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
        var result = tokenService.GenerateToken(user);
        return result.Match<IResult>(
            token =>
                Results.Ok(token),
            error =>
                Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Authentication Failed",
                    detail: error.Message)
        );
    }
}