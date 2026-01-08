using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;

    public JwtAuthenticationStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public void NotifyUserChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

        var claims = new List<Claim>();

        foreach (var kvp in keyValuePairs)
        {
            var claimType = kvp.Key;
            var value = kvp.Value;

            // Skip standard JWT claims that are not needed for authorization
            if (claimType == "exp" || claimType == "iat" || claimType == "nbf" || claimType == "jti")
                continue;

            // Map standard JWT claim names to ClaimTypes
            if (claimType == "name")
                claimType = ClaimTypes.Name;
            else if (claimType == "nameid")
                claimType = ClaimTypes.NameIdentifier;
            else if (claimType == "role" || claimType == ClaimTypes.Role ||
                     claimType == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            {
                claimType = ClaimTypes.Role;
                // Handle role as array or single value
                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in value.EnumerateArray())
                    {
                        claims.Add(new Claim(ClaimTypes.Role, GetStringValue(role)));
                    }
                    continue;
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, GetStringValue(value)));
                    continue;
                }
            }
            else if (claimType == "http://schemas.microsoft.com/ws/2008/06/identity/claims/name")
                claimType = ClaimTypes.Name;
            else if (claimType == "http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier")
                claimType = ClaimTypes.NameIdentifier;

            // Handle other claim types
            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                {
                    claims.Add(new Claim(claimType, GetStringValue(item)));
                }
            }
            else
            {
                claims.Add(new Claim(claimType, GetStringValue(value)));
            }
        }

        return claims;
    }

    private static string GetStringValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetInt64().ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            _ => element.ToString()
        };
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}