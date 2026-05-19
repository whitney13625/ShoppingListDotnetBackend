namespace ShoppingListApi.Models;

public class ShoppingItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public bool Purchased { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
}