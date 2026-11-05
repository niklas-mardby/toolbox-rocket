# 🔑 Facit: Legacy modernization

> ⚖️ Det här är en öppen uppgift. Här finns ett **exempelsvar** och, för varje fråga, en lista över **vad ett bra svar tar hänsyn till**.

## 1. 🧪 Innan något mönster: tester

**Exempelsvar:**
> Jag börjar med **characterization tests** för de delar som ska ändras först: avgiftsberäkningen och rapporteringen till förbundet. Testerna ska beskriva hur systemet beter sig *idag*, även om det är fel, så att jag märker om något ändras oavsiktligt. För `MembershipProcessor` kan jag anropa de publika metoderna med olika medlemstyper och datum, och spara resultaten som förväntade värden ("golden master"). Eftersom koden använder `DateTime.Now` och den statiska databasen kan jag behöva en testdatabas till att börja med. Det är okej. Långsamma tester är bättre än inga tester.

✅ **Ett bra svar...**
- sätter **tester före refaktorering**, och vet att refaktorering utan tester egentligen är "ändra och hoppas".
- förstår att characterization tests fångar **nuvarande** beteende, buggar inklusive. Buggar fixas *separat* och medvetet.
- accepterar att de första testerna kan vara **långsamma och fula**, och blir bättre när sömmar har skapats.
- börjar med de delar som **ska ändras snart**, inte med hela systemet.

## 2. 🔌 SOAP till REST → **Adapter** + konfiguration

**Exempelsvar:**
> Jag definierar ett interface i *vår* domäns språk, `IFederationGateway`, med metoder som `ReportMemberCount(clubId, season, count)` och `GetLicenses(clubId)`. Först skriver jag en `SoapFederationAdapter` som anropar `RfSoapClient`. Sedan flyttar jag de 30 anropen, några i taget, till interfacet, och kör testerna efter varje steg. När REST-API:t finns skriver jag en `RestFederationAdapter`. Under övergångsperioden väljer en inställning (feature flag) vilken adapter som används, kanske per förening. Det gör att vi kan börja med några pilotföreningar och backa direkt, utan en ny release, om något går fel.

✅ **Ett bra svar...**
- designar interfacet utifrån **vad systemet behöver**, inte som en kopia av SOAP-tjänsten. Annars blir REST-adaptern svår att skriva.
- flyttar de 30 anropen **innan** REST-API:t finns, så att bytet blir en liten förändring när det väl är dags. Det är *preparatory refactoring*.
- använder **konfiguration eller feature flags** för att byta och backa utan release.
- kan nämna att en **Factory** eller en DI-registrering med villkor väljer adapter, och att en **Decorator** kan logga och jämföra svaren från båda under övergången.

## 3. 🗄️ Den statiska databasklassen → **Adapter** + **DI**, gradvis

**Exempelsvar:**
> Jag skapar ett interface, `IDatabase`, med samma metoder som `Db` (`Execute` och `Query<T>`), och en `StaticDbAdapter : IDatabase` som bara anropar `Db`. Sedan ändrar jag *en klass i taget* till att få `IDatabase` via konstruktorn, och börjar med klasserna jag ändå ska ändra (som `MembershipProcessor`). De andra 170 ställena får vara som de är tills vidare. När jag rör dem av andra skäl byter jag till interfacet. Om några år finns det inga anrop kvar till `Db` direkt. Det är strangler fig i liten skala.

✅ **Ett bra svar...**
- **ändrar inte alla 180 ställen på en gång**. Det är för riskabelt och för dyrt.
- ser att interfacet först kan vara en **direkt kopia** av den statiska klassen, och att domänspecifika repositories kan komma senare, där det lönar sig.
- använder **opportunistisk refaktorering**: förbättra koden där du ändå arbetar.
- noterar att SQL-strängar som byggs med `+` är en **säkerhetsrisk** (SQL injection) och bör prioriteras separat, oavsett mönster.

## 4. 🧮 Familj och `MembershipProcessor` → det beror på, men ofta **Strategy först, minimalt**

**Exempelsvar:**
> Med sex veckor på mig skulle jag *inte* refaktorera hela `MembershipProcessor` till Strategy först. Det är 2 000 rader utan tester. Jag skulle i stället:
> 1. skriva characterization tests för avgiftsberäkningen (vecka 1),
> 2. bryta ut **bara avgiftsberäkningen** till ett interface, `IMembershipFee`, och en klass per medlemstyp, med de fyra `switch`-satserna som *ett* uppslag (vecka 2–3),
> 3. lägga till `FamilyMembershipFee` som en ny klass (vecka 4).
>
> Om tiden inte räcker lägger jag till ett `case` i varje `switch`, levererar Familj och refaktorerar direkt efteråt. Ett sjunde `case` på fyra ställen är inte farligt *om det finns tester*. Utan tester är det farligt oavsett.

