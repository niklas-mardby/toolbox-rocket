# 📝 NOTES: Mystery codebase

Den fullständiga listan över mönster, med roller och bevis, finns på main i [`answers/02-spot-the-pattern/mystery-codebase.md`](../../../answers/02-spot-the-pattern/mystery-codebase.md).

## 💡 Resonemanget

- **Börja i `Program.cs`.** Där kopplas allt ihop, och stapeln `Diary(Doorman(SeatMap(...)))` och `boxOffice.Sold +=` avslöjar Decorator och Observer direkt.
- **Leta efter "samma interface in och ut"** (`ISeatMap`, `IShowCatalog`): Decorator eller Proxy. Skillnaden: Proxy styr *åtkomst* (eller tidpunkt), Decorator *lägger till beteende*.
- **`PriceDecorator` är fällan.** En statisk formatterare utan interface och utan inre objekt.

## ➕ Bonusen

- **Gruppbiljett:** kräver en ändring av `TariffDesk.Pick`, eftersom fabriken saknar information om sällskapets storlek. Ett bra exempel på att Strategy gör *nya algoritmer* lätta, men inte nödvändigtvis *ny indata*.
- **`TicketLost`:** en record, en `IOperator<TicketLost>` och en rad i `Program.cs`. Inga befintliga klasser ändras.

## 🔀 Andra giltiga svar

- **`OnDemandShowCatalog`** kan också beskrivas med Fowlers **Lazy Load**.
- **`Switchboard`** kan också beskrivas som **Command**, eftersom anropen är objekt. Chain of Responsibility passar däremot inte: växeln väljer exakt *en* operatör och skickar inte vidare i en kedja. Mediator är den bästa beskrivningen.
- **`Printout`** med Template Method kan också ses som **Strategy** via arv. Den klassiska skillnaden är att Template Method använder *arv* och Strategy *komposition*.
