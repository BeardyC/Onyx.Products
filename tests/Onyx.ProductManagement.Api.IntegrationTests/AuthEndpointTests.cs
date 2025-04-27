using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Onyx.ProductManagement.Api.Services.Interfaces;

namespace Onyx.ProductManagement.Api.IntegrationTests;

public class AuthEndpointTests(ApiWebFactory factory) : IClassFixture<ApiWebFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ShouldGenerateTokenSuccessfullyForValidUser()
    {
        var username = "adminUser";

        var response = await _client.GetAsync($"/auth/{username}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await response.Content.ReadFromJsonAsync<string>();
        token.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task ShouldGenerateTokenSuccessfullyForInvalidUser()
    {
        var username = "testUser";

        var response = await _client.GetAsync($"/auth/{username}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}