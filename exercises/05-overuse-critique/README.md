# 🧹 05 – Overuse critique

Här är det tvärtom mot tidigare övningar. Koden använder mönster, men den blir **sämre** av dem. Din uppgift är att **kritisera** och **förenkla**.

> 💡 Att kunna ta bort ett mönster är lika viktigt som att kunna lägga till ett. En del av koden du möter i arbetslivet är överdesignad, och ibland av någon som just har läst en bok om design patterns. 😉

## 🧭 Så jobbar du

1. ▶️ Kör testerna. De är gröna.
2. 👃 Läs koden och **skriv ner din kritik** innan du ändrar något. Vilka mönster används? Vilket problem *skulle* de lösa? Finns det problemet här?
3. ✂️ **Förenkla** i små steg. Kör testerna efter varje steg.
4. 🚫 Ändra inte testerna. Där det finns en `Composition.cs` får du ändra den.
5. ⚖️ Fundera på om något av mönstren faktiskt **ska få stanna**. Det rätta svaret är inte alltid "ta bort allt".

## 📋 Övningarna

Ungefär ordnade från mindre till större:

| Övning | Överanvänt mönster | Kort beskrivning |
|--------|--------------------|------------------|
| [🧮 one-algorithm-strategy](one-algorithm-strategy/) | Strategy | Sex interfaces för att räkna ut moms på 25 %. |
| [🏭 factory-of-factories](factory-of-factories/) | Factory | Tre lager fabriker för att skapa ett välkomstmejl. |
| [🔌 interface-for-everything](interface-for-everything/) | Dependency Injection | Varje klass har ett interface, och allt registreras i DI. |
| [🏓 event-ping-pong](event-ping-pong/) | Observer | Events som triggar events som triggar events. |
| [🗄️ repository-over-ef-core](repository-over-ef-core/) | Repository + Unit of Work | Ett generiskt repository som gör EF Core sämre. |
| [🏢 enterprise-todo-api](enterprise-todo-api/) | Allt! | En todo-lista med mediator, decorators, builders, fabriker och mappers. |
