namespace WBAPI.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product() { }

    public static Product Create(string name, string description, decimal price, string category, int stock)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            Category = category,
            Stock = stock,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, decimal price, string category, int stock)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        Stock = stock;
        UpdatedAt = DateTime.UtcNow;
    }
}
