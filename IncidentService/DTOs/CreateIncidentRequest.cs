namespace IncidentService.DTOs;

public record CreateIncidentRequest(
    double Latitude,
    double Longitude,
    int TypeId,
    int? SubTypeId,
    string Description,
    string? ImageUrl = null
);