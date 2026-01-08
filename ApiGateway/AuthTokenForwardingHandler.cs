public class AuthTokenForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthTokenForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext != null)
        {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader))
            {
                // Dodaj header tačno onako kako je stigao
                request.Headers.Remove("Authorization"); // Ukloni postojeći ako postoji
                request.Headers.TryAddWithoutValidation("Authorization", authHeader);

                Console.WriteLine($"🔑 Forwarding Authorization header: {authHeader.Substring(0, Math.Min(50, authHeader.Length))}...");
            }
            else
            {
                Console.WriteLine($"⚠️ No Authorization header found in request");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}