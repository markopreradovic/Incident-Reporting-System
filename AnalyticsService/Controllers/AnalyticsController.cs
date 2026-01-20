using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsDbContext _db;

    public AnalyticsController(AnalyticsDbContext db)
    {
        _db = db;
    }

    private static string GetTypeName(int typeId) => typeId switch
    {
        1 => "Saobraćajni problem",
        2 => "Komunalni problem",
        3 => "Javni red i mir",
        _ => "Nepoznato"
    };

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var total = await _db.Incidents.CountAsync();
            var pending = await _db.Incidents.CountAsync(i => i.Status == 0);
            var approved = await _db.Incidents.CountAsync(i => i.Status == 1);
            var rejected = await _db.Incidents.CountAsync(i => i.Status == 2);

            return Ok(new
            {
                Total = total,
                Pending = pending,
                Approved = approved,
                Rejected = rejected
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("by-type")]
    public async Task<IActionResult> GetByType()
    {
        try
        {
            var rawData = await _db.Incidents
                .GroupBy(i => i.TypeId)
                .Select(g => new
                {
                    TypeId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var result = rawData.Select(x => new
            {
                x.TypeId,
                x.Count,
                TypeName = GetTypeName(x.TypeId)
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("by-date")]
    public async Task<IActionResult> GetByDate([FromQuery] int days = 30)
    {
        try
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            var rawData = await _db.Incidents
                .Where(i => i.CreatedAt >= startDate)
                .GroupBy(i => i.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var data = rawData.Select(x => new
            {
                Date = x.Date.ToString("yyyy-MM-dd"),
                Count = x.Count
            }).ToList();

            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }
}