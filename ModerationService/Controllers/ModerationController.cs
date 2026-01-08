using IncidentService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModerationService.Data;

namespace ModerationService.Controllers;

[ApiController]
[Route("api/moderation")]
[Authorize(Roles = "Moderator")]
public class ModerationController : ControllerBase
{
    private readonly AppDbContext _context;

    public ModerationController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<PendingIncidentDto>>> GetPending()
    {
        var pending = await _context.Incidents
            .Where(i => i.Status == IncidentStatus.Pending)
            .Select(i => new PendingIncidentDto
            {
                Id = i.Id,
                Latitude = i.Latitude,
                Longitude = i.Longitude,
                TypeId = i.TypeId,
                SubTypeId = i.SubTypeId,
                Description = i.Description,
                CreatedAt = i.CreatedAt
            })
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(pending);
    }

    [HttpPost("approve/{id}")]
    public async Task<IActionResult> Approve(int id)
    {
        var incident = await _context.Incidents.FindAsync(id);
        if (incident == null) return NotFound();

        if (incident.Status != IncidentStatus.Pending)
            return BadRequest("Incident nije u Pending statusu.");

        incident.Status = IncidentStatus.Approved;
        await _context.SaveChangesAsync();

        return Ok("Incident odobren.");
    }

    [HttpPost("reject/{id}")]
    public async Task<IActionResult> Reject(int id)
    {
        var incident = await _context.Incidents.FindAsync(id);
        if (incident == null) return NotFound();

        if (incident.Status != IncidentStatus.Pending)
            return BadRequest("Incident nije u Pending statusu.");

        incident.Status = IncidentStatus.Rejected;
        await _context.SaveChangesAsync();

        return Ok("Incident odbijen.");
    }
}

public class PendingIncidentDto
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TypeId { get; set; }
    public int? SubTypeId { get; set; }
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}