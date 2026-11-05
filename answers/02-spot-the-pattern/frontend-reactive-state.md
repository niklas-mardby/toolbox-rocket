# 🔑 Facit: Frontend reactive state

## 🏷️ 1–2. Mönster och motsvarighet i C#

| Snutt | Mönster | Motsvarighet i C# / .NET |
|-------|---------|--------------------------|
| **A** Knappen | **Observer** | `button.Click += handler;` (C#-events). Knappen är subject och lyssnarna är observers. Den vet inte vad de gör. |
| **B** Räknaren | **Observer** | `INotifyPropertyChanged` i WPF/MAUI eller `StateHasChanged()` i Blazor. React "prenumererar" komponenten på dess state, och `setCount` notifierar React så att komponenten ritas om. |
| **C** Varukorgen | **Mediator** + **Observer** + **Command** (+ Strategy) | En MediatR-liknande mediator för `dispatch`, events för `subscribe`, och request-objekt som `record AddItemCommand(...)` för actions. Reducern är en ren funktion som bestämmer *hur* state ändras, en sorts Strategy. |
| **D** Sökrutan | **Observer** (+ Iterator, Decorator/pipeline) | `IObservable<T>` med Rx.NET (`System.Reactive`): `Observable.FromEventPattern(...).Throttle(...).Select(...)`. Varje operator lindar in den föregående observablen. |
| **E** API-klienten | **Chain of Responsibility** / **Decorator** (+ Factory) | `DelegatingHandler` för `HttpClient`, eller ASP.NET Core-middleware. `axios.create` är en fabrik som `IHttpClientFactory`. |
| **F** Sorteringen | **Strategy** | `IComparer<T>` eller en `Comparison<T>`-delegate: `products.Sort((a, b) => a.Price.CompareTo(b.Price))`. |
| **G** HOC och Context | **Decorator** + **Dependency Injection** | `withLoadingSpinner` är en Decorator: samma "interface" (props in, UI ut) med tillagt beteende. `ApiContext.Provider` är DI: föräldern bestämmer implementationen, precis som `builder.Services.AddSingleton<IApiClient, ...>()`. |

## 🔍 3. Redux-storens tre roller

1. **Mediator:** komponenter skickar actions till storen i stället för att anropa varandra. En knapp som lägger till en vara vet inget om varukorgsikonen eller om att varukorgen sparas i `localStorage`. Allt går via storen.
2. **Observer:** `subscribe` registrerar lyssnare som körs efter varje ändring.
3. **Unit of state / "single source of truth":** storen äger allt state. Ingen annan får ändra det direkt, bara via `dispatch`.

Och actions är **Command**-objekt: de beskriver *vad* som ska hända som data. Därför kan de loggas, spelas upp igen och användas för "time travel debugging" i Redux DevTools. Det är samma fördel som en mediator-pipeline i C# ger: ett ställe där varje command passerar.

## 🌊 4. "Observer och Iterator i ett"

- **Iterator (pull):** *du* frågar efter nästa värde (`foreach`, `MoveNext()`). Konsumenten styr tempot.
- **Observer (push):** *källan* skickar värden till dig när de finns (`OnNext`). Producenten styr tempot.

En `Observable` har Observers *push-modell*, men med Iterators *rika API*: `map`, `filter`, `take`, `debounce` och så vidare, precis som LINQ har för `IEnumerable<T>`. Rx.NET är faktiskt LINQ för `IObservable<T>`. `IEnumerable<T>` och `IObservable<T>` är matematiskt varandras "dualer": samma idé med flödet åt motsatt håll.

## 🔁 5. Snutt G jämfört med ASP.NET Core

- `withLoadingSpinner(Component)` ↔ **Decorator**, som en `CachingWeatherService(inner)` eller en middleware med kod före `next()`. Den kan också *kortsluta* (visa spinner i stället för att rendera `Component`), vilket gör den lite Chain of Responsibility-lik.
- `ApiContext.Provider` ↔ **Dependency Injection**. Komponenter som anropar `useContext(ApiContext)` får den implementation som en förälder har bestämt. Det är nära DI, men tekniskt sett mer likt en **Service Locator**, eftersom komponenten själv *frågar* efter beroendet i stället för att få det som en prop.

## 🤔 Reflektionsfrågorna

### 1. Varför är Observer så centralt i front end?
Ett UI är i grunden **en funktion av state**. När state ändras, på grund av en användare som klickar, ett svar från servern eller en timer, måste rätt delar av skärmen uppdateras. Händelser kommer hela tiden, i oförutsägbar ordning och asynkront. En backend är oftare *request in, svar ut*. Där är flödet linjärt och Observer behövs mest för sidoeffekter (som domänevents).

### 2. Fördelar med rena reducers
- ✅ **Förutsägbarhet:** samma state och samma action ger alltid samma resultat.
- ✅ **Enkel ändringsdetektering:** ett nytt objekt betyder att något har ändrats, och samma objekt betyder att inget har ändrats. React kan jämföra med `===` i stället för att jämföra djupt.
- ✅ **Testbarhet:** inga mocks behövs, bara in och ut.
- ✅ **Time travel:** gamla state-objekt finns kvar och kan visas igen.

I C# motsvaras det av immutabla `record`-typer med `with`-uttryck.

### 3. Vad tar du med dig till C#?
Öppen fråga. Vanliga och bra svar är **immutabla state-objekt** (`record` med `with`), **Command-objekt som data** (som MediatR-requests, som kan loggas och köas) och **reaktiva strömmar** med Rx.NET för till exempel realtidsdata via SignalR.
