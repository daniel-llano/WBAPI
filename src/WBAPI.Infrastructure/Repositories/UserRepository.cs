using Microsoft.EntityFrameworkCore;
using WBAPI.Domain.Entities;
using WBAPI.Domain.Ports;
using WBAPI.Infrastructure.Data;

namespace WBAPI.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _ctx;

    public UserRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _ctx.Users.AsNoTracking()
                           .FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _ctx.Users.AddAsync(user, ct);
}
