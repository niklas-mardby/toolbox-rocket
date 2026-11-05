namespace RepositoryAndUnitOfWork;

// ✅ The application service. Finished. It only knows the interfaces in Contracts.cs,
// so it has no idea that EF Core exists. Your job is to make it work with EF Core.
public class LoanService(
    IBookRepository books,
    IMemberRepository members,
    ILoanRepository loans,
    IUnitOfWork unitOfWork,
    TimeProvider time)
{
    public const int MaxActiveLoans = 3;
    public static readonly int LoanDays = 21;

    public async Task<Loan> BorrowAsync(int memberId, string isbn, CancellationToken ct = default)
    {
        var member = await members.GetByIdAsync(memberId, ct)
            ?? throw new KeyNotFoundException($"Member {memberId} not found.");

        var book = await books.GetByIsbnAsync(isbn, ct)
            ?? throw new KeyNotFoundException($"Book {isbn} not found.");

        if (!book.IsAvailable)
            throw new InvalidOperationException($"'{book.Title}' is already on loan.");

        if (await loans.CountActiveForMemberAsync(memberId, ct) >= MaxActiveLoans)
            throw new InvalidOperationException($"{member.Name} already has {MaxActiveLoans} books.");

        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var loan = new Loan
        {
            Book = book,
            Member = member,
            BorrowedOn = today,
            DueOn = today.AddDays(LoanDays),
        };

        book.IsAvailable = false;   // change 1
        loans.Add(loan);            // change 2

        await unitOfWork.SaveChangesAsync(ct);   // both changes saved together, or neither
        return loan;
    }

    public async Task ReturnAsync(int loanId, CancellationToken ct = default)
    {
        var loan = await loans.GetByIdAsync(loanId, ct)
            ?? throw new KeyNotFoundException($"Loan {loanId} not found.");

        if (loan.ReturnedOn is not null)
            throw new InvalidOperationException("Already returned.");

        loan.ReturnedOn = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        loan.Book.IsAvailable = true;

        await unitOfWork.SaveChangesAsync(ct);
    }

    public Task<IReadOnlyList<Loan>> GetOverdueAsync(CancellationToken ct = default) =>
        loans.GetOverdueAsync(DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime), ct);
}
