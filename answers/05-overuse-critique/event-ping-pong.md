# 🔑 Facit: Event ping-pong

> 🧩 Den omskrivna koden finns på branchen `solutions` i `solutions/05-overuse-critique/event-ping-pong/`.

## ❓ Produktägarens frågor

### 1. Om lojalitetsprogrammet stängs av?
**Kunderna slutar få orderbekräftelser.** `EmailService` skickar bekräftelsen på eventet `PointsAwarded`, som bara höjs av `LoyaltyService`. Det finns ingen koppling i koden mellan "lojalitet" och "orderbekräftelse" som syns. Den finns bara i kedjan. Det är en **dold koppling**.

### 2. Bekräftelsen före paketet?
Inte utan att flytta prenumerationen till ett annat event (`PaymentSucceeded`). Men då försvinner poängen ur mejlet, eftersom de ännu inte är utdelade. Och den som ändrar måste förstå hela kedjan för att veta vad som påverkas.

### 3. Vem sätter statusen?
**Tre olika klasser:** `InventoryService` (Rejected), `PaymentService` (PaymentFailed) och `ShippingService` (Shipped). `CheckoutService` läser av statusen *efteråt* och hoppas att någon har satt den. Annars kastas `InvalidOperationException`.

## ✍️ Kritiken

- 🔍 **Flödet är osynligt.** För att förstå ett köp måste du öppna sex filer och hålla en graf i huvudet.
- 🔗 **Kopplingen finns kvar, men den syns inte.** Tjänsterna *beror* på varandras events i en viss ordning. De bara *nämner* inte varandra.
- 🎲 **Ordningen beror på prenumerationsordningen** i `Composition.cs`, och inget i koden visar det.
- 🧵 **Felhantering och kompensation** (släppa lager vid nekad betalning) är ytterligare events som är lätta att missa.
- 🚫 **Steg och reaktioner blandas.** Lager och betalning är *steg* som kan stoppa flödet. De passar inte som observers.

## ✂️ Efter omskrivningen

```csharp
public CheckoutResult Checkout(Order order)
{
    if (!inventory.TryReserve(order))
    {
        order.Status = OrderStatus.Rejected;
        emails.Send(order.Email, "Tyvärr, en vara är slut i lager");
        return CheckoutResult.OutOfStock;
    }

    if (!payments.TryCharge(order))
    {
        inventory.Release(order);
        order.Status = OrderStatus.PaymentFailed;
        emails.Send(order.Email, "Betalningen nekades");
        return CheckoutResult.PaymentDeclined;
    }

    shipping.Ship(order);
    order.Status = OrderStatus.Shipped;
    var points = loyalty.Award(order);
    emails.Send(order.Email, $"Tack för din order! Du fick {points} poäng.");
    return CheckoutResult.Completed;
}
```

Allt står på 25 rader, felvägarna syns och tjänsterna känner fortfarande inte till varandra.

## ⚖️ Steg 6: var ett event *är* rätt

För **sidoeffekter som inte påverkar resultatet** och som andra vill lägga till: statistik, analys och notiser till säljteamet. Ett enda `OrderCompleted`-event i slutet av `Checkout`, utan kedjor, är en bra kompromiss.

## 🤔 Reflektionsfrågorna

### 1. Var den gamla koden frikopplad?
**Nej, inte på riktigt.** Klasserna *nämnde* inte varandra, men de *berodde* på varandra: `EmailService` antog att `PointsAwarded` betyder "ordern är klar", och `CheckoutService` antog att någon satte statusen. Det är **implicit koppling**, och den är *svårare* att hantera än explicit koppling, eftersom kompilatorn och IDE:n inte kan hjälpa dig att hitta den.

> 📚 Att klasser **inte känner till varandra** handlar om *referenser i koden*. Att de **inte beror på varandra** handlar om *beteende*: om den ena slutar fungera eller ändras, påverkas den andra?

### 2. Lägga till "kontrollera spärrad kund"
- **Gamla versionen:** ett nytt event (`CustomerApproved`), ändra `InventoryService` så att den prenumererar på det i stället för `OrderPlaced`, ett nytt felevent (`CustomerBlocked`) och en ny `EmailService`-prenumeration. Det blir fyra ändringar i tre filer, och risken är stor att något missas.
- **Nya versionen:** tre rader först i `Checkout`: `if (customers.IsBlocked(order.CustomerId)) { ... return CheckoutResult.Blocked; }`.

### 3. När är en händelsekedja motiverad?
När lager, betalning och leverans är **separata tjänster** (mikrotjänster i olika team, med egna databaser och driftsättningar) kan de inte anropa varandra direkt på ett pålitligt sätt. Då *måste* de kommunicera med meddelanden. Men även då är rådet att **samla flödet i en process manager eller saga**, som håller ordning på stegen och kompensationerna på *ett* ställe, i stället för att låta events studsa fritt mellan tjänster. Kedjor av events kallas ibland *choreography* och en central styrning *orchestration*. Båda har sina platser, men orchestration är oftast lättare att förstå.
