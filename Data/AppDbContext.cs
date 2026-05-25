using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Models;

namespace ShoppingListApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>(); // This is alreay a repository!
    public DbSet<Category> Categories => Set<Category>();

    // Fluent API model builder is used to configure the model (source of truth), such as relationships, constraints, indexes, etc.
    // Snapshot of the model is generated to detect changes and create migrations. It is not used at runtime, but always have to refelct the model.
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    {
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();
    }
}