namespace NotificationChannels.Tests;

// ✨ The new requirement. Switch these on with Todo.Skip = null. Don't change these tests.
public class ChannelAcceptanceTests : NotificationTestBase
{
    private const string Url = "https://track.example.com/1001";

    [Fact(Skip = Todo.Skip)]
    public void Sms_only_customers_get_an_sms_and_no_email()
    {
        var service = CreateService(Customer(channels: NotificationChannel.Sms));

        service.NotifyOrderShipped("anna", "1001", Url);

        Assert.Empty(Email.Sent);
        Assert.Equal(new[] { ("0701234567", $"Toolbox: Din order 1001 är skickad. Spåra: {Url}") }, Sms.Sent);
    }

    [Fact(Skip = Todo.Skip)]
    public void Push_uses_the_subject_as_title_and_the_short_text_as_body()
    {
        var service = CreateService(Customer(channels: NotificationChannel.Push));

        service.NotifyOrderDelivered("anna", "1001");

        Assert.Equal(new[] { ("device-1", "Din order 1001 har levererats", "Din order 1001 har levererats.") }, Push.Sent);
    }

    [Fact(Skip = Todo.Skip)]
    public void Customers_can_choose_several_channels()
    {
        var service = CreateService(Customer(channels: [NotificationChannel.Email, NotificationChannel.Sms, NotificationChannel.Push]));

        service.NotifyOrderShipped("anna", "1001", Url);

        Assert.Single(Email.Sent);
        Assert.Single(Sms.Sent);
        Assert.Single(Push.Sent);
    }

    [Fact(Skip = Todo.Skip)]
    public void No_preferences_means_email()
    {
        var service = CreateService(Customer(channels: []));

        service.NotifyOrderDelivered("anna", "1001");

        Assert.Single(Email.Sent);
        Assert.Empty(Sms.Sent);
        Assert.Empty(Push.Sent);
    }

    [Fact(Skip = Todo.Skip)]
    public void Channels_without_contact_details_are_skipped()
    {
        var service = CreateService(Customer(phone: null, channels: [NotificationChannel.Sms, NotificationChannel.Push]));

        service.NotifyOrderShipped("anna", "1001", Url);

        Assert.Empty(Sms.Sent);
        Assert.Single(Push.Sent);
        Assert.Empty(Email.Sent);
    }

    [Fact(Skip = Todo.Skip)]
    public void If_no_preferred_channel_can_reach_the_customer_email_is_the_fallback()
    {
        var service = CreateService(Customer(phone: null, pushToken: null, channels: [NotificationChannel.Sms, NotificationChannel.Push]));

        service.NotifyOrderShipped("anna", "1001", Url);

        Assert.Equal("Din order 1001 är skickad", Email.Sent.Single().Subject);
    }

    [Fact(Skip = Todo.Skip)]
    public void Delivered_sms_has_its_own_short_text()
    {
        var service = CreateService(Customer(channels: NotificationChannel.Sms));

        service.NotifyOrderDelivered("anna", "1001");

        Assert.Equal("Toolbox: Din order 1001 har levererats.", Sms.Sent.Single().Text);
    }
}
