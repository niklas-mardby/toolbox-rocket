# 🔍 02 – Spot the pattern

Mönster finns överallt i koden du redan använder. Här tränar du på att **känna igen** dem i riktig .NET-kod och i ramverk, och på att förklara *hur* de är implementerade.

> 💡 Att kunna känna igen ett mönster är ofta viktigare än att kunna skriva det från grunden. När du ser att `DelegatingHandler` är en Decorator vet du direkt hur du använder den.

## 🧭 Så jobbar du

1. 👀 Läs koden eller beskrivningen noga. Vem anropar vem? Vem känner till vem?
2. 🧩 Leta efter *strukturen*, inte namnen. En klass kan heta `Wrapper` utan att vara en Decorator, och en Decorator kan heta vad som helst.
3. ✍️ Skriv ner ditt svar *och* din motivering innan du tittar i facit.

## 📋 Övningarna

Ungefär ordnade från mindre till större:

| Övning | Kort beskrivning |
|--------|------------------|
| [🧩 dotnet-pattern-match](dotnet-pattern-match/) | Para ihop 23 .NET-features med de 12 mönster de bygger på. |
| [🌊 stream-layers](stream-layers/) | Ett litet körbart program som staplar streams. Vem är Decorator och vem är Adapter? |
| [🚀 aspnet-core-startup](aspnet-core-startup/) | En typisk `Program.cs`, rad för rad. Hur många mönster hittar du? |
| [🗃️ ef-core-under-the-hood](ef-core-under-the-hood/) | Kör experiment mot EF Core och SQLite och se Unit of Work, Identity Map och Proxy i aktion. |
| [⚛️ frontend-reactive-state](frontend-reactive-state/) | Samma mönster i JavaScript och TypeScript: events, React, Redux och RxJS. |
| [🕵️ mystery-codebase](mystery-codebase/) | En kodbas där inget heter något med "pattern". Hitta och namnge mönstren ändå. |
