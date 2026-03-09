namespace WBAPI.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Password, string Role = "Invitado");

public record AuthResponse(string AccessToken, string Username, string Role, DateTime ExpiresAt);

public record UserDto(Guid Id, string Username, string Role, DateTime CreatedAt);

public record ChangeRoleRequest(string Role);
