# 🏭 Factory of factories

> 🧩 **Överanvänt mönster:** Factory (och Builder och Singleton)
> 📏 **Omfattning:** fem lager av fabriker, builders och en singleton för att skapa *ett* välkomstmejl med tre fält. Det är lätt att se att det är för mycket. Det svåra är att förklara exakt *varför*, och att veta vad som är värt att behålla.
> 🧪 **Tester:** finns och är gröna. Låt dem förbli gröna.

## 🎯 Mål

Du ska känna igen när skapande mönster staplas på varandra utan att lösa något problem, och förenkla koden utan att förlora det som faktiskt är värdefullt.

## 📖 Bakgrund

När någon registrerar sig i Verktygsboden skickas ett välkomstmejl. Mejlet har en mottagare, en rubrik och en text. Appen skickar inga andra sorters mejl.

Öppna `MessageCreation.cs`. För att skapa mejlet går `SignupService` igenom:

```
MessageFactoryProvider.Instance          (en singleton)
  .GetFactoryFactory()                   (en fabrik som skapar fabriker)
  .CreateFactory("welcome")              (väljer fabrik med en sträng)
  .Create(email, name)                   (en fabrik som använder...)
      builderFactory.CreateBuilder()     (...en fabrik som skapar builders...)
        .SetRecipient(...).SetSubject(...).SetBody(...).Build()   (...en builder)
```

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och se att de är gröna.
2. ✍️ **Skriv ner din kritik** för *varje* lager: vad gör det? Vilket problem skulle det lösa? Finns det problemet här?
3. 🔍 **Hitta de verkliga riskerna.** Det finns minst två ställen där mönstren inte bara är onödiga, utan gör att fel upptäcks *senare* än de hade behövt.
4. ✂️ **Förenkla.** Kör testerna efter varje steg.
5. 💎 **Behåll det värdefulla.** Finns det något i den gamla koden som faktiskt var en bra idé? Se till att det finns kvar i någon form.

## ▶️ Kom igång

```bash
dotnet test exercises/05-overuse-critique/factory-of-factories/FactoryOfFactories.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Börja inifrån</summary>

Det innersta lagret är `WelcomeMessageBuilder`. Vad gör den som inte en vanlig konstruktor till `WelcomeMessage` gör? Vad händer om någon glömmer `SetSubject`?
</details>

<details>
<summary>💡 Hint 2: Strängen "welcome"</summary>

Vad händer om någon skriver `CreateFactory("Welcome")` med stor bokstav? När upptäcks felet?
</details>

<details>
<summary>💡 Hint 3: Singletonen</summary>

`MessageFactoryProvider.Instance` är global. Hur skulle du göra för att få `SignupService` att skapa ett *annat* mejl i ett test? Syns det i `SignupService`-konstruktorn att den beror på meddelandefabrikerna?
</details>

<details>
<summary>💡 Hint 4: Det värdefulla</summary>

Någonstans måste texten "Välkommen, {name}!" finnas. Det är en bra idé att samla den på *ett* ställe med ett tydligt namn. Det behöver inte vara fem klasser. En statisk metod på `WelcomeMessage` räcker gott.
</details>

### 📚 Central concepts

- **Factory Method** – och när den är lagom stor. 🔗 [Refactoring.Guru: Factory Method](https://refactoring.guru/design-patterns/factory-method)
- **Abstract Factory** – mönstret som ofta överanvänds på det här sättet. 🔗 [Refactoring.Guru: Abstract Factory](https://refactoring.guru/design-patterns/abstract-factory)
- **Singleton** – och varför det ofta ses som ett problem. 🔗 [Refactoring.Guru: Singleton](https://refactoring.guru/design-patterns/singleton)
- **Middle Man** – klasser som bara skickar vidare. 🔗 [Refactoring.Guru: Middle Man](https://refactoring.guru/smells/middle-man)
- **Speculative generality** – flexibilitet som ingen har bett om. 🔗 [Refactoring.Guru: Speculative Generality](https://refactoring.guru/smells/speculative-generality)
- **Replace Constructor with Factory Method** – den *rimliga* storleken på en fabrik. 🔗 [Refactoring.Guru: Replace Constructor with Factory Method](https://refactoring.guru/replace-constructor-with-factory-method)

## 🤔 Reflektionsfrågor

1. Vilket lager var mest skadligt? Varför?
2. Varför tror du att kod som den här uppstår? Vad tänkte den förra utvecklaren?
3. Om appen i framtiden ska skicka mejl på fem språk, hur skulle du designa det då?

---

🔑 **[Facit](../../../answers/05-overuse-critique/factory-of-factories.md)**. Gör ett ärligt försök först!
