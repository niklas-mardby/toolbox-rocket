# 🔑 Facit: Product catalog caching

> 🧩 En fullständig lösning finns på branchen `solutions` i `solutions/06-change-request/product-catalog-caching/`.

## 🤔 Del 2: Reflektionen efter del 1

- **Antal ställen:** caching och loggning i **tre** tjänster, oftast med kopierad kod. Och produktsidan och varukorgen har var sin kopia av *samma* produktcache.
- **Delad cache:** för att få den delad krävs en gemensam cache-klass eller en statisk dictionary som båda tjänsterna använder. Det är ett tecken på att cachen egentligen inte hör hemma i tjänsterna.
- **Den fjärde tjänsten:** utvecklaren måste *komma ihåg* att lägga till caching och loggning, och göra det likadant. Om den glöms blir rekommendationerna långsamma och syns inte i loggen.

## 🔧 Del 3: Efter refaktoreringen

```csharp
// Composition.cs
IProductCatalog decorated = new CachingProductCatalog(
    new TimingProductCatalog(catalog, time, log),
    time);

return new Shop(new ProductPageService(decorated), new SearchPageService(decorated), new CartService(decorated));
```

- **Cache ytterst:** cache-träffar når aldrig timing-decoratorn och loggas inte.
- **En instans:** alla tjänster delar cache.
- **Tjänsterna och `Catalog/` är orörda.** Den fjärde tjänsten får caching och loggning automatiskt, så länge den får sin `IProductCatalog` från samma ställe.

**Retry (nästa förändring):** innanför timing (`Caching → Timing → Retry → Remote`) loggas ett anrop med den *totala* tiden inklusive retries. Utanför timing (`Caching → Retry → Timing → Remote`) loggas *varje försök* för sig. Det senare är oftast mer användbart för drift. Retry ska i båda fallen ligga innanför cachen.

## 🤔 Reflektionsfrågorna

### 1. Varför cachas inte "hittades inte"?
En produkt som saknas *nu* kan skapas om en minut. Med fem minuters cache skulle den vara osynlig för kunder i upp till fem minuter. Det kallas *negative caching*, och det kan ändå vara motiverat, med en **kort** tid, om många frågar efter något som inte finns, till exempel botar som provar slumpmässiga SKU:er och därmed belastar fjärrsystemet.

### 2. 100 samtidiga användare
Med den enkla lösningen kontrollerar alla 100 cachen samtidigt, alla får "miss", och **alla 100 anropar katalogen**. Det kallas **cache stampede** (eller *thundering herd*). Lösningar:
- **`HybridCache`** i .NET 9+, som ser till att bara *ett* anrop görs per nyckel och att de andra väntar på det,
- att spara den pågående `Task<Product?>` i cachen i stället för resultatet, så att alla väntar på samma uppgift,
- `SemaphoreSlim` per nyckel.

### 3. Gamla priser i varukorgen
Det **kan** vara ett problem: kunden ser 199 kr men debiteras det nya priset 249 kr, eller tvärtom. Vanliga lösningar:
- varukorgen **läser priset direkt**, utan cache, vid själva köpet, och cachen används bara för visning,
- katalogteamet **publicerar en händelse** vid prisändring, och vi tömmer cachen för den produkten (Observer!),
- kortare cachetid för priser än för beskrivningar.

✅ Ett bra svar ser att caching alltid är en avvägning mellan **snabbhet** och **färskhet**, och att avvägningen kan vara olika för olika användningar av samma data.
