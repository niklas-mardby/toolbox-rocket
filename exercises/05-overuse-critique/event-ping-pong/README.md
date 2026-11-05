# 🏓 Event ping-pong

> 🧩 **Överanvänt mönster:** Observer (events som triggar events)
> 📏 **Omfattning:** sex tjänster och åtta events som tillsammans bildar en kassa. Varje klass är liten och "frikopplad", men för att förstå vad som händer vid ett köp måste du följa en kedja genom hela kodbasen. Det finns minst en dold koppling som inte syns förrän du letar.
> 🧪 **Tester:** finns och är gröna. Du får ändra `Composition.cs` men inte testerna.

## 🎯 Mål

Du ska förstå varför en kedja av events gör ett affärsflöde svårt att följa och ändra, och skriva om det till ett tydligt flöde, utan att förlora frikopplingen där den faktiskt behövs.

## 📖 Bakgrund

Kassan i Verktygsboden är "helt händelsestyrd". Ingen tjänst känner till någon annan:

```
OrderPlaced ─▶ Inventory ─▶ StockReserved ─▶ Payment ─▶ PaymentSucceeded ─▶ Shipping ─▶ ShipmentCreated ─▶ Loyalty ─▶ PointsAwarded ─▶ Email
                   │                            │
                   └▶ StockReservationFailed ─▶ Email
                                                └▶ PaymentFailed ─▶ Inventory (släpp lager) ─▶ StockReleased
                                                                 └▶ Email
```

Det fungerar, och testerna är gröna. Men produktägaren har frågat tre saker som ingen i teamet kunde svara på direkt:

1. ❓ "Om vi stänger av lojalitetsprogrammet, vad händer då?"
2. ❓ "Kan vi skicka orderbekräftelsen *innan* paketet skapas?"
3. ❓ "Vem sätter egentligen orderns status?"

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och se att de är gröna.
2. 🗺️ **Följ ett köp** från `CheckoutService.Checkout` genom alla events och skriv ner varje steg. Hur många filer behövde du öppna?
3. ❓ **Svara på produktägarens tre frågor.** Det kräver att du läser koden noga.
4. ✍️ **Skriv ner din kritik.** Vad är skillnaden mellan en *reaktion* på något som hänt och ett *steg* i ett flöde?
5. ✂️ **Skriv om** så att `CheckoutService` uttryckligen styr flödet. Kör testerna efter varje steg.
6. ⚖️ **Fundera:** finns det något i kassan där ett event faktiskt *är* en bra idé? Lägg i så fall tillbaka just det.

## ▶️ Kom igång

```bash
dotnet run --project exercises/05-overuse-critique/event-ping-pong/EventPingPong
dotnet test exercises/05-overuse-critique/event-ping-pong/EventPingPong.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Fråga 1, lojalitetsprogrammet</summary>

Titta i `EmailService`. Vilket event utlöser orderbekräftelsen? Och vem höjer det eventet?
</details>

<details>
<summary>💡 Hint 2: Fråga 3, statusen</summary>

Sök efter `order.Status =` i hela projektet. Hur många ställen hittar du? Hur vet `CheckoutService` vad som hände?
</details>

<details>
<summary>💡 Hint 3: Steg eller reaktion?</summary>

Ett *steg* kan stoppa flödet eller påverka vad som händer sedan (lagret, betalningen). En *reaktion* behöver bara veta att något har hänt och påverkar inte resultatet. Steg passar som direkta anrop. Reaktioner kan passa som events.
</details>

<details>
<summary>💡 Hint 4: Hur blir tjänsterna kvar frikopplade?</summary>

Låt tjänsterna *returnera* resultat (`bool TryReserve(Order)`, `bool TryCharge(Order)`) i stället för att höja events. Då känner de fortfarande inte till varandra. Det är bara `CheckoutService` som känner till alla, och det är dess jobb.
</details>

### 📚 Central concepts

- **Observer** – och när det inte passar. 🔗 [Refactoring.Guru: Observer](https://refactoring.guru/design-patterns/observer)
- **Event-driven, fyra olika saker** – Fowlers genomgång av vad "event-driven" kan betyda. 🔗 [Martin Fowler: What do you mean by "Event-Driven"?](https://martinfowler.com/articles/201701-event-driven.html)
- **Domain events** – events som beskriver något i domänen. 🔗 [Martin Fowler: Domain Event](https://martinfowler.com/eaaDev/DomainEvent.html)
- **Coupling** – synlig kontra osynlig koppling. 🔗 [Wikipedia: Coupling](https://en.wikipedia.org/wiki/Coupling_%28computer_programming%29)
- **Shotgun surgery** – när en ändring kräver ändringar på många ställen. 🔗 [Refactoring.Guru: Shotgun Surgery](https://refactoring.guru/smells/shotgun-surgery)
- **Tell, don't ask** – låt objekt göra saker, och läs inte av deras status efteråt. 🔗 [Martin Fowler: TellDontAsk](https://martinfowler.com/bliki/TellDontAsk.html)

## 🤔 Reflektionsfrågor

1. Den gamla koden var "frikopplad". Var den det på riktigt? Vad är skillnaden mellan att klasser *inte känner till varandra* och att de *inte beror på varandra*?
2. Hur skulle du lägga till ett steg, "kontrollera att kunden inte är spärrad", före lagerreservationen i den gamla versionen? Och i din nya?
3. När skulle en händelsekedja som den här vara *motiverad*? Tänk på system där lager, betalning och leverans är separata tjänster i olika team.

---

🔑 **[Facit](../../../answers/05-overuse-critique/event-ping-pong.md)**. Gör ett ärligt försök först!
