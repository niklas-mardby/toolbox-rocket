using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RepositoryAndUnitOfWork;

// A small demo. It throws NotImplementedException until you've built the EF implementations.
using var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<LibraryDbContext>().UseSqlite(connection).Options;

using (var setup = new LibraryDbContext(options))
{
    setup.Database.EnsureCreated();
    setup.Members.Add(new Member { Name = "Anna", Email = "anna@example.com" });
    setup.Books.AddRange(
        new Book { Isbn = "978-91-0-012345-6", Title = "Pippi Långstrump", Author = "Astrid Lindgren" },
        new Book { Isbn = "978-91-0-065432-1", Title = "Mio, min Mio", Author = "Astrid Lindgren" });
    setup.SaveChanges();
}

using var db = new LibraryDbContext(options);
var service = new LoanService(
    new EfBookRepository(db), new EfMemberRepository(db), new EfLoanRepository(db), new EfUnitOfWork(db), TimeProvider.System);

var loan = await service.BorrowAsync(memberId: 1, isbn: "978-91-0-012345-6");
Console.WriteLine($"📚 Anna lånade '{loan.Book.Title}', åter senast {loan.DueOn:yyyy-MM-dd}.");

var available = await new EfBookRepository(db).GetAvailableAsync();
Console.WriteLine($"📖 Tillgängliga: {string.Join(", ", available.Select(b => b.Title))}");
