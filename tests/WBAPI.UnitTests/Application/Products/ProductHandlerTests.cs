using FluentAssertions;
using Moq;
using WBAPI.Application.Commands.Products;
using WBAPI.Application.Queries.Products;
using WBAPI.Domain.Entities;
using WBAPI.Domain.Exceptions;
using WBAPI.Domain.Ports;

namespace WBAPI.UnitTests.Application.Products;

// ── Create ────────────────────────────────────────────────────────
public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CreateProductHandler _sut;

    public CreateProductHandlerTests()
        => _sut = new CreateProductHandler(_repo.Object, _uow.Object);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsProductDto()
    {
        _repo.Setup(r => r.AddAsync(It.IsAny<Product>(), default)).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var cmd = new CreateProductCommand("Monitor 4K", "27 inch", 499.99m, "Electronics", 20);

        var result = await _sut.Handle(cmd, default);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Monitor 4K");
        result.Price.Should().Be(499.99m);
        result.Stock.Should().Be(20);
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}

// ── Update ────────────────────────────────────────────────────────
public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly UpdateProductHandler _sut;

    public UpdateProductHandlerTests()
        => _sut = new UpdateProductHandler(_repo.Object, _uow.Object);

    [Fact]
    public async Task Handle_ExistingProduct_UpdatesAndReturnsDto()
    {
        var existing = Product.Create("Old Name", "Old desc", 100m, "OldCat", 5);
        _repo.Setup(r => r.GetByIdAsync(existing.Id, default)).ReturnsAsync(existing);
        _repo.Setup(r => r.Update(existing));
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var cmd = new UpdateProductCommand(existing.Id, "New Name", "New desc", 200m, "NewCat", 10);
        var result = await _sut.Handle(cmd, default);

        result.Name.Should().Be("New Name");
        result.Price.Should().Be(200m);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(missingId, default)).ReturnsAsync((Product?)null);

        var cmd = new UpdateProductCommand(missingId, "X", "Y", 1m, "Z", 0);
        var act = () => _sut.Handle(cmd, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

// ── Delete ────────────────────────────────────────────────────────
public class DeleteProductHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly DeleteProductHandler _sut;

    public DeleteProductHandlerTests()
        => _sut = new DeleteProductHandler(_repo.Object, _uow.Object);

    [Fact]
    public async Task Handle_ExistingProduct_DeletesSuccessfully()
    {
        var product = Product.Create("ToDelete", "desc", 10m, "cat", 1);
        _repo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _repo.Setup(r => r.Delete(product));
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _sut.Handle(new DeleteProductCommand(product.Id), default);

        _repo.Verify(r => r.Delete(product), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(missingId, default)).ReturnsAsync((Product?)null);

        var act = () => _sut.Handle(new DeleteProductCommand(missingId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _repo.Verify(r => r.Delete(It.IsAny<Product>()), Times.Never);
    }
}

// ── Queries ───────────────────────────────────────────────────────
public class GetAllProductsHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly GetAllProductsHandler _sut;

    public GetAllProductsHandlerTests()
        => _sut = new GetAllProductsHandler(_repo.Object);

    [Fact]
    public async Task Handle_ReturnsAllProducts()
    {
        var products = new List<Product>
        {
            Product.Create("A", "d", 1m, "c", 1),
            Product.Create("B", "d", 2m, "c", 2)
        };
        _repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(products.AsReadOnly());

        var result = await _sut.Handle(new GetAllProductsQuery(), default);

        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().BeEquivalentTo(new[] { "A", "B" });
    }

    [Fact]
    public async Task Handle_EmptyRepository_ReturnsEmptyList()
    {
        _repo.Setup(r => r.GetAllAsync(default))
             .ReturnsAsync(new List<Product>().AsReadOnly());

        var result = await _sut.Handle(new GetAllProductsQuery(), default);

        result.Should().BeEmpty();
    }
}

public class GetProductByIdHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly GetProductByIdHandler _sut;

    public GetProductByIdHandlerTests()
        => _sut = new GetProductByIdHandler(_repo.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsProductDto()
    {
        var product = Product.Create("Widget", "Small widget", 9.99m, "Gadgets", 100);
        _repo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var result = await _sut.Handle(new GetProductByIdQuery(product.Id), default);

        result.Id.Should().Be(product.Id);
        result.Name.Should().Be("Widget");
    }

    [Fact]
    public async Task Handle_MissingId_ThrowsNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(missingId, default)).ReturnsAsync((Product?)null);

        var act = () => _sut.Handle(new GetProductByIdQuery(missingId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
