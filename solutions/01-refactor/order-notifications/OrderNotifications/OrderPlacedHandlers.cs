namespace OrderNotifications;

// OBSERVERS: each one reacts to OrderPlaced. None of them can stop the order.

public class ConfirmationEmailHandler(EmailOutbox outbox)
{
    public void Handle(Order order) => outbox.Send(
        order.CustomerEmail,
        "Tack för din order!",
        $"Din order {order.Id} på {order.Total:0.00} kr är mottagen.");
}

public class LoyaltyPointsHandler(LoyaltyProgram loyalty)
{
    public void Handle(Order order) => loyalty.AddPoints(order.CustomerId, (int)Math.Floor(order.Total / 10m));
}

public class BigOrderAlertHandler(EmailOutbox outbox)
{
    public const decimal Threshold = 1000m;

    public void Handle(Order order)
    {
        if (order.Total < Threshold) return;

        outbox.Send(
            "sales@toolbox-shop.se",
            "Stor order!",
            $"Kund {order.CustomerId} lade en order på {order.Total:0.00} kr.");
    }
}
