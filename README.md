# 🧰🚀 Toolbox Rocket

> **"Hur lär jag mig design patterns?"**
> Det här repot är svaret. Du lär dig inte design patterns genom att läsa en lista med UML-diagram. Du lär dig dem genom att stöta på problemen som mönstren löser, prova lösningarna själv och märka när ett mönster gör koden bättre och när det gör den sämre.

Du har skrivit C# ett tag nu. Du kan klasser, interfaces, generics, LINQ, async, EF Core och lite ASP.NET Core. Du vet ungefär vad ett design pattern är och har kanske tittat på ett eller två. Nu är det dags att fylla verktygslådan 🧰 och komma iväg 🚀.

---

## 🎯 Vad du lär dig

Repot fokuserar på de mönster du som full stack-utvecklare i .NET faktiskt möter i vardagen:

| # | Pattern | Var du möter det |
|---|---------|------------------|
| 1 | 💉 **Dependency Injection** | Grunden i ASP.NET Core: `builder.Services.AddScoped<...>()` |
| 2 | 🗄️ **Repository** och **Unit of Work** | EF Core: `DbSet<T>` och `DbContext.SaveChanges()` |
| 3 | 🔀 **Strategy** | Ersätter långa `if/else`- och `switch`-kedjor |
| 4 | 📣 **Observer** | C#-events, `IObservable<T>`, reaktivt state i front end |
| 5 | 🎁 **Decorator** | Middleware, loggning och caching som "lindas runt" en tjänst |
| 6 | 🏭 **Factory** och 🧱 **Builder** | `IHttpClientFactory`, `WebApplication.CreateBuilder()` |
| 7 | 📨 **Mediator** / **CQRS** | Command/query-handlers med pipelines (MediatR-stil) |
| 8 | 🔌 **Adapter** | Att linda in tredjeparts-API:er bakom ditt eget interface |

Målet är inte att du ska kunna alla mönster som finns. Målet är att du ska få en riktigt bra överblick över de viktigaste och känna igen dem när du ser dem.

---

## 🗺️ Vägen genom repot

1. 📖 **Läs [`docs/01-overview.md`](docs/01-overview.md).** En kort introduktion till varje mönster: vilket problem det löser, idén bakom det och när du *inte* ska använda det.
2. 🏋️ **Gör övningar i [`exercises/`](exercises/).** Det är här du lär dig på riktigt.
3. 🌍 **Läs [`docs/02-patterns-in-the-wild.md`](docs/02-patterns-in-the-wild.md)** när du har gjort några övningar. Den visar mönster som kom efter "Gang of Four" och mönster som redan är inbyggda i .NET.
4. 📋 **Ha [`docs/03-pattern-cheatsheet.md`](docs/03-pattern-cheatsheet.md) nära till hands.** En sida med allt sammanfattat.
5. 📚 **Gå vidare med [`docs/04-further-reading.md`](docs/04-further-reading.md)** när du vill fördjupa dig.

---

## 🧭 Så här jobbar du med en övning

1. 👀 **Skumma övningarna** och välj en som passar din nivå och din ambition.
2. 💪 **Försök själv först.** Det är när du kämpar lite som du lär dig mest.
3. 💡 **Fastnar du?** Öppna hintsen, en i taget. De börjar med en liten knuff och blir mer konkreta.
4. 📚 **Fortfarande fast, eller är ett begrepp okänt?** Kolla listan med *Central concepts* i övningen. Läs på om begreppet och kom sedan tillbaka till övningen.
5. 🤔 **Avsluta med reflektionsfrågorna.** Det är där kunskapen sätter sig.
6. 🔑 **Jämför med facit** i [`answers/`](answers/) när du har gjort ett ärligt försök.

---

## 🍽️ Fler övningar än du hinner med (och det är meningen!)

Det finns medvetet **fler övningar än någon hinner göra**. Se det som en buffé, inte som en checklista.

- ✅ **Välj** de övningar som ger *dig* mest, utifrån var du är i din utveckling och vad du vill bli bättre på.
- ⏭️ **Hoppa över** det som känns för enkelt eller inte intresserar dig. Det är inte bara okej, det är uppmuntrat.
- 📈 Inom varje kategori är övningarna ungefär ordnade från mindre till större. De första är små, med ett mönster och tydliga steg. De sista är större kodbaser med flera mönster, öppna krav och avvägningar du själv måste ta ställning till.
- 🔍 Läs början av övningens README. Den berättar hur stor uppgiften är, så du kan avgöra om den passar dig.

---

## 🏋️ Övningstyperna

Varje typ tränar en egen förmåga. Variera gärna!

