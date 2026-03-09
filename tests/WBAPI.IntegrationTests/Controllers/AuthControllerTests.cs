using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WBAPI.Application.DTOs;
using WBAPI.IntegrationTests.Helpers;

namespace WBAPI.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<WbapiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WbapiFactory factory)
        => _client = factory.CreateClient();

    // ── Register ─────────────────────────────────────────────────
    [Fact]
    public async Task Register_NewUser_Returns201WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("newuser", "P@ssw0rd1", "User"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Username.Should().Be("newuser");
        body.Role.Should().Be("User");
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns400()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("dupuser", "P@ssw0rd1", "User"));

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("dupuser", "P@ssw0rd2", "Admin"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Login ─────────────────────────────────────────────────────
    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("loginuser", "P@ssw0rd1", "User"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("loginuser", "P@ssw0rd1"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("wrongpw_user", "P@ssw0rd1", "User"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("wrongpw_user", "Wrong!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownUser_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("nobody", "nobody"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
