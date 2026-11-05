namespace RepositoryAndUnitOfWork.Tests;

public class RepositoryTests : IDisposable
{
    private readonly LibraryDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact(Skip = Todo.Skip)]
    public async Task Book_can_be_found_by_isbn()
    {
        using var db = _database.NewContext();

        var book = await new EfBookRepository(db).GetByIsbnAsync("222");

        Assert.Equal("Mio, min Mio", book?.Title);
        Assert.Null(await new EfBookRepository(db).GetByIsbnAsync("999"));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Available_books_exclude_borrowed_ones_and_are_ordered_by_title()
    {
        using var db = _database.NewContext();

        var available = await new EfBookRepository(db).GetAvailableAsync();

        Assert.Equal(
            new[] { "Bröderna Lejonhjärta", "Mio, min Mio", "Pippi Långstrump", "Ronja Rövardotter" },
            available.Select(b => b.Title));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Added_entities_are_saved_by_the_unit_of_work()
    {
        using (var db = _database.NewContext())
        {
            new EfBookRepository(db).Add(new Book { Isbn = "666", Title = "Karlsson på taket", Author = "Astrid Lindgren" });
            new EfMemberRepository(db).Add(new Member { Name = "Cilla", Email = "cilla@example.com" });
            await new EfUnitOfWork(db).SaveChangesAsync();
        }

        using var check = _database.NewContext();
        Assert.NotNull(await new EfBookRepository(check).GetByIsbnAsync("666"));
        Assert.Equal("Cilla", (await new EfMemberRepository(check).GetByIdAsync(3))?.Name);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Repositories_never_save_on_their_own()
    {
        using (var db = _database.NewContext())
        {
            new EfBookRepository(db).Add(new Book { Isbn = "666", Title = "Karlsson på taket", Author = "Astrid Lindgren" });
            // no unit of work!
        }

        using var check = _database.NewContext();
        Assert.Null(await new EfBookRepository(check).GetByIsbnAsync("666"));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Repositories_on_the_same_context_share_one_unit_of_work()
    {
        using var db = _database.NewContext();
        var books = new EfBookRepository(db);
        var unitOfWork = new EfUnitOfWork(db);

        var book = await books.GetByIsbnAsync("111");
        book!.Title = "Pippi Långstrump (ny utgåva)";
        new EfMemberRepository(db).Add(new Member { Name = "Cilla", Email = "cilla@example.com" });

        var saved = await unitOfWork.SaveChangesAsync();

        Assert.Equal(2, saved); // one updated book + one new member, in ONE save
    }
}
