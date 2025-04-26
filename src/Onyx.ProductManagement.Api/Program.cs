using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Onyx.ProductManagement.Api;
using Onyx.ProductManagement.Api.Constants;
using Onyx.ProductManagement.Api.Endpoints.Auth;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using Onyx.ProductManagement.Api.Services;
using Onyx.ProductManagement.Api.Services.Interfaces;
using Onyx.ProductManagement.Data.Context;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsDb")));

builder.Services.AddJwtAuthentication(configuration);

builder.Services.AddHttpLogging(options =>
    {
        options.CombineLogs = true;
        options.LoggingFields = HttpLoggingFields.Duration
                                | HttpLoggingFields.RequestPath
                                | HttpLoggingFields.RequestMethod
                                | HttpLoggingFields.RequestProtocol
                                | HttpLoggingFields.RequestScheme
                                | HttpLoggingFields.ResponseStatusCode
                                | HttpLoggingFields.RequestQuery;
    });

builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppRoles.ProductWriteAccess, policy =>
        policy.RequireRole(AppRoles.ProductWriteAccess));

    options.AddPolicy(AppRoles.ProductReadAccess, policy =>
        policy.RequireRole(AppRoles.ProductReadAccess, AppRoles.ProductWriteAccess));
});


builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseHttpLogging();
app.UseSwagger();
app.UseSwaggerUI();
// app.MapHealthChecks("/health") :: TODO: HealthCheck
//     .AllowAnonymous();
app.MapGroup("/auth")
    .MapAuthEndpoints()
    .AllowAnonymous();
app.MapGroup("/products")
    .MapProductEndpoints();

app.Run();
