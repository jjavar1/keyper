using Keyper.API.Data;
using Keyper.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Get database configuration from environment variables or appsettings
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "postgresql";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=keyper.db";

builder.Services.AddDbContext<ApiKeyDbContext>(options =>
{
    switch (dbProvider.ToLower())
    {
        case "postgresql":
            options.UseNpgsql(connectionString);
            break;
        case "mysql":
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            break;
        case "mssql":
            options.UseSqlServer(connectionString);
            break;
        case "sqlite":
            options.UseSqlite(connectionString);
            break;
        default:
            throw new InvalidOperationException("Unsupported database provider");
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Generate API Key Endpoint
app.MapPost("/api/keys/create", async ([FromBody] string name, ApiKeyDbContext db) =>
{
    string newKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    var apiKey = new ApiKey { Name = name, Key = newKey };
    db.ApiKeys.Add(apiKey);
    await db.SaveChangesAsync();
    return Results.Ok(apiKey);
});

// List API Keys Endpoint
app.MapGet("/api/keys/list", async (ApiKeyDbContext db) =>
{
    var keys = await db.ApiKeys.Where(k => !k.IsRevoked).ToListAsync();
    return Results.Ok(keys);
});

// Validate API Key Endpoint
app.MapPost("/api/keys/validate", async ([FromBody] string key, ApiKeyDbContext db) =>
{
    var apiKey = await db.ApiKeys.FirstOrDefaultAsync(k => k.Key == key && !k.IsRevoked);
    if (apiKey != null)
    {
        apiKey.LastUsedAt = DateTime.UtcNow;
        apiKey.UsageCount++;
        await db.SaveChangesAsync();
        return Results.Ok(true);
    }
    return Results.BadRequest("Invalid or revoked key");
});

// Revoke API Key Endpoint
app.MapDelete("/api/keys/revoke/{key}", async (string key, ApiKeyDbContext db) =>
{
    var apiKey = await db.ApiKeys.FirstOrDefaultAsync(k => k.Key == key);
    if (apiKey == null) return Results.NotFound("API Key not found");
    apiKey.IsRevoked = true;
    await db.SaveChangesAsync();
    return Results.Ok("API Key revoked");
});

app.Run();
