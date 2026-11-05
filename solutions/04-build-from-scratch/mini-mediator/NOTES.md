# 📝 NOTES: Mini mediator

## 💡 Resonemanget

- **Reflection en gång, sedan typad kod.** `Send<TResponse>` känner bara `TResponse`. Med `MakeGenericMethod(request.GetType(), typeof(TResponse))` hoppar vi in i `SendCore<TRequest, TResponse>`, där DI-anropen är vanliga generiska anrop (`GetService<IRequestHandler<TRequest, TResponse>>()`). Det är lättare att läsa och felsöka än reflection eller `dynamic` hela vägen.
- **`SendCore` är `async` med flit.** Exceptions som kastas synkront inne i en metod som anropas via `MethodInfo.Invoke` lindas in i `TargetInvocationException`. Med `async` hamnar de i stället i den returnerade `Task`:en, och anroparen får den riktiga exceptionen (`InvalidOperationException`, `ValidationException`). Testerna upptäckte detta. Ett alternativ är `BindingFlags.DoNotWrapExceptions` i `Invoke`.
- **Kedjan byggs inifrån och ut**, med `Reverse()` så att första registrerade behavior blir yttersta, plus en lokal `next`-variabel per varv så att varje lambda fångar *sin* föregående pipeline.
- **Kortslutning** blir gratis: en behavior som inte anropar `next()` stoppar kedjan (`CachedPingBehavior`, `ValidationBehavior`).
- **Closed behaviors** (`IPipelineBehavior<Ping, string>`) och **open behaviors** (`IPipelineBehavior<,>`) hämtas med samma `GetServices`-anrop. Containern sköter matchningen.
- **Scanning** registrerar varje handler under *varje* handler-interface den implementerar. En klass kan alltså hantera flera notifications.
- **Mediatorn är scoped**, så att handlers kan bero på scoped tjänster som `DbContext`. Handlers är transient, vilket är samma som MediatR:s standard.

## 🔀 Alternativa lösningar

- **Cacha reflection:** `MakeGenericMethod` vid varje anrop är långsamt i stor skala. En `ConcurrentDictionary<Type, Func<...>>` med kompilerade delegates (eller expression trees) gör det snabbt. MediatR gör ungefär så.
- **Wrapper-klasser i stället för `MakeGenericMethod`:** skapa en `RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper<TResponse>` med `Activator.CreateInstance`. Det är också MediatR:s teknik, och den ger typad kod utan `Invoke`.
- **Source generators** kan generera dispatch-koden när programmet kompileras, helt utan reflection. Bibliotek som Mediator (av martinothamar) gör så.
- **Publish parallellt** med `Task.WhenAll(...)`. Det är snabbare, men ordningen blir odefinierad och felhanteringen mer komplex. Sekventiellt är ett bra standardval.
- **Inget bibliotek alls:** minimal API-endpoints plus endpoint filters täcker många av samma behov.
