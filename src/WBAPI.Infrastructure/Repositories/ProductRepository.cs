using Microsoft.EntityFrameworkCore;
using WBAPI.Domain.Entities;
using WBAPI.Domain.Ports;
using WBAPI.Infrastructure.Data;

namespace WBAPI.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _ctx;

    public ProductRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Products.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Products.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default)
        => await _ctx.Products.AddAsync(product, ct);

    public void Update(Product product)
        => _ctx.Products.Update(product);

    public void Delete(Product product)
        => _ctx.Products.Remove(product);
}
