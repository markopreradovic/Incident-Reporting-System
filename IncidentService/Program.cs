using Consul;
using IncidentService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Steeltoe.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);

// DB Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=postgres;Port=5432;Database=incidentdb;Username=postgres;Password=postgres"));

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddConfigServer();

var app = builder.Build();

// Apply migrations
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

// Swagger dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Static files
// 1. Kreiraj direktorij ako ne postoji
var imagesPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "images", "incidents");
Directory.CreateDirectory(imagesPath);

// 2. Omogući static files iz wwwroot (default putanja)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images/incidents"  // ← OVO je ključno – mapira /images/incidents na taj folder
});

// 3. Opcionalno: omogući directory browsing (za debug)
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images/incidents"
});
app.UseAuthorization();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "incident-service" }));

app.MapControllers();

// Consul registration
var consulClient = new ConsulClient(c => c.Address = new Uri("http://consul:8500"));
var serviceName = "incident-service";
var servicePort = 80;

var registration = new AgentServiceRegistration
{
    ID = $"{serviceName}-{Guid.NewGuid()}",
    Name = serviceName,
    Address = serviceName,
    Port = servicePort,
    Check = new AgentServiceCheck
    {
        HTTP = $"http://{serviceName}:{servicePort}/health",
        Interval = TimeSpan.FromSeconds(10),
        Timeout = TimeSpan.FromSeconds(5),
        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
    }
};

await consulClient.Agent.ServiceRegister(registration);
Console.WriteLine($"✅ Registered service: {serviceName} at {serviceName}:{servicePort} with ID: {registration.ID}");

app.Lifetime.ApplicationStopping.Register(async () =>
{
    try
    {
        await consulClient.Agent.ServiceDeregister(registration.ID);
        Console.WriteLine($"✅ Deregistered service on shutdown: {registration.ID}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Failed to deregister: {ex.Message}");
    }
});

app.Run();