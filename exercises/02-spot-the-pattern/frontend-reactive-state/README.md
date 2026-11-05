# ⚛️ Frontend reactive state

> 🧩 **Mönster:** Observer, Strategy, Decorator, Mediator, Chain of Responsibility och Dependency Injection, fast i JavaScript och TypeScript
> 📏 **Omfattning:** sju korta kodsnuttar från front end-världen. Du behöver kunna läsa grundläggande JavaScript. Ingen tidigare erfarenhet av React, Redux eller RxJS krävs, men det hjälper.
> 🧪 **Ingen kod att köra.** Vill du testa snuttarna kan du klistra in dem i en sandbox online.

## 🎯 Mål

Du ska känna igen samma design patterns i front end-kod som i C#, och se att mönstren handlar om *idéer*, inte om ett visst språk.

## 📖 Bakgrund

Som full stack-utvecklare skriver du kod på båda sidor. Front end-ramverk som React, Vue och Angular bygger i hög grad på **Observer**: när state ändras ska allt som beror på det uppdateras. Men där finns också Decorator, Strategy, Mediator och Chain of Responsibility, ofta med andra namn.

## 📄 Kodsnuttarna

### A. Knappen

```javascript
const button = document.querySelector("#buy");

button.addEventListener("click", () => cart.add(product));
button.addEventListener("click", () => analytics.track("buy-clicked"));
```

### B. Räknaren (React)

```jsx
function Counter() {
  const [count, setCount] = useState(0);

  return (
    <button onClick={() => setCount(count + 1)}>
      Klickat {count} gånger
    </button>
  );
}
```

### C. Varukorgen (Redux-stil)

```javascript
function cartReducer(state, action) {
  switch (action.type) {
    case "item/added":
      return { ...state, items: [...state.items, action.item] };
    case "item/removed":
      return { ...state, items: state.items.filter(i => i.id !== action.id) };
    default:
      return state;
  }
}

const store = createStore(cartReducer, { items: [] });

store.subscribe(() => renderCartIcon(store.getState()));
store.subscribe(() => saveToLocalStorage(store.getState()));

store.dispatch({ type: "item/added", item: { id: 1, name: "Hammare" } });
```

### D. Sökrutan (RxJS)

```typescript
const results$ = fromEvent<InputEvent>(searchInput, "input").pipe(
  map(e => (e.target as HTMLInputElement).value),
  debounceTime(300),
  distinctUntilChanged(),
  switchMap(term => fetchProducts(term))
);

results$.subscribe(products => renderList(products));
```

### E. API-klienten

```typescript
const api = axios.create({ baseURL: "/api" });

api.interceptors.request.use(config => {
  config.headers.Authorization = `Bearer ${getToken()}`;
  return config;
});

api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) redirectToLogin();
    return Promise.reject(error);
  }
);
```

### F. Sorteringen

```typescript
type SortOrder = (a: Product, b: Product) => number;

const byPrice: SortOrder = (a, b) => a.price - b.price;
const byName: SortOrder = (a, b) => a.name.localeCompare(b.name);
const byRating: SortOrder = (a, b) => b.rating - a.rating;

function ProductList({ products, sortOrder }: { products: Product[]; sortOrder: SortOrder }) {
  const sorted = [...products].sort(sortOrder);
  return <ul>{sorted.map(p => <li key={p.id}>{p.name}</li>)}</ul>;
}
```

### G. Komponenten med extra funktioner

```jsx
function withLoadingSpinner(Component) {
  return function Wrapped({ isLoading, ...props }) {
    if (isLoading) return <Spinner />;
    return <Component {...props} />;
  };
}

const ProductListWithSpinner = withLoadingSpinner(ProductList);

// Och i React Context:
const ApiContext = createContext(realApiClient);

function App() {
  return (
    <ApiContext.Provider value={isTest ? fakeApiClient : realApiClient}>
      <Shop />
    </ApiContext.Provider>
  );
}
```

## 🛠️ Uppgift

