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

builder.Services.AddScoped<ISparePartService, SparePartService>();
builder.Services.AddScoped<IPartRequestService, PartRequestService>();
builder.Services.AddSingleton<ILowStockEventPublisher, LowStockEventPublisher>();
builder.Services.AddScoped<IJobCardGateway, JobCardGateway>();
builder.Services.AddSingleton<IPartIssuedEventPublisher, PartIssuedEventPublisher>();
builder.Services.AddHostedService<PartRequestedConsumer>();
builder.Services.AddHttpClient("JobMaintenanceService", client =>
{
    var baseUrl = builder.Configuration["JobMaintenanceService:BaseUrl"]
        ?? throw new InvalidOperationException("JobMaintenanceService:BaseUrl is missing.");
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key is missing. Configure it with user secrets or environment variables.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ======================================================
// CORS FOR REACT & AZURE FRONTEND
// ======================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173", 
                "http://144.24.106.68:8080",
                "https://zealous-sand-061bb6b00.6.azurestaticapps.net"
              )
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ======================================================
// AUTOMATIC MIGRATION / TABLE CREATION ON STARTUP
// ======================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();