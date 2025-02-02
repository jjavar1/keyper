namespace Keyper.API.Models
{
    public class ApiKey
    {
        public int Id { get; set; }
        public required string Key { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public int UsageCount { get; set; }
        public bool IsRevoked { get; internal set; }
    }
}