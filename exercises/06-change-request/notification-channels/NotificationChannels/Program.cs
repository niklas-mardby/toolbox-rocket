using NotificationChannels;

var customers = new InMemoryCustomerDirectory(
    new Customer("anna", "Anna", "anna@example.com", "0701234567", null, [NotificationChannel.Sms]),
    new Customer("bo", "Bo", "bo@example.com", null, "device-bo-1", [NotificationChannel.Email, NotificationChannel.Push]));

var service = Composition.CreateNotificationService(
    customers, new ConsoleEmailGateway(), new ConsoleSmsGateway(), new ConsolePushGateway());

service.NotifyOrderShipped("anna", "1001", "https://track.example.com/1001");
service.NotifyOrderShipped("bo", "1002", "https://track.example.com/1002");
service.NotifyOrderDelivered("bo", "1002");
