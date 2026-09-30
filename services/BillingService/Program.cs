using BillingService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("DefaultConnection")!
    )
);
builder.Services.AddHostedService<BillingService.Services.PartIssuedConsumer>();
builder.Services.AddHostedService<BillingService.Services.ServiceCompletedConsumer>();
builder.Services.AddScoped<BillingService.Services.IInvoiceService, BillingService.Services.InvoiceService>();
builder.Services.AddScoped<BillingService.Services.IInvoiceEventPublisher, BillingService.Services.InvoiceEventPublisher>();
builder.Services.AddScoped<BillingService.Services.IPaymentService, BillingService.Services.PaymentService>();
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ClockSkew = TimeSpan.Zero });
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.MigrateAsync();

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
