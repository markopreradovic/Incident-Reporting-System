namespace IncidentService.Models;

public class Incident
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TypeId { get; set; }
    public int? SubTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public IncidentStatus Status { get; set; } = IncidentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum IncidentStatus
{
    Pending,
    Approved,
    Rejected
}