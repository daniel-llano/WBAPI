namespace WBAPI.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Password, string Role = "User");

public record AuthResponse(string AccessToken, string Username, string Role, DateTime ExpiresAt);
