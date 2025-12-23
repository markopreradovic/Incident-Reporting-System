using System.Collections.Generic;

namespace IncidentService.Models;

public class IncidentType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<IncidentSubType> SubTypes { get; set; } = new();
}

public class IncidentSubType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TypeId { get; set; }
}