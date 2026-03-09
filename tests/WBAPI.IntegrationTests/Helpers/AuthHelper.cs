using System.Net.Http.Headers;
using System.Net.Http.Json;
using WBAPI.Application.DTOs;

namespace WBAPI.IntegrationTests.Helpers;

/// <summary>
/// Shared helpers for retrieving tokens and building typed HTTP content.
/// </summary>
public static class AuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(
        HttpClient client,
        string username = "testadmin",
        string password = "Test@1234",
        string role = "Admin")
    {
        // Register
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(username, password, role));

        // Login
        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(username, password));

        resp.EnsureSuccessStatusCode();
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }

    public static void SetBearer(this HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("Bearer", token);
}
