# 📝 NOTES: Legacy payment gateway

## 💡 Resonemanget

- **`IPaymentGateway` är designat utifrån vad *vår* kod behöver:** `decimal`, vår `Card`-typ, `async` och en `PaymentFailure`-enum med *våra* felorsaker. Det är inte en kopia av LegacyPays API.
- **All LegacyPay-kunskap finns i `LegacyPayAdapter`:** ören, `"MMYY"`, `out`-parametrar och felkoderna 12, 13, 14, 51, 54 och 96. `CheckoutService` och `RefundService` har inget `using LegacyPay.Sdk;` längre.
- **Kundmeddelandena ligger kvar i tjänsterna.** De är *affärsbeslut* (vad kunden ska läsa), inte översättning. Därför är de desamma oavsett leverantör.
- **Återförsöket ligger kvar i `CheckoutService`.** Det är en affärspolicy ("försök en gång till vid tillfälliga fel") och gäller nu automatiskt för NovaPay också. Ett alternativ är en `RetryingPaymentGateway`-decorator, se facit.
- **Bonusen:** `NovaPayAdapter` översätter exceptions till `PaymentResult`. `NovaPayTests.cs` kör samma scenarier mot NovaPay och får samma kundmeddelanden. Inga filer utanför adaptern och `Composition.cs` behövde ändras.
- **Checkout och refunds delar en gateway**, eftersom båda SDK:erna kommer ihåg köp per instans.

## 🔀 Alternativ

- **Kontraktstester:** gör `PaymentTests` abstrakt med `protected abstract PaymentServices Create()` och en subklass per leverantör, så att *alla* tester körs mot båda. Det är den snyggaste formen av bonustestet.
- **Retry som decorator** runt `IPaymentGateway`, så att policyn kan återanvändas och testas för sig.
- **Leverantör via konfiguration** (`"Payments:Provider": "NovaPay"`) och en fabrik i composition root, för att kunna byta utan release.
