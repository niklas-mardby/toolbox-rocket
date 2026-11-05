using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace RepositoryAndUnitOfWork.Tests;

/// <summary>
/// A real SQLite database in memory, one per test. NewContext() gives you a FRESH DbContext,
/// which is how the tests check what was really saved (not just tracked in memory).
/// </summary>
public sealed class LibraryDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<LibraryDbContext> _options;

    public LibraryDatabase()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<LibraryDbContext>().UseSqlite(_connection).Options;

        using var db = NewContext();
        db.Database.EnsureCreated();
        db.Members.AddRange(
            new Member { Name = "Anna", Email = "anna@example.com" },   // Id 1
            new Member { Name = "Bo", Email = "bo@example.com" });      // Id 2
        db.Books.AddRange(
            new Book { Isbn = "111", Title = "Pippi Långstrump", Author = "Astrid Lindgren" },
            new Book { Isbn = "222", Title = "Mio, min Mio", Author = "Astrid Lindgren" },
            new Book { Isbn = "333", Title = "Bröderna Lejonhjärta", Author = "Astrid Lindgren" },
            new Book { Isbn = "444", Title = "Ronja Rövardotter", Author = "Astrid Lindgren" },
            new Book { Isbn = "555", Title = "Emil i Lönneberga", Author = "Astrid Lindgren", IsAvailable = false });
        db.SaveChanges();
    }

    public LibraryDbContext NewContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

public class FixedTime(DateOnly today) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
}
