using Keyper.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Keyper.API.Data
{
    public class ApiKeyDbContext : DbContext
    {
        public ApiKeyDbContext(DbContextOptions<ApiKeyDbContext> options) : base(options)
        {
        }

        public DbSet<ApiKey> ApiKeys { get; set; }
    }
}