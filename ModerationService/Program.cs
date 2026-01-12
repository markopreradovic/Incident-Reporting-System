using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ModerationService.Data;
using Steeltoe.Configuration.ConfigServer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=postgres;Port=5432;Database=incidentdb;Username=postgres;Password=postgres"));

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "IncidentSystem",
            ValidAudience = "IncidentSystem",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("tvoj-super-tajni-kljuc-od-barem-32-karaktera!!")),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    context.Token = authHeader.Substring(7);
                return Task.CompletedTask;
            }
        };
    });

builder.Configuration.AddConfigServer();

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Static files
var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "incidents");
Directory.CreateDirectory(imagesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images/incidents"
});

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "moderation-service" }));

app.MapControllers();

// Consul registration
var consulClient = new ConsulClient(c => c.Address = new Uri("http://consul:8500"));
var serviceName = "moderation-service";
var servicePort = 8080;

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