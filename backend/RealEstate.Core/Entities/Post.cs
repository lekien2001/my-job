using System;

namespace RealEstate.Core.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public string Address { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public int CategoryId { get; set; }
        public string Type { get; set; } = "Rent"; // Rent, Sale
        public int? UserId { get; set; }
        public string Source { get; set; } = "Web"; // Web, Facebook
        public string? ExternalId { get; set; }
        public string? ExternalUrl { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string Status { get; set; } = "Active"; // Pending, Active, Rejected, Hidden
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
