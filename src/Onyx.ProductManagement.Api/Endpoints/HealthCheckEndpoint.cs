using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Onyx.ProductManagement.Api.Endpoints;

public static class HealthCheckEndpoint
{
    public static RouteGroupBuilder MapHealthCheckEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", HealthCheck)
            .WithName("HealthCheck")
            .WithDescription("HealthCheck");

        return group;
    }

    private static async Task<IResult> HealthCheck(HttpContext context, HealthCheckService healthCheckService)
    {
        var report = await healthCheckService.CheckHealthAsync();

        var response = new
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration.TotalSeconds
            })
        };

        
        return report.Status switch
        {
            HealthStatus.Healthy => Results.Ok(response), 
            HealthStatus.Degraded => Results.Ok(response),
            _ => Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable) 
        };
    }
}