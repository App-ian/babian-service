using Babian.Infrastructure.Persistence;
using Babian.Service.Hubs;
using Babian.Service.Services;
using Babian.Infrastructure.Persistence.Repositories;
using Babian.Domain.Interfaces;
using Babian.BusinessLayers.MarketEngine.Services;
using Babian.BusinessLayers.MarketSessions.Features.StartSession;
using Babian.BusinessLayers.Orders.Features.AddOrder;
using Babian.BusinessLayers.Drinks.Features.ActivateDrink;
using Babian.BusinessLayers.MarketConfigs.Features.UpdateConfig;
using Babian.BusinessLayers.Profiles.Features.Login;
using Babian.Domain.Interfaces;
using Babian.BusinessLayers.Profiles.Services;
using Babian.BusinessLayers.GlobalDrinks.Features.GetGlobalDrinks;
using Babian.BusinessLayers.MarketEngine.Features.UpdatePrices;
using Babian.BusinessLayers.MarketEvents.Features.GetEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Babian.Service.Middlewares;
using FluentValidation;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// CORS — origines lues depuis Cors:AllowedOrigins (appsettings) ou env ALLOWED_ORIGINS (séparateur ';'), fallback http://localhost:3000
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(';', StringSplitOptions.RemoveEmptyEntries)
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.SnakeCaseLower));
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure Swagger/OpenAPI (US 0.1)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Babian API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// Auth Services
builder.Services.AddScoped<IJwtService, JwtService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "vibe_code_babian_secret_key_extremely_long_and_secure_2026";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddSignalR();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
    
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/market"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});



// Infrastructure - Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(10, 11, 0)), // MariaDB 10.11 (Docker)
        b => b.MigrationsAssembly("Babian.Infrastructure.Persistence")
    );
});

// Repositories
builder.Services.AddScoped<IMarketConfigRepository, MarketConfigRepository>();
builder.Services.AddScoped<IDrinkRepository, DrinkRepository>();
builder.Services.AddScoped<IGlobalDrinkRepository, GlobalDrinkRepository>();
builder.Services.AddScoped<IMarketSessionRepository, MarketSessionRepository>();
builder.Services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMarketEventRepository, MarketEventRepository>();

// Infrastructure Services
builder.Services.AddSingleton<IPriceCalculator, AsymptoticPriceCalculator>();
builder.Services.AddScoped<IMarketSimulationService, MarketSimulationService>();
builder.Services.AddScoped<IPriceRankingService, PriceRankingService>();
builder.Services.AddScoped<IPriceCalculationService, PriceCalculationService>();
builder.Services.AddScoped<IMarketEventApplier, MarketEventApplier>();
builder.Services.AddScoped<IMarketNotificationService, MarketNotificationService>();

// Configure MediatR across all business layer assemblies
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        typeof(StartMarketSessionCommand).Assembly,
        typeof(AddOrderCommand).Assembly,
        typeof(ActivateDrinkFromGlobalCommand).Assembly,
        typeof(UpdateMarketConfigCommand).Assembly,
        typeof(GetGlobalDrinksQuery).Assembly,
        typeof(UpdateMarketPricesCommand).Assembly,
        typeof(LoginUserQuery).Assembly,
        typeof(GetMarketEventsQuery).Assembly
    );
    cfg.AddOpenBehavior(typeof(Babian.Common.Behaviors.ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblies(new[] {
    typeof(Program).Assembly,
    typeof(StartMarketSessionCommand).Assembly,
    typeof(AddOrderCommand).Assembly,
    typeof(ActivateDrinkFromGlobalCommand).Assembly,
    typeof(UpdateMarketConfigCommand).Assembly,
    typeof(GetGlobalDrinksQuery).Assembly,
    typeof(UpdateMarketPricesCommand).Assembly,
    typeof(LoginUserQuery).Assembly,
    typeof(GetMarketEventsQuery).Assembly
});

// Background Services
builder.Services.AddHostedService<Babian.Service.Workers.MarketWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("FrontendPolicy");

// Correlation ID Middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() 
                       ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MarketHub>("/hubs/market");

app.Run();

public partial class Program { }
