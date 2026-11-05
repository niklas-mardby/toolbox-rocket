# 🏷️ Multi-tenant pricing

> 🧩 **Mönstret som hjälper:** Strategy + Factory (och kanske Composite)
> 📏 **Omfattning:** en prismotor med hårdkodade regler för *en* kund ska bli en motor för *många* kunder med egna, konfigurerbara regler. Regelsemantiken är exakt specificerad, och ordningen spelar roll. Det här är den mest öppna övningen i kategorin: designen är din, och det finns flera rimliga vägar.
> 🧪 **Tester:** 8 befintliga tester är gröna. 9 acceptanstester för det nya kravet är avstängda med `Todo.Skip`.

## 🎯 Mål

Du ska göra om en prismotor från hårdkodade regler till regler som byggs från **konfiguration**, och uppleva varför Strategy och Factory passar så bra ihop när beteende ska styras av data.

## 📖 Bakgrund

Prismotorn byggdes när **Verktygsboden** var plattformens enda kund. Deras regler (medlemsrabatt, mängdrabatt och helgkampanj på trädgård) är hårdkodade som `if`-satser i `PricingEngine`.

Nu säljer plattformen till fler butiker, och var och en vill ha egna regler. Plattformsteamet har redan byggt en **configmodell** (`TenantConfig.cs`) och en lagring för den (`ITenantConfigStore`). Läs kommentaren högst upp i `TenantConfig.cs` noga. Den beskriver *exakt* hur varje regeltyp ska fungera. Prismotorn får redan in config-lagringen i konstruktorn, men använder den inte.

### 📋 Det nya kravet

> **Som** butiksägare **vill jag** att mina egna prisregler gäller i min butik, **så att** jag kan styra kampanjer och rabatter utan att vänta på en utvecklare.

Två nya kunder ska in direkt:
- 🧱 **byggmax-light:** mängdrabatt 10 % från 5 st, "köp 3 skruv, betala för 2", och 49 kr i småordertillägg under 500 kr.
- 🌱 **tradgardsbutiken:** 20 % på trädgård på *vardagar*, och 5 % medlemsrabatt.

Och **Verktygsboden ska fungera exakt som förut**, men nu via sin config. Konfigurationerna finns i `TestTenants.cs`.

## 🛠️ Uppgift

### Del 1: Lägg till funktionen 🛠️

1. 📖 Läs `TenantConfig.cs` (särskilt kommentaren) och `PricingEngine.cs`.
2. ▶️ Kör testerna. Sätt `Todo.Skip` till `null`. 9 tester blir röda.
3. 🛠️ Försök göra dem gröna med den nuvarande strukturen, alltså fler `if`-satser i `PricingEngine`. Hur långt kommer du innan det blir ohållbart? Det är okej att stanna innan alla är gröna, men anteckna var det tog stopp.

### Del 2: Reflektera 🤔

4. Svara på frågorna:
   - Vad var svårast att få in i den befintliga strukturen?
   - Hur många ställen i koden känner till att "medlemsrabatt" finns?
   - Vad händer när en *fjärde* kund vill ha en *ny* regeltyp, till exempel "fri frakt över 1 000 kr"?
   - Var finns gränsen mellan **data** (configen) och **beteende** (koden)?

### Del 3: Refaktorera 🔧

5. 🔀 Inför **en strategi per regeltyp**. Fundera på om regler som gäller en *rad* och regler som gäller hela *ordern* ska ha samma interface.
6. 🏭 Inför en **fabrik** som gör om en `PriceRuleConfig` till rätt regelobjekt.
7. ⚙️ Låt `PricingEngine` hämta tenantens config, bygga regellistan och köra den i rätt ordning.
8. 🧹 Ta bort alla hårdkodade regler.
9. ✅ Alla 17 tester ska vara gröna, inklusive Verktygsbodens gamla tester, utan ändringar.

### ➕ Nästa förändring

10. Lägg till regeltypen `FreeShippingOver(decimal Threshold, decimal ShippingFee)`: alla ordrar får fraktavgiften som en `QuoteFee`, *utom* om raderna summerar till minst tröskelvärdet. Hur många befintliga filer ändrade du?

