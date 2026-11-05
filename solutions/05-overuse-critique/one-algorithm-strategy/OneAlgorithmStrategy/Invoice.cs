namespace OneAlgorithmStrategy;

public record InvoiceLine(string Description, int Quantity, decimal UnitPrice);

public record InvoiceTotals(decimal Net, decimal Vat, decimal Gross);
