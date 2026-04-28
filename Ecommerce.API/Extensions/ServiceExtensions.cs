using System.Text;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Services;
using Ecommerce.Persistence.DbContext;
using Ecommerce.Persistence.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace Ecommerce.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("Ecommerce.Persistence").EnableRetryOnFailure(3)));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<ITenantAuthService, TenantAuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddValidatorsFromAssemblyContaining<CreateOrderDto>();
        return services;
    }

    public static IServiceCollection AddExternalApiClients(this IServiceCollection services)
    {
        services.AddHttpClient("WhatsApp", client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/v19.0");
        }).AddPolicyHandler(RetryPolicy());

        services.AddHttpClient("Messenger", client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/v19.0");
        }).AddPolicyHandler(RetryPolicy());

        services.AddHttpClient("Pathao")
            .AddPolicyHandler(RetryPolicy());

        services.AddHttpClient("Shopify")
            .AddPolicyHandler(RetryPolicy());

        services.AddScoped<IWhatsAppService, WhatsAppService>();
        services.AddScoped<IMessengerService, MessengerService>();
        services.AddScoped<IPathaoService, PathaoService>();
        services.AddScoped<IShopifyService, ShopifyService>();

        return services;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var key = Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]
            ?? "DefaultSecretKey-Change-This-In-Production-32chars!");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ecommerce Automation API — Multi-Tenant",
                Version = "v1",
                Description = "WhatsApp + Messenger + Pathao + Shopify automation. Each client registers and gets their own webhook URLs."
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter: Bearer {your-token}",
                Name = "Authorization", In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey, Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {{
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }});
        });
        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> RetryPolicy() =>
        HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(3, r => TimeSpan.FromSeconds(Math.Pow(2, r)));
}
