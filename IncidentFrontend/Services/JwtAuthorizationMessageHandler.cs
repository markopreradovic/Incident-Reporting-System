using Microsoft.JSInterop;
using System.Net.Http.Headers;

public class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;

    public JwtAuthorizationMessageHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await _js.InvokeVoidAsync("console.log", "Token present:", !string.IsNullOrEmpty(token));

            try
            {
                var payload = token.Split('.')[1];
                var base64 = payload;
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }
                var jsonBytes = Convert.FromBase64String(base64);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                await _js.InvokeVoidAsync("console.log", "Token payload:", json);
            }
            catch { }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}