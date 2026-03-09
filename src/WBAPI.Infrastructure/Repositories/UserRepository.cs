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

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Users.AsNoTracking().OrderBy(u => u.Username).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _ctx.Users.AddAsync(user, ct);

    public Task DeleteAsync(User user, CancellationToken ct = default)
    {
        _ctx.Users.Remove(user);
        return Task.CompletedTask;
    }
}
