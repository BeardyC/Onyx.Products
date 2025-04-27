using Onyx.ProductManagement.Api.Endpoints;
using Onyx.ProductManagement.Api.Endpoints.Auth;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using Onyx.ProductManagement.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services.AddServiceDependencies(configuration)
    .AddAuthenticationAndAuthorisation(configuration)
    .AddHttpLogging()
    .AddServiceHealthChecks()
    .AddEndpointsApiExplorer()
    .AddSwaggerDocumentation();

// Add Otel for observability
// Add Serilog for structured logging

var app = builder.Build();
app.UseHttpLogging();
app.UseSwagger()
    .UseSwaggerUI();
app.UseSwaggerUI();

app.MapGroup("/health")
    .MapHealthCheckEndpoints()
    .AllowAnonymous();
app.MapGroup("/auth")
    .MapAuthEndpoints()
    .AllowAnonymous();

var v1 = app.MapGroup("/v1");
v1.MapGroup("/products")
    .MapProductEndpoints();

app.Run();
