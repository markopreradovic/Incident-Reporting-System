using IncidentService.Data;
using IncidentService.DTOs;
using IncidentService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IncidentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public IncidentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Incident>>> GetAll()
    {
        return await _db.Incidents.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Incident>> Create(CreateIncidentRequest request)
    {
        var incident = new Incident
        {
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            TypeId = request.TypeId,
            SubTypeId = request.SubTypeId,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Status = IncidentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Incident>> GetById(int id)
    {
        var incident = await _db.Incidents.FindAsync(id);
        if (incident == null) return NotFound();
        return incident;
    }
}