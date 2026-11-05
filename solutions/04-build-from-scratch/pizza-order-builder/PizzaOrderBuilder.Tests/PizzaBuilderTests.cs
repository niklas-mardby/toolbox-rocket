namespace PizzaOrderBuilder.Tests;

public class PizzaBuilderTests
{
    [Fact(Skip = Todo.Skip)]
    public void Builds_a_pizza_with_the_chosen_parts()
    {
        var pizza = new PizzaBuilder()
            .WithSize(PizzaSize.Large)
            .WithCrust(Crust.Thin)
            .AddTopping("Skinka")
            .AddTopping("Ananas")
            .WithExtraCheese()
            .Build();

        Assert.Equal(PizzaSize.Large, pizza.Size);
        Assert.Equal(Crust.Thin, pizza.Crust);
        Assert.Equal(new[] { "Skinka", "Ananas" }, pizza.Toppings);
        Assert.True(pizza.ExtraCheese);
        Assert.Equal(139m + 20m + 15m, pizza.Price);
    }

    [Fact(Skip = Todo.Skip)]
    public void Crust_defaults_to_classic_and_no_extra_cheese()
    {
        var pizza = new PizzaBuilder().WithSize(PizzaSize.Small).Build();

        Assert.Equal(Crust.Classic, pizza.Crust);
        Assert.False(pizza.ExtraCheese);
        Assert.Empty(pizza.Toppings);
        Assert.Equal(89m, pizza.Price);
    }

    [Fact(Skip = Todo.Skip)]
    public void Size_is_required()
    {
        var builder = new PizzaBuilder().AddTopping("Skinka");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact(Skip = Todo.Skip)]
    public void At_most_six_toppings()
    {
        var builder = new PizzaBuilder().WithSize(PizzaSize.Medium);
        for (var i = 1; i <= 7; i++) builder.AddTopping($"Topping {i}");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact(Skip = Todo.Skip)]
    public void Six_toppings_is_fine()
    {
        var builder = new PizzaBuilder().WithSize(PizzaSize.Medium);
        for (var i = 1; i <= 6; i++) builder.AddTopping($"Topping {i}");

        Assert.Equal(6, builder.Build().Toppings.Count);
    }

    [Fact(Skip = Todo.Skip)]
    public void Gluten_free_is_not_available_in_small()
    {
        var builder = new PizzaBuilder().WithSize(PizzaSize.Small).WithCrust(Crust.GlutenFree);

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Theory(Skip = Todo.Skip)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_toppings_are_rejected(string topping)
    {
        Assert.Throws<ArgumentException>(() => new PizzaBuilder().AddTopping(topping));
    }

    [Fact(Skip = Todo.Skip)]
    public void Toppings_are_trimmed_and_may_repeat()
    {
        var pizza = new PizzaBuilder().WithSize(PizzaSize.Medium).AddTopping("  Ost ").AddTopping("Ost").Build();

        Assert.Equal(new[] { "Ost", "Ost" }, pizza.Toppings);
    }

    [Fact(Skip = Todo.Skip)]
    public void Changing_the_builder_after_Build_does_not_change_the_built_pizza()
    {
        var builder = new PizzaBuilder().WithSize(PizzaSize.Medium).AddTopping("Skinka");
        var first = builder.Build();

        builder.AddTopping("Ananas").WithSize(PizzaSize.Large);
        var second = builder.Build();

        Assert.Equal(new[] { "Skinka" }, first.Toppings);
        Assert.Equal(PizzaSize.Medium, first.Size);
        Assert.Equal(new[] { "Skinka", "Ananas" }, second.Toppings);
    }

    [Fact(Skip = Todo.Skip)]
    public void Menu_pizzas_come_with_toppings_but_need_a_size()
    {
        Assert.Equal(new[] { "Mozzarella", "Basilika" }, PizzaMenu.Margherita().WithSize(PizzaSize.Small).Build().Toppings);
        Assert.Equal(new[] { "Mozzarella", "Skinka" }, PizzaMenu.Vesuvio().WithSize(PizzaSize.Small).Build().Toppings);
        Assert.Equal(new[] { "Mozzarella", "Skinka", "Champinjoner" }, PizzaMenu.Capricciosa().WithSize(PizzaSize.Small).Build().Toppings);
        Assert.Throws<InvalidOperationException>(() => PizzaMenu.Margherita().Build());
    }

    [Fact(Skip = Todo.Skip)]
    public void Each_menu_call_gives_a_fresh_builder()
    {
        PizzaMenu.Vesuvio().AddTopping("Ananas");

        var pizza = PizzaMenu.Vesuvio().WithSize(PizzaSize.Small).Build();

        Assert.DoesNotContain("Ananas", pizza.Toppings);
    }
}
