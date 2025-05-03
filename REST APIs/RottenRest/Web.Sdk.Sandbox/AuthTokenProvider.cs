using System.Net.Http;
using System.Net.Http.Json;

using Microsoft.IdentityModel.JsonWebTokens;

namespace RottenRest.Web.Sdk.Sandbox;

public class AuthTokenProvider
{
    private readonly HttpClient _httpClient;
    private string _token = string.Empty;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public AuthTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_token))
        {
            var jwt = new JsonWebToken(_token);

            if (jwt.ValidTo > DateTime.Now)
            {
                return _token;
            }
        }

        await Lock.WaitAsync();

        var response = await _httpClient.PostAsJsonAsync("https://localhost:5003/token", new
        {
            userid = "d8566de3-b1a6-4a9b-b842-8e3887a82e42",
            email = "nick@nickchapsas.com",
            customClaims = new Dictionary<string, object>
            {
                { "admin", true },
                { "trusted_member", true }
            }
        });

        _token = await response.Content.ReadAsStringAsync();
        Lock.Release();

        return _token;
    }
}
