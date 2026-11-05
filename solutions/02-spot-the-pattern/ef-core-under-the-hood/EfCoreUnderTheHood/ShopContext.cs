using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCoreUnderTheHood;

// Navigation properties are virtual so EF Core's lazy-loading proxies can override them.
public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = [];
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;
    public int Total { get; set; } // whole kronor
}

public class ShopContext(SqliteConnection connection) : DbContext
{
    /// <summary>Flip this on and off to see (or hide) the SQL that EF Core sends.</summary>
    public static bool ShowSql { get; set; }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options
        .UseSqlite(connection)
        .UseLazyLoadingProxies()
        .LogTo(
            message => { if (ShowSql) Console.WriteLine($"      🛢️ {Compact(message)}"); },
            [RelationalEventId.CommandExecuted, RelationalEventId.TransactionCommitted, RelationalEventId.TransactionRolledBack]);

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
    }

    // Keeps the log output to one short line: just the SQL, or just the transaction event.
    private static string Compact(string message)
    {
        var lines = message
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Skip(1) // the "dbug: <timestamp> <event id>" header
            .ToList();

        if (lines.Count > 0 && lines[0].StartsWith("Executed DbCommand"))
            return "SQL: " + string.Join(" ", lines.Skip(1));

        return string.Join(" ", lines);
    }
}
