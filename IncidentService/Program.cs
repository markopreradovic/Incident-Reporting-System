using IncidentService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=incidentdb;Username=postgres;Password=postgres"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
       
        db.Database.Migrate();
        app.Logger.LogInformation("Migracije uspješno primijenjene.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Aplikacija nastavlja bez baze.");
       
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();


//builder.Services.AddDbContext<AppDbContext>(options =>
//options.UseNpgsql("Host=172.17.0.3;Port=5432;Database=incidentdb;Username=postgres;Password=postgres;"));