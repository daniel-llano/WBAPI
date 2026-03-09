using MediatR;
using WBAPI.Application.DTOs;
using WBAPI.Domain.Entities;
using WBAPI.Domain.Exceptions;
using WBAPI.Domain.Ports;

namespace WBAPI.Application.Commands.Products;

// ── Create ────────────────────────────────────────────────────────
public record CreateProductCommand(
    string Name, string Description, decimal Price, string Category, int Stock
) : IRequest<ProductDto>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repo;
    private readonly IUnitOfWork _uow;

    public CreateProductHandler(IProductRepository repo, IUnitOfWork uow)
    {
        _repo = repo; _uow = uow;
    }

    public async Task<ProductDto> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = Product.Create(cmd.Name, cmd.Description, cmd.Price, cmd.Category, cmd.Stock);
        await _repo.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);
        return product.ToDto();
    }
}

// ── Update ────────────────────────────────────────────────────────
public record UpdateProductCommand(
    Guid Id, string Name, string Description, decimal Price, string Category, int Stock
) : IRequest<ProductDto>;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _repo;
    private readonly IUnitOfWork _uow;

    public UpdateProductHandler(IProductRepository repo, IUnitOfWork uow)
    {
        _repo = repo; _uow = uow;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(Product), cmd.Id);

        product.Update(cmd.Name, cmd.Description, cmd.Price, cmd.Category, cmd.Stock);
        _repo.Update(product);
        await _uow.SaveChangesAsync(ct);
        return product.ToDto();
    }
}

// ── Delete ────────────────────────────────────────────────────────
public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _repo;
    private readonly IUnitOfWork _uow;

    public DeleteProductHandler(IProductRepository repo, IUnitOfWork uow)
    {
        _repo = repo; _uow = uow;
    }

    public async Task Handle(DeleteProductCommand cmd, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(Product), cmd.Id);

        _repo.Delete(product);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Mapping helper ────────────────────────────────────────────────
internal static class ProductMappings
{
    internal static ProductDto ToDto(this Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Category, p.Stock, p.CreatedAt, p.UpdatedAt);
}
