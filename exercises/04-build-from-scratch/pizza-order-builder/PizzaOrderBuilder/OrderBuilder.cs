namespace PizzaOrderBuilder;

/// <summary>
/// 🛠️ YOUR JOB: build an Order step by step.
///
/// Rules (see the README for details):
///  - At least one pizza.                           -> Build throws InvalidOperationException
///  - Quantity must be at least 1.                  -> AddPizza throws ArgumentOutOfRangeException
///  - Exactly one of DeliverTo / PickUp must be chosen. The last call wins.
///    Neither chosen                                -> Build throws InvalidOperationException
///  - Empty delivery address                        -> DeliverTo throws ArgumentException
///  - Delivery costs PriceList.DeliveryFee, unless the pizzas cost at least
///    PriceList.FreeDeliveryFrom. Pick-up is always free.
///  - At(time) is optional. Without it, RequestedTime is null ("as soon as possible").
/// </summary>
public class OrderBuilder
{
    /// <summary>Add a pizza built inline: .AddPizza(p => p.WithSize(PizzaSize.Large).AddTopping("Skinka"))</summary>
    public OrderBuilder AddPizza(Func<PizzaBuilder, PizzaBuilder> configure, int quantity = 1) =>
        throw new NotImplementedException();

    /// <summary>Add a pizza that was built elsewhere.</summary>
    public OrderBuilder AddPizza(Pizza pizza, int quantity = 1) =>
        throw new NotImplementedException();

    public OrderBuilder DeliverTo(string address) => throw new NotImplementedException();

    public OrderBuilder PickUp() => throw new NotImplementedException();

    public OrderBuilder At(DateTimeOffset requestedTime) => throw new NotImplementedException();

    public Order Build() => throw new NotImplementedException();
}
