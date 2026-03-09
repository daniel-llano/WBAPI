using MediatR;
using WBAPI.Application.DTOs;
using WBAPI.Domain.Exceptions;
using WBAPI.Domain.Ports;

namespace WBAPI.Application.Commands.Users;

public static class ValidRoles
{
    public static readonly string[] All = ["Admin", "User", "Invitado"];
}

// ── Change user role ──────────────────────────────────────────────────────────
public record ChangeUserRoleCommand(Guid UserId, string Role) : IRequest<UserDto>;

public class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public ChangeUserRoleHandler(IUserRepository users, IUnitOfWork uow)
    {
        _users = users;
        _uow   = uow;
    }

    public async Task<UserDto> Handle(ChangeUserRoleCommand request, CancellationToken ct)
    {
        if (!ValidRoles.All.Contains(request.Role))
            throw new ArgumentException($"Invalid role '{request.Role}'. Valid roles: {string.Join(", ", ValidRoles.All)}");

        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        user.ChangeRole(request.Role);
        await _uow.SaveChangesAsync(ct);

        return new UserDto(user.Id, user.Username, user.Role, user.CreatedAt);
    }
}

// ── Delete user ───────────────────────────────────────────────────────────────
public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public DeleteUserHandler(IUserRepository users, IUnitOfWork uow)
    {
        _users = users;
        _uow   = uow;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);

        await _users.DeleteAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
