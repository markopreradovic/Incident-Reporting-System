// Data/AnalyticsDbContext.cs
using AnalyticsService.Models;
using Microsoft.EntityFrameworkCore;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Incident> Incidents { get; set; } = null!;
}