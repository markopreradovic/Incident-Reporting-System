using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ModerationService.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=postgres;Port=5432;Database=incidentdb;Username=postgres;Password=postgres"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("tvoj-super-tajni-kljuc-od-barem-32-karaktera!!")),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
            Console.WriteLine($"   Exception type: {context.Exception.GetType().Name}");
            if (context.Exception.InnerException != null)
            {
                Console.WriteLine($"   Inner exception: {context.Exception.InnerException.Message}");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"✅ Token validated for user: {context.Principal?.Identity?.Name}");
            var roles = context.Principal?.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
            Console.WriteLine($"   Roles: {string.Join(", ", roles ?? new List<string>())}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            Console.WriteLine($"📩 Received Authorization header: {(string.IsNullOrEmpty(authHeader) ? "EMPTY" : "Present")}");

            if (!string.IsNullOrEmpty(authHeader))
            {
                Console.WriteLine($"   Full header value: {authHeader}");
                Console.WriteLine($"   Header length: {authHeader.Length}");

                // Provjeri da li header počinje sa "Bearer "
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring(7); // Ukloni "Bearer "
                    Console.WriteLine($"   Token (first 50 chars): {token.Substring(0, Math.Min(50, token.Length))}");
                    Console.WriteLine($"   Token has dots: {token.Count(c => c == '.')}");

                    // Ručno postavi token
                    context.Token = token;
                }
                else
                {
                    Console.WriteLine($"   ⚠️ Header doesn't start with 'Bearer '");
                }
            }

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"⚠️ OnChallenge: {context.Error}, {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "incidents");
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images/incidents"
});

app.MapControllers();

app.Run();