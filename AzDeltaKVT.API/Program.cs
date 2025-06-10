using AzDeltaKVT.API.Controllers;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AzDeltaKVTDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<GeneService>();
builder.Services.AddScoped<TranscriptService>();
builder.Services.AddScoped<VariantService>();
builder.Services.AddScoped<GeneVariantService>();
builder.Services.AddScoped<UploadService>();

var app = builder.Build();


//Set seed to true to seed the database with initial data
//Don't forget to set it to false after the first run to duplicates
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AzDeltaKVTDbContext>();

try
{
    context.Database.Migrate();

    if (!context.Genes.Any())
    {
        context.Seed();
    }
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error during migration or seeding");
    throw;  // optionally rethrow to crash app so you see error in logs
}

app.UseSwagger();
    app.UseSwaggerUI();


app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