✅ **Ett bra svar...**
- ser **avvägningen mellan leveranstid och designkvalitet**, och att båda vägarna kan vara rätt.
- ser att Familj *skiljer sig* från de andra typerna, eftersom avgiften beror på antalet familjemedlemmar. Strategy-interfacet måste få mer information än bara medlemstypen. Det är bra att upptäcka det *innan* interfacet designas.
- refaktorerar **en del av klassen** (avgifterna), inte hela.

## 5. ⏰ Säsongsbuggarna → **DI av `TimeProvider`**

**Exempelsvar:**
> Buggarna inträffar vid säsongsskiften, och de går inte att återskapa eftersom koden använder `DateTime.Now`. Jag injicerar `TimeProvider` i de klasser som avgör säsong, och skriver tester för exakt de datum som är kritiska: dagen före, dagen för och dagen efter skiftet, och skottår. Säsongslogiken bryts ut till en ren funktion, `Season.For(DateOnly date)`, som är lätt att testa.

✅ **Ett bra svar...**
- förstår att `DateTime.Now` är ett **dolt beroende** som gör buggen omöjlig att reproducera i tester.
- skriver tester för **gränsfallen** innan buggen fixas.

## 6. 🚫 Vad du inte gör

✅ **Exempel på bra svar:**
- ❌ **Skriva om systemet från grunden.** 400 föreningar är beroende av det, och all kunskap som finns i den gamla koden (inklusive "konstiga" specialfall som faktiskt är affärsregler) riskerar att försvinna.
- ❌ **Införa ett generiskt `IRepository<T>` och byta alla 180 databasanrop.** Det är för stort, för riskabelt och löser inte de problem som faktiskt gör ont.
- ❌ **Införa CQRS och en mediator** nu. Det löser inget av de akuta problemen.
- ❌ **Refaktorera utan tester.**
- ❌ **Fixa alla buggar samtidigt som refaktoreringen.** Blanda inte ändring av *struktur* med ändring av *beteende* i samma steg.

## 7. 📅 Exempel på fyraveckorsplan

| Vecka | Fokus | Varför |
|-------|-------|--------|
| **1** | Characterization tests för avgiftsberäkning och förbundsrapportering. `TimeProvider` i säsongslogiken plus tester för säsongsskiften. | Skyddsnät för allt som ska ändras. Säsongsbuggarna kostar supporttid varje dag. |
| **2** | `IFederationGateway` + `SoapFederationAdapter`. Flytta de första 10 av 30 anropen. Börja bryta ut avgifterna till `IMembershipFee`. | SOAP-bytet har en hård deadline, så börja förbereda tidigt. |
| **3** | Flytta resterande SOAP-anrop. Avsluta Strategy för avgifterna. Fixa säsongsbuggarna med tester. | När alla 30 anrop går via interfacet är REST-bytet en ny klass. |
| **4** | Lägg till Familj som en ny avgiftsklass. Leverera. | Den nya funktionen blir liten tack vare förarbetet. |

✅ **Ett bra svar...**
- **levererar affärsvärde** (Familj, buggfixar) samtidigt som designen förbättras, inte "först refaktorering i tre månader, sedan funktioner".
- prioriterar efter **deadline och risk**: SOAP-bytet har en hård deadline, och buggarna kostar varje dag.
- lämnar det mesta av koden (de 170 databasanropen, mejlen) **orört tills vidare**.

## 🤔 Reflektionsfrågorna

### 1. Att förklara för en produktägare
Tala om **risk och leveranstid**, inte om kodkvalitet som ett mål i sig:
> "Vi kan lägga till Familj direkt, men då rör vi 2 000 rader kod utan tester, och varje ändring riskerar att ge fel avgifter till 400 föreningar. Om vi lägger en vecka på tester och en på att strukturera om avgiftsberäkningen blir Familj en liten ändring, och nästa medlemstyp blir ännu mindre. Dessutom kan vi äntligen hitta orsaken till säsongsbuggarna."

### 2. Farligast på sikt
Rimliga svar:
- **SQL-strängar som byggs med `+`**, eftersom det är en säkerhetsrisk (SQL injection) som kan bli en dataläcka.
- **Avsaknaden av tester**, eftersom den gör *alla* andra förbättringar riskabla.
- **SOAP-beroendet**, eftersom det har en hård deadline och 30 anrop är spridda överallt.

### 3. Skriva om från grunden?
Ibland, men sällan. Det kan vara rätt om tekniken är helt död (inget går att köra eller underhålla), om systemet är **litet**, eller om verksamheten har förändrats så mycket att det gamla systemet löser fel problem. Risken är att omskrivningen tar längre tid än planerat, att den gamla koden måste underhållas parallellt, och att dold kunskap går förlorad. *Strangler fig* är ett sätt att få många av fördelarna med en omskrivning, men gradvis och med lägre risk.
