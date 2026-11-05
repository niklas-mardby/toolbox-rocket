using FactoryOfFactories;

var outbox = new Outbox();
var signups = new SignupService(outbox);

signups.Register("anna@example.com", "Anna");
signups.Register("bo@example.com", "Bo");

foreach (var message in outbox.Messages)
    Console.WriteLine($"📧 {message.To}: {message.Subject}\n   {message.Body}");
