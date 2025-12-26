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
    [HttpGet("approved")]
    public async Task<ActionResult<List<Incident>>> GetApproved(
        [FromQuery] string timeFilter = null,
        [FromQuery] int? typeId = null)
    {
        var query = _db.Incidents.Where(i => i.Status == IncidentStatus.Approved);

        if (typeId.HasValue)
            query = query.Where(i => i.TypeId == typeId.Value);

        if (timeFilter != null)
        {
            var now = DateTime.UtcNow;
            query = timeFilter switch
            {
                "24h" => query.Where(i => i.CreatedAt >= now.AddHours(-24)),
                "7d" => query.Where(i => i.CreatedAt >= now.AddDays(-7)),
                "31d" => query.Where(i => i.CreatedAt >= now.AddDays(-31)),
                _ => query
            };
        }

        var incidents = await query.Select(i => new IncidentDto
        {
            Id = i.Id,
            Latitude = i.Latitude,
            Longitude = i.Longitude,
            Description = i.Description ?? "Bez opisa",
            TypeId = i.TypeId,
            Status = i.Status
        }).ToListAsync();

        return Ok(incidents);
    }

    [HttpPost("upload-image")]
    public async Task<ActionResult<string>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine("wwwroot/images/incidents", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Ok($"/images/incidents/{fileName}");
    }

    public class IncidentDto
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; } = "";
        public int TypeId { get; set; }
        public IncidentStatus Status { get; set; }
    }

}