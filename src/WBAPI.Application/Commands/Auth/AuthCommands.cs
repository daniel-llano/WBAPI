using MediatR;
using WBAPI.Application.DTOs;
using WBAPI.Application.Interfaces;
using WBAPI.Domain.Entities;
using WBAPI.Domain.Ports;

namespace WBAPI.Application.Commands.Auth;

// ── Register ─────────────────────────────────────────────────────
public record RegisterCommand(string Username, string Password, string Role) : IRequest<AuthResponse>;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public RegisterHandler(IUserRepository users, IUnitOfWork uow, IJwtService jwt)
    {
        _users = users; _uow = uow; _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        var existing = await _users.GetByUsernameAsync(cmd.Username, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Username '{cmd.Username}' is already taken.");

        var hash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);
        var user = User.Create(cmd.Username, hash, cmd.Role);
        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        var token = _jwt.GenerateToken(user);
        return new AuthResponse(token, user.Username, user.Role, DateTime.UtcNow.AddHours(1));
    }
}

// ── Login ─────────────────────────────────────────────────────────
public record LoginCommand(string Username, string Password) : IRequest<AuthResponse>;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;

    public LoginHandler(IUserRepository users, IJwtService jwt)
    {
        _users = users; _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByUsernameAsync(cmd.Username, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwt.GenerateToken(user);
        return new AuthResponse(token, user.Username, user.Role, DateTime.UtcNow.AddHours(1));
    }
}
