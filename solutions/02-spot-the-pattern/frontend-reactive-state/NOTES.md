# 📝 NOTES: Frontend reactive state

Facit per snutt finns på main i [`answers/02-spot-the-pattern/frontend-reactive-state.md`](../../../answers/02-spot-the-pattern/frontend-reactive-state.md).

## 💡 Resonemanget

- **Observer är front end-utvecklingens grundmönster.** UI:t är en funktion av state, och allt som ändrar state måste kunna notifiera det som visas.
- **Redux är tre mönster i ett:** Mediator (`dispatch` via storen), Observer (`subscribe`) och Command (actions som data).
- **RxJS** kombinerar Observers push-modell med Iterators operatorer. Rx.NET gör samma sak för `IObservable<T>` i C#.
- **Higher-order components** är Decorators, och **React Context** liknar DI men är tekniskt sett närmare Service Locator.

## 🔀 Andra giltiga svar

- **Snutt E** (axios-interceptors) kan också ses som **Observer**: interceptors "lyssnar" på varje request och response. Men eftersom de kan ändra och stoppa flödet är Chain of Responsibility och Decorator bättre beskrivningar.
- **Snutt F:** att ge komponenten `sortOrder` som prop är också en sorts **Dependency Injection** (av en strategi).
- **Snutt B:** `useState` kan beskrivas som **Memento** eller State, men Observer är det som förklarar varför komponenten ritas om.
