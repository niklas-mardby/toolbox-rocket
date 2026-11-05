using MysteryCodebase;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// --- Wiring ---
IShowCatalog catalog = new OnDemandShowCatalog(() =>
{
    var c = new ShowCatalog();
    c.Register(new Show("HAM", "Hamlet", new DateOnly(2026, 3, 10), 350m, Rows: 10, SeatsPerRow: 12));
    c.Register(new Show("NUT", "Nötknäpparen", new DateOnly(2026, 12, 12), 420m, Rows: 15, SeatsPerRow: 20));
    c.Register(new Show("IMP", "Improkväll", new DateOnly(2026, 3, 13), 150m, Rows: 4, SeatsPerRow: 8));
    return c;
});

var diaryPages = new List<string>();
ISeatMap seats = new Diary(new Doorman(new SeatMap(catalog), maxPerHold: 4), diaryPages);

var boxOffice = new BoxOffice(seats);
var scoreboard = new Scoreboard();
IMessenger messenger = new PagerLink(new TelePager3000());

boxOffice.Sold += scoreboard.Count;
boxOffice.Sold += t => messenger.Send(t.Visitor.Phone, $"Tack {t.Visitor.Name}! {t.Show.Title} {t.Show.Date:d/M}, {string.Join(", ", t.Seats)}.");

var switchboard = new Switchboard();
switchboard.Connect(new RefundOperator());
switchboard.Connect(new RescheduleOperator(catalog));

Console.WriteLine("🎭 Välkommen till Toolbox-teatern\n");

// --- Selling tickets ---
var anna = new Visitor("Anna", 34, HasStudentCard: false, Phone: "0701234567");
var bo = new Visitor("Bo", 9, HasStudentCard: false, Phone: "0709876543");
var cilla = new Visitor("Cilla", 22, HasStudentCard: true, Phone: "+46705550000");
var doris = new Visitor("Doris", 71, HasStudentCard: false, Phone: "0731112233");

var tickets = new[]
{
    new TicketDraft().For(catalog.ByCode("HAM")!).Visitor(anna).Seat(3, 5).Seat(3, 6).WithProgramme().Finish(),
    new TicketDraft().For(catalog.ByCode("HAM")!).Visitor(bo).Seat(3, 6).Finish(),
    new TicketDraft().For(catalog.ByCode("IMP")!).Visitor(cilla).Seat(1, 1).Finish(),
    new TicketDraft().For(catalog.ByCode("HAM")!).Visitor(doris).Seat(1, 1).Seat(1, 2).Seat(1, 3).Seat(1, 4).Seat(1, 5).Finish(),
    new TicketDraft().For(catalog.ByCode("NUT")!).Visitor(doris).Seat(7, 10).Finish(),
};

foreach (var ticket in tickets)
{
    var sold = boxOffice.Sell(ticket);
    Console.WriteLine($"🎟️ {ticket.Visitor.Name,-6} {ticket.Show.Title,-13} {ticket.TariffLabel,-10} {PriceDecorator.Pretty(ticket.Total),-12} {(sold ? "✅ såld" : "❌ ej såld")}");
}

Console.WriteLine($"\n🪑 Lediga platser på Hamlet: {seats.Free("HAM")}\n");

// --- Customer service ---
Console.WriteLine(switchboard.Dial(new RefundWanted("IMP", "Cilla")));
Console.WriteLine(switchboard.Dial(new ShowMoved("HAM", new DateOnly(2026, 3, 11))));
Console.WriteLine(switchboard.Dial("Hallå?"));
Console.WriteLine();

// --- Reports ---
new TopListPrintout(scoreboard).Print();
new DiaryPrintout(diaryPages).Print();
