# 📚 Vidare läsning

Här är handplockade källor per mönster. Varje källa har en rad om varför den är värd din tid. Du behöver inte läsa allt. Välj det som hjälper dig just nu.

---

## 🌐 Allmänt om design patterns

- 📕 **Design Patterns: Elements of Reusable Object-Oriented Software** (Gamma, Helm, Johnson, Vlissides, 1994). [Om boken på Wikipedia](https://en.wikipedia.org/wiki/Design_Patterns). Originalet. Tungt att läsa från pärm till pärm, men bra som uppslagsverk när du vill veta vad ett mönster *ursprungligen* betydde.
- 🌐 **[Refactoring.Guru: Design Patterns](https://refactoring.guru/design-patterns/catalog)**. Den bästa gratis introduktionen. Tydliga illustrationer, problem före lösning och kodexempel i [C#](https://refactoring.guru/design-patterns/csharp).
- 🌐 **[Refactoring.Guru: Criticism of patterns](https://refactoring.guru/design-patterns/criticism)**. En kort och ärlig text om när mönster gör mer skada än nytta.
- 📕 **Head First Design Patterns** (Freeman och Robson, 2:a upplagan 2020). Den mest pedagogiska boken om mönster. Lättläst, med bilder och humor. Exemplen är i Java men översätts lätt till C#.
- 📕 **[Refactoring](https://martinfowler.com/books/refactoring.html)** (Martin Fowler, 2:a upplagan 2018). Lär dig *hur* du tar dig från rörig kod till ett mönster i små, säkra steg. Perfekt till övningarna i `01-refactor`.
- 🌐 **[Refactoring.Guru: Code smells](https://refactoring.guru/refactoring/smells)**. Lär dig känna igen symptomen som brukar betyda att ett mönster behövs.
- 🌐 **[Martin Fowler: Is Design Dead?](https://martinfowler.com/articles/designDead.html)**. En klassisk essä om att låta designen växa fram i stället för att planera allt i förväg.
- 🌐 **[Microsoft Learn: Architectural principles](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles)**. SOLID, separation of concerns och dependency inversion förklarat med .NET-exempel.

---

## 💉 Dependency Injection

- 🌐 **[Martin Fowler: Inversion of Control Containers and the Dependency Injection pattern](https://martinfowler.com/articles/injection.html)**. Artikeln som gav mönstret dess namn. Den förklarar också skillnaden mot Service Locator.
- 🌐 **[Microsoft Learn: Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)**. Den officiella guiden till containern du använder varje dag, med livstider (transient, scoped, singleton).
- 🌐 **[Microsoft Learn: Dependency injection guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)**. Konkreta råd och vanliga fallgropar, till exempel *captive dependencies*.
- 📕 **[Dependency Injection Principles, Practices, and Patterns](https://www.manning.com/books/dependency-injection-principles-practices-patterns)** (Seemann och van Deursen). Den definitiva boken om DI i .NET, för dig som vill gå på djupet.
- 🌐 **[Mark Seemann: Service Locator is an Anti-Pattern](https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/)**. Kort och övertygande om varför dolda beroenden är ett problem.

## 🗄️ Repository och Unit of Work

- 🌐 **[Martin Fowler: Repository](https://martinfowler.com/eaaCatalog/repository.html)** och **[Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html)**. De ursprungliga definitionerna, korta och precisa.
- 🌐 **[Microsoft Learn: Design the infrastructure persistence layer](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)**. Microsofts eget resonemang om repositories ovanpå EF Core, med för- och nackdelar.
- 🌐 **[Microsoft Learn: Testing without your production database](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database)**. Diskuterar repository som "testsöm" jämfört med andra sätt att testa EF Core-kod.
- 🌐 **[Microsoft Learn: Change tracking in EF Core](https://learn.microsoft.com/en-us/ef/core/change-tracking/)**. Förstå hur `DbContext` fungerar som Unit of Work under huven.

## 🔀 Strategy

- 🌐 **[Refactoring.Guru: Strategy](https://refactoring.guru/design-patterns/strategy)**. Tydlig förklaring med bra exempel på när det passar.
- 🌐 **[Refactoring.Guru: Replace Conditional with Polymorphism](https://refactoring.guru/replace-conditional-with-polymorphism)**. Refaktoreringen som tar dig från `switch` till Strategy, steg för steg.
- 🌐 **[Refactoring.Guru: Switch Statements (code smell)](https://refactoring.guru/smells/switch-statements)**. Hjälper dig avgöra när en `switch` faktiskt är ett problem.

## 📣 Observer

- 🌐 **[Microsoft Learn: Observer design pattern](https://learn.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern)**. Hur `IObservable<T>` och `IObserver<T>` är tänkta att användas.
- 🌐 **[Microsoft Learn: Events (C#)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)**. Grunden i C#-events, som är Observer inbyggt i språket.
- 🌐 **[Martin Fowler: What do you mean by "Event-Driven"?](https://martinfowler.com/articles/201701-event-driven.html)**. Reder ut fyra olika saker som alla kallas "event-driven". Bra när du går från events i en klass till events mellan system.
- 🌐 **[RxJS: Observable](https://rxjs.dev/guide/observable)**. Observer-mönstret i front end-världen, väl förklarat.

## 🎁 Decorator

- 🌐 **[Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)**. Bra illustrationer av hur wrappers staplas på varandra.
- 🌐 **[Microsoft Learn: ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)**. Se hur middleware-pipelinen lindar in anrop, med kod före och efter `next()`.
- 🌐 **[Microsoft Learn: DelegatingHandler](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler)**. Decorators runt `HttpClient`, som används för retry, loggning och autentisering.

## 🏭 Factory och 🧱 Builder

- 🌐 **[Refactoring.Guru: Factory Method](https://refactoring.guru/design-patterns/factory-method)**, **[Abstract Factory](https://refactoring.guru/design-patterns/abstract-factory)** och **[Builder](https://refactoring.guru/design-patterns/builder)**. Reder ut skillnaderna mellan de tre skapande mönstren.
- 🌐 **[Microsoft Learn: IHttpClientFactory with .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)**. En fabrik i praktiken, och varför `new HttpClient()` överallt är en dålig idé.
- 🌐 **[Martin Fowler: FluentInterface](https://martinfowler.com/bliki/FluentInterface.html)**. Om stilen bakom moderna builders med `.With...().With...()`.

## 📨 Mediator och CQRS

- 🌐 **[Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html)**. Förklarar idén och varnar för att använda den där den inte behövs. Läs varningen!
- 🌐 **[Microsoft Learn: CQRS pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)**. När CQRS passar i större system, och vilka problem det för med sig.
- 🌐 **[Microsoft Learn: Apply simplified CQRS and DDD patterns](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/apply-simplified-microservice-cqrs-ddd-patterns)**. Den pragmatiska varianten av CQRS, i en .NET-kontext.
- 🌐 **[Refactoring.Guru: Mediator](https://refactoring.guru/design-patterns/mediator)**. Det ursprungliga GoF-mönstret, som är bra att jämföra med MediatR-stilen.
- 🌐 **[MediatR på GitHub](https://github.com/jbogard/MediatR)**. Biblioteket som gjorde stilen populär i .NET. Läs koden, den är förvånansvärt liten.

## 🔌 Adapter

- 🌐 **[Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)**. Enkel och tydlig förklaring med "fyrkantig pinne i rund hålighet"-exemplet.
- 🌐 **[Martin Fowler: Gateway](https://martinfowler.com/eaaCatalog/gateway.html)**. Fowlers variant: ett objekt som kapslar in åtkomsten till ett externt system. Det är ofta så Adapter används i praktiken.

---

## 🌍 Mönster efter GoF

- 🌐 **[Microsoft Learn: Options pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)**. Typad konfiguration, och skillnaden mellan `IOptions`, `IOptionsSnapshot` och `IOptionsMonitor`.
- 🌐 **[Microsoft Learn: Retry pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry)** och **[Circuit Breaker pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)**. Tydliga förklaringar av när och hur.
- 🌐 **[Microsoft Learn: Build resilient HTTP apps](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)**. Hur du lägger till retry och circuit breaker i .NET med en rad kod.
- 🌐 **[Martin Fowler: Replacing Throwing Exceptions with Notification in Validations](https://martinfowler.com/articles/replaceThrowWithNotification.html)**. Ett tidigt resonemang om att samla fel i ett objekt i stället för att kasta exceptions, besläktat med Result pattern.
- 🌐 **[Microsoft Learn: Azure Architecture Center: Cloud design patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)**. En katalog med moderna mönster för molnet, för dig som vill vidare mot arkitektur.

## 🛑 Mot överanvändning

- 🌐 **[Martin Fowler: Yagni](https://martinfowler.com/bliki/Yagni.html)**. "You aren't gonna need it", argumentet mot att bygga för en framtid som kanske aldrig kommer.
- 🌐 **[Refactoring.Guru: Speculative Generality](https://refactoring.guru/smells/speculative-generality)**. Kodlukten för abstraktioner som skapats "ifall att".
- 🌐 **[Martin Fowler: BeckDesignRules](https://martinfowler.com/bliki/BeckDesignRules.html)**. Kent Becks fyra regler för enkel design. Ett bra filter innan du inför ett mönster.
