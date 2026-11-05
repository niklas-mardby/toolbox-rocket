# 🔑 Facit: Factory of factories

> 🧩 Den förenklade koden finns på branchen `solutions` i `solutions/05-overuse-critique/factory-of-factories/`.

## ✍️ Kritiken per lager

| Lager | Vad det gör | Problemet det skulle lösa | Finns problemet? |
|-------|-------------|---------------------------|------------------|
| `WelcomeMessageBuilder` | Sätter tre fält, ett i taget | Objekt med många *valfria* delar | ❌ Tre *obligatoriska* fält. En konstruktor räcker och är säkrare. |
| `WelcomeMessageBuilderFactory` | `return new WelcomeMessageBuilder();` | Dölja vilken builder som skapas | ❌ Det finns bara en. |
| `WelcomeMessageFactory` | Bestämmer texterna | Samla hur mejlet ser ut | ✅ **Idén** är bra, men den behöver inte vara en klass med ett beroende. |
| `DefaultMessageFactoryFactory` | Väljer fabrik med en sträng | Välja meddelandetyp medan programmet körs | ❌ Det finns en meddelandetyp. |
| `MessageFactoryProvider` | En global singleton | Dela en instans | ❌ Det finns inget tillstånd att dela, och den gör testning svårare. |

## ⚠️ De verkliga riskerna

1. **Buildern flyttar fel från kompileringen till körningen.** `new WelcomeMessage(to, subject, body)` *kan inte* kompileras utan alla tre värden. Med buildern upptäcks ett glömt `SetSubject` först när koden körs (`InvalidOperationException`).
2. **Den magiska strängen `"welcome"`.** Ett stavfel eller fel versal (`"Welcome"`) ger `NotSupportedException` först när någon registrerar sig, i produktion.
3. **Singletonen döljer ett beroende.** `SignupService`-konstruktorn säger bara `Outbox`, men klassen beror i själva verket på en global kedja av fem objekt. Den går inte att byta ut i ett test.

## ✂️ Efter förenklingen

```csharp
public record WelcomeMessage(string To, string Subject, string Body)
{
    public static WelcomeMessage For(string email, string name) => new(
        email,
        $"Välkommen, {name}!",
        $"Hej {name}! Ditt konto {email} är nu skapat. Välkommen till Verktygsboden.");
}

// I SignupService:
outbox.Add(WelcomeMessage.For(email, name));
```

**Det värdefulla som behölls:** att texterna finns på *ett* ställe med ett tydligt namn. Det är exakt vad en *factory method* är till för, i lagom storlek.

## 🤔 Reflektionsfrågorna

### 1. Mest skadligt?
Rimliga svar är **singletonen** (global, dold och otestbar) eller **den magiska strängen** (fel som bara syns i produktion). Buildern är också dålig, eftersom den gör en säker konstruktion osäker. Ett bra svar motiverar med *konsekvenser*, inte bara med "det är onödigt".

### 2. Varför uppstår sådan kod?
Vanliga orsaker:
- 📚 **Man har just lärt sig mönster** och vill använda dem. Det är helt mänskligt!
- 🔮 **Man bygger för en tänkt framtid** ("tänk om vi får 20 meddelandetyper").
- 🏢 **Kopierad arkitektur** från ett stort projekt där mönstren *var* motiverade.
- 🧱 **Regler** som "allt ska skapas via fabriker", utan att någon frågar varför.

### 3. Fem språk?
Fortfarande inga fabriker av fabriker:
```csharp
public interface IWelcomeMessageComposer
{
    WelcomeMessage Compose(string email, string name, CultureInfo language);
}
```
*En* abstraktion, injicerad i `SignupService`, som till exempel läser texter från resursfiler (`.resx`) eller en databas. Valet av språk är *data* (användarens inställning), inte en ny klass per språk. ✅ Ett bra svar ser att variation i *data* sällan kräver nya *klasser*.
