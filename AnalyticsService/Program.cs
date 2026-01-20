using Consul;
using Microsoft.EntityFrameworkCore;
using Steeltoe.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfigServer();

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok("Healthy"));

var consulClient = new ConsulClient(c => c.Address = new Uri("http://consul:8500"));
var serviceName = "analytics-service";
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
Console.WriteLine($"✅ Registered service: {serviceName} at {serviceName}:{servicePort}");

app.Lifetime.ApplicationStopping.Register(async () =>
{
    await consulClient.Agent.ServiceDeregister(registration.ID);
    Console.WriteLine($"✅ Deregistered: {registration.ID}");
});

await app.RunAsync();  