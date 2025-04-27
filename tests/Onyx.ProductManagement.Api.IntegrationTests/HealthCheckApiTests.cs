using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Onyx.ProductManagement.Api.IntegrationTests;

public class HealthCheckApiTests(ApiWebFactory factory) : IClassFixture<ApiWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ShouldReturnOkAndHealthStatusWhenHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var healthReportResponse = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
        healthReportResponse.Should().NotBeNull();
        healthReportResponse.Status.Should().Be("Healthy");
        healthReportResponse.Checks.Should().NotBeNull();
        healthReportResponse.Checks.Should().ContainSingle(c => c.Name == "products_db");
        healthReportResponse.Checks.Should().ContainSingle(c => c.Name == "api_status");
    }

    private record HealthCheckResponse(string Status, IEnumerable<HealthCheckEntryResponse> Checks);

    private record HealthCheckEntryResponse(string Name, string Status, string? Description, double Duration);
}