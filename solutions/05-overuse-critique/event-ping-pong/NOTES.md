# 📝 NOTES: Event ping-pong

## 💡 Resonemanget

- **Kassan är ett *flöde*, inte en uppsättning reaktioner.** Varje steg kan stoppa processen (slut i lager, nekad betalning), och ordningen är en affärsregel. Det är precis när Observer *inte* passar: subjectet bryr sig om resultatet.
- **`CheckoutService` orkestrerar nu explicit.** Hela flödet, inklusive felvägarna, står på ett ställe och läses uppifrån och ned. Förut var det utspritt över sex klasser och åtta events, och resultatet lästes ut från ett `Status`-fält som "någon" förhoppningsvis hade satt.
- **Tjänsterna returnerar resultat** (`TryReserve`, `TryCharge`, `Award` returnerar poängen) i stället för att höja nya events. De känner fortfarande inte till varandra. Det är `CheckoutService` som kopplar ihop dem. Frikopplingen är alltså kvar, men den är synlig.
- **Den dolda kopplingen är borta.** Förut skickades orderbekräftelsen när *poäng delades ut*. Om lojalitetsprogrammet hade stängts av skulle kunderna sluta få orderbekräftelser, utan att någon märkte det. Nu är det explicit: först poäng, sedan mejl med poängen.
- **Ordningen i `Composition.cs` spelar ingen roll längre.** Förut berodde det på prenumerationsordningen om mejlet skickades före eller efter att lagret släpptes.
- **`ShopEvents.cs` är borttagen.**

## ⚖️ När events *är* rätt här

Events passar för **sidoeffekter som inte påverkar flödet**, och som någon annan vill kunna lägga till utan att ändra kassan. Till exempel: statistik, analys, notiser till säljteamet och uppdatering av ett sökindex. En rimlig design är att `CheckoutService` höjer *ett* event, `OrderCompleted`, i slutet, och att sådana lyssnare prenumererar på det. Ett event, inga kedjor.

## 🔀 Alternativ

- **Saga / process manager**: i ett distribuerat system (lager, betalning och leverans som *separata tjänster*) går det inte att anropa allt direkt. Då behövs meddelanden, men koordinerade av en process manager som håller flödet på ett ställe, inte en kedja av fria events.
- **Mediator med notifications**: samma problem som C#-events om notifications triggar nya notifications.
