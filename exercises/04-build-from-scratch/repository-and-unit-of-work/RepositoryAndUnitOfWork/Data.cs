using Microsoft.EntityFrameworkCore;

namespace RepositoryAndUnitOfWork;

// ✅ Entities and DbContext are finished. You don't need to change them.

public class Book
{
    public int Id { get; set; }
    public required string Isbn { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class Member
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateOnly BorrowedOn { get; set; }
    public DateOnly DueOn { get; set; }
    public DateOnly? ReturnedOn { get; set; }
}

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Book>().HasIndex(b => b.Isbn).IsUnique();
    }
}
