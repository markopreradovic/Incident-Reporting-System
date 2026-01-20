using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Steeltoe.Configuration.ConfigServer;
using System.Text;
using UserService.Data;
using UserService.Models;

var builder = WebApplication.CreateBuilder(args);

// Config Server
builder.Configuration.AddConfigServer();

// DB Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=postgres;Port=5432;Database=incidentdb;Username=postgres;Password=postgres"));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication - KLJUČNA IZMENA!
builder.Services.AddAuthentication(options =>
{
    // Ovo je KRITIČNO - postavlja JWT kao default scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Za development
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "IncidentSystem",
        ValidAudience = "IncidentSystem",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("tvoj-super-tajni-kljuc-od-barem-32-karaktera!!"))
    };

    // Opciono: dodaj event handlers za debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"🔴 JWT Auth failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"✅ JWT Token validated for: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"📨 Received JWT token");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(policy =>
    policy.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Moderator", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"✅ Created role: {role}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Migration/Seed error: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();

// VAŽNO: UseAuthentication MORA biti PRIJE UseAuthorization!
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "user-service",
    timestamp = DateTime.UtcNow
}));

app.MapControllers();

var consulClient = new ConsulClient(c => c.Address = new Uri("http://consul:8500"));
var serviceName = "user-service";
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