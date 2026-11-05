namespace PizzaOrderBuilder;

/// <summary>Builds an Order step by step.</summary>
public class OrderBuilder
{
    private readonly List<OrderLine> _lines = [];
    private bool _fulfilmentChosen;
    private string? _deliveryAddress;
    private DateTimeOffset? _requestedTime;

    public OrderBuilder AddPizza(Func<PizzaBuilder, PizzaBuilder> configure, int quantity = 1) =>
        AddPizza(configure(new PizzaBuilder()).Build(), quantity);

    public OrderBuilder AddPizza(Pizza pizza, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        _lines.Add(new OrderLine(pizza, quantity));
        return this;
    }

    public OrderBuilder DeliverTo(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Ange en leveransadress.", nameof(address));

        _fulfilmentChosen = true;
        _deliveryAddress = address.Trim();
        return this;
    }

    public OrderBuilder PickUp()
    {
        _fulfilmentChosen = true;
        _deliveryAddress = null;
        return this;
    }

    public OrderBuilder At(DateTimeOffset requestedTime)
    {
        _requestedTime = requestedTime;
        return this;
    }

    public Order Build()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("En beställning måste innehålla minst en pizza.");
        if (!_fulfilmentChosen)
            throw new InvalidOperationException("Välj leverans eller avhämtning.");

        var pizzaTotal = _lines.Sum(l => l.LineTotal);
        var fee = _deliveryAddress is not null && pizzaTotal < PriceList.FreeDeliveryFrom
            ? PriceList.DeliveryFee
            : 0m;

        return new Order(_lines.ToList(), _deliveryAddress, _requestedTime, fee);
    }
}
