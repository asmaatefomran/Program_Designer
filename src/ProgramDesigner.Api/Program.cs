using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Api.Data;
using ProgramDesigner.Api.Services;
using ProgramDesigner.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<IProgramBuilderService, ProgramBuilderService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IProgramLoaderService, ProgramLoaderService>();
builder.Services.AddScoped<IProgramService, ProgramService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();