# 🔑 Facit: Repository over EF Core

> 🧩 Den omskrivna koden, inklusive testet för buggen, finns på branchen `solutions` i `solutions/05-overuse-critique/repository-over-ef-core/`.

## 🔍 Fynden

### 🐢 Prestandaproblemet: `GetAll()`
`GetAll()` gör `ToList()` och hämtar **hela tabellen** till minnet. All filtrering, sortering och räkning sker sedan i C#. `CountLowStock` hämtar alla produkter för att räkna tre. `AddProduct` hämtar alla produkter för att kontrollera en SKU. Med en riktig produktkatalog blir det långsamt och minneskrävande.

### 🐛 Buggen i Unit of Work
`EfRepository.Update` anropar `SaveChanges()` **varje gång**. I `RaisePrices` sparas produkterna en och en. `EfUnitOfWork.Commit()` i slutet gör ingenting, eftersom allt redan är sparat. Om den andra produkten bryter mot prisregeln har den första **redan fått sitt nya pris**. Databasen hamnar i ett halvfärdigt läge, alltså precis det Unit of Work ska förhindra.

### 🐛 Den dolda fällan: `Search`
`string.Contains(term, StringComparison.OrdinalIgnoreCase)` **kan inte översättas till SQL**. Den fungerar bara för att `GetAll()` redan hämtat allt till minnet. Om någon "fixar" prestandaproblemet genom att låta `GetAll` returnera `IQueryable<T>` kraschar sökningen. Prestandaproblemet döljer en bugg.

## 🧪 Testet som bevisar buggen

```csharp
[Fact]
public void Raising_prices_is_all_or_nothing()
{
    // Två produkter i "Maskiner": Borrmaskin 1 499 kr och Industrirobot 19 000 kr.
    using (var db = new ShopDbContext(_options))
    {
        Assert.Throws<DbUpdateException>(() => Composition.CreateProductService(db).RaisePrices("Maskiner", 10));
    }

    using var check = new ShopDbContext(_options);
    Assert.Equal(1499, check.Products.Single(p => p.Sku == "B1").Price);   // med originalkoden: 1649 ❌
}
```

## ✂️ Efter omskrivningen

```csharp
public class ProductService(ShopDbContext db)
{
    public int CountLowStock(int threshold) =>
        db.Products.Count(p => !p.Discontinued && p.Stock < threshold);       // SELECT COUNT(*)

    public void RaisePrices(string category, int percent)
    {
        foreach (var product in db.Products.Where(p => p.Category == category))
            product.Price = (int)Math.Round(product.Price * (100 + percent) / 100m, MidpointRounding.AwayFromZero);
        db.SaveChanges();                                                     // en transaktion
    }
    // ...
}
```

**SQL-kommandona i programmet:** varje fråga skickar nu bara det som behövs (`WHERE` och `ORDER BY` i databasen), och `RaisePrices` gör en `SELECT` och sparar alla ändringar i *ett* `SaveChanges`.

## 🤔 Reflektionsfrågorna

### 1. Varför syntes inte buggarna i testerna?
Testerna kontrollerade **lyckade** flöden med **lite data**. Buggarna syns bara när något *misslyckas halvvägs* (atomicitet) eller när datan är *stor* (prestanda). Tester visar att koden gör rätt i de fall du har testat, men de säger inget om de fall du inte har tänkt på. Det är därför det är värdefullt att skriva tester för *felvägar* och *gränsfall*.

### 2. Hade det varit lätt att byta ORM?
**Nej.** Med Dapper finns ingen change tracking, så `product.Price = ...` följt av `Commit()` hade inte sparat något. Varje metod i `ProductService` som förlitar sig på EF Core:s beteende hade behövt skrivas om. Och `GetAll()` följt av `Where` i minnet hade fortfarande varit lika långsamt. Abstraktionen dolde *syntaxen* men inte *beteendet*. Det kallas en **leaky abstraction**.

### 3. När behålla ett repository?
Om frågorna om produkter (aktiva i kategori, sökning, lågt lager) används på **många ställen**, eller innehåller **domänregler** som måste vara konsekventa, är ett *domänspecifikt* repository rimligt:
```csharp
public interface IProductCatalog
{
    IReadOnlyList<Product> ActiveIn(string category);
    IReadOnlyList<Product> Search(string term);
}
```
Det ska **aldrig spara själv**, och det ska exponera *domänfrågor*, inte `GetAll()`.

### 4. Att förklara för en kollega
Utgå från **konkreta konsekvenser**, inte principer:
> "Jag skrev ett test som visar att `RaisePrices` kan lämna halva kategorin med nya priser om något går fel. Det beror på att repositoryt sparar vid varje `Update`. Om vi använder `DbContext` direkt får vi en transaktion gratis. Och programmet skickar färre och mindre frågor till databasen."

Ett rött test och en mätning övertygar bättre än "det är ett anti-pattern".
