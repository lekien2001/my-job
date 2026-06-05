using System;

namespace RealEstate.Core.Entities
{
    public class PostImage
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
