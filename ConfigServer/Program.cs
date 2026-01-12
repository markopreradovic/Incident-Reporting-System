using Steeltoe.Configuration.ConfigServer;
using Steeltoe.Management.Endpoint.Actuators;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Actuators.All; 

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfigServer();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAllActuators();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapActuators(); 

app.MapControllers();



app.Run();
