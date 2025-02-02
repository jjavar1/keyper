using Keyper.API.Data;
using Keyper.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace Keyper.API.Controllers
{
    [ApiController]
    [Route("api/v1/keys")]
    [EnableRateLimiting("default")]
    public class ApiKeyController : ControllerBase
    {
        private readonly ApiKeyDbContext _dbContext;
        private readonly ILogger<ApiKeyController> _logger;
        public ApiKeyController(ApiKeyDbContext dbContext, ILogger<ApiKeyController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse("Invalid request data"));
            }
            string newKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var apiKey = new ApiKey
            {
                Name = request.Name,
                Description = request.Description,
                Key = newKey,
                CreatedAt = DateTime.UtcNow,
                UsageCount = 0,
                IsRevoked = false
            };
            _dbContext.ApiKeys.Add(apiKey);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("API key created with ID {ApiKeyId}", apiKey.Id);
            return Ok(apiKey);
        }
        [HttpGet("list")]
        public async Task<IActionResult> ListApiKeys()
        {
            var keys = await _dbContext.ApiKeys.Where(k => !k.IsRevoked).ToListAsync();
            return Ok(keys);
        }
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateApiKey([FromBody] ValidateApiKeyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse("Invalid request data"));
            }
            var apiKey = await _dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Key == request.Key && !k.IsRevoked);
            if (apiKey != null)
            {
                apiKey.LastUsedAt = DateTime.UtcNow;
                apiKey.UsageCount++;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("API key validated with ID {ApiKeyId}", apiKey.Id);
                return Ok(new { valid = true });
            }
            return BadRequest(new ErrorResponse("Invalid or revoked key"));
        }
        [HttpDelete("revoke/{key}")]
        public async Task<IActionResult> RevokeApiKey(string key)
        {
            var apiKey = await _dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Key == key);
            if (apiKey == null)
            {
                return NotFound(new ErrorResponse("API Key not found"));
            }
            apiKey.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("API key revoked with ID {ApiKeyId}", apiKey.Id);
            return Ok(new { message = "API Key revoked" });
        }
    }
    public record CreateApiKeyRequest
    {
        [Required]
        public string Name { get; init; }
        public string? Description { get; init; }
    }
    public record ValidateApiKeyRequest
    {
        [Required]
        public string Key { get; init; }
    }
    public record ErrorResponse(string Message);
}