1. 🏷️ **Identifiera mönstren.** Skriv för varje snutt (A–G) vilket eller vilka mönster den använder. Några snuttar har mer än ett.
2. 🔁 **Hitta motsvarigheten i C#.** Vad är närmaste motsvarighet i .NET till varje snutt? (Till exempel: vad motsvarar `addEventListener`? Vad motsvarar axios-interceptors?)
3. 🔍 **Snutt C på djupet.** Redux-storen har tre roller i ett. Vilka? Vilka objekt kommunicerar via storen i stället för direkt med varandra?
4. 🌊 **Snutt D på djupet.** RxJS kallas ibland "Observer och Iterator i ett". Vad menas med det? Vad är skillnaden mellan att *dra* värden (pull) och att *få dem skickade* till sig (push)?
5. 🔁 **Jämför G med C#.** `withLoadingSpinner` och `ApiContext.Provider` motsvarar två mönster du har sett i ASP.NET Core. Vilka?

## 💡 Hints

<details>
<summary>💡 Hint 1: Lyssnare</summary>

I A, C och D registrerar du funktioner som ska köras *när något händer*. Det heter olika saker (`addEventListener`, `subscribe`), men det är samma idé som `+=` på ett C#-event.
</details>

<details>
<summary>💡 Hint 2: Funktioner som parametrar</summary>

I F skickas en *funktion* in som avgör hur sorteringen ska ske. Vad heter det i C#, och vilket .NET-interface gör samma sak?
</details>

<details>
<summary>💡 Hint 3: Actions och reducers</summary>

Ett Redux-*action* är ett objekt som beskriver vad användaren vill göra (`{ type: "item/added" }`), skickat till en central plats. Jämför med `PlaceOrderCommand` som skickas till en mediator. Komponenterna som skickar actions vet inte vilka andra komponenter som reagerar.
</details>

<details>
<summary>💡 Hint 4: Interceptors</summary>

Varje interceptor får requesten, kan ändra den och skickar den vidare. Det motsvarar en `DelegatingHandler` eller en middleware. Vilka två mönster förknippar du med dem?
</details>

<details>
<summary>💡 Hint 5: Higher-order components och Context</summary>

`withLoadingSpinner` tar en komponent och returnerar en ny komponent med samma props plus lite extra. `ApiContext.Provider` låter en förälder bestämma vilken implementation alla barn får, och barnen frågar inte själva efter en specifik klass.
</details>

### 📚 Central concepts

- **Observer** – prenumerera på förändringar. 🔗 [Refactoring.Guru: Observer](https://refactoring.guru/design-patterns/observer)
- **addEventListener** – DOM:ens inbyggda Observer. 🔗 [MDN: EventTarget.addEventListener()](https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener)
- **React state** – hur React vet att en komponent ska ritas om. 🔗 [React: useState](https://react.dev/reference/react/useState)
- **Reducers** – samla state-logik på ett ställe. 🔗 [React: Extracting State Logic into a Reducer](https://react.dev/learn/extracting-state-logic-into-a-reducer)
- **Redux principer** – en store, actions och rena reducers. 🔗 [Redux: Three Principles](https://redux.js.org/understanding/thinking-in-redux/three-principles)
- **Observables i RxJS** – push-baserade värdeströmmar. 🔗 [RxJS: Observable](https://rxjs.dev/guide/observable)
- **Mediator** – kommunikation via en central mellanhand. 🔗 [Refactoring.Guru: Mediator](https://refactoring.guru/design-patterns/mediator)
- **Signals i Angular** – ett modernt sätt att uttrycka reaktivt state. 🔗 [Angular: Signals](https://angular.dev/guide/signals)

## 🤔 Reflektionsfrågor

1. Varför tror du att Observer är *så* centralt i front end-utveckling, mer än i en typisk backend?
2. Redux kräver att reducers är *rena* funktioner som alltid returnerar ett nytt state-objekt i stället för att ändra det gamla. Vilka fördelar ger det?
3. Vilket front end-mönster skulle du helst vilja ta med dig till din C#-kod?

---

🔑 **[Facit](../../../answers/02-spot-the-pattern/frontend-reactive-state.md)**. Gör ett ärligt försök först!
