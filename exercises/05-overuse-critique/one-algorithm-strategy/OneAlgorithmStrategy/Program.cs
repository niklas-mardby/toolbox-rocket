using OneAlgorithmStrategy;

var totals = new InvoiceCalculator().CalculateTotals(
[
    new InvoiceLine("Hammare", 2, 199m),
    new InvoiceLine("Skruv 100-pack", 3, 59.90m),
]);

Console.WriteLine($"🧾 Netto: {totals.Net} kr");
Console.WriteLine($"   Moms:  {totals.Vat} kr");
Console.WriteLine($"   Totalt: {totals.Gross} kr");
