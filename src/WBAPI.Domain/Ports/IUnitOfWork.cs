namespace WBAPI.Domain.Ports;

/// <summary>
/// Puerto de salida para confirmar operaciones de escritura (Unit of Work).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
