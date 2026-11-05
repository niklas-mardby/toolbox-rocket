using System.IO.Compression;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

// ---------------------------------------------------------------
// Part 1: Write text, buffered and compressed, into memory
// ---------------------------------------------------------------
var storage = new MemoryStream();

using (var gzip = new GZipStream(storage, CompressionLevel.Optimal, leaveOpen: true))
using (var buffered = new BufferedStream(gzip, bufferSize: 4096))
using (var writer = new StreamWriter(buffered, Encoding.UTF8))
{
    for (var i = 1; i <= 1000; i++)
        writer.WriteLine($"Rad {i}: Toolbox Rocket 🧰🚀 lär dig design patterns!");
}

var uncompressedSize = Encoding.UTF8.GetByteCount(
    string.Concat(Enumerable.Range(1, 1000).Select(i => $"Rad {i}: Toolbox Rocket 🧰🚀 lär dig design patterns!{Environment.NewLine}")));

Console.WriteLine("✍️  Del 1: skriva");
Console.WriteLine($"   Text i bytes:         {uncompressedSize,8}");
Console.WriteLine($"   Lagrat (komprimerat): {storage.Length,8}");
Console.WriteLine();

// ---------------------------------------------------------------
// Part 2: Read it back
// ---------------------------------------------------------------
storage.Position = 0;
using (var gunzip = new GZipStream(storage, CompressionMode.Decompress, leaveOpen: true))
using (var reader = new StreamReader(gunzip, Encoding.UTF8))
{
    Console.WriteLine("📖 Del 2: läsa");
    for (var i = 0; i < 3; i++)
        Console.WriteLine($"   {reader.ReadLine()}");

    var remaining = 0;
    while (reader.ReadLine() is not null) remaining++;
    Console.WriteLine($"   ... och {remaining} rader till");
}
Console.WriteLine();

// ---------------------------------------------------------------
// Part 3: Code that only knows about Stream
// ---------------------------------------------------------------
Console.WriteLine("🔢 Del 3: samma metod, olika streams");

storage.Position = 0;
Console.WriteLine($"   MemoryStream:           {CountBytes(storage),8} bytes");

storage.Position = 0;
using (var gunzip = new GZipStream(storage, CompressionMode.Decompress, leaveOpen: true))
    Console.WriteLine($"   GZipStream (dekomp.):   {CountBytes(gunzip),8} bytes");

storage.Position = 0;
using (var gunzip = new GZipStream(storage, CompressionMode.Decompress, leaveOpen: true))
using (var buffered = new BufferedStream(gunzip))
    Console.WriteLine($"   Buffered + GZipStream:  {CountBytes(buffered),8} bytes");

// This method has no idea what kind of stream it gets.
static long CountBytes(Stream stream)
{
    var buffer = new byte[1024];
    long total = 0;
    int read;
    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        total += read;
    return total;
}
