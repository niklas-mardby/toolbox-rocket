namespace NotificationChannels;

public static class Composition
{
    public static NotificationService CreateNotificationService(
        ICustomerDirectory customers, IEmailGateway email, ISmsGateway sms, IPushGateway push) =>
        new(customers, [new EmailChannel(email), new SmsChannel(sms), new PushChannel(push)]);
}
