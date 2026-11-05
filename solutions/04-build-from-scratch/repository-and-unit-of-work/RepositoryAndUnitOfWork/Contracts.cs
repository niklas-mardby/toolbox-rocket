namespace RepositoryAndUnitOfWork;

// ✅ The contracts the application layer depends on. Finished.
//
// Note: repositories NEVER save. They only register changes (Add) and answer questions.
// Saving is the Unit of Work's job, so several changes are saved together.

public interface IBookRepository
{
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct = default);

    /// <summary>Available books, ordered by title.</summary>
    Task<IReadOnlyList<Book>> GetAvailableAsync(CancellationToken ct = default);

    void Add(Book book);
}

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id, CancellationToken ct = default);
    void Add(Member member);
}

public interface ILoanRepository
{
    /// <summary>The loan with its Book loaded, or null.</summary>
    Task<Loan?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Number of loans for the member that are not returned yet.</summary>
    Task<int> CountActiveForMemberAsync(int memberId, CancellationToken ct = default);

    /// <summary>Not returned and due BEFORE today. Book and Member loaded. Ordered by due date.</summary>
    Task<IReadOnlyList<Loan>> GetOverdueAsync(DateOnly today, CancellationToken ct = default);

    void Add(Loan loan);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