### 🔧 [01 – Refactor](exercises/01-refactor/)
Du får rörig men fungerande kod och tester som är gröna. Din uppgift är att refaktorera koden till ett mönster, så att testerna fortfarande är gröna efteråt. Testerna är ditt skyddsnät.

| Övning | Kort beskrivning |
|--------|------------------|
| [shipping-cost-calculator](exercises/01-refactor/shipping-cost-calculator/) | En klass med två `switch`-satser för fraktpriser. Ett mönster, tydliga steg. |
| [order-notifications](exercises/01-refactor/order-notifications/) | En ordertjänst som känner till alla som vill veta att en order lagts. |
| [weather-service-wrappers](exercises/01-refactor/weather-service-wrappers/) | Loggning, caching och affärslogik hoptrasslat i samma metod. |
| [subscription-reminders](exercises/01-refactor/subscription-reminders/) | Hårdkodade beroenden och `DateTime.Now` gör koden omöjlig att testa. |
| [legacy-payment-gateway](exercises/01-refactor/legacy-payment-gateway/) | Ett gammalt betal-SDK läcker in överallt i affärskoden. |
| [order-api-pipeline](exercises/01-refactor/order-api-pipeline/) | Ett minimal API med en "gudsklass" full av duplicerad validering och loggning. |

### 🔍 [02 – Spot the pattern](exercises/02-spot-the-pattern/)
Du läser riktig .NET-kod och ramverksanvändning och identifierar vilka mönster som används och hur.

| Övning | Kort beskrivning |
|--------|------------------|
| [dotnet-pattern-match](exercises/02-spot-the-pattern/dotnet-pattern-match/) | Para ihop 23 .NET-features med de 12 mönster de bygger på. |
| [stream-layers](exercises/02-spot-the-pattern/stream-layers/) | Ett litet körbart program som staplar streams på varandra. |
| [aspnet-core-startup](exercises/02-spot-the-pattern/aspnet-core-startup/) | En typisk `Program.cs`, rad för rad. |
| [ef-core-under-the-hood](exercises/02-spot-the-pattern/ef-core-under-the-hood/) | Kör EF Core-experiment och se mönstren i aktion. |
| [frontend-reactive-state](exercises/02-spot-the-pattern/frontend-reactive-state/) | Samma mönster, fast i JavaScript och TypeScript. |
| [mystery-codebase](exercises/02-spot-the-pattern/mystery-codebase/) | En kodbas där mönstren inte heter något. Hitta dem ändå. |

### 🤔 [03 – Pick the pattern](exercises/03-pick-the-pattern/)
Scenarier utan kod. Du väljer ett mönster (eller inget!) och motiverar valet, med avvägningar och alternativ.

| Övning | Kort beskrivning |
|--------|------------------|
| [everyday-scenarios](exercises/03-pick-the-pattern/everyday-scenarios/) | Sex korta scenarier, ett mönster vardera. |
| [webshop-scenarios](exercises/03-pick-the-pattern/webshop-scenarios/) | Åtta scenarier från samma webbshop. |
| [pattern-or-plain-code](exercises/03-pick-the-pattern/pattern-or-plain-code/) | Ibland är det bästa mönstret inget mönster alls. |
| [competing-patterns](exercises/03-pick-the-pattern/competing-patterns/) | Scenarier där två mönster passar. Vilket väljer du, och varför? |
| [legacy-modernization](exercises/03-pick-the-pattern/legacy-modernization/) | Ett gammalt system ska moderniseras utan att gå sönder. |
| [booking-system-design](exercises/03-pick-the-pattern/booking-system-design/) | En öppen designuppgift för ett helt bokningssystem. |

### 🏗️ [04 – Build from scratch](exercises/04-build-from-scratch/)
Du implementerar ett mönster själv i en liten domän. Testerna är avstängda tills du är redo att slå på dem.

| Övning | Kort beskrivning |
|--------|------------------|
| [notification-observer](exercises/04-build-from-scratch/notification-observer/) | Bygg ett Observer-flöde för notiser med C#-events. |
| [pizza-order-builder](exercises/04-build-from-scratch/pizza-order-builder/) | En fluent Builder för pizzabeställningar. |
| [payment-provider-adapter](exercises/04-build-from-scratch/payment-provider-adapter/) | Två betal-SDK:er med helt olika API:er, ett gemensamt interface. |
| [message-sender-decorators](exercises/04-build-from-scratch/message-sender-decorators/) | Retry, loggning och filtrering som staplingsbara Decorators. |
| [repository-and-unit-of-work](exercises/04-build-from-scratch/repository-and-unit-of-work/) | Repository och Unit of Work ovanpå EF Core och SQLite, och frågan om det behövs. |
| [mini-mediator](exercises/04-build-from-scratch/mini-mediator/) | Bygg en egen MediatR-liknande mediator med pipeline behaviors. |

### 🧹 [05 – Overuse critique](exercises/05-overuse-critique/)
Kod där ett mönster gör saker *sämre*. Du kritiserar och förenklar, och testerna ska fortfarande vara gröna.

| Övning | Kort beskrivning |
|--------|------------------|
| [one-algorithm-strategy](exercises/05-overuse-critique/one-algorithm-strategy/) | Strategy för en algoritm som aldrig varierar. |
| [factory-of-factories](exercises/05-overuse-critique/factory-of-factories/) | Tre lager fabriker för att skapa ett enkelt objekt. |
| [interface-for-everything](exercises/05-overuse-critique/interface-for-everything/) | Varje klass har ett interface och allt registreras i DI. |
| [event-ping-pong](exercises/05-overuse-critique/event-ping-pong/) | Events som triggar events som triggar events. Vad händer egentligen? |
| [repository-over-ef-core](exercises/05-overuse-critique/repository-over-ef-core/) | Ett generiskt repository som bara skickar vidare till EF Core. |
| [enterprise-todo-api](exercises/05-overuse-critique/enterprise-todo-api/) | En todo-lista med mediator, decorators, builders och fabriker. Vad ska bort, och vad får stanna? |

### 🔄 [06 – Change request](exercises/06-change-request/)
Du får en fungerande app och ett nytt krav. Lägg först till funktionen, fundera sedan på om designen klarade förändringen och refaktorera så att nästa förändring blir lätt.

| Övning | Kort beskrivning |
|--------|------------------|
| [report-export-formats](exercises/06-change-request/report-export-formats/) | Rapporten kan exporteras till CSV. Nu vill alla ha JSON och Markdown också. |
| [notification-channels](exercises/06-change-request/notification-channels/) | Kunder vill få besked via SMS och push, inte bara e-post. |
| [product-catalog-caching](exercises/06-change-request/product-catalog-caching/) | Katalogen är långsam. Lägg till caching och tidsmätning utan att röra katalogen. |
| [second-payment-provider](exercises/06-change-request/second-payment-provider/) | Butiken ska stödja en betalleverantör till, och välja leverantör per land. |
| [customer-audit-trail](exercises/06-change-request/customer-audit-trail/) | Ett API ska få audit-loggning och validering på alla operationer. |
| [multi-tenant-pricing](exercises/06-change-request/multi-tenant-pricing/) | En prismotor ska stödja flera kunder med egna, konfigurerbara regler. |

---

## ▶️ Kom igång

Du behöver [.NET SDK 10](https://dotnet.microsoft.com/download) (eller senare).

```bash
# Bygg allt och kör alla tester från roten
dotnet build
dotnet test

# Kör en enskild övnings program
dotnet run --project exercises/01-refactor/shipping-cost-calculator/ShippingCostCalculator

# Kör bara en övnings tester
dotnet test exercises/01-refactor/shipping-cost-calculator/ShippingCostCalculator.Tests
```

> 💡 I övningarna under *Build from scratch* och *Change request* är vissa tester markerade med `Skip`. De är avstängda så att hela repot är grönt från början. När du har byggt din lösning tar du bort `Skip` och ser om testerna går igenom.

---

## 🔑 Facit och lösningar

- 🔑 **[`answers/`](answers/)** innehåller facit till frågor med ganska tydliga svar: mönstermatchning, "pick the pattern"-scenarier och reflektionsfrågor. Där svaret är öppet hittar du ett exempelsvar och en lista över vad ett bra svar tar hänsyn till. Varje facit förklarar *varför* svaret är rätt.
- 🧩 **Fullständiga kodlösningar** finns på branchen `solutions`, i mappen `/solutions/`, som speglar `/exercises/`. Varje lösning har en `NOTES.md` med resonemang och alternativa lösningar.

> ⚠️ Gör ett ärligt försök innan du tittar. Facit är till för att du ska kunna kontrollera ditt arbete, inte för att hoppa över det.

---

## 📁 Struktur

```
/README.md            -> Du är här
/docs/                -> Självstudiematerial: översikt, mönster i verkligheten, fusklapp, läslista
/exercises/           -> Övningarna, sorterade efter typ
/answers/             -> Facit till frågor med tydliga svar
DesignPatterns.sln    -> En solution med alla projekt
```

Lycka till, och kom ihåg: ett mönster är ett verktyg, inte ett mål. 🧰🚀
