# 📝 NOTES: Stream layers

Facit med klassificering och förklaringar finns på main i [`answers/02-spot-the-pattern/stream-layers.md`](../../../answers/02-spot-the-pattern/stream-layers.md).

## 💡 `CountingStream`

`StreamLayers/CountingStream.cs` är en färdig decorator. Så här kan du använda den på två ställen i del 1:

```csharp
var storage = new MemoryStream();
var compressedCounter = new CountingStream(storage, leaveOpen: true);       // UNDER GZip: komprimerade bytes
using (var gzip = new GZipStream(compressedCounter, CompressionLevel.Optimal, leaveOpen: true))
{
    var textCounter = new CountingStream(gzip, leaveOpen: true);            // OVANFÖR GZip: okomprimerade bytes
    using (var writer = new StreamWriter(textCounter, Encoding.UTF8))
    {
        for (var i = 1; i <= 1000; i++) writer.WriteLine($"Rad {i}: ...");
    }
    Console.WriteLine($"Ovanför GZip: {textCounter.BytesWritten}");
}
Console.WriteLine($"Under GZip:   {compressedCounter.BytesWritten}");
```

Notera att den undre räknaren ska läsas först när `GZipStream` har kasserats, eftersom det sista komprimerade blocket skrivs ut i `Dispose`.

## 💡 Resonemanget

- **Överskugga också `Write(ReadOnlySpan<byte>)`.** Moderna .NET-streams anropar ofta span-varianten. Basklassens standardimplementation skickar vidare till `Write(byte[], ...)`, men den hyr en buffert och kopierar, så en egen override är både snabbare och säkrare för räkningen.
- **`leaveOpen`** följer samma konvention som .NET:s egna streams. Decoratorn äger den inre streamen om inget annat sägs.

## 🔀 Alternativ

- Ärv från en färdig "delegating stream"-basklass om du skriver många stream-decorators. Det finns ingen sådan publik i .NET, men den är lätt att skriva en gång.
- Räkna i en `DelegatingHandler` i stället, om det är HTTP-trafik du vill mäta.
