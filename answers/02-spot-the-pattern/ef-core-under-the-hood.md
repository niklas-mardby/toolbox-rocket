# 🔑 Facit: EF Core under the hood

## 🔬 Experimenten

### 1. Två frågor efter samma rad → **Identity Map**

Båda frågorna returnerar **samma objekt** (`ReferenceEquals` är `True`). `DbContext` håller en Identity Map: för varje entitet den spårar finns exakt ett objekt per primärnyckel. När den andra frågan returnerar raden med `Id = 1` ser EF Core att den redan har ett objekt för den raden och ger dig det.

🤨 **Överraskningen:** det skickas fortfarande **en SQL-fråga för den andra frågan**. Identity Map är *inte* en cache. Den sparar inga databasanrop, utan garanterar bara att du inte får två olika objekt för samma rad, så att ändringar inte hamnar i fel kopia. Med `AsNoTracking()` stängs mönstret av och du får ett nytt objekt.

🧪 **Med `Find(id)` två gånger:** bara **en** SQL-fråga. `Find` letar först i Identity Map och går till databasen bara om entiteten inte redan spåras. Det är det enda stället där Identity Map fungerar som en cache.

### 2. Flera ändringar, en `SaveChanges` → **Unit of Work**

`ChangeTracker` vet om alla tre ändringarna innan något har skickats: `Modified`, `Added`, `Added`. Det är Unit of Work: ett objekt som *registrerar* ändringar under en affärstransaktion och sedan *skickar dem tillsammans*. Alla tre SQL-satserna körs i **en** transaktion (`Committed transaction.`).

🤨 **`CustomerProxy` och `Customer`:** De kunder som hämtats från databasen är proxies. Cecilia skapades med `new Customer { ... }` i din kod, så hon är en vanlig `Customer` utan lazy loading. Vill du ha en proxy för ett nytt objekt använder du `db.Customers.CreateProxy()`.

### 3. När något går fel → **Unit of Work** (allt eller inget)

Den andra `INSERT`:en bryter mot unique-indexet. Eftersom båda ingår i samma transaktion **rullas båda tillbaka**, och David, som inte hade något fel, sparas inte heller. Det är den viktigaste egenskapen hos en Unit of Work: databasen hamnar aldrig i ett halvfärdigt läge.

### 4. Vad är det för objekt? → **Proxy** (virtual proxy) + **Lazy Load**

`anna.GetType()` är `CustomerProxy`, en subklass av `Customer` som EF Core genererat medan programmet körs (med biblioteket Castle.Core). Den överskuggar de `virtual` navigeringsproperties. Första gången du rör `Orders` skickar proxyn en SQL-fråga och fyller i samlingen. Andra gången finns datan redan och ingen fråga skickas.

🧪 **Utan `UseLazyLoadingProxies()`:** `anna.Orders.Count` blir `0`, eftersom samlingen aldrig laddas och inget fel kastas. Du laddar ordrarna explicit med *eager loading* (`db.Customers.Include(c => c.Orders)`) eller *explicit loading* (`db.Entry(anna).Collection(c => c.Orders).Load()`).

### 5. En fråga som inte körts än → **Iterator** (deferred execution) + **Interpreter** och **Repository**

Frågan är ett `IQueryable<T>` med ett **uttrycksträd** (`query.Expression`). Inget skickas till databasen förrän du itererar. Då översätter EF Core:s query provider trädet till SQL. `DbSet<T>` beter sig som ett **Repository**: en samlingslik ingång där du beskriver *vad* du vill ha, inte *hur* det hämtas.

🧪 **Med `.ToList()`:** SQL:en skickas direkt på den raden, och `query` blir en `List<int>`. `query.Expression` och `ToQueryString()` fungerar inte längre.

## 🤔 Steg 5: eget `IUnitOfWork` och `IRepository<Customer>`?

Ett **generiskt** `IUnitOfWork` med `Commit()` som anropar `SaveChanges()` och ett `IRepository<Customer>` med `GetAll`, `Add` och `Remove` tillför nästan inget. Du lindar in en Unit of Work i en Unit of Work och ett repository i ett repository. Dessutom riskerar du att gömma `Include`, `AsNoTracking` och projektioner.

Det som *kan* tillföra något:
- ✅ ett **domänspecifikt** repository med metoder som `GetCustomersWithUnpaidOrders()`, som ger frågor ett namn och samlar dem på ett ställe,
- ✅ att **gömma EF Core** för domänlagret, om du har ett starkt arkitekturellt skäl till det,
- ⚠️ testbarhet, men det är ett svagare argument än det låter, eftersom EF Core går bra att testa mot SQLite in-memory, precis som det här programmet gör.

## 🤔 Reflektionsfrågorna

### 1. Varför inte en långlivad `DbContext`?
- **Identity Map växer.** Varje entitet som någonsin laddats ligger kvar och spåras. Minnet fylls och varje `SaveChanges` måste gå igenom allt.
- **Gammal data.** Identity Map ger dig samma objekt som förut, även om någon annan har ändrat raden i databasen.
- **Unit of Work blir otydlig.** Vilken affärstransaktion hör ändringarna till? I en webbapp vill du ha *en Unit of Work per request*. Därför registreras `DbContext` som **scoped**.
- **Trådsäkerhet.** `DbContext` är inte trådsäker, och en singleton i en webbapp används av många requests samtidigt.

### 2. Prestandaproblemet med lazy loading
**N+1-problemet:** en fråga för att hämta 100 kunder, och sedan *en fråga till per kund* när du rör `Orders`, alltså 101 frågor. Med `Include(c => c.Orders)` räcker det med en eller två. Lazy loading gömmer databasanrop bakom något som ser ut som en vanlig property, och det är priset för Proxy-mönstrets bekvämlighet.

### 3. En metod EF Core inte kan översätta
EF Core kastar ett `InvalidOperationException` med texten *"The LINQ expression ... could not be translated"*. En query provider kan bara översätta det den känner till: operatorer, vissa `string`- och `Math`-metoder och så vidare. Din egen C#-metod finns inte i SQL. (Äldre versioner av EF Core körde sådana delar i minnet i det tysta. Det togs bort i EF Core 3.0 eftersom det dolde stora prestandaproblem.) Lösningen är att uttrycka villkoret med översättningsbara delar, eller att medvetet hämta datan först med `AsEnumerable()` och filtrera i minnet.
