using InventoryService.Data;
using InventoryService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("DefaultConnection")!
    )
);

// ======================================================
// INVENTORY SERVICES
// ======================================================

builder.Services.AddScoped<IPartRequestService, PartRequestService>();

builder.Services.AddScoped<ISparePartService, SparePartService>();

builder.Services.AddSingleton<IJobCardGateway, JobCardGateway>();

builder.Services.AddScoped<ILowStockEventPublisher, LowStockEventPublisher>();

builder.Services.AddSingleton<IPartIssuedEventPublisher, PartIssuedEventPublisher>();

// ======================================================
// KAFKA CONSUMER
// ======================================================

builder.Services.AddHostedService<PartRequestedConsumer>();

// ======================================================
// HTTP CLIENT - JOB MAINTENANCE SERVICE
// ======================================================

builder.Services.AddHttpClient("JobMaintenanceService", client =>
{
    var baseUrl = builder.Configuration["JobMaintenanceService:BaseUrl"]
        ?? throw new InvalidOperationException(
            "JobMaintenanceService:BaseUrl is missing."
        );

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
});

// ======================================================
// JWT AUTHENTICATION
// ======================================================

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key is missing. Configure it with user secrets or environment variables."
    );
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

// ======================================================
// CONTROLLERS
// ======================================================

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// ======================================================
// CORS FOR REACT & AZURE FRONTEND
// ======================================================

var allowedFrontendOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "http://localhost:5173",
    "http://144.24.106.68:8080",
    "https://zealous-sand-061bb6b00.6.azurestaticapps.net"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy => policy
        .SetIsOriginAllowed(origin => allowedFrontendOrigins.Contains(origin.TrimEnd('/')))
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// ======================================================
// AUTOMATIC MIGRATION / TABLE CREATION ON STARTUP
// SAFE MODE
// ======================================================

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<InventoryDbContext>();

        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Migration warning: {ex.Message}"
        );
    }
}

// ======================================================
// HTTP REQUEST PIPELINE
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = typeof(Program).Assembly.GetName().Name })).AllowAnonymous();

app.MapControllers();

app.Run();