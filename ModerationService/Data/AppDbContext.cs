using IncidentService.Models;
using Microsoft.EntityFrameworkCore;

namespace ModerationService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Incident> Incidents => Set<Incident>();
}