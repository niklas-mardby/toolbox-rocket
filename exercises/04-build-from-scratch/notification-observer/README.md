# 📦 Notification observer

> 🧩 **Mönster:** Observer, på två sätt: C#-events och `IObservable<T>`
> 📏 **Omfattning:** tre små klasser i en tydlig domän. Del 1 är vanliga events. Del 2 går ett steg djupare med .NET:s formella Observer-interfaces och några knepiga detaljer.
> 🧪 **Tester:** 14 tester finns, avstängda med `Todo.Skip`. Slå på dem när du är redo.

## 🎯 Mål

Du ska bygga ett subject som notifierar observers när ett pakets status ändras, och lära dig både det vardagliga sättet (C#-`event`) och det formella sättet (`IObservable<T>` och `IObserver<T>`) i .NET.

## 📖 Bakgrund

Ett paketföretag vill att flera delar av systemet ska få veta när ett paket byter status, till exempel när det är *på väg ut för leverans* eller har *levererats*. `PackageTracker` ska inte behöva känna till vilka som lyssnar. Idag finns två lyssnare:

- 📱 `SmsNotifier` skickar SMS till mottagaren vid vissa statusar
- 📜 `HistoryLog` sparar alla förändringar

Imorgon kanske det är tio.

## 🛠️ Uppgift

### Del 1: C#-events

1. 📖 Läs `Contracts.cs`, `PackageTracker.cs` och `Observers.cs`.
2. 🧪 Slå på testerna genom att sätta `Todo.Skip` till `null` i `NotificationObserver.Tests/Todo.cs`.
3. 🛠️ Implementera `StatusOf` och `UpdateStatus` i `PackageTracker`. Höj eventet `StatusChanged`, men **bara om statusen faktiskt ändrades**.
4. 📱 Implementera `SmsNotifier`. `Attach` prenumererar och `Detach` avslutar prenumerationen.

### Del 2: `IObservable<T>`

5. 🔭 Implementera `Subscribe` i `PackageTracker`. Den ska returnera ett `IDisposable` som avslutar prenumerationen när det kasseras.
6. 📜 Implementera `HistoryLog` som en `IObserver<StatusChange>`.
7. 🛑 Implementera `Shutdown`: alla observers får `OnCompleted()`, och fler uppdateringar ska kasta `InvalidOperationException`.
8. ✅ Alla 14 tester ska vara gröna. Kör också programmet:
   ```bash
   dotnet run --project exercises/04-build-from-scratch/notification-observer/NotificationObserver
   ```

## 💡 Hints

<details>
<summary>💡 Hint 1: Lagra statusen</summary>

En `Dictionary<string, PackageStatus>` räcker. `StatusOf` ska returnera `null` för okända paket, så returtypen är `PackageStatus?`.
</details>

<details>
<summary>💡 Hint 2: Att höja eventet</summary>

```csharp
StatusChanged?.Invoke(this, new StatusChangedEventArgs(change));
```

`?.` behövs eftersom eventet är `null` när ingen prenumererar.
</details>

<details>
<summary>💡 Hint 3: Detach fungerar inte?</summary>

`-=` måste få *samma* delegate som gavs till `+=`. Två lambdas med samma kod är *inte* samma delegate. Använd en privat metod (`tracker.StatusChanged += OnStatusChanged;`) i stället för en lambda.
</details>

<details>
<summary>💡 Hint 4: Vad ska Subscribe returnera?</summary>

En liten privat klass som implementerar `IDisposable` och tar bort observern ur listan när `Dispose` anropas. Testet `Disposing_twice_is_harmless` kräver att den tål att kasseras två gånger.
</details>

<details>
<summary>💡 Hint 5: En observer som avregistrerar sig mitt i</summary>

Testet `An_observer_may_unsubscribe_while_being_notified` gör något knepigt. Om du loopar över listan med observers med `foreach` och en observer tar bort sig själv under loopen, vad händer? Lösningen är att loopa över en *kopia* av listan.
</details>

### 📚 Central concepts

- **Observer pattern** – subject och observers. 🔗 [Refactoring.Guru: Observer](https://refactoring.guru/design-patterns/observer)
- **Events i C#** – `event`, `+=`, `-=` och `Invoke`. 🔗 [Microsoft Learn: Events](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)
- **Observer design pattern i .NET** – hur `IObservable<T>` och `IObserver<T>` är tänkta att användas. 🔗 [Microsoft Learn: Observer design pattern](https://learn.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern)
- **IObservable&lt;T&gt;** – API-referensen. 🔗 [Microsoft Learn: IObservable&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iobservable-1)
- **IDisposable** – mönstret för att städa upp, här för att avsluta en prenumeration. 🔗 [Microsoft Learn: IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable)
- **Delegates** – det events bygger på, och varför två lambdas inte är lika. 🔗 [Microsoft Learn: Delegates](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/)

## 🤔 Reflektionsfrågor

1. Vad är skillnaden mellan C#-events och `IObservable<T>`? När skulle du välja det ena framför det andra?
2. Vad kan hända om en observer glömmer att avsluta sin prenumeration, och subjectet lever längre än observern?
3. Vad händer i din implementation om en observer kastar en exception? Får de andra observers notisen ändå?
4. Varför valde `IObservable<T>` att returnera ett `IDisposable` i stället för att ha en `Unsubscribe`-metod?

---

🔑 **[Facit](../../../answers/04-build-from-scratch/notification-observer.md)**. Gör ett ärligt försök först!
