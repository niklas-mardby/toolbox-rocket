namespace RepositoryAndUnitOfWork;

// 🛠️ YOUR JOB: implement the contracts with EF Core.
// Remember: repositories register changes and answer queries. They never call SaveChanges.

public class EfBookRepository(LibraryDbContext db) : IBookRepository
{
    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Book>> GetAvailableAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public void Add(Book book) => throw new NotImplementedException();
}

public class EfMemberRepository(LibraryDbContext db) : IMemberRepository
{
    public Task<Member?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
    public void Add(Member member) => throw new NotImplementedException();
}

public class EfLoanRepository(LibraryDbContext db) : ILoanRepository
{
    public Task<Loan?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<int> CountActiveForMemberAsync(int memberId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Loan>> GetOverdueAsync(DateOnly today, CancellationToken ct = default) => throw new NotImplementedException();
    public void Add(Loan loan) => throw new NotImplementedException();
}

public class EfUnitOfWork(LibraryDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => throw new NotImplementedException();
}
