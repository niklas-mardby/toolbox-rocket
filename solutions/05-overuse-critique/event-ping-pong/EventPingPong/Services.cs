namespace EventPingPong;

/// <summary>
/// The checkout FLOW, written as a flow. Each step can stop the process,
/// and every step and its order is visible right here.
/// </summary>
public class CheckoutService(
    InventoryService inventory,
    PaymentService payments,
    ShippingService shipping,
    LoyaltyService loyalty,
    EmailService emails)
{
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
}

// The services now just DO their job and report back. None of them knows about the others.

public class InventoryService(Dictionary<string, int> initialStock)
{
    private readonly Dictionary<string, int> _stock = new(initialStock);

    public int StockOf(string sku) => _stock.GetValueOrDefault(sku);

    public bool TryReserve(Order order)
    {
        if (order.Lines.Any(l => StockOf(l.Sku) < l.Quantity))
            return false;

        foreach (var line in order.Lines)
            _stock[line.Sku] -= line.Quantity;
        return true;
    }

    public void Release(Order order)
    {
        foreach (var line in order.Lines)
            _stock[line.Sku] += line.Quantity;
    }
}

public class PaymentService
{
    private readonly List<Charge> _charges = [];
    public IReadOnlyList<Charge> Charges => _charges;

    public bool TryCharge(Order order)
    {
        if (order.CardNumber.EndsWith("0002"))   // test card: always declined
            return false;

        _charges.Add(new Charge(order.Id, order.Total));
        return true;
    }
}

public class ShippingService(Dictionary<string, string> addressByCustomer)
{
    private readonly List<Shipment> _shipments = [];
    public IReadOnlyList<Shipment> Shipments => _shipments;

    public Shipment Ship(Order order)
    {
        var shipment = new Shipment(order.Id, addressByCustomer.GetValueOrDefault(order.CustomerId, "Hämtas i butik"));
        _shipments.Add(shipment);
        return shipment;
    }
}

public class LoyaltyService
{
    private readonly Dictionary<string, int> _points = [];

    public int PointsFor(string customerId) => _points.GetValueOrDefault(customerId);

    public int Award(Order order)
    {
        var points = (int)(order.Total / 10m);
        _points[order.CustomerId] = PointsFor(order.CustomerId) + points;
        return points;
    }
}

public class EmailService
{
    private readonly List<SentEmail> _sent = [];
    public IReadOnlyList<SentEmail> Sent => _sent;

    public void Send(string to, string subject) => _sent.Add(new SentEmail(to, subject));
}
