namespace EventPingPong;

// "Fully decoupled, event-driven architecture. No service knows about any other service!" – the original author

public class ShopEvents
{
    public event Action<Order>? OrderPlaced;
    public event Action<Order>? StockReserved;
    public event Action<Order>? StockReservationFailed;
    public event Action<Order>? PaymentSucceeded;
    public event Action<Order>? PaymentFailed;
    public event Action<Order>? StockReleased;
    public event Action<Order, Shipment>? ShipmentCreated;
    public event Action<Order, int>? PointsAwarded;

    public void RaiseOrderPlaced(Order o) => OrderPlaced?.Invoke(o);
    public void RaiseStockReserved(Order o) => StockReserved?.Invoke(o);
    public void RaiseStockReservationFailed(Order o) => StockReservationFailed?.Invoke(o);
    public void RaisePaymentSucceeded(Order o) => PaymentSucceeded?.Invoke(o);
    public void RaisePaymentFailed(Order o) => PaymentFailed?.Invoke(o);
    public void RaiseStockReleased(Order o) => StockReleased?.Invoke(o);
    public void RaiseShipmentCreated(Order o, Shipment s) => ShipmentCreated?.Invoke(o, s);
    public void RaisePointsAwarded(Order o, int points) => PointsAwarded?.Invoke(o, points);
}
