using SubscriptionReminders;

Console.WriteLine("⏰ Skickar påminnelser...");
var result = Composition.CreateReminderService().SendRenewalReminders();
Console.WriteLine($"✅ {result.Recipients.Count} påminnelser skickade.");
