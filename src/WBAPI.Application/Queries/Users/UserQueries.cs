using MediatR;
using WBAPI.Application.DTOs;
using WBAPI.Domain.Ports;

namespace WBAPI.Application.Queries.Users;

// ── Get all users ─────────────────────────────────────────────────────────────
public record GetAllUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _users;

    public GetAllUsersHandler(IUserRepository users) => _users = users;

    public async Task<IReadOnlyList<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var users = await _users.GetAllAsync(ct);
        return users.Select(u => new UserDto(u.Id, u.Username, u.Role, u.CreatedAt)).ToList();
    }
}
