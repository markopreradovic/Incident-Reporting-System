using IncidentService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModerationService.Data;

namespace ModerationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModerationController : ControllerBase
{
    private readonly ModerationDbContext _db;

    public ModerationController(ModerationDbContext db)
    {
        _db = db;
    }

    // List of unapproven incidents
    [HttpGet("pending")]
    public async Task<ActionResult<List<Incident>>> GetPending()
    {
        var pending = await _db.Incidents
            .Where(i => i.Status == IncidentStatus.Pending)
            .ToListAsync();

        return Ok(pending);
    }

    // Approve incident
    [HttpPost("approve/{id}")]
    public async Task<IActionResult> Approve(int id)
    {
        var incident = await _db.Incidents.FindAsync(id);
        if (incident == null) return NotFound();

        incident.Status = IncidentStatus.Approved;
        await _db.SaveChangesAsync();

        return Ok();
    }

    // Reject incident
    [HttpPost("reject/{id}")]
    public async Task<IActionResult> Reject(int id)
    {
        var incident = await _db.Incidents.FindAsync(id);
        if (incident == null) return NotFound();

        incident.Status = IncidentStatus.Rejected;
        await _db.SaveChangesAsync();

        return Ok();
    }
}