using AspNetCoreRateLimit;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Services;
using Ecommerce.Infrastructure.Settings;
using Ecommerce.Persistence.DbContext;
using Ecommerce.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace Ecommerce.API.Extensions;

/// <summary>
/// All DI registrations moved here to keep Program.cs clean.
/// Each method handles one concern.
/// </summary>
public static class ServiceExtensions
{
    // ── Database ──────────────────────────────────────────────
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("Ecommerce.Persistence")
                          .EnableRetryOnFailure(3)));    // Auto-retry on transient DB errors
        return services;
    }

    // ── Business logic + Repositories ─────────────────────────
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories (Persistence layer)
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();

        // Application services (business logic)
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ILeadService, LeadService>();

        // FluentValidation: scan Application assembly for all validators
        services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();

        return services;
    }

    // ── Named HTTP Clients (WhatsApp, Messenger, Pathao) ──────
    public static IServiceCollection AddExternalApiClients(this IServiceCollection services, IConfiguration config)
    {
        // Bind settings sections to strongly-typed classes
        services.Configure<WhatsAppSettings>(config.GetSection("WhatsApp"));
        services.Configure<MessengerSettings>(config.GetSection("Messenger"));
        services.Configure<PathaoSettings>(config.GetSection("Pathao"));

        // WhatsApp HTTP client with retry policy
        services.AddHttpClient("WhatsApp", (sp, client) =>
        {
            var settings = config.GetSection("WhatsApp").Get<WhatsAppSettings>()!;
            client.BaseAddress = new Uri(settings.ApiBaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.AccessToken}");
        }).AddPolicyHandler(RetryPolicy());

        // Messenger HTTP client
        services.AddHttpClient("Messenger", (sp, client) =>
        {
            var settings = config.GetSection("Messenger").Get<MessengerSettings>()!;
            client.BaseAddress = new Uri(settings.ApiBaseUrl);
        }).AddPolicyHandler(RetryPolicy());

        // Pathao HTTP client
        services.AddHttpClient("Pathao", (sp, client) =>
        {
            var settings = config.GetSection("Pathao").Get<PathaoSettings>()!;
            client.BaseAddress = new Uri(settings.ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddPolicyHandler(RetryPolicy());

        // Register infrastructure services
        services.AddScoped<IWhatsAppService, WhatsAppService>();
        services.AddScoped<IMessengerService, MessengerService>();
        services.AddScoped<IPathaoService, PathaoService>();

        return services;
    }

    // ── Polly Retry Policy ────────────────────────────────────
    // Retries up to 3 times on 5xx or network errors, with exponential backoff
    private static IAsyncPolicy<HttpResponseMessage> RetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    // ── Swagger ───────────────────────────────────────────────
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ecommerce Automation API",
                Version = "v1",
                Description = "WhatsApp + Messenger + Pathao courier automation for ecommerce orders."
            });
        });
        return services;
    }

    // ── Rate Limiting ─────────────────────────────────────────
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration config)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        return services;
    }
}
