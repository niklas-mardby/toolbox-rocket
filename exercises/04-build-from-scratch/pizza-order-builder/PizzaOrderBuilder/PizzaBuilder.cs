namespace PizzaOrderBuilder;

/// <summary>
/// 🛠️ YOUR JOB: build a Pizza step by step. Every With/Add method returns the builder,
/// so calls can be chained. Build() validates and returns an immutable Pizza.
///
/// Rules (see the README for details):
///  - A size is required.                        -> Build throws InvalidOperationException
///  - Crust defaults to Classic.
///  - At most 6 toppings.                         -> Build throws InvalidOperationException
///  - GlutenFree crust is not available in Small. -> Build throws InvalidOperationException
///  - Empty or whitespace toppings are rejected.  -> AddTopping throws ArgumentException
///  - Toppings are trimmed. The same topping may be added twice (double cheese!).
///  - Build can be called many times. Changing the builder afterwards must not
///    change pizzas that were already built.
/// </summary>
public class PizzaBuilder
{
    public PizzaBuilder WithSize(PizzaSize size) => throw new NotImplementedException();

    public PizzaBuilder WithCrust(Crust crust) => throw new NotImplementedException();

    public PizzaBuilder AddTopping(string topping) => throw new NotImplementedException();

    public PizzaBuilder WithExtraCheese() => throw new NotImplementedException();

    public Pizza Build() => throw new NotImplementedException();
}

/// <summary>
/// 🛠️ YOUR JOB: ready-made starting points ("director" style). Each method returns
/// a PizzaBuilder with the toppings already added. The customer still picks the size.
///   Margherita:  Mozzarella, Basilika
///   Vesuvio:     Mozzarella, Skinka
///   Capricciosa: Mozzarella, Skinka, Champinjoner
/// </summary>
public static class PizzaMenu
{
    public static PizzaBuilder Margherita() => throw new NotImplementedException();
    public static PizzaBuilder Vesuvio() => throw new NotImplementedException();
    public static PizzaBuilder Capricciosa() => throw new NotImplementedException();
}
