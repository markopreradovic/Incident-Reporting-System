using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor(); 

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(
                "https://localhost:7274",    // Razvojni HTTPS
                "http://localhost:7274",     // Razvojni HTTP
                "http://localhost:5000",     // API Gateway
                "http://localhost:5048",     // Blazor port 
                "http://localhost:6001",     // Moderation service
                "http://localhost:7000"      // User service
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

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
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Query.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddTransient<AuthTokenForwardingHandler>();

builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<AuthTokenForwardingHandler>(true);

var app = builder.Build();

app.UseCors("AllowAll"); 

app.UseAuthentication();
app.UseAuthorization(); 

await app.UseOcelot();

app.Run();