namespace AnalyticsService.Models
{
    public class Incident
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TypeId { get; set; }
        public int? SubTypeId { get; set; }
        public string Description { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int Status { get; set; } // 0=Pending, 1=Approved, 2=Rejected
        public DateTime CreatedAt { get; set; }
    }
}
