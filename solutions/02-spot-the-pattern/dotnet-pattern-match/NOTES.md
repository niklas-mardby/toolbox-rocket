# 📝 NOTES: .NET pattern match

Det fullständiga facit, med alla 23 matchningar och förklaringar, finns på main i [`answers/02-spot-the-pattern/dotnet-pattern-match.md`](../../../answers/02-spot-the-pattern/dotnet-pattern-match.md). Här är resonemanget bakom några av de svåraste fallen.

## 💡 Resonemanget

- **Fråga om strukturen, inte om namnet.** "Samma typ in och ut" betyder Decorator. "Annan typ ut än in" betyder Adapter. "Basklassen äger flödet" betyder Template Method. "Ett utbytbart beteende skickas in" betyder Strategy.
- **De vanligaste förväxlingarna:**
  - `BufferedStream` och `GZipStream` (Decorator) mot `StreamReader` (Adapter).
  - `DelegatingHandler` (Decorator *och* Chain of Responsibility) mot middleware (främst Chain of Responsibility).
  - `StringBuilder` (lös Builder) mot `WebApplication.CreateBuilder()` (tydlig Builder).
  - Två sorters Proxy: gRPC-klient (remote) och lazy loading (virtual).
- **`DbContext` och `DbSet<T>`** är *två* mönster i samma feature: Unit of Work och Repository.

## 🔀 Andra giltiga svar

- **Middleware som Decorator** i stället för Chain of Responsibility, om du lägger vikten vid "kod före och efter `next`".
- **Lambdas i LINQ som Command** i stället för Strategy. Det är rimligt om man ser lambdan som "en handling att utföra senare", men Strategy är vanligare eftersom lambdan *byter ut* en del av en algoritm.
- **`IEnumerable<T>` som Iterator *och* Template Method.** LINQ-operatorerna är generella algoritmer som fylls i med dina lambdas.
- **`ILoggerFactory` som Abstract Factory**, eftersom den kan skapa loggers från flera providers. Factory räcker som svar.
