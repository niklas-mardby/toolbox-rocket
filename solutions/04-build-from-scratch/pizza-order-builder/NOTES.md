# 📝 NOTES: Pizza order builder

## 💡 Resonemanget

- **Valideringen ligger på två ställen, med flit.** Regler om *ett enskilt värde*, som en tom topping, kontrolleras direkt i `AddTopping`. Regler om *helheten*, som att storlek saknas, för många toppings eller glutenfritt i Small, kontrolleras i `Build()`, när alla val är kända. Det är en av de stora fördelarna med en Builder: du kan validera kombinationer som en konstruktor med många parametrar har svårt att uttrycka tydligt.
- **`_size` är nullable** (`PizzaSize?`) för att skilja på "inte vald" och "vald". Ett standardvärde hade gjort att storlek inte var obligatorisk.
- **`_toppings.ToList()`** ger den färdiga pizzan en *egen kopia*. Utan den skulle `first.Toppings` ändras när buildern får fler toppings, eftersom listan delas.
- **`PizzaMenu` returnerar en ny builder varje gång.** Om den returnerade en sparad builder skulle en kunds `AddTopping("Ananas")` hamna på allas Vesuvio. `Capricciosa()` återanvänder `Vesuvio()`, vilket visar att färdiga startpunkter kan byggas av varandra.
- **`OrderBuilder.AddPizza(Func<PizzaBuilder, PizzaBuilder>)`** låter anroparen bygga en pizza inline, och bygger den direkt. Därför kastar en ogiltig pizza redan i `AddPizza`, där felet är lätt att hitta.
- **"Sista anropet vinner"** för `DeliverTo` och `PickUp` hanteras med en flagga (`_fulfilmentChosen`) plus adressen. Med bara adressen hade du inte kunnat skilja "avhämtning vald" från "inget valt".

## 🔀 Alternativa lösningar

- **Kasta direkt i `AddTopping` vid den sjunde toppingen** i stället för i `Build()`. Det ger tidigare fel, men buildern kan då inte användas för att *förbereda* en pizza i flera steg som tillfälligt bryter mot reglerna. Båda är rimliga.
- **Immutabel builder**, där varje `With...` returnerar en *ny* builder. Det gör buildern säker att dela och återanvända (`var baseVesuvio = ...; var a = baseVesuvio.WithSize(Small); var b = baseVesuvio.WithSize(Large);`), men det kräver mer kod. Med `record` och `with` blir det ganska smidigt.
- **Steg-builder (step builder)**, där `WithSize` måste anropas först och returnerar ett annat interface. Det gör att kompilatorn tvingar fram storleken. Det är kraftfullt men mer komplext. Fundera på om det vore värt det här.
- **Utan builder:** `new Pizza(PizzaSize.Large, Crust.Thin, ["Skinka"], ExtraCheese: true)` fungerar också, eftersom `Pizza` bara har fyra fält. Builderns värde här är främst valideringen, menyn och `OrderBuilder`:s leveranslogik.
