using AspNetCoreRateLimit;
using Ecommerce.API.Extensions;
using Ecommerce.API.Middlewares;
using Ecommerce.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ═══════════════════════════════════════════════════════════
//  STEP 1: Configure Serilog (structured logging to console + file)
// ═══════════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").Build())
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/ecommerce-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ═══════════════════════════════════════════════════════════
//  STEP 2: Register Services (Dependency Injection container)
// ═══════════════════════════════════════════════════════════
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddExternalApiClients(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddRateLimiting(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        // Keep camelCase JSON, include string representation of enums
        opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opt.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(opt =>
    opt.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

// ═══════════════════════════════════════════════════════════
//  STEP 3: Build app + configure middleware pipeline
// ═══════════════════════════════════════════════════════════
var app = builder.Build();

// Auto-apply EF migrations on startup (Development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    Log.Information("✅ Database migrations applied");
}

// ── Middleware pipeline — ORDER MATTERS ──────────────────
app.UseMiddleware<ExceptionMiddleware>();   // 1. Catch all errors → clean JSON
app.UseIpRateLimiting();                   // 2. Rate limit before processing
app.UseSerilogRequestLogging();            // 3. Log every request

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce Automation API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("🚀 Ecommerce Automation API started on {Env}", app.Environment.EnvironmentName);
await app.RunAsync();
