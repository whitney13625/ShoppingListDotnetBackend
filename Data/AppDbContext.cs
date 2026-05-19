using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Models;

namespace ShoppingListApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>(); // This is alreay a repository!
    public DbSet<Category> Categories => Set<Category>();
}