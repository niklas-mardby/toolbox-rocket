namespace PizzaOrderBuilder.Tests;

public class OrderBuilderTests
{
    private static readonly Pizza Medium = new(PizzaSize.Medium, Crust.Classic, [], false); // 109 kr

    [Fact(Skip = Todo.Skip)]
    public void Builds_an_order_with_inline_and_prebuilt_pizzas()
    {
        var order = new OrderBuilder()
            .AddPizza(p => p.WithSize(PizzaSize.Large).AddTopping("Skinka"))   // 149
            .AddPizza(Medium, quantity: 2)                                       // 218
            .PickUp()
            .Build();

        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(149m, order.Lines[0].Pizza.Price);
        Assert.Equal(2, order.Lines[1].Quantity);
        Assert.Equal(367m, order.Total);
    }

    [Fact(Skip = Todo.Skip)]
    public void Order_needs_at_least_one_pizza()
    {
        Assert.Throws<InvalidOperationException>(() => new OrderBuilder().PickUp().Build());
    }

    [Theory(Skip = Todo.Skip)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quantity_must_be_positive(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderBuilder().AddPizza(Medium, quantity));
    }

    [Fact(Skip = Todo.Skip)]
    public void Must_choose_delivery_or_pick_up()
    {
        Assert.Throws<InvalidOperationException>(() => new OrderBuilder().AddPizza(Medium).Build());
    }

    [Fact(Skip = Todo.Skip)]
    public void Delivery_under_300_costs_49()
    {
        var order = new OrderBuilder().AddPizza(Medium).DeliverTo("Storgatan 1").Build();

        Assert.True(order.IsDelivery);
        Assert.Equal("Storgatan 1", order.DeliveryAddress);
        Assert.Equal(49m, order.DeliveryFee);
        Assert.Equal(158m, order.Total);
    }

    [Fact(Skip = Todo.Skip)]
    public void Delivery_from_300_is_free()
    {
        var order = new OrderBuilder().AddPizza(Medium, 3).DeliverTo("Storgatan 1").Build(); // 327

        Assert.Equal(0m, order.DeliveryFee);
    }

    [Fact(Skip = Todo.Skip)]
    public void Pick_up_is_free()
    {
        var order = new OrderBuilder().AddPizza(Medium).PickUp().Build();

        Assert.False(order.IsDelivery);
        Assert.Null(order.DeliveryAddress);
        Assert.Equal(0m, order.DeliveryFee);
    }

    [Fact(Skip = Todo.Skip)]
    public void The_last_of_DeliverTo_and_PickUp_wins()
    {
        var pickUp = new OrderBuilder().AddPizza(Medium).DeliverTo("Storgatan 1").PickUp().Build();
        var delivery = new OrderBuilder().AddPizza(Medium).PickUp().DeliverTo("Storgatan 1").Build();

        Assert.False(pickUp.IsDelivery);
        Assert.True(delivery.IsDelivery);
    }

    [Theory(Skip = Todo.Skip)]
    [InlineData("")]
    [InlineData("  ")]
    public void Empty_address_is_rejected(string address)
    {
        Assert.Throws<ArgumentException>(() => new OrderBuilder().DeliverTo(address));
    }

    [Fact(Skip = Todo.Skip)]
    public void Requested_time_is_optional()
    {
        var time = new DateTimeOffset(2026, 11, 5, 18, 30, 0, TimeSpan.FromHours(1));

        var asap = new OrderBuilder().AddPizza(Medium).PickUp().Build();
        var later = new OrderBuilder().AddPizza(Medium).PickUp().At(time).Build();

        Assert.Null(asap.RequestedTime);
        Assert.Equal(time, later.RequestedTime);
    }

    [Fact(Skip = Todo.Skip)]
    public void Invalid_inline_pizza_fails_when_added()
    {
        Assert.Throws<InvalidOperationException>(() => new OrderBuilder().AddPizza(p => p.AddTopping("Skinka")));
    }
}
