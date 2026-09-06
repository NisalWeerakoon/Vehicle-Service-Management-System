using System.Text;
using JobMaintenanceService.Data;
using JobMaintenanceService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");

builder.Services.AddDbContext<JobMaintenanceDbContext>(options =>
    options.UseMySQL(connectionString));

builder.Services.AddScoped<IJobCardService, JobCardService>();
builder.Services.AddScoped<IMechanicAssignmentService, MechanicAssignmentService>();
builder.Services.AddHttpClient("CustomerBookingService");
builder.Services.AddHostedService<VehicleCheckedInConsumer>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key is missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // .NET 10 JwtBearer defaults to JsonWebTokenHandler which has different
        // key resolution. UseSecurityTokenValidators = true restores the legacy
        // JwtSecurityTokenHandler that correctly reads IssuerSigningKey.
        options.UseSecurityTokenValidators = true;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// app.UseHttpsRedirection(); // Disabled for HTTP development — HTTPS redirect strips the Authorization header
app.UseCors("AllowReactFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
