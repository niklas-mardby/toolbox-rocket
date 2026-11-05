namespace NotificationChannels;

/// <summary>
/// The composition root. The tests only use this method, so you may change how the service is built.
/// SMS and push gateways are passed in because the app already has them (for the marketing module).
/// </summary>
public static class Composition
{
    public static NotificationService CreateNotificationService(
        ICustomerDirectory customers, IEmailGateway email, ISmsGateway sms, IPushGateway push) =>
        new(customers, email);
}
