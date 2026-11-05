using Microsoft.EntityFrameworkCore;

namespace RepositoryAndUnitOfWork.Tests;

public class LoanServiceTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 11, 5);
    private readonly LibraryDatabase _database = new();

    public void Dispose() => _database.Dispose();

    private static LoanService CreateService(LibraryDbContext db, DateOnly? today = null) => new(
        new EfBookRepository(db),
        new EfMemberRepository(db),
        new EfLoanRepository(db),
        new EfUnitOfWork(db),
        new FixedTime(today ?? Today));

    [Fact(Skip = Todo.Skip)]
    public async Task Borrowing_creates_a_loan_and_marks_the_book_as_unavailable()
    {
        using (var db = _database.NewContext())
        {
            var loan = await CreateService(db).BorrowAsync(memberId: 1, isbn: "111");
            Assert.Equal(Today.AddDays(21), loan.DueOn);
        }

        using var check = _database.NewContext();
        var saved = await check.Loans.Include(l => l.Book).SingleAsync();
        Assert.Equal(1, saved.MemberId);
        Assert.Equal("111", saved.Book.Isbn);
        Assert.False(saved.Book.IsAvailable);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Borrowing_an_unavailable_book_fails_and_saves_nothing()
    {
        using (var db = _database.NewContext())
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(db).BorrowAsync(1, "555"));
        }

        using var check = _database.NewContext();
        Assert.Empty(check.Loans);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Members_can_have_at_most_three_active_loans()
    {
        using var db = _database.NewContext();
        var service = CreateService(db);
        await service.BorrowAsync(1, "111");
        await service.BorrowAsync(1, "222");
        await service.BorrowAsync(1, "333");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.BorrowAsync(1, "444"));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Returned_loans_do_not_count_as_active()
    {
        using var db = _database.NewContext();
        var service = CreateService(db);
        var first = await service.BorrowAsync(1, "111");
        await service.BorrowAsync(1, "222");
        await service.BorrowAsync(1, "333");
        await service.ReturnAsync(first.Id);

        var fourth = await service.BorrowAsync(1, "444");

        Assert.NotEqual(0, fourth.Id);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Unknown_member_or_book_is_reported()
    {
        using var db = _database.NewContext();
        var service = CreateService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.BorrowAsync(99, "111"));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.BorrowAsync(1, "999"));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Returning_makes_the_book_available_again()
    {
        int loanId;
        using (var db = _database.NewContext())
        {
            loanId = (await CreateService(db).BorrowAsync(1, "111")).Id;
        }

        using (var db = _database.NewContext())   // a NEW context, like a new web request
        {
            await CreateService(db, Today.AddDays(5)).ReturnAsync(loanId);
        }

        using var check = _database.NewContext();
        var loan = await check.Loans.Include(l => l.Book).SingleAsync();
        Assert.Equal(Today.AddDays(5), loan.ReturnedOn);
        Assert.True(loan.Book.IsAvailable);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Overdue_loans_are_listed_with_book_and_member_oldest_first()
    {
        using (var db = _database.NewContext())
        {
            await CreateService(db, Today.AddDays(-30)).BorrowAsync(1, "111");   // due Today-9: overdue
            await CreateService(db, Today.AddDays(-25)).BorrowAsync(2, "222");   // due Today-4: overdue
            await CreateService(db, Today.AddDays(-21)).BorrowAsync(2, "333");   // due Today: NOT overdue yet
            var returned = await CreateService(db, Today.AddDays(-40)).BorrowAsync(1, "444");
            await CreateService(db, Today.AddDays(-35)).ReturnAsync(returned.Id); // returned: not overdue
        }

        using var fresh = _database.NewContext();
        var overdue = await CreateService(fresh).GetOverdueAsync();

        Assert.Equal(new[] { "Pippi Långstrump", "Mio, min Mio" }, overdue.Select(l => l.Book.Title));
        Assert.Equal(new[] { "Anna", "Bo" }, overdue.Select(l => l.Member.Name));
    }
}
