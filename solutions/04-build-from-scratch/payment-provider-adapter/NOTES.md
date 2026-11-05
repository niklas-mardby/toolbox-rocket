# 📝 NOTES: Payment provider adapter

## 💡 Resonemanget

- **Två adapters, ett interface.** All kunskap om ören, `"YYMM"`, statussträngar, `Iso4217` och exceptions finns *bara* i adaptrarna. `Program.cs` loopar över båda utan att veta vilken som är vilken.
- **`MinorUnits.From`** delas av båda adaptrarna. Reglerna för hur *vår* `Money` blir minor units (positivt belopp, högst två decimaler) är våra egna, inte leverantörens. Därför kastas `ArgumentException` för ogiltiga belopp: det är en bugg hos anroparen, inte ett betalningsfel.
- **Exceptions blir resultat.** GlobalCard signalerar fel med exceptions, men vårt interface lovar `PaymentResult`. Adaptern fångar *bara* de exceptions den känner till. Okända exceptions, som `NullReferenceException`, får bubbla upp, eftersom de är buggar och inte affärsfel.
- **Valutan kontrolleras före anropet** i `GlobalCardAdapter`, eftersom SDK:t tar en enum och "JPY" inte ens går att uttrycka. `Enum.TryParse` accepterar numeriska strängar som `"752"`, därför kontrolleras namnet explicit.
- **`checked((int)...)`**: GlobalCard tar cent som `int`. Ett väldigt stort belopp ska ge ett tydligt fel, inte ett negativt tal.
- **Kontraktstesterna** i en abstrakt basklass gör att båda adaptrarna testas mot *exakt samma* regler. En tredje adapter får alla tester gratis.

## 🔀 Alternativa lösningar

- **Klassadapter (arv)** i stället för objektadapter: `class QuickPayAdapter : QuickPayClient, IPaymentProvider`. Det fungerar bara om SDK-klassen inte är `sealed`, och binder adaptern hårdare till SDK:t. Objektadaptern (komposition) är nästan alltid bättre, och den går att testa med en fejkad klient.
- **Mapping-tabeller** i stället för `switch`: `static readonly Dictionary<string, FailureReason>`. Det är smidigt om koderna är många.
- **En gemensam basklass** för adaptrarna med valideringen. Det fungerar, men en liten hjälpklass är enklare och kräver inget arv.
- **Factory för att välja leverantör** (bonusidé): `IPaymentProviderFactory.For(currency)` som väljer QuickPay för SEK/NOK/DKK och GlobalCard för EUR/USD. Se [second-payment-provider](../../../exercises/06-change-request/second-payment-provider/) för en övning om just det.
