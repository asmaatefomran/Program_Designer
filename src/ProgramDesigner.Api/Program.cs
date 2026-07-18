using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Api.Data;
using ProgramDesigner.Api.Services;
using ProgramDesigner.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<IProgramBuilderService, ProgramBuilderService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IProgramLoaderService, ProgramLoaderService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddScoped<IProgramService, ProgramService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for the frontend (Vite dev server + containerized build). Configurable
// via appsettings so the allowed origin(s) can differ between local dev and
// docker-compose without a code change.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply pending migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    const int maxRetries = 10;

    for (int retry = 1; retry <= maxRetries; retry++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch
        {
            if (retry == maxRetries)
                throw;

            Thread.Sleep(3000);
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();