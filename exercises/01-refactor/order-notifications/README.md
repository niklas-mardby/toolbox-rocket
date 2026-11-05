# 📣 Order notifications

> 🧩 **Mönster:** Observer
> 📏 **Omfattning:** en tjänst med fem beroenden, ungefär 150 rader totalt. Ett mönster, men med en fälla du måste upptäcka.
> 🧪 **Tester:** finns och är gröna. Du får ändra `Composition.cs` men inte testerna.

## 🎯 Mål

Du ska refaktorera `OrderService` så att den inte längre behöver känna till alla som vill veta att en order har lagts. Nya intressenter ska kunna läggas till **utan att `OrderService` ändras**.

## 📖 Bakgrund

När en kund lägger en order i webbshoppen händer en hel del:

- 📦 lagret reserverar varorna
- 📧 kunden får en orderbekräftelse
- ⭐ kunden får lojalitetspoäng
- 💰 säljteamet får ett mejl om ordern är stor
- 📊 statistiken uppdateras
- 🏭 lagret får en plockorder

Allt detta sker i `OrderService.PlaceOrder`. `OrderService` har fem beroenden i konstruktorn och växer för varje ny önskan. Nu vill produktteamet ha ett Slack-meddelande för varje order också.

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och se att de är gröna.
2. 👀 **Läs `OrderService.PlaceOrder`** och dela in stegen i två grupper:
   - steg som är *en del av att lägga ordern* (om de misslyckas ska ordern inte gå igenom)
   - steg som är *reaktioner på att ordern har lagts* (de behöver bara få veta att det hänt)
3. 📣 **Låt `OrderService` publicera en händelse** när en order har lagts, till exempel med ett C#-`event`. Det ska hända efter att ordern är sparad.
4. 🎧 **Flytta reaktionerna** till egna prenumeranter (handlers) som lyssnar på händelsen.
5. 🔌 **Koppla ihop allt i `Composition.cs`.** Där skapas objekten och prenumeranterna registreras.
6. ✂️ **Ta bort beroenden** som `OrderService` inte längre behöver ur konstruktorn.
7. ✅ **Kör testerna.** De ska vara gröna.
8. ➕ **Bonus:** Posta `"Ny order: {total} kr"` till `SlackChannel` för varje order, utan att ändra `OrderService`. Skriv ett eget test för det.

## ▶️ Kom igång

```bash
dotnet run --project exercises/01-refactor/order-notifications/OrderNotifications
dotnet test exercises/01-refactor/order-notifications/OrderNotifications.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Vilka steg är reaktioner?</summary>

Fråga dig för varje steg: "Om det här steget misslyckas, ska kunden ändå få sin order?" Om svaret är ja, eller om steget bara behöver *veta* att något har hänt, är det en reaktion. Titta särskilt på testet `Out_of_stock_order_fails_and_nothing_else_happens`.
</details>

<details>
<summary>💡 Hint 2: Lagerreservationen</summary>

Lagret *måste* reservera varorna innan ordern får gå igenom, och om det misslyckas ska ingenting annat hända. Det gör reservationen till ett steg i flödet, inte en reaktion. Låt den ligga kvar som ett direkt anrop i `OrderService`. (Plockordern till lagret är däremot en reaktion.)
</details>

<details>
<summary>💡 Hint 3: Hur ser händelsen ut?</summary>

Ett enkelt sätt är ett C#-event:

```csharp
public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;
```

`OrderPlacedEventArgs` kan vara en `record` som innehåller ordern. Glöm inte `?.Invoke(...)`, eftersom eventet är `null` om ingen prenumererar.
</details>

<details>
<summary>💡 Hint 4: Var registreras prenumeranterna?</summary>

I `Composition.CreateShop`. Till exempel:

```csharp
orders.OrderPlaced += (_, e) => statistics.Record(e.Order);
```

En lambda per reaktion fungerar fint för små reaktioner. För de större (bekräftelsemejlet, säljmejlet) kan en egen klass med en `Handle(Order order)`-metod göra koden tydligare.
</details>

<details>
<summary>💡 Hint 5: Ett alternativ till event</summary>

I stället för ett C#-event kan `OrderService` ta in en lista: `IEnumerable<IOrderPlacedHandler>`. Den anropar då `Handle` på varje handler. Det är fortfarande Observer, men nu ser det ut som något en DI-container kan fylla i automatiskt. Jämför gärna de två sätten.
</details>

### 📚 Central concepts

- **Observer pattern** – ett subject publicerar händelser och observers prenumererar på dem, utan att subjectet känner till dem. 🔗 [Refactoring.Guru: Observer](https://refactoring.guru/design-patterns/observer)
- **C#-events** – språkets inbyggda stöd för Observer, med `event`, `+=` och `-=`. 🔗 [Microsoft Learn: Events](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)
- **Delegates** – typer som representerar en metod, och grunden som events bygger på. 🔗 [Microsoft Learn: Delegates](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/)
- **Composition root** – den enda platsen där objekt skapas och kopplas ihop. 🔗 [Microsoft Learn: Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- **Single responsibility principle** – en klass ska bara ha ett skäl att ändras. 🔗 [Wikipedia: Single-responsibility principle](https://en.wikipedia.org/wiki/Single-responsibility_principle)
- **Coupling** – hur mycket klasser är beroende av varandra. Observer minskar kopplingen från subject till observers. 🔗 [Wikipedia: Coupling](https://en.wikipedia.org/wiki/Coupling_%28computer_programming%29)

## 🤔 Reflektionsfrågor

1. Varför är lagerreservationen *inte* en bra kandidat för Observer, medan plockordern är det?
2. Vad händer i din lösning om bekräftelsemejlet kastar en exception? Får ordern gå igenom ändå? Vad *borde* hända?
3. Spelar ordningen mellan prenumeranterna någon roll i den här koden? Hur kan du veta vilken ordning de körs i?
4. Med Observer syns inte längre allt som händer vid en order i `OrderService`. Var letar en ny utvecklare nu? Är det en nackdel?

---

🔑 **[Facit](../../../answers/01-refactor/order-notifications.md)**. Gör ett ärligt försök först!
