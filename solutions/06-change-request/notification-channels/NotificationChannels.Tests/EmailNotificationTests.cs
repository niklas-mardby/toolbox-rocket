namespace NotificationChannels.Tests;

// Existing behaviour. Must stay green throughout. Don't change these tests.
public class EmailNotificationTests : NotificationTestBase
{
    [Fact]
    public void Shipped_sends_an_email_with_tracking_link()
    {
        var service = CreateService(Customer(channels: NotificationChannel.Email));

        service.NotifyOrderShipped("anna", "1001", "https://track.example.com/1001");

        Assert.Equal(
            new[] { ("anna@example.com", "Din order 1001 är skickad", "Hej Anna! Följ paketet här: https://track.example.com/1001") },
            Email.Sent);
    }

    [Fact]
    public void Delivered_sends_an_email()
    {
        var service = CreateService(Customer(channels: NotificationChannel.Email));

        service.NotifyOrderDelivered("anna", "1001");

        Assert.Equal(
            new[] { ("anna@example.com", "Din order 1001 har levererats", "Hej Anna! Ditt paket har kommit fram. Hoppas du blir nöjd!") },
            Email.Sent);
    }

    [Fact]
    public void Unknown_customer_throws()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() => service.NotifyOrderShipped("nobody", "1", "url"));
        Assert.Throws<KeyNotFoundException>(() => service.NotifyOrderDelivered("nobody", "1"));
    }
}
