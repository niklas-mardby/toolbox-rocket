# 📝 NOTES: Product catalog caching

## 💡 Resonemanget

- **Två decorators, var och en med ett ansvar:** `CachingProductCatalog` och `TimingProductCatalog`. Båda implementerar `IProductCatalog` och tar en `IProductCatalog`. Katalogteamets kod och våra tre tjänster är **helt orörda**. Bara `Composition.cs` ändrades.
- **Ordningen:** cache *ytterst*, timing *innanför*. Då loggas bara riktiga anrop till fjärrsystemet, vilket är det kravet säger ("varje anrop till fjärrkatalogen"). Med omvänd ordning skulle cache-träffar också loggas, med 0 ms.
- **En gemensam instans:** alla tre tjänster får *samma* dekorerade katalog. Annars skulle produktsidan och varukorgen ha var sin cache, och acceptanstestet om delad cache skulle bli rött.
- **"Hittades inte" cachas inte.** En produkt som saknas nu kan läggas upp om en minut, och då vill ingen vänta fem minuter på att den ska synas.
- **Sökningar normaliseras** (trimmas, gemener) som *cachenyckel*, men den ursprungliga termen skickas vidare till katalogen, så att loggen och katalogen ser exakt det användaren skrev.
- **`TimeProvider.GetTimestamp` och `GetElapsedTime`** för tidsmätning. Det gör tiden testbar: testernas `ManualTime` styr även timestampen. Med `Stopwatch` hade testerna inte kunnat kontrollera "120 ms" exakt.
- **`try/finally` i timing** gör att även misslyckade anrop loggas.
- **`lock`** runt cachens dictionaries, eftersom katalogen används av många requests samtidigt i en riktig app.

## 🔀 Alternativ

- **`IMemoryCache`** (`Microsoft.Extensions.Caching.Memory`) i stället för egna dictionaries. Den ger utgångstider, storleksbegränsning och trådsäkerhet direkt. Den tar dock inte `TimeProvider` lika enkelt, så testerna blir svårare. **`HybridCache`** (.NET 9+) skyddar dessutom mot att många samtidiga anrop för samma nyckel alla går till fjärrsystemet (*cache stampede*).
- **Scrutor:s `services.Decorate<IProductCatalog, CachingProductCatalog>()`** i en app med DI-container.
- **HTTP-caching** om katalogen är en HTTP-tjänst som skickar `Cache-Control`-headers, då via en `DelegatingHandler`.
