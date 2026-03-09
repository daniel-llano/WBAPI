using MediatR;
using WBAPI.Application.Commands.Products;
using WBAPI.Application.DTOs;
using WBAPI.Domain.Exceptions;
using WBAPI.Domain.Ports;

namespace WBAPI.Application.Queries.Products;

// ── Get By Id ─────────────────────────────────────────────────────
public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repo;

    public GetProductByIdHandler(IProductRepository repo) => _repo = repo;

    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Product), query.Id);

        return product.ToDto();
    }
}

// ── Get All ───────────────────────────────────────────────────────
public record GetAllProductsQuery : IRequest<IReadOnlyList<ProductDto>>;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _repo;

    public GetAllProductsHandler(IProductRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<ProductDto>> Handle(GetAllProductsQuery query, CancellationToken ct)
    {
        var products = await _repo.GetAllAsync(ct);
        return products.Select(p => p.ToDto()).ToList().AsReadOnly();
    }
}
