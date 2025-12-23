using IncidentService.Models;
using Microsoft.EntityFrameworkCore;

namespace IncidentService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentType> IncidentTypes => Set<IncidentType>();
    public DbSet<IncidentSubType> IncidentSubTypes => Set<IncidentSubType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IncidentType>().HasData(
            new IncidentType { Id = 1, Name = "Saobraćajni problem" },
            new IncidentType { Id = 2, Name = "Komunalni problem" },
            new IncidentType { Id = 3, Name = "Javni red i mir" }
        );

        modelBuilder.Entity<IncidentSubType>().HasData(
            new IncidentSubType { Id = 1, Name = "Rupa na kolovozu", TypeId = 1 },
            new IncidentSubType { Id = 2, Name = "Neispravna semaforska signalizacija", TypeId = 1 },
            new IncidentSubType { Id = 3, Name = "Divlje deponije", TypeId = 2 },
            new IncidentSubType { Id = 4, Name = "Neispravno javno osvjetljenje", TypeId = 2 }
        );
    }
}