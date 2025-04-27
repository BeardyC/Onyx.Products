using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Onyx.ProductManagement.Api.Constants;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;
using Onyx.ProductManagement.Api.Services;
using Onyx.ProductManagement.Api.Services.Interfaces;
using Onyx.ProductManagement.Api.Validators;
using Onyx.ProductManagement.Data.Context;

namespace Onyx.ProductManagement.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddServiceDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
        services.AddDbContext<ProductsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ProductsDb")));

        return services;
    }

    public static IServiceCollection AddHttpLogging(this IServiceCollection services)
    {
        services.AddHttpLogging(options =>
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

        return services;
    }

    public static IServiceCollection AddServiceHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("api_status", () => HealthCheckResult.Healthy("API is running."))
            .AddDbContextCheck<ProductsDbContext>("products_db", failureStatus: HealthStatus.Unhealthy, null,
                (ctx, token) => ctx.Database.CanConnectAsync(token));

        return services;
    }

    public static IServiceCollection AddAuthenticationAndAuthorisation(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration)
            .AddAuthorisation();

        return services;
    }

    private static IServiceCollection AddAuthorisation(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddAuthorizationBuilder()
            .AddPolicy(AppRoles.ProductWriteAccess, policy =>
                policy.RequireRole(RoleGroups.WriteRoles))
            .AddPolicy(AppRoles.ProductReadAccess, policy =>
                policy.RequireRole(RoleGroups.ReadRoles))
            .AddPolicy(AppRoles.ProductAdminAccess, policy =>
                policy.RequireRole(AppRoles.ProductAdminAccess)); ;
        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var configurationSection = configuration.GetSection(JwtSettings.SectionName);
        services.AddOptions<JwtSettings>().Bind(configurationSection);

        var jwtSettings = configurationSection.Get<JwtSettings>();
        
        if (jwtSettings is null)
        {
            throw new InvalidOperationException("JWT settings are missing or invalid in configuration.");
        }
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Products API",
            });

            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorisation header using the Bearer scheme. Enter your token below.",
                Name = "Authorisation",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}