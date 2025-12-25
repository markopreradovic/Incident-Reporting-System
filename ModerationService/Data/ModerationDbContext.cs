using IncidentService.Models;
using Microsoft.EntityFrameworkCore;

namespace ModerationService.Data;

public class ModerationDbContext : DbContext
{
    public ModerationDbContext(DbContextOptions<ModerationDbContext> options) : base(options) { }

    public DbSet<Incident> Incidents => Set<Incident>();
}