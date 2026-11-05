# 📝 NOTES: Booking system design

En exempeldesign med avvägningar finns på main i [`answers/03-pick-the-pattern/booking-system-design.md`](../../../answers/03-pick-the-pattern/booking-system-design.md).

## 💡 Kärnan i en bra design

| Område | Mönster | Motivering |
|--------|---------|------------|
| Bokningsregler per hall | Strategy (lista per hall) | Regeltyper i kod, parametrar i config |
| Prissättning | Data (pristabell) + Strategy (justeringar) | Priser varierar i *värde*, regler i *logik* |
| Betalning | Adapter + Factory | Tre SDK:er, val per land och metod |
| Dörrlås | Adapter + Factory | Två (snart tre) leverantörer, val per hall |
| Notiser | Observer + Strategy (kanaler) | Bokningen ska inte bry sig om vem som lyssnar eller hur de nås |
| Väntelista | Observer + BackgroundService | Reagera på avbokning, och tidsgränsen via `TimeProvider` |
| Rapporter | Enkel CQRS | Läsning med andra behov än skrivning |
| Revision | Pipeline behavior eller EF-interceptor | Måste gälla *alla* ändringar |
| Avbokningsregler | **Inget mönster** | Få och stabila regler |

## 💡 Testet av designen

Förändring A, B och C ska vardera bli **en ny klass plus konfiguration**. Om din design krävde mer: hitta var variationen inte var isolerad.

## 🔀 Andra giltiga designer

- **Utan mediator:** endpoint filters för audit och validering, och en EF Core-interceptor för revision. Enklare, och helt giltigt om allt går via HTTP.
- **Regelmotor:** om hallarna ska konfigurera *helt nya* regeltyper själva, en DSL eller färdig regelmotor. Det är stort, och troligen överdesign för version 1.
- **Separata tjänster** för betalning och notiser: motiverat först när teamet och lasten växer.
