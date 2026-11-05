using EfCoreUnderTheHood;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// An in-memory SQLite database lives as long as the connection is open.
using var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

using (var setup = new ShopContext(connection))
{
    setup.Database.EnsureCreated();
    var anna = new Customer { Name = "Anna", Email = "anna@example.com" };
    var bo = new Customer { Name = "Bo", Email = "bo@example.com" };
    anna.Orders.Add(new Order { Total = 499 });
    anna.Orders.Add(new Order { Total = 129 });
    setup.Customers.AddRange(anna, bo);
    setup.SaveChanges();
}

ShopContext.ShowSql = true;

// ---------------------------------------------------------------
Section("1. Två frågor efter samma rad");
// ---------------------------------------------------------------
using (var db = new ShopContext(connection))
{
    var first = db.Customers.Single(c => c.Email == "anna@example.com");
    var second = db.Customers.First(c => c.Name == "Anna");
    Console.WriteLine($"   Samma objekt? {ReferenceEquals(first, second)}");

    var untracked = db.Customers.AsNoTracking().Single(c => c.Name == "Anna");
    Console.WriteLine($"   Samma objekt med AsNoTracking? {ReferenceEquals(first, untracked)}");
}

// ---------------------------------------------------------------
Section("2. Flera ändringar, en SaveChanges");
// ---------------------------------------------------------------
using (var db = new ShopContext(connection))
{
    var anna = db.Customers.Single(c => c.Name == "Anna");
    var bo = db.Customers.Single(c => c.Name == "Bo");

    anna.Name = "Anna Andersson";
    db.Orders.Add(new Order { CustomerId = bo.Id, Total = 2499 });
    db.Customers.Add(new Customer { Name = "Cecilia", Email = "cecilia@example.com" });

    Console.WriteLine("   Innan SaveChanges, vad vet DbContext?");
    foreach (var entry in db.ChangeTracker.Entries())
        Console.WriteLine($"   - {entry.Entity.GetType().Name,-12} {entry.State}");

    Console.WriteLine("   Anropar SaveChanges():");
    var affected = db.SaveChanges();
    Console.WriteLine($"   {affected} rader påverkades.");
}

// ---------------------------------------------------------------
Section("3. När något går fel i SaveChanges");
// ---------------------------------------------------------------
using (var db = new ShopContext(connection))
{
    db.Customers.Add(new Customer { Name = "David", Email = "david@example.com" });
    db.Customers.Add(new Customer { Name = "Falsk Anna", Email = "anna@example.com" }); // duplicate email!

    try
    {
        db.SaveChanges();
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine($"   💥 {ex.InnerException?.Message}");
    }
}
using (var db = new ShopContext(connection))
{
    Console.WriteLine($"   Finns David i databasen? {db.Customers.Any(c => c.Name == "David")}");
}

// ---------------------------------------------------------------
Section("4. Vad är det här för objekt?");
// ---------------------------------------------------------------
using (var db = new ShopContext(connection))
{
    var anna = db.Customers.Single(c => c.Email == "anna@example.com");
    Console.WriteLine($"   Deklarerad typ: {nameof(Customer)}");
    Console.WriteLine($"   Faktisk typ:    {anna.GetType().Name}");
    Console.WriteLine($"   Bastyp:         {anna.GetType().BaseType?.Name}");
    Console.WriteLine("   Rör anna.Orders för första gången:");
    Console.WriteLine($"   Anna har {anna.Orders.Count} ordrar.");
    Console.WriteLine("   Rör anna.Orders en gång till:");
    Console.WriteLine($"   Anna har {anna.Orders.Count} ordrar.");
}

// ---------------------------------------------------------------
Section("5. En fråga som inte körts än");
// ---------------------------------------------------------------
using (var db = new ShopContext(connection))
{
    var query = db.Orders.Where(o => o.Total > 200).OrderBy(o => o.Total).Select(o => o.Total);
    Console.WriteLine($"   Typ: {query.GetType().Name}");
    Console.WriteLine($"   Uttryck: {query.Expression}");
    Console.WriteLine($"   SQL: {query.ToQueryString().ReplaceLineEndings(" ")}");
    Console.WriteLine("   Nu itererar vi:");
    foreach (var total in query)
        Console.WriteLine($"   - {total} kr");
}

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"🔬 {title}");
}
