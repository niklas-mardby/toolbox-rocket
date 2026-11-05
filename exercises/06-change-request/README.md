# 🔄 06 – Change request

Här får du en **fungerande app** och ett **nytt krav**, precis som i arbetslivet. Övningen har tre delar:

1. 🛠️ **Lägg till funktionen** med den nuvarande designen, på det sätt som känns naturligt.
2. 🤔 **Reflektera:** klarade designen förändringen? Vad var jobbigt? Hur många ställen behövde du ändra?
3. 🔧 **Refaktorera** så att *den här* och *nästa* förändring blir enkel, med hjälp av ett mönster.

> 💡 Det är medvetet att du först gör det "fula" sättet. Det är när du känner smärtan i en design som du förstår varför mönstret finns.

## 🧭 Så jobbar du

1. ▶️ Kör testerna. De befintliga är gröna. **Acceptanstesterna** för det nya kravet är avstängda med `Todo.Skip`.
2. 🧪 Slå på acceptanstesterna genom att sätta `Todo.Skip` till `null`. Nu är de röda.
3. 🛠️ **Del 1:** gör dem gröna, på enklaste sätt. Spara gärna en kopia av koden, eller gör en commit.
4. 📝 **Del 2:** svara på reflektionsfrågorna i README:n för just den delen.
5. 🔧 **Del 3:** refaktorera. Alla tester ska fortfarande vara gröna.
6. ➕ Gör gärna "nästa förändring" som finns i varje övning, och jämför hur lätt den var före och efter.

> 🚫 Ändra inte de befintliga testerna eller acceptanstesterna. Där det finns en `Composition.cs` får du ändra den.

## 📋 Övningarna

Ungefär ordnade från mindre till större:

| Övning | Mönstret som hjälper | Kort beskrivning |
|--------|----------------------|------------------|
| [📊 report-export-formats](report-export-formats/) | Strategy | Rapporten kan exporteras till CSV. Nu vill alla ha JSON och Markdown också. |
| [🔔 notification-channels](notification-channels/) | Strategy (+ Observer) | Kunderna vill få besked via SMS och push, inte bara mejl. |
| [🐢 product-catalog-caching](product-catalog-caching/) | Decorator | Katalogen är långsam. Lägg till caching och tidsmätning utan att röra katalogen. |
| [🌍 second-payment-provider](second-payment-provider/) | Adapter + Factory | Butiken ska ta betalt via olika leverantörer beroende på land. |
| [🧾 customer-audit-trail](customer-audit-trail/) | Decorator / Chain of Responsibility / Mediator | Alla ändringar i ett API ska loggas för revision och valideras på samma sätt. |
| [🏷️ multi-tenant-pricing](multi-tenant-pricing/) | Strategy + Factory (+ Composite) | En prismotor för en kund ska bli en prismotor för många, med konfigurerbara regler. |
