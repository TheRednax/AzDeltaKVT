using AzDeltaKVT.API.Controllers;
using AzDeltaKVT.Core;
using AzDeltaKVT.Dto.Requests;
using AzDeltaKVT.Services;
using Microsoft.EntityFrameworkCore;
using System;

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

// ? DbContext configureren met MariaDB (Pomelo)
builder.Services.AddDbContext<AzDeltaKVTDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(10, 11, 2))
    )
);


// ? Dependency injection
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<GeneService>();
builder.Services.AddScoped<TranscriptService>();
builder.Services.AddScoped<VariantService>();
builder.Services.AddScoped<GeneVariantService>();
builder.Services.AddScoped<UploadService>();

var app = builder.Build();

// ? Automatische migraties en seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AzDeltaKVTDbContext>();

    context.Database.Migrate();

    if (!context.Genes.Any())
    {
        context.Seed();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
