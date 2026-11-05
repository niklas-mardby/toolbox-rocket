# 🔑 Facit: Stream layers

## 🗺️ Lagren i del 1

```
din kod ──string──▶ StreamWriter ──bytes──▶ BufferedStream ──bytes──▶ GZipStream ──bytes──▶ MemoryStream
                    (Adapter)               (Decorator)              (Decorator)           (konkret komponent)
                    text → bytes            samlar ihop små          komprimerar           lagrar i minnet
                                            skrivningar
```

| Klass | Mönster | Varför |
|-------|---------|--------|
| `MemoryStream` | **Konkret komponent** | Den faktiska lagringen. Den lindar inte in något. |
| `GZipStream` | **Decorator** | *Är* en `Stream` och tar en `Stream`. Lägger till komprimering. |
| `BufferedStream` | **Decorator** | *Är* en `Stream` och tar en `Stream`. Lägger till buffring. |
| `StreamWriter` | **Adapter** | *Är* en `TextWriter` (text-API) men tar en `Stream` (byte-API). |
| `StreamReader` | **Adapter** | *Är* en `TextReader` (`ReadLine()`) men tar en `Stream`. |

Och `Stream` själv är **component**-abstraktionen som alla decorators och den konkreta komponenten delar.

## 🔍 Del 3: Varför fungerar `CountBytes` med alla?

Därför att decorators **har samma interface som det de lindar in**. Koden som använder en `Stream` kan inte se skillnad på en "naken" `MemoryStream` och en `MemoryStream` inlindad i två decorators. Det är hela poängen med Decorator: du kan lägga till beteende utan att koden som *använder* objektet behöver ändras (Liskov substitution principle).

## 🧪 Experimenten

**Byta plats på `GZipStream` och `BufferedStream`:** Det fungerar fortfarande, och den komprimerade datan blir likadan. (Se bara till att `MemoryStream` fortfarande lämnas öppen: nu är det `BufferedStream` som ligger närmast den, och den saknar `leaveOpen`. Det är en fin liten fälla i sig.) Men nu buffras den *komprimerade* datan i stället för texten. Det är oftast mindre effektivt, eftersom det är de många små skrivningarna från `StreamWriter` till komprimeringen som var dyra. Poängen är att **ordningen påverkar beteendet**, även när resultatet ser likadant ut.

**Ta bort `leaveOpen: true`:** När `GZipStream` kasseras kasserar den också `MemoryStream`. I del 2 kastas då ett `ObjectDisposedException` (eller `NotSupportedException`) när programmet försöker sätta `storage.Position`. Decorators **äger** normalt sin inre komponent och kasserar den, och `leaveOpen` är ett sätt att säga "den här äger du inte".

## 🛠️ `CountingStream`

```csharp
public class CountingStream(Stream inner, bool leaveOpen = false) : Stream
{
    public long BytesWritten { get; private set; }

    public override void Write(byte[] buffer, int offset, int count)
    {
        BytesWritten += count;
        inner.Write(buffer, offset, count);
    }

    public override void Flush() => inner.Flush();
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => inner.CanWrite;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen) inner.Dispose();
        base.Dispose(disposing);
    }
}
```

Lagd **ovanför** `GZipStream` räknar den cirka 59 900 bytes (okomprimerat). Lagd **under** `GZipStream` räknar den cirka 2 700 bytes (komprimerat). Samma decorator ser olika data beroende på var den sitter i stapeln.

## 🧩 Bonus: de tre bytesen

`StreamWriter` med `Encoding.UTF8` skriver en **BOM** (byte order mark), `EF BB BF`, i början av strömmen. `Encoding.UTF8.GetByteCount` räknar bara texten. Därför är dekomprimerad data 3 bytes större. Använd `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` om du inte vill ha BOM. Det är ett litet exempel på att en adapter kan lägga till saker i översättningen.

## 🤔 Reflektionsfrågorna

### 1. Hur många klasser med arv?
Med tre oberoende funktioner (buffring, komprimering, kryptering) som alla kan vara av eller på finns 2³ = 8 kombinationer *per* konkret stream: fil, minne, nätverk och så vidare. Med fyra konkreta streams blir det 32 klasser, och varje ny funktion fördubblar antalet. Med decorators behövs **en klass per funktion** plus en per konkret stream: 3 + 4 = 7.

### 2. Är `StreamReader` bara en Adapter?
Nej. Den **buffrar** också och sköter **avkodning** av teckenkodning, inklusive att känna igen en BOM. Det gör den en aning Decorator-lik också. Men det viktigaste den gör är att översätta från ett byte-API till ett text-API, och det är därför Adapter är den bästa beskrivningen.

### 3. Vad visar räknarna?
Att en decorator bara ser det **lager den sitter mellan**. Ovanför komprimeringen ser den okomprimerad data, och under ser den komprimerad. Det är samma lärdom som i [weather-service-wrappers](../../exercises/01-refactor/weather-service-wrappers/): ordningen i stapeln är en del av designen.

### 4. Liknande staplar i egen kod
Vanliga exempel: `HttpClient` med `DelegatingHandler`s (loggning, retry, autentisering), tjänster med caching- och loggningsdecorators och ASP.NET Core-middleware.
