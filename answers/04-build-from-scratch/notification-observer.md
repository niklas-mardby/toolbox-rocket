# 🔑 Facit: Notification observer

> 🧩 En fullständig, testad lösning finns på branchen `solutions` i `solutions/04-build-from-scratch/notification-observer/`.

## 🏗️ Kärnan i lösningen

```csharp
public void UpdateStatus(string trackingNumber, PackageStatus newStatus)
{
    if (_isShutDown) throw new InvalidOperationException("The tracker has been shut down.");

    var oldStatus = StatusOf(trackingNumber);
    if (oldStatus == newStatus) return;                       // ingen ändring, ingen notis

    _statuses[trackingNumber] = newStatus;
    var change = new StatusChange(trackingNumber, oldStatus, newStatus, time.GetUtcNow());

    StatusChanged?.Invoke(this, new StatusChangedEventArgs(change));   // C#-event
    foreach (var observer in _observers.ToList())                     // kopia!
        observer.OnNext(change);
}

public IDisposable Subscribe(IObserver<StatusChange> observer)
{
    _observers.Add(observer);
    return new Unsubscriber(() => _observers.Remove(observer));
}
```

## 🤔 Reflektionsfrågorna

### 1. Events eller `IObservable<T>`?

| | C#-`event` | `IObservable<T>` |
|-|------------|------------------|
| **Prenumerera** | `+=` | `Subscribe(observer)` |
| **Avsluta** | `-=` (kräver samma delegate) | `Dispose()` på det returnerade objektet |
| **Slut på strömmen** | Inget inbyggt | `OnCompleted()` |
| **Fel** | Inget inbyggt | `OnError(ex)` |
| **Ekosystem** | Överallt i .NET (UI, `INotifyPropertyChanged`) | Rx.NET (`System.Reactive`): filtrera, kombinera och tidsstyra med LINQ-operatorer |

**Välj events** för enkla notiser inom en app, som UI-händelser eller "något har ändrats". **Välj `IObservable<T>`** när du har en *ström* av värden som kan ta slut eller misslyckas, eller när du vill använda Rx-operatorer som `Throttle`, `Buffer` och `Merge`.

### 2. Glömd avregistrering

Subjectet håller en **referens** till observern, via delegaten eller listan. Så länge subjectet lever kan observern inte städas bort av garbage collectorn, även om ingen annan använder den. Det är en **minnesläcka**. Dessutom fortsätter observern att reagera, till exempel genom att skicka SMS, långt efter att den borde ha slutat. Klassiskt exempel: en Blazor- eller WPF-komponent som prenumererar på en singleton-tjänsts event och aldrig avregistrerar sig när komponenten stängs.

### 3. En observer som kastar

I lösningen ovan **avbryts** notifieringen, och observers efter den som kastade får ingen notis. Exceptionen bubblar upp till den som anropade `UpdateStatus`, trots att statusen redan har sparats. Det är oftast inte vad du vill. Alternativ:
- fånga exceptions per observer och logga dem (eller anropa `OnError` på den observern),
- loopa över `StatusChanged.GetInvocationList()` för att göra samma sak för event-prenumeranter.

✅ Ett bra svar ser att **observers inte ska kunna förstöra för subjectet eller för varandra**.

### 4. Varför `IDisposable` i stället för `Unsubscribe`?

- ✅ Det fungerar med `using`: `using var sub = tracker.Subscribe(observer);`.
- ✅ Prenumerationen blir ett **objekt** som kan sparas, skickas vidare och kombineras (Rx.NET har `CompositeDisposable` för att avsluta många på en gång).
- ✅ Den som prenumererar behöver inte ha kvar en referens till *subjectet* för att kunna avsluta, bara till prenumerationen.
- ✅ Det undviker problemet från hint 3: du behöver inte samma delegate igen.
