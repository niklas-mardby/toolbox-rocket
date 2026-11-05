# 🌊 Stream layers

> 🧩 **Mönster:** Decorator och Adapter, och skillnaden mellan dem
> 📏 **Omfattning:** ett körbart program på ungefär 80 rader. Du läser, kör och experimenterar, och skriver till sist en egen liten decorator.
> 🧪 **Inga tester.** Programmets utskrift är ditt facit när du experimenterar.

## 🎯 Mål

Du ska känna igen **Decorator** och **Adapter** i `System.IO` och kunna förklara skillnaden mellan dem utifrån kodens struktur.

## 📖 Bakgrund

`System.IO` är ett av de mest kända exemplen på design patterns i .NET. Programmet i `StreamLayers/Program.cs` skriver tusen rader text, buffrar och komprimerar den till en `MemoryStream` och läser sedan tillbaka den. Allt görs genom att **stapla objekt på varandra**.

## 🛠️ Uppgift

1. ▶️ **Kör programmet** och läs koden:
   ```bash
   dotnet run --project exercises/02-spot-the-pattern/stream-layers/StreamLayers
   ```
2. 🗺️ **Rita lagren** i del 1, från `StreamWriter` och ner till `MemoryStream`. Skriv för varje lager vad det *tar emot* och vad det *skickar vidare* (text eller bytes?).
3. 🏷️ **Klassificera varje klass** i programmet: `MemoryStream`, `GZipStream`, `BufferedStream`, `StreamWriter` och `StreamReader`. Vilka är Decorators? Vilka är Adapters? Vilken är den konkreta komponenten som de andra lindas runt?
4. 🔍 **Titta på del 3.** Metoden `CountBytes` tar en `Stream`. Varför fungerar den lika bra med alla tre? Vilken egenskap hos mönstret visar det?
5. 🧪 **Experimentera:**
   - Byt plats på `GZipStream` och `BufferedStream` i del 1. Fungerar det fortfarande? Vad ändras?
   - Ta bort `leaveOpen: true` på den första `GZipStream`. Vad händer, och varför?
6. 🛠️ **Bygg en egen decorator:** skriv en `CountingStream : Stream` som räknar hur många bytes som skrivs genom den. Lägg in den på två ställen i del 1, ovanför och under `GZipStream`, och skriv ut båda räknarna. Vad visar de?
7. 🧩 **Bonus:** Del 1 och del 3 visar olika antal bytes för den okomprimerade texten, med en skillnad på exakt 3. Varför?

## 💡 Hints

<details>
<summary>💡 Hint 1: Hur skiljer jag Decorator från Adapter?</summary>

Ställ två frågor om varje klass:
1. Vilken typ *tar den emot* i konstruktorn?
2. Vilken typ *är den själv*, alltså vilket API erbjuder den utåt?

Om svaren är samma typ är det en **Decorator**. Om de skiljer sig, så att den översätter mellan två olika API:er, är det en **Adapter**.
</details>

<details>
<summary>💡 Hint 2: Vad händer med Dispose?</summary>

När du kasserar (dispose) den yttersta decoratorn kasserar den normalt också den inre. Det är bekvämt, men i del 1 vill programmet kunna läsa `storage` efteråt. Läs om parametern `leaveOpen` i dokumentationen för `GZipStream`.
</details>

<details>
<summary>💡 Hint 3: Att skriva CountingStream</summary>

`Stream` är en abstrakt klass med många medlemmar, men för att skriva behöver du främst `Write(byte[], int, int)`, `Flush()`, `CanWrite` och `Dispose`. De flesta andra kan kasta `NotSupportedException` eller skicka vidare till den inre streamen:

```csharp
public class CountingStream(Stream inner) : Stream
{
    public long BytesWritten { get; private set; }

    public override void Write(byte[] buffer, int offset, int count)
    {
        BytesWritten += count;
        inner.Write(buffer, offset, count);
    }
    // ...
}
```

Fundera på om din `CountingStream` ska kassera den inre streamen när den själv kasseras.
</details>

<details>
<summary>💡 Hint 4: De tre mystiska bytesen</summary>

Jämför vad `Encoding.UTF8.GetByteCount` räknar med vad `StreamWriter` faktiskt skriver i början av en fil. Sök på "UTF-8 BOM".
</details>

### 📚 Central concepts

- **Decorator** – samma interface utåt och inåt, med tillagt beteende. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Adapter** – ett interface utåt, ett annat inåt. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **File and stream I/O i .NET** – översikt över `Stream`, readers och writers. 🔗 [Microsoft Learn: File and stream I/O](https://learn.microsoft.com/en-us/dotnet/standard/io/)
- **GZipStream** – komprimering som en stream. 🔗 [Microsoft Learn: GZipStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream)
- **BufferedStream** – buffring som en stream. 🔗 [Microsoft Learn: BufferedStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.bufferedstream)
- **StreamReader** – från bytes till text. 🔗 [Microsoft Learn: StreamReader](https://learn.microsoft.com/en-us/dotnet/api/system.io.streamreader)
- **IDisposable** – hur resurser frigörs och varför ordningen spelar roll. 🔗 [Microsoft Learn: IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable)
- **Composition over inheritance** – varför streams kombineras i stället för att ärva. 🔗 [Wikipedia: Composition over inheritance](https://en.wikipedia.org/wiki/Composition_over_inheritance)

## 🤔 Reflektionsfrågor

1. Hur många klasser hade behövts om .NET hade löst buffring, komprimering och kryptering med arv i stället för decorators?
2. Är `StreamReader` *bara* en Adapter, eller gör den mer än så?
3. Vad visade dina två `CountingStream`-räknare? Vad säger det om var i stapeln en decorator "ser" datan?
4. Var i din egen kod skulle en liknande stapel kunna vara användbar?

---

🔑 **[Facit](../../../answers/02-spot-the-pattern/stream-layers.md)**. Gör ett ärligt försök först!
