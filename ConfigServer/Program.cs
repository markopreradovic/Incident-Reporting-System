using Steeltoe.Configuration.ConfigServer;
using Steeltoe.Management.Endpoint.Actuators;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Actuators.All; // Ensure this namespace is included for actuator mapping extensions

var builder = WebApplication.CreateBuilder(args);

// Config Server
builder.Configuration.AddConfigServer();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Steeltoe 4.0.0 Actuators
builder.Services.AddAllActuators();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Bitno: Redosled je važan u .NET 8
app.UseRouting();

app.MapGet("/health", () => Results.Ok("Healthy"));

// U Steeltoe 4.0, MapAllActuators se mapira ovde
app.MapActuators(); // Corrected method name for mapping actuators

app.MapControllers();



app.Run();
