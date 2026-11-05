using Microsoft.EntityFrameworkCore;

namespace RepositoryAndUnitOfWork;

// Repositories register changes and answer queries. They never call SaveChanges.
// All of them share ONE LibraryDbContext (scoped per request in a web app), and that
// context IS the unit of work. EfUnitOfWork is just a thin door to it.

public class EfBookRepository(LibraryDbContext db) : IBookRepository
{
    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct = default) =>
        db.Books.SingleOrDefaultAsync(b => b.Isbn == isbn, ct);

    public async Task<IReadOnlyList<Book>> GetAvailableAsync(CancellationToken ct = default) =>
        await db.Books
            .Where(b => b.IsAvailable)
            .OrderBy(b => b.Title)
            .ToListAsync(ct);

    public void Add(Book book) => db.Books.Add(book);
}

public class EfMemberRepository(LibraryDbContext db) : IMemberRepository
{
    public async Task<Member?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Members.FindAsync([id], ct);

    public void Add(Member member) => db.Members.Add(member);
}

public class EfLoanRepository(LibraryDbContext db) : ILoanRepository
{
    public Task<Loan?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Loans.Include(l => l.Book).SingleOrDefaultAsync(l => l.Id == id, ct);

    public Task<int> CountActiveForMemberAsync(int memberId, CancellationToken ct = default) =>
        db.Loans.CountAsync(l => l.MemberId == memberId && l.ReturnedOn == null, ct);

    public async Task<IReadOnlyList<Loan>> GetOverdueAsync(DateOnly today, CancellationToken ct = default) =>
        await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.ReturnedOn == null && l.DueOn < today)   // the domain rule, in ONE place
            .OrderBy(l => l.DueOn)
            .ToListAsync(ct);

    public void Add(Loan loan) => db.Loans.Add(loan);
}

public class EfUnitOfWork(LibraryDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
