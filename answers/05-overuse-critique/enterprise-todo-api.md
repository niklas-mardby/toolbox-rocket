# 🔑 Facit: Enterprise todo API

> 🧩 En förenklad version (cirka 100 rader i stället för 420) finns på branchen `solutions` i `solutions/05-overuse-critique/enterprise-todo-api/`. Det finns flera bra lösningar. Det viktiga är motiveringen.

## 📋 Inventeringen

| Abstraktion | Skulle lösa | Finns problemet? | Kostnad |
|-------------|-------------|------------------|---------|
| Mediator + handlers | Gudsklasser, tvärgående logik | ❌ Fem små endpoints | Indirektion, 5 klasser, krånglig `SendAsync<,>`-syntax |
| `RequestHandlerBase` (Template Method) | Samma flöde i alla handlers | ❌ Bara ett command har validering | Arvshierarki, en dold `logger`-parameter i varje handler |
| `IRepository<T>` | Byta lagring | ❌ Lagringen är en lista | Ett interface med 5 metoder |
| `LoggingRepositoryDecorator` | Spårbarhet | ❌ Loggar på `Debug`, som inte syns | En klass |
| `CachingRepositoryDecorator` | Långsam datakälla | ❌ Datan ligger redan i minnet | ⚠️ **Inte trådsäker** i en singleton |
| `IUnitOfWork` | Atomiska ändringar | ❌ `CommitAsync()` gör inget | Ett anrop i varje handler som *ser* viktigt ut |
| Builder + builder factory | Komplexa objekt | ❌ Två fält | Två interfaces, två klasser |
| Mapper + mapper factory | Komplex mappning | ❌ En rad | Två interfaces, två klasser, en Service Locator |
| Validator + validator factory | Validering | ✅ **Valideringen** behövs, ❌ fabriken inte | En Service Locator till |
| `Result<T>` + `ToHttpResult` | Fel utan exceptions | ⚖️ Delvis | Ett lager mellan logik och HTTP |
| `Unit` | "void" i generics | Bara på grund av mediatorn | En typ till |

## 🔢 "Byt namn" i originalkoden

Ungefär: ett nytt command (`RenameTodoCommand`), en ny handler, en ny validator (eller en ändring av den befintliga, som bara gäller `CreateTodoCommand`), en DI-registrering för handlern, en DI-registrering för validatorn, en ny DTO och en ny endpoint. Det blir **4 filer och 3–4 nya typer**, plus att utvecklaren måste förstå mediatorn, `RequestHandlerBase`, båda fabrikerna och `Result<T>`.

**I den förenklade versionen:** en metod i `TodoStore` (`Rename`) och en endpoint som anropar samma `Validate`. Det är **2 ställen, 0 nya typer**.

## ⚖️ Vad som får stanna (exempel)

- ✅ **Valideringen:** reglerna är affärslogik. De ska finnas på *ett* ställe och kunna återanvändas av både skapa och byt namn.
- ✅ **`TimeProvider`:** fortfarande ett verkligt beroende som gör `CreatedAt` testbart.
- ✅ **En trådsäker lagringsklass.**
- ⚖️ **`Result<T>`:** försvarbart om du vill skilja logik från HTTP, men inte nödvändigt med fem endpoints.

## 📝 Exempel på motivering för en pull request

> **Förenkla todo-API:t**
>
> API:t har fem endpoints och lagrar data i minnet, men koden har 35 typer och 420 rader. Att lägga till en endpoint kräver ändringar i fyra filer och kunskap om mediatorn, två fabriker och `Result<T>`.
>
> Den här PR:en ersätter mediatorn, repository-lagret, decoratorerna, buildern och mapparna med en trådsäker `TodoStore` och fem minimal API-endpoints. Valideringen finns kvar som en metod, och `TimeProvider` är fortfarande injicerad.
>
> **Beteendet är oförändrat:** alla 11 integrationstester är gröna utan ändringar.
> **Bonus:** caching-decoratorn var inte trådsäker. Den buggen försvinner.
>
> Om API:t växer kan vi införa endpoint filters för gemensam validering, och en mediator om vi får många use cases med gemensam tvärgående logik. Men vi gör det när behovet finns, inte innan.

✅ **En bra motivering...** utgår från *konkreta kostnader* (antal filer för en ändring), visar att *beteendet är oförändrat* (testerna), nämner *buggar som försvinner* och beskriver *när* mönstren kan behövas igen. Det visar respekt för det ursprungliga syftet.

## 🤔 Reflektionsfrågorna

### 1. Svårast att bestämma sig om?
Vanligast är **`Result<T>`** (det *är* ett bra mönster i större kodbaser) och **valideringen** (den ska finnas kvar, men i vilken form?). Det är bra att det känns svårt, för det betyder att du väger för- och nackdelar i stället för att följa en regel.

### 2. När införa en mediator igen?
Rimliga utlösande faktorer:
- 🔢 **många use cases**, säg 20–30 eller fler, och endpoints som har börjat bli stora,
- 🔁 **tvärgående logik** som gäller *alla* (audit, behörighet, transaktioner) och som endpoint filters inte räcker till för,
- 📨 **flera ingångar**, där samma operationer anropas från HTTP, en kö och ett schemalagt jobb.

### 3. Kan kod vara för enkel?
**Ja.** Varningssignaler är **duplicering** (samma validering och felhantering kopierad till många ställen), **långa metoder** som gör många saker, och att **ändringar ofta ger buggar** på oväntade ställen. Då är det dags att införa struktur, och mönstren i det här repot är verktygen. Målet är *lagom*, och lagom ändras när kodbasen växer.

### 4. Vad lär man sig av att ta bort mönster?
Öppen fråga. Vanliga insikter: **varje abstraktion har en kostnad** som betalas av alla som läser koden; **mönster är inte ett mål i sig**; och att **förenkling kräver tester**, eftersom utan testerna hade det varit för riskabelt att ta bort så mycket.
