# 🔑 Facit: Order notifications

> 🧩 En fullständig kodlösning finns på branchen `solutions` i `solutions/01-refactor/order-notifications/`.

## 🗂️ Indelningen av stegen

| Steg | Typ | Varför |
|------|-----|--------|
| Validera att ordern har rader | **Del av flödet** | Om den är ogiltig ska inget hända. |
| Reservera lager | **Del av flödet** | Måste lyckas innan ordern får gå igenom. Resultatet påverkar flödet. |
| Spara ordern | **Del av flödet** | Det är själva operationen. |
| Bekräftelsemejl | Reaktion | Behöver bara veta *att* ordern lagts. |
| Lojalitetspoäng | Reaktion | Samma. |
| Mejl till sälj vid stor order | Reaktion | Samma, plus ett eget villkor som hör hemma i reaktionen. |
| Statistik | Reaktion | Samma. |
| Plockorder | Reaktion | Lagret behöver *veta* att det finns en ny order att plocka. |

## 🏗️ Så här kan strukturen se ut

```csharp
public record OrderPlacedEventArgs(Order Order);

public class OrderService(Inventory inventory)
{
    private readonly List<Order> _orders = [];
    public IReadOnlyList<Order> Orders => _orders;

    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    public void PlaceOrder(Order order)
    {
        if (order.Lines.Count == 0) throw new ArgumentException("...", nameof(order));
        inventory.ReserveAll(order.Lines);
        _orders.Add(order);
        OrderPlaced?.Invoke(this, new OrderPlacedEventArgs(order));
    }
}

// Composition.cs
orders.OrderPlaced += (_, e) => new ConfirmationEmailHandler(outbox).Handle(e.Order);
orders.OrderPlaced += (_, e) => loyalty.AddPoints(e.Order.CustomerId, (int)Math.Floor(e.Order.Total / 10m));
orders.OrderPlaced += (_, e) => new BigOrderAlertHandler(outbox).Handle(e.Order);
orders.OrderPlaced += (_, e) => statistics.Record(e.Order);
orders.OrderPlaced += (_, e) => picking.Enqueue(e.Order);
orders.OrderPlaced += (_, e) => slack.Post($"Ny order: {e.Order.Total:0.00} kr"); // bonus
```

`OrderService` gick från fem beroenden till ett. Bonusen krävde **noll** ändringar i `OrderService`.

## 🤔 Reflektionsfrågorna

### 1. Varför är lagerreservationen inte en bra Observer?

Observer passar när subjectet **inte bryr sig om vad som händer** hos observers. Lagerreservationen har ett *resultat* som avgör om ordern får gå igenom, och den måste ske *innan* ordern sparas. Om den var en prenumerant skulle ordern redan vara sparad när reservationen misslyckas, och de andra prenumeranterna kanske redan hade skickat mejl. Testet `Out_of_stock_order_fails_and_nothing_else_happens` skulle då bli rött.

Plockordern är annorlunda. Den behöver bara *veta* att en order finns, och ordern ska gå igenom även om plocklistan av någon anledning skulle krångla.

> 📚 Tumregel: **Om steget kan stoppa flödet är det inte en notis.**

### 2. Vad händer om bekräftelsemejlet kastar en exception?

Med ett vanligt C#-event körs prenumeranterna **synkront, en i taget**. Om en kastar avbryts anropet, och prenumeranterna efter den körs aldrig. Exceptionen bubblar upp till den som anropade `PlaceOrder`. Men ordern *är redan sparad*, så kunden tror att det misslyckades trots att det lyckades.

Vad som *borde* hända beror på affären, men ofta vill du att ordern går igenom och att felet loggas och hanteras separat. Vanliga lösningar:
- fånga exceptions per prenumerant (loopa över `GetInvocationList()` eller över en lista med handlers),
- lägga händelsen i en kö (outbox pattern) och låta en bakgrundstjänst hantera den med retry.

✅ Ett bra svar ser att Observer **flyttar** felhanteringen men inte löser den.

### 3. Spelar ordningen roll?

I den här koden: nästan inte. Ingen prenumerant beror på en annan. C#-events anropar prenumeranterna i den **ordning de registrerades**, så du kan se ordningen i `Composition.cs`. Men det är en implementationsdetalj som är lätt att missa. Om två prenumeranter börjar bero på varandra, till exempel om säljmejlet ska innehålla kundens nya poängsaldo, har du ett dolt beroende. Det är en varningssignal.

### 4. Var letar en ny utvecklare nu?

I **composition root** (`Composition.cs`, i en riktig app `Program.cs`), där prenumeranterna registreras. Det *är* en nackdel att flödet inte längre syns på ett ställe: kopplingen har blivit lösare men också mer osynlig. Du kan motverka det med
- tydliga namn (`OrderPlaced`, `ConfirmationEmailHandler`),
- att samla alla registreringar på ett ställe, och
- att inte kedja events vidare (se övningen [event-ping-pong](../../exercises/05-overuse-critique/event-ping-pong/)).
