using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor(); // DODAJ OVO

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policyBuilder =>
    {
        policyBuilder
            .WithOrigins("https://localhost:7274")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
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
            // Map role claims correctly
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Registruj handler
builder.Services.AddTransient<AuthTokenForwardingHandler>();

// Dodaj Ocelot sa custom handler-om
builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<AuthTokenForwardingHandler>(true); // global = true

var app = builder.Build();

app.UseCors("AllowBlazor");
app.UseAuthentication();
await app.UseOcelot();

app.Run();