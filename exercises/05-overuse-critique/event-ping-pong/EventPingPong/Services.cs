namespace EventPingPong;

public class CheckoutService(ShopEvents events)
{
    public CheckoutResult Checkout(Order order)
    {
        events.RaiseOrderPlaced(order);

        // By now, "someone" has hopefully set the status...
        return order.Status switch
        {
            OrderStatus.Shipped => CheckoutResult.Completed,
            OrderStatus.Rejected => CheckoutResult.OutOfStock,
            OrderStatus.PaymentFailed => CheckoutResult.PaymentDeclined,
            _ => throw new InvalidOperationException($"Checkout ended in an unexpected state: {order.Status}")
        };
    }
}

public class InventoryService
{
    private readonly ShopEvents _events;
    private readonly Dictionary<string, int> _stock;

    public InventoryService(ShopEvents events, Dictionary<string, int> initialStock)
    {
        _events = events;
        _stock = new Dictionary<string, int>(initialStock);
        events.OrderPlaced += Reserve;
        events.PaymentFailed += Release;
    }

    public int StockOf(string sku) => _stock.GetValueOrDefault(sku);

    private void Reserve(Order order)
    {
        if (order.Lines.Any(l => StockOf(l.Sku) < l.Quantity))
        {
            order.Status = OrderStatus.Rejected;
            _events.RaiseStockReservationFailed(order);
            return;
        }

        foreach (var line in order.Lines)
            _stock[line.Sku] -= line.Quantity;
        _events.RaiseStockReserved(order);
    }

    private void Release(Order order)
    {
        foreach (var line in order.Lines)
            _stock[line.Sku] += line.Quantity;
        _events.RaiseStockReleased(order);
    }
}

public class PaymentService
{
    private readonly ShopEvents _events;
    private readonly List<Charge> _charges = [];

    public PaymentService(ShopEvents events)
    {
        _events = events;
        events.StockReserved += ChargeCard;
    }

    public IReadOnlyList<Charge> Charges => _charges;

    private void ChargeCard(Order order)
    {
        if (order.CardNumber.EndsWith("0002"))   // test card: always declined
        {
            order.Status = OrderStatus.PaymentFailed;
            _events.RaisePaymentFailed(order);
            return;
        }

        _charges.Add(new Charge(order.Id, order.Total));
        _events.RaisePaymentSucceeded(order);
    }
}

public class ShippingService
{
    private readonly ShopEvents _events;
    private readonly Dictionary<string, string> _addresses;
    private readonly List<Shipment> _shipments = [];

    public ShippingService(ShopEvents events, Dictionary<string, string> addressByCustomer)
    {
        _events = events;
        _addresses = addressByCustomer;
        events.PaymentSucceeded += Ship;
    }

    public IReadOnlyList<Shipment> Shipments => _shipments;

    private void Ship(Order order)
    {
        var shipment = new Shipment(order.Id, _addresses.GetValueOrDefault(order.CustomerId, "Hämtas i butik"));
        _shipments.Add(shipment);
        order.Status = OrderStatus.Shipped;
        _events.RaiseShipmentCreated(order, shipment);
    }
}

public class LoyaltyService
{
    private readonly ShopEvents _events;
    private readonly Dictionary<string, int> _points = [];

    public LoyaltyService(ShopEvents events)
    {
        _events = events;
        events.ShipmentCreated += (order, _) => Award(order);
    }

    public int PointsFor(string customerId) => _points.GetValueOrDefault(customerId);

    private void Award(Order order)
    {
        var points = (int)(order.Total / 10m);
        _points[order.CustomerId] = PointsFor(order.CustomerId) + points;
        _events.RaisePointsAwarded(order, points);
    }
}

public class EmailService
{
    private readonly List<SentEmail> _sent = [];

    public EmailService(ShopEvents events)
    {
        // The order confirmation is sent when points are awarded, because that's
        // the last thing that happens... right?
        events.PointsAwarded += (order, points) =>
            _sent.Add(new SentEmail(order.Email, $"Tack för din order! Du fick {points} poäng."));
        events.PaymentFailed += order =>
            _sent.Add(new SentEmail(order.Email, "Betalningen nekades"));
        events.StockReservationFailed += order =>
            _sent.Add(new SentEmail(order.Email, "Tyvärr, en vara är slut i lager"));
    }

    public IReadOnlyList<SentEmail> Sent => _sent;
}