## ▶️ Kom igång

```bash
dotnet run --project exercises/06-change-request/multi-tenant-pricing/MultiTenantPricing
dotnet test exercises/06-change-request/multi-tenant-pricing/MultiTenantPricing.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Data och beteende</summary>

`PriceRuleConfig` och dess subtyper är *data*: de säger *vilken* regel och *vilka parametrar*. De vet inte hur man räknar. Du behöver något som *gör* något med en rad. Det är din strategi. Och något som översätter från data till beteende. Det är din fabrik.
</details>

<details>
<summary>💡 Hint 2: Hur ser en radregel ut?</summary>

En regel behöver veta produkten, antalet, kunden, tidpunkten och radens *nuvarande* summa, och kunna ändra summan. En liten klass `PricedLine` med en settable `Total` fungerar bra. Låt regeln returnera `true` om den ändrade något, så att motorn vet vilka namn som ska med i `AppliedRules`.
</details>

<details>
<summary>💡 Hint 3: Fabriken</summary>

Pattern matching på typen är perfekt här:

```csharp
config switch
{
    PercentOffForMembers c => new MemberDiscountRule(c.Percent),
    QuantityBreak c => new QuantityBreakRule(c.MinQuantity, c.Percent),
    // ...
}
```
</details>

<details>
<summary>💡 Hint 4: Köp 3 betala för 2</summary>

Med 7 skruv blir det `7 / 3 = 2` hela tretal, så `2 × (3 − 2) = 2` gratis enheter. De värderas till radens pris per enhet *just då*, alltså efter eventuell mängdrabatt. Testet `Line_rules_build_on_each_other_in_config_order` räknar steg för steg i en kommentar.
</details>

<details>
<summary>💡 Hint 5: Namnen</summary>

`10m.ToString("0.##", CultureInfo.InvariantCulture)` ger `"10"`, och `7.5m` ger `"7.5"`. Utan `InvariantCulture` får du `"7,5"` på en svensk dator.
</details>

### 📚 Central concepts

- **Strategy** – en klass per regeltyp. 🔗 [Refactoring.Guru: Strategy](https://refactoring.guru/design-patterns/strategy)
- **Factory Method** – från config till objekt. 🔗 [Refactoring.Guru: Factory Method](https://refactoring.guru/design-patterns/factory-method)
- **Composite** – om regler ska kunna grupperas. 🔗 [Refactoring.Guru: Composite](https://refactoring.guru/design-patterns/composite)
- **Multitenancy** – ett system, många kunder med egna inställningar. 🔗 [Wikipedia: Multitenancy](https://en.wikipedia.org/wiki/Multitenancy)
- **Pattern matching** – `switch` på typ, för fabriken. 🔗 [Microsoft Learn: Pattern matching](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching)
- **Records och arv** – configmodellen. 🔗 [Microsoft Learn: Records](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- **Replace Conditional with Polymorphism** – från `if`-satser till klasser. 🔗 [Refactoring.Guru: Replace Conditional with Polymorphism](https://refactoring.guru/replace-conditional-with-polymorphism)
- **Speculative generality** – vad du *inte* ska bygga ännu. 🔗 [Refactoring.Guru: Speculative Generality](https://refactoring.guru/smells/speculative-generality)

## 🤔 Reflektionsfrågor

1. Hur bevisar Verktygsbodens gamla tester att din refaktorering inte ändrade beteendet? Varför var det viktigt att de inte fick ändras?
2. Var är det minst risk att en ny utvecklare gör fel: när hen lägger till en ny *kund* eller en ny *regeltyp*? Vad gör din design för att skydda mot fel i det andra fallet?
3. Butiksägarna vill på sikt kunna kombinera regler fritt ("välj den bästa rabatten av dessa tre"). Vad i din design skulle behöva ändras? Vilket mönster skulle hjälpa?
4. Finns det något i din lösning som är *mer* generellt än kraven kräver? Är det motiverat?

---

🔑 **[Facit](../../../answers/06-change-request/multi-tenant-pricing.md)**. Gör ett ärligt försök först!
