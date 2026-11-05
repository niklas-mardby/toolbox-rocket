namespace PizzaOrderBuilder;

/// <summary>Builds a Pizza step by step and validates the whole in Build().</summary>
public class PizzaBuilder
{
    public const int MaxToppings = 6;

    private PizzaSize? _size;
    private Crust _crust = Crust.Classic;
    private readonly List<string> _toppings = [];
    private bool _extraCheese;

    public PizzaBuilder WithSize(PizzaSize size)
    {
        _size = size;
        return this;
    }

    public PizzaBuilder WithCrust(Crust crust)
    {
        _crust = crust;
        return this;
    }

    public PizzaBuilder AddTopping(string topping)
    {
        if (string.IsNullOrWhiteSpace(topping))
            throw new ArgumentException("En topping måste ha ett namn.", nameof(topping));

        _toppings.Add(topping.Trim());
        return this;
    }

    public PizzaBuilder WithExtraCheese()
    {
        _extraCheese = true;
        return this;
    }

    public Pizza Build()
    {
        // Rules that concern the WHOLE pizza are checked here, when all choices are known.
        if (_size is null)
            throw new InvalidOperationException("Välj en storlek.");
        if (_toppings.Count > MaxToppings)
            throw new InvalidOperationException($"Högst {MaxToppings} toppings.");
        if (_crust == Crust.GlutenFree && _size == PizzaSize.Small)
            throw new InvalidOperationException("Glutenfri botten finns inte i storlek Small.");

        // ToList() makes a COPY, so later changes to the builder don't leak into this pizza.
        return new Pizza(_size.Value, _crust, _toppings.ToList(), _extraCheese);
    }
}

/// <summary>Director-style starting points. A new builder every call.</summary>
public static class PizzaMenu
{
    public static PizzaBuilder Margherita() => new PizzaBuilder().AddTopping("Mozzarella").AddTopping("Basilika");

    public static PizzaBuilder Vesuvio() => new PizzaBuilder().AddTopping("Mozzarella").AddTopping("Skinka");

    public static PizzaBuilder Capricciosa() => Vesuvio().AddTopping("Champinjoner");
}
