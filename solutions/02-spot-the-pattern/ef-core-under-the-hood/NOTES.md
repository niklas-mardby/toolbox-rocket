# 📝 NOTES: EF Core under the hood

Facit för varje experiment finns på main i [`answers/02-spot-the-pattern/ef-core-under-the-hood.md`](../../../answers/02-spot-the-pattern/ef-core-under-the-hood.md).

## 💡 Resonemanget

- **Identity Map är inte en cache.** Den andra frågan i experiment 1 skickar fortfarande SQL. Bara `Find` letar i Identity Map först. Det är den vanligaste missuppfattningen.
- **Unit of Work syns i `ChangeTracker.Entries()`** och på raden `Committed transaction.`. Experiment 3 visar allt-eller-inget: David sparas inte, trots att han inte hade något fel.
- **Proxyn syns med `GetType()`** (`CustomerProxy`). Objekt som skapas med `new` är *inte* proxies, och det förklarar blandningen i experiment 2.
- **`IQueryable` är en beskrivning, inte ett resultat.** SQL:en skickas när du itererar.

## 🧪 Ändringarna i steg 4 (förväntat resultat)

| Ändring | Resultat |
|---------|----------|
| `Find(id)` två gånger | En SQL-fråga |
| Utan `UseLazyLoadingProxies()` | `anna.Orders.Count` blir 0. Lös med `Include` eller `Entry(...).Collection(...).Load()`. |
| `.ToList()` i experiment 5 | SQL skickas direkt, och `Expression`/`ToQueryString()` fungerar inte längre |

## 🔀 Andra giltiga svar

- Experiment 5 kan också beskrivas som **Adapter**: query providern översätter LINQ till SQL för en viss databas. Det kompletterar Iterator och Interpreter.
