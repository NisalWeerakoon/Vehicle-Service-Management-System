using BillingService.Data;
using BillingService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// DATABASE
// ======================================================

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("DefaultConnection")!
    )
);

// ======================================================
// BACKGROUND CONSUMERS
// ======================================================

builder.Services.AddHostedService<PartIssuedConsumer>();

builder.Services.AddHostedService<ServiceCompletedConsumer>();

// ======================================================
// BILLING SERVICES
// ======================================================

builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddScoped<
    IInvoiceEventPublisher,
    InvoiceEventPublisher>();

builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

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
// CORS
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://144.24.106.68:8080",
                "https://zealous-sand-061bb6b00.6.azurestaticapps.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ======================================================
// DATABASE MIGRATION
// SAFE MODE
// ======================================================

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<BillingDbContext>();

        dbContext.Database.Migrate();

        Console.WriteLine(
            "Billing database migrations applied successfully."
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Billing database migration warning: {ex.Message}"
        );

        // Do not crash the whole application
        // if migration fails.
    }
}

// ======================================================
// HTTP PIPELINE
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