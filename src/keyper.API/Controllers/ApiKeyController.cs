using Keyper.API.Data;
using Keyper.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Keyper.API.Controllers;

[ApiController]
[Route("api/keys")]
public class ApiKeyController : ControllerBase
{
    private readonly ApiKeyDbContext _dbContext;

    public ApiKeyController(ApiKeyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateApiKey([FromBody] string name)
    {
        string newKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var apiKey = new ApiKey { Name = name, Key = newKey };
        _dbContext.ApiKeys.Add(apiKey);
        await _dbContext.SaveChangesAsync();
        return Ok(apiKey);
    }

    [HttpGet("list")]
    public async Task<IActionResult> ListApiKeys()
    {
        var keys = await _dbContext.ApiKeys.Where(k => !k.IsRevoked).ToListAsync();
        return Ok(keys);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateApiKey([FromBody] string key)
    {
        var apiKey = await _dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Key == key && !k.IsRevoked);
        return apiKey != null ? Ok(true) : BadRequest("Invalid or revoked key");
    }

    [HttpDelete("revoke/{key}")]
    public async Task<IActionResult> RevokeApiKey(string key)
    {
        var apiKey = await _dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Key == key);
        if (apiKey == null) return NotFound("API Key not found");
        apiKey.IsRevoked = true;
        await _dbContext.SaveChangesAsync();
        return Ok("API Key revoked");
    }
}
