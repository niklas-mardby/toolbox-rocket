# 🛑 Pattern or plain code?

> 🧩 **Mönster:** kanske inget! Svaret är ofta "nej", ibland "ja" och ibland "det beror på".
> 📏 **Omfattning:** åtta scenarier. Varje scenario är kort, men kräver att du vågar säga nej och kan motivera var gränsen går.
> 🧪 **Ingen kod.** Skriv dina svar i en egen fil.

## 🎯 Mål

Du ska träna på den kanske viktigaste färdigheten med design patterns: att avgöra **när ett mönster inte behövs**.

## 📖 Bakgrund

När du just har lärt dig ett mönster är det frestande att se det överallt. Men varje mönster kostar: fler klasser, fler filer, mer indirektion och mer att förstå för nästa utvecklare. Ett mönster ska *betala* för sig genom att lösa ett problem som faktiskt finns.

> 📚 **YAGNI** (*You Aren't Gonna Need It*): bygg inte för en framtid som kanske aldrig kommer.

## 🛠️ Uppgift

Svara för varje scenario:

1. ✅ / ❌ **Behövs ett mönster?** Ja, nej, eller "inte än".
2. 💬 **Motivering:** Varför, eller varför inte?
3. 🚦 **Vad skulle få dig att ändra dig?** Vilken förändring i förutsättningarna skulle göra att du väljer annorlunda?

---

### 💸 Scenario 1: Två rabatter

Ett bokningssystem har två sorters rabatt: 10 % för studenter och 20 % för pensionärer. Reglerna har inte ändrats på fem år, och verksamheten har inga planer på fler. Koden är en `if/else` på fyra rader i en metod. En kollega vill göra om den till Strategy "för att följa open/closed principle".

### 🧰 Scenario 2: Det interna verktyget

Ett internt administrationsverktyg har tre endpoints: lista, skapa och ta bort användare. Det används av fem personer på ekonomiavdelningen. En utvecklare föreslår att införa Repository, Unit of Work, CQRS med en mediator och pipeline behaviors "så att det blir rätt från början".

### 📅 Scenario 3: Datumformatet

Datum visas på ett ställe i appen, i formatet "5 november 2026". En kollega föreslår ett `IDateFormatter`-interface med en `SwedishDateFormatter`-implementation, registrerad i DI-containern.

### 📧 Scenario 4: Mejltjänsten

`OrderService` skickar mejl via en `SmtpEmailSender`-klass. Det finns bara en implementation, och ni planerar inte att byta. En kollega säger: "Ett `IEmailSender`-interface är onödigt eftersom det bara finns en implementation."

### 📇 Scenario 5: Kundposten

`Customer` har fyra properties: `Name`, `Email`, `Phone` och `CustomerNumber`. Alla är obligatoriska. En utvecklare har skrivit en `CustomerBuilder` med `WithName()`, `WithEmail()`, `WithPhone()`, `WithCustomerNumber()` och `Build()`.

### 📝 Scenario 6: Loggningen

En metod i `ImportService` ska logga när importen startar och när den är klar. Det finns inga andra planer på loggning i den klassen. En kollega föreslår en `LoggingImportServiceDecorator`.

### 🔄 Scenario 7: Databasbytet

Teamet använder SQL Server och EF Core. En arkitekt vill att all dataåtkomst ska gå via ett generiskt `IRepository<T>` "så att vi kan byta till MongoDB om vi vill". Ingen har nämnt MongoDB tidigare, och det finns inga planer på det.

### 🧾 Scenario 8: Fakturaexporten

Ekonomisystemet exporterar fakturor till CSV. Förra månaden lade någon till XML för en ny kund, och i förrgår kom ett krav på JSON för en tredje kund. Koden är en `switch` på formatnamnet, och varje `case` är 40 rader lång. Två av formaten har en bugg i avrundningen, men det tredje inte.

---

## 💡 Hints

<details>
<summary>💡 Hint 1: Fråga efter smärtan</summary>

Fråga för varje scenario: **Vilket problem har vi idag?** Inte vilket problem vi *kanske* får. Om du inte kan peka på ett konkret problem, som duplicering, buggar, otestbar kod eller kod som ändras ofta, är det troligen för tidigt för ett mönster.
</details>

<details>
<summary>💡 Hint 2: Interfaces för testning</summary>

Ett interface kan vara motiverat även med en enda implementation i produktionskoden. Fundera på vad som händer i ett test av `OrderService` i scenario 4. Vill du skicka riktiga mejl?
</details>

<details>
<summary>💡 Hint 3: Rule of Three</summary>

En tumregel säger: första gången gör du det bara. Andra gången noterar du dupliceringen men lever med den. Tredje gången refaktorerar du. Vilket scenario har nått "tredje gången"?
</details>

### 📚 Central concepts

- **YAGNI** – bygg inte det du inte behöver än. 🔗 [Martin Fowler: Yagni](https://martinfowler.com/bliki/Yagni.html)
- **Speculative generality** – kodlukten för abstraktioner "ifall att". 🔗 [Refactoring.Guru: Speculative Generality](https://refactoring.guru/smells/speculative-generality)
- **Lazy class** – klasser som inte gör tillräckligt för att motivera sig själva. 🔗 [Refactoring.Guru: Lazy Class](https://refactoring.guru/smells/lazy-class)
- **KISS** – *Keep It Simple, Stupid*. 🔗 [Wikipedia: KISS principle](https://en.wikipedia.org/wiki/KISS_principle)
- **Beck design rules** – fyra regler för enkel design. 🔗 [Martin Fowler: BeckDesignRules](https://martinfowler.com/bliki/BeckDesignRules.html)
- **Kritik mot design patterns** – när mönster gör mer skada än nytta. 🔗 [Refactoring.Guru: Criticism of patterns](https://refactoring.guru/design-patterns/criticism)
- **Records och required members** – alternativet till många builders. 🔗 [Microsoft Learn: Records](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/records)

## 🤔 Reflektionsfrågor

1. I hur många scenarier svarade du "nej"? Var det fler eller färre än du väntade dig?
2. Hur skulle du säga nej till en kollega som vill införa ett mönster, utan att det blir en konflikt?
3. Kostar det mer att lägga till ett mönster för tidigt, eller för sent? Varför?

---

🔑 **[Facit](../../../answers/03-pick-the-pattern/pattern-or-plain-code.md)**. Gör ett ärligt försök först!
