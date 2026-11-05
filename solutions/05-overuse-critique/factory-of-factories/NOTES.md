# 📝 NOTES: Factory of factories

## 💡 Resonemanget

- **Fem lager blev en statisk metod.** `WelcomeMessage.For(email, name)` är en *factory method* i sin enklaste form: den ger skapandet ett namn och samlar texterna på ett ställe. Det var det enda värdefulla i den gamla koden.
- **Borttaget:**
  - `IMessageBuilder` och `WelcomeMessageBuilder`: en builder för en `record` med tre obligatoriska fält. Den gjorde dessutom felen *sämre*: att glömma ett fält gav ett runtime-fel i stället för ett kompileringsfel.
  - `IMessageBuilderFactory`: en fabrik för en builder, alltså en fabrik som gör `new`.
  - `IMessageFactory` och `WelcomeMessageFactory`: OK som idé, men inte som klass, interface och beroende.
  - `IMessageFactoryFactory` och `DefaultMessageFactoryFactory`: väljer fabrik med en *magisk sträng* (`"welcome"`). Ett stavfel ger ett `NotSupportedException` när programmet körs.
  - `MessageFactoryProvider`: en Singleton, alltså ett globalt tillstånd som gör det omöjligt att byta beteende i ett test, och som döljer beroendet för den som läser `SignupService`.
- **Valideringen och dubblettkontrollen** fanns redan i `SignupService` och är kvar där. Det var den koden som faktiskt gjorde något.

## ⚖️ När skulle en fabrik vara motiverad?

- Om meddelandet ska **översättas** (språk per användare) eller hämtas från **mallar** i en databas, behövs ett beroende: `IWelcomeMessageComposer` som injiceras i `SignupService`. Fortfarande *ett* lager.
- Om det finns **flera meddelandetyper** som väljs medan programmet körs, och valet görs på flera ställen, kan en factory vara rätt. Men den ska väljas med typer, inte med strängar.

## 🔀 Alternativ

- Bygg meddelandet direkt i `SignupService` med `new WelcomeMessage(...)`. Det är helt OK. `WelcomeMessage.For` vinner bara på att texterna hör ihop med typen och kan återanvändas, till exempel från ett adminverktyg som skickar om välkomstmejlet.
