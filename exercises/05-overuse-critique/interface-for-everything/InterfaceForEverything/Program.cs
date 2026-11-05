using InterfaceForEverything;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine(Composition.CreateReportService(TimeProvider.System).BuildReport());
