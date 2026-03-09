using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WBAPI.Application.DTOs;
using WBAPI.IntegrationTests.Helpers;

namespace WBAPI.IntegrationTests.Controllers;

public class ProductsControllerTests : IClassFixture<WbapiFactory>
{
    // Store the factory so each test gets a fresh client with independent headers
    private readonly WbapiFactory _factory;

    public ProductsControllerTests(WbapiFactory factory)
        => _factory = factory;

    private HttpClient NewClient() => _factory.CreateClient();

    private CreateProductRequest SampleProduct(string name = "Laptop Pro")
        => new(name, "High-end laptop", 1299.99m, "Electronics", 50);

    private async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];
        return await AuthHelper.RegisterAndLoginAsync(
            client, $"admin_{suffix}", "Admin@1234", "Admin");
    }

    // ── Create ────────────────────────────────────────────────────
    [Fact]
    public async Task CreateProduct_AsAdmin_Returns201()
    {
        var client = NewClient();
        var token = await GetAdminTokenAsync(client);
        client.SetBearer(token);

        var response = await client.PostAsJsonAsync("/api/products", SampleProduct());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProductDto>();
        body!.Id.Should().NotBeEmpty();
        body.Name.Should().Be("Laptop Pro");
    }

    [Fact]
    public async Task CreateProduct_WithoutToken_Returns401()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/products", SampleProduct("Ghost"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Get All ───────────────────────────────────────────────────
    [Fact]
    public async Task GetAllProducts_AsUser_Returns200()
    {
        var client = NewClient();
        var suffix = Guid.NewGuid().ToString("N")[..6];
        var token = await AuthHelper.RegisterAndLoginAsync(
            client, $"user_{suffix}", "User@1234", "User");
        client.SetBearer(token);

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllProducts_WithoutToken_Returns401()
    {
        var client = NewClient();

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Get By Id ─────────────────────────────────────────────────
    [Fact]
    public async Task GetById_ExistingProduct_Returns200()
    {
        var client = NewClient();
        var token = await GetAdminTokenAsync(client);
        client.SetBearer(token);

        var created = await client.PostAsJsonAsync("/api/products", SampleProduct("Widget"));
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        var response = await client.GetAsync($"/api/products/{product!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductDto>();
        body!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        var client = NewClient();
        var token = await GetAdminTokenAsync(client);
        client.SetBearer(token);

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Update ────────────────────────────────────────────────────
    [Fact]
    public async Task UpdateProduct_AsAdmin_Returns200WithUpdatedData()
    {
        var client = NewClient();
        var token = await GetAdminTokenAsync(client);
        client.SetBearer(token);

        var created = await client.PostAsJsonAsync("/api/products", SampleProduct("OldName"));
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        var updateReq = new UpdateProductRequest("NewName", "Updated desc", 1599.99m, "Electronics", 30);
        var response = await client.PutAsJsonAsync($"/api/products/{product!.Id}", updateReq);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductDto>();
        body!.Name.Should().Be("NewName");
        body.Price.Should().Be(1599.99m);
        body.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateProduct_NonExistentId_Returns404()
    {
        var client = NewClient();
        var token = await GetAdminTokenAsync(client);
        client.SetBearer(token);

        var response = await client.PutAsJsonAsync(
            $"/api/products/{Guid.NewGuid()}",
            new UpdateProductRequest("X", "Y", 1m, "Z", 0));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Delete ────────────────────────────────────────────────────
    [Fact]
    public async Task DeleteProduct_AsAdmin_Returns204()
    {
        var client = NewClient();
        var token = await GetAdminTokenAsync(client);
        client.SetBearer(token);

        var created = await client.PostAsJsonAsync("/api/products", SampleProduct("ToDelete"));
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        var response = await client.DeleteAsync($"/api/products/{product!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_ThenGetById_Returns404()
    {
        var client = NewClient();
        var token = await GetAdminTokenAsync(client);
        client.SetBearer(token);

        var created = await client.PostAsJsonAsync("/api/products", SampleProduct("DeleteMe"));
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        await client.DeleteAsync($"/api/products/{product!.Id}");
        var getResponse = await client.GetAsync($"/api/products/{product.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_NonExistentId_Returns404()
    {
        var client = NewClient();
        var token = await GetAdminTokenAsync(client);
        client.SetBearer(token);

        var response = await client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
