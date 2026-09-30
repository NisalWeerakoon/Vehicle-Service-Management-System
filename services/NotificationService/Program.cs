using NotificationService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("DefaultConnection")!
    )
);

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.UseCors("AllowReactFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();