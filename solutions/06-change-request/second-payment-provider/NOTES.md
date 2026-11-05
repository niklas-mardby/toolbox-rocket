# 📝 NOTES: Second payment provider

## 💡 Resonemanget

- **Två mönster, två problem:**
  - **Adapter:** tre SDK:er med olika API:er (statussträngar, enum-tillstånd, exceptions; decimal, öre och cent) görs lika bakom `IPaymentProvider`.
  - **Factory** (`PaymentProviderSelector`): *vilken* leverantör som gäller för ett land.
- **`CheckoutService` blev tre rader.** Den validerar det som är gemensamt (beloppet), frågar selectorn och betalar. Den vet inte att Swish, Vipps eller kort finns.
- **Felmeddelandena** (`Declined()`, `TechnicalError()`) definieras på *ett* ställe. Alla leverantörer översätter till samma kundtexter.
- **Leverantörsspecifik validering** (telefonnummer för Swish och Vipps, kortnummer för kort) ligger i respektive adapter, eftersom det är där kravet kommer ifrån.
- **`Phone.ToDigits`** och **`MinorUnits.From`** delas av adaptrarna. Det är översättningslogik som återkommer.
- **Länder utan stöd** ger fortfarande `NotSupportedException`, nu från selectorn.

## 🔀 Alternativ

- **Konfigurationsstyrd selector:** land → leverantör i `appsettings.json` (`"Payments": { "SE": "swish", "NO": "vipps", "default": "card" }`) och adaptrarna registrerade med *keyed services* i DI (`services.AddKeyedSingleton<IPaymentProvider, VippsAdapter>("vipps")`). Det är bra om marknadsavdelningen ofta öppnar nya länder.
- **Kunden väljer** betalsätt (Swish *eller* kort i Sverige): selectorn tar då `(country, method)`. Det är en liten ändring tack vare designen.
- **`Supports(country)` på varje adapter** i stället för en central `switch`: selectorn tar `IEnumerable<IPaymentProvider>` och väljer den första som stöder landet. Det är mer utbyggbart, men reglerna blir utspridda. Med tre leverantörer är en central tabell tydligare.
