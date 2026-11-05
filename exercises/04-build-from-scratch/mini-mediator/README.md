# 📨 Mini mediator

> 🧩 **Mönster:** Mediator, med pipeline behaviors (Decorator), notifications (Observer) och assembly scanning
> 📏 **Omfattning:** du bygger kärnan i ett MediatR-liknande bibliotek: en mediator, DI-registrering med reflection och ett valideringsbeteende. Kräver att du är bekväm med generics, och kanske nyfiken på reflection. Färre steg-för-steg-instruktioner än i de tidigare övningarna.
> 🧪 **Tester:** 16 tester finns, avstängda med `Todo.Skip`. 12 enhetstester för mediatorn och 4 tester mot ett todo-API som använder den.

## 🎯 Mål

Du ska bygga en egen **mediator** och förstå vad som händer när du skriver `mediator.Send(new PlaceOrderCommand(...))` i en kodbas som använder MediatR.

## 📖 Bakgrund

MediatR är ett av de mest använda biblioteken i .NET-världen. Det gör att endpoints och controllers kan skicka *request-objekt* till en mediator i stället för att anropa tjänster direkt. Mediatorn hittar rätt *handler*, och runt alla handlers kan du lägga *pipeline behaviors* för validering, loggning och transaktioner.

Det känns nästan magiskt, men det är bara några få koncept:
1. 🔍 **Hitta handlern** för requestens typ, via DI-containern.
2. 🧅 **Kedja ihop behaviors** runt handlern, som decorators.
3. 📣 **Publicera notifications** till alla som lyssnar, alltså Observer.
4. 🗂️ **Registrera allt automatiskt** genom att leta igenom en assembly efter handlers.

Kontrakten i `Mediator/Contracts.cs` är färdiga och ser ut ungefär som MediatR:s. Todo-appen i `Features/Todos.cs` och `Program.cs` är också färdig. Den fungerar så fort din mediator gör det.

## 🛠️ Uppgift

1. 📖 Läs `Contracts.cs`, `Features/Todos.cs` och `Program.cs`. Följ en request från endpoint till handler i huvudet. Var tar det stopp?
2. 🧪 Slå på testerna genom att sätta `Todo.Skip` till `null`.
3. 🗂️ Implementera `AddMiniMediator` i `ServiceCollectionExtensions.cs`. Kör testet `Scanning_registers_handlers...`.
4. 📨 Implementera `Send` i `Mediator.cs`, först *utan* behaviors. Kör `Send_...`-testerna.
5. 🧅 Lägg till pipeline behaviors i `Send`. Kör `Behaviors_...` och `A_behavior_...`.
6. 📣 Implementera `Publish`.
7. ✅ Implementera `ValidationBehavior` i `Behaviors.cs`.
8. 🚀 Alla 16 gröna? Starta API:t och prova:
   ```bash
   dotnet run --project exercises/04-build-from-scratch/mini-mediator/MiniMediator
   ```
9. ➕ **Bonus:** Skriv ett `TransactionBehavior` som bara gäller requests som implementerar ett markörinterface `ICommand<T>`. Hur får du en open generic behavior att bara gälla vissa requests?

## 💡 Hints

<details>
<summary>💡 Hint 1: Problemet med Send</summary>

`Send<TResponse>(IRequest<TResponse> request)` vet bara `TResponse` vid kompilering. Handlern är registrerad som `IRequestHandler<Ping, string>`, och för att hitta den behöver du requestens *faktiska* typ: `request.GetType()`. Den finns bara när programmet körs. Två vägar:
- `typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse))` och sedan `services.GetService(thatType)`. Men då har du ett `object` som måste anropas med reflection eller `dynamic`.
- Använd reflection *en gång* för att anropa en privat generisk metod `SendCore<TRequest, TResponse>`. Efter det är all kod vanlig, typad C#.
</details>

<details>
<summary>💡 Hint 2: Kedjan av behaviors</summary>

Börja med det innersta steget, handlern, som en `RequestHandlerDelegate<TResponse>`:

```csharp
RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(request, ct);
```

Linda sedan varje behavior runt den föregående pipelinen. Fundera på i vilken ordning du ska gå igenom behaviors för att den *första* registrerade ska hamna *ytterst*. Och se upp med att fånga rätt variabel i en lambda inne i en loop.
</details>

<details>
<summary>💡 Hint 3: Assembly scanning</summary>

```csharp
foreach (var type in assembly.GetTypes())
    foreach (var iface in type.GetInterfaces())
        if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            services.AddTransient(iface, type);
```

Glöm inte abstrakta klasser, open generics och `INotificationHandler<>`.
</details>

<details>
<summary>💡 Hint 4: Fel exception i testerna?</summary>

Om ett test förväntar sig `InvalidOperationException` men får `TargetInvocationException` så kommer exceptionen genom `MethodInfo.Invoke`, som lindar in exceptions som kastas synkront. Om metoden du anropar är `async` hamnar exceptionen i stället inuti den returnerade `Task`:en.
</details>

### 📚 Central concepts

- **Mediator pattern** – kommunikation via en mellanhand. 🔗 [Refactoring.Guru: Mediator](https://refactoring.guru/design-patterns/mediator)
- **MediatR** – biblioteket som inspirerade övningen. Läs gärna dess källkod. 🔗 [MediatR på GitHub](https://github.com/jbogard/MediatR)
- **Decorator** – det pipeline behaviors egentligen är. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Generics** – typparametrar och constraints. 🔗 [Microsoft Learn: Generics in .NET](https://learn.microsoft.com/en-us/dotnet/standard/generics/)
- **Reflection** – att inspektera och anropa typer medan programmet körs. 🔗 [Microsoft Learn: Attributes and reflection](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/)
- **DI med open generics** – registrera `IPipelineBehavior<,>` en gång för alla typer. 🔗 [Microsoft Learn: Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- **Generic constraints** – `where TRequest : ICommand<TResponse>` för bonusen. 🔗 [Microsoft Learn: Constraints on type parameters](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/where-generic-type-constraint)
- **CQRS** – varför commands och queries ofta skiljs åt. 🔗 [Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html)

## 🤔 Reflektionsfrågor

1. Nu när du har byggt en mediator själv: vad är det egentligen MediatR gör? Är det "magi", eller några dussin rader kod?
2. `CreateTodoHandler` publicerar en notification via mediatorn. Vad är för- och nackdelarna med att handlers använder mediatorn själva?
3. Hur skulle en ny utvecklare hitta vilken kod som körs för `mediator.Send(new CreateTodo(...))`? Vad kunde göra det lättare?
4. Vad skulle du vinna och förlora på att ersätta mediatorn i todo-API:t med vanliga tjänster som injiceras direkt i endpoints?

---

🔑 **[Facit](../../../answers/04-build-from-scratch/mini-mediator.md)**. Gör ett ärligt försök först!
