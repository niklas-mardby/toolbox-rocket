namespace PizzaOrderBuilder;

// ✅ These types are finished. You don't need to change them.

public enum PizzaSize { Small, Medium, Large }

public enum Crust { Classic, Thin, GlutenFree }

/// <summary>The finished product of PizzaBuilder. Immutable.</summary>
public record Pizza(PizzaSize Size, Crust Crust, IReadOnlyList<string> Toppings, bool ExtraCheese)
{
    public decimal Price => PriceList.For(this);
}

public record OrderLine(Pizza Pizza, int Quantity)
{
    public decimal LineTotal => Pizza.Price * Quantity;
}

/// <summary>The finished product of OrderBuilder. DeliveryAddress is null for pick-up orders.</summary>
public record Order(IReadOnlyList<OrderLine> Lines, string? DeliveryAddress, DateTimeOffset? RequestedTime, decimal DeliveryFee)
{
    public bool IsDelivery => DeliveryAddress is not null;
    public decimal PizzaTotal => Lines.Sum(l => l.LineTotal);
    public decimal Total => PizzaTotal + DeliveryFee;
}

public static class PriceList
{
    public const decimal ToppingPrice = 10m;
    public const decimal ExtraCheesePrice = 15m;
    public const decimal GlutenFreeSurcharge = 20m;
    public const decimal DeliveryFee = 49m;
    public const decimal FreeDeliveryFrom = 300m;

    public static decimal BasePrice(PizzaSize size) => size switch
    {
        PizzaSize.Small => 89m,
        PizzaSize.Medium => 109m,
        PizzaSize.Large => 139m,
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };

    public static decimal For(Pizza pizza) =>
        BasePrice(pizza.Size)
        + pizza.Toppings.Count * ToppingPrice
        + (pizza.ExtraCheese ? ExtraCheesePrice : 0m)
        + (pizza.Crust == Crust.GlutenFree ? GlutenFreeSurcharge : 0m);
}
