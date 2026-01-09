using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor(); // ✅ DODAJ OVO

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(
                "https://localhost:7274",    // Razvojni HTTPS
                "http://localhost:7274",     // Razvojni HTTP
                "http://localhost:5000",     // API Gateway
                "http://localhost:5048",     // Blazor port (proverite koji je)
                "http://localhost:6001",     // Moderation service
                "http://localhost:7000"      // User service
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    // ✅ DODAJTE OVU POLICY ZA SVE ORIGINE (za development)
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
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

        // ✅ DODAJTE OVO za WebSocket CORS
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Proverite da li je WebSocket request
                if (context.Request.Query.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Registruj handler
builder.Services.AddTransient<AuthTokenForwardingHandler>();

// Dodaj Ocelot sa custom handler-om
builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<AuthTokenForwardingHandler>(true); // global = true

var app = builder.Build();

// ✅ PRAVILAN REDOSLED MIDDLEWARE-A
app.UseCors("AllowAll"); // Ili "AllowBlazor"

app.UseAuthentication();
app.UseAuthorization(); // ✅ DODAJTE OVO!

await app.UseOcelot();

app.Run();