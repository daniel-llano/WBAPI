using WBAPI.Domain.Entities;

namespace WBAPI.Domain.Ports;

/// <summary>
/// Puerto de salida (driven port) para la persistencia de productos.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    void Delete(Product product);
}
