namespace RealEstate.Core.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string Type { get; set; } = string.Empty; // Province, District, Ward
    }
}
