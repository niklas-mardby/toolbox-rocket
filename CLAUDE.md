# CLAUDE.md – Design Patterns Learning Repo

## Purpose

This repo is the answer to the student question: "How do I learn design patterns?" It is part of a problem-solving course for students who have studied C# for about 1.5 years (C#, databases, some front end). They know what design patterns are and have looked at one or two. The goal is not to cover every pattern, but to give the best possible overview of the most important patterns for a full-stack .NET developer, through varied exercises and self-study material.

## Audience and tone

- Students with an intermediate C# level. Assume they know classes, interfaces, generics, LINQ, async, and basic EF Core and ASP.NET Core.
- All student-facing text (READMEs, docs, exercises, hints, solution notes) speaks directly to the student, using "you". It is self-study material students work through in their own time, so never address or mention the teacher, and never write teacher notes in student files. Language: folders, file names, code and code comments are in English. Pattern names, concepts and technical terms stay in English. All student-facing text (exercise texts, hints, docs, answer keys) is in clear, friendly Swedish. Explain the _problem_ before the _pattern_. Students should discover why a pattern exists, not just memorise its structure.
- Keep examples small, realistic and runnable.

**Emojis are an important rule.** All documentation (READMEs, docs, exercises, hints) uses emojis, for example in headings and to mark sections like hints 💡, central concepts 📚 and reflection questions 🤔.

**Repo name.** The repo is named `toolbox-rocket`, after the two emojis 🧰🚀. Wherever the name appears as a heading, show both the emojis and the words: the root README title is `# 🧰🚀 Toolbox Rocket`.

## Tech stack

- .NET 8 (or latest LTS), C#.
- One solution file at the root: `DesignPatterns.sln`.
- Console apps for most exercises; a minimal ASP.NET Core API where web context matters (middleware, DI, CQRS).
- xUnit test projects where exercises have verifiable outcomes.
- Every project must build and run with `dotnet build` and `dotnet test` from the root.

## Core patterns to cover

Prioritised for full-stack .NET work:

1. Dependency Injection (foundation of ASP.NET Core)
2. Repository and Unit of Work (tie to EF Core, and discuss whether EF Core already does this)
3. Strategy (replacing if/else and switch chains)
4. Observer (C# events, delegates, IObservable, front-end reactive state)
5. Decorator (middleware, logging/caching wrappers)
6. Factory and Builder (object creation; WebApplication.CreateBuilder)
7. Mediator / CQRS (e.g. MediatR-style pipelines)
8. Adapter (wrapping third-party APIs)

## Repo structure

```
/README.md                  -> Entry point: "How do I learn design patterns?" Explains the path through the repo.
/docs/
  01-overview.md            -> Short intro to each core pattern: problem, idea, when NOT to use it.
  02-patterns-in-the-wild.md -> Self-study: modern patterns and patterns built into .NET.
  03-pattern-cheatsheet.md  -> One-page summary table.
  04-further-reading.md     -> Curated books, articles and docs per pattern, each with one line on why it's worth reading.
/exercises/
  01-refactor/
  02-spot-the-pattern/
  03-pick-the-pattern/
  04-build-from-scratch/
  05-overuse-critique/
  06-change-request/
/solutions/                 -> Mirrors /exercises. Kept on a separate `solutions` branch.
```

Each exercise folder contains its own `README.md` that follows the exercise template below.

## Exercise template

Every exercise README uses the same structure, so students always know where to look.

1. **The exercise.** Goal, short background and the task steps. This comes first and stands on its own.
2. **Hints.** A separate `## Hints` heading after the exercise. Give suggestions for how to move on when stuck, ordered from gentle nudges to more concrete help. Put each hint in a collapsed `<details>` block so students reveal only what they need. Hints never give the full solution.
3. **Central concepts.** Under the hints, a `### Central concepts` list of the key concepts the exercise touches on (e.g. interfaces, polymorphism, open/closed principle, composition over inheritance, dependency injection). Each item gets a one-line description, so students know what to research, plus a 🔗 link to a good source on that concept. Prefer Microsoft Learn, Refactoring.Guru and martinfowler.com. Only use links you can verify exist; never guess a URL.
4. **Reflection questions.** A few short questions to finish with.

## How students are expected to work

Explain this workflow in the root README:

1. Skim the exercises and pick ones that match your level and ambition.
2. Try the exercise on your own first.
3. Stuck? Open the hints one at a time.
4. Still stuck, or a term is unfamiliar? Check the central concepts list, research that concept, then come back to the exercise.
5. Finish with the reflection questions.

## Exercise types

Each type has a different focus. Create more exercises per type than any student can finish (aim for 5–7), spread across the core patterns.

### Teaching philosophy: more exercises than time

Students deliberately get more exercises than they have time to do. They choose which ones to do based on what benefits them, their ambition and where they are in their learning journey. Skipping exercises is expected and fine.

- **Varied difficulty within every category.** Each exercise type has a spread from easier, to medium, to harder, to the hardest. Order them roughly from easiest to hardest within the folder.
- **Never label difficulty.** Do not use words like easy, medium, hard, beginner, advanced, stars or levels in folder names, titles or READMEs. Name exercises by their topic, e.g. `01-refactor/shipping-cost-calculator`.
- **Let the description signal the level.** The README's opening should make the scope clear, so a student can read it and think "this sounds too easy for me" or "this suits me". Easier ones: one pattern, small code, clear steps. Harder ones: larger codebases, combined patterns, open-ended requirements, trade-off discussion, fewer hints.
- **Explain this in the root README.** Tell students there are more exercises than they can do, that they should skim and pick what fits them, and that skipping is encouraged.

### 1. Refactor

Give messy but working starter code (e.g. a giant switch statement for discount or shipping calculation). Students refactor it into a pattern (e.g. Strategy). Include tests that pass before and after, so the refactoring is safe.

### 2. Spot the pattern

Show real .NET code or framework usage (middleware pipeline, ILogger, DbContext, HttpClientFactory, events). Students identify which patterns are in use and explain how.

**Include this exercise: `02-spot-the-pattern/dotnet-pattern-match`.** The student matches .NET features to the patterns they implement. Every pattern is used at least once, and some features fit more than one.

Features: `foreach`, lazy loading proxies in EF Core, the ASP.NET Core middleware pipeline, `IHttpClientFactory`, `IObservable<T>`, `BackgroundService.ExecuteAsync`, `IComparer<T>`, lambdas in LINQ, `IEqualityComparer<T>`, `BufferedStream`, `ILoggerFactory`, `DbContext` and `DbSet<T>` in EF Core, `StreamReader`, gRPC clients, `IEnumerable<T>`, `TextWriter` on top of a `Stream`, `StringBuilder`, `GZipStream`, `yield return`, C# events, `INotifyPropertyChanged`, `DelegatingHandler`, `WebApplication.CreateBuilder()`.

Patterns: Iterator, Observer, Decorator, Builder, Factory, Chain of Responsibility, Template Method, Strategy, Unit of Work, Repository, Adapter, Proxy.

Tasks, building in depth:

1. Match each feature to a pattern.
2. For three features of your choice, explain _how_ the feature implements the pattern, with a short code example.
3. Some features can be read as more than one pattern (`DelegatingHandler`, `StringBuilder`, `StreamReader`, `DbSet<T>`). Pick one and argue for both readings, then say which you find more convincing.
4. Find one more pattern in a .NET library or your own earlier projects that isn't on the list.

The solution's `NOTES.md` gives the matches and these nuances: `StringBuilder` is a loose Builder (a mutable buffer, no director), while `WebApplication.CreateBuilder()` is the clearer example. `StreamReader` and `StreamWriter` are Adapters from bytes to text, and `TextWriter` is only the abstract base. `DelegatingHandler` is also Decorator-like. gRPC clients are a remote Proxy, EF Core lazy loading a virtual Proxy. `DbSet<T>` is Repository-like, which is why adding your own repository on top of EF Core is debated.

### 3. Pick the pattern

Scenario-based questions in markdown (no starter code required). Students choose a pattern and justify the choice, including trade-offs and alternatives.

### 4. Build from scratch

Students implement a pattern in a small domain, e.g. a pizza order Builder, a notification Observer, a payment provider Adapter.

### 5. Overuse critique

Show code where a pattern makes things worse (e.g. a factory of factories for a trivial object, a repository wrapping EF Core with no added value). Students critique it and simplify it.

### 6. Change request

Give a working app plus a new requirement. Students first try to add the feature, then reflect on whether the design coped. Follow up with a refactor that makes the change easy.

## Self-study: Patterns in the wild

Write `docs/02-patterns-in-the-wild.md` with two sections, each ending in a short task.

### Part A: Patterns that came after the Gang of Four (1994)

Cover briefly what each is, where it came from, and why it matters in .NET:

- Dependency Injection / Inversion of Control
- Repository and Unit of Work (Martin Fowler, Patterns of Enterprise Application Architecture)
- CQRS and Mediator pipelines
- Options pattern (IOptions\<T>)
- Result pattern (instead of exceptions for control flow)
- Resilience patterns: Retry, Circuit Breaker (Polly / Microsoft.Extensions.Resilience) Also mention how C# language features absorbed classic patterns: delegates/lambdas vs Strategy and Command, events vs Observer, IEnumerable/yield vs Iterator, Lazy\<T> and DI singletons vs Singleton.

### Part B: Patterns already built into .NET

- EF Core: DbContext = Unit of Work, DbSet = Repository-like, change tracking = Identity Map, lazy loading = Proxy.
- LINQ: Iterator, expression trees, query provider.
- Events / IObservable: Observer.
- ASP.NET Core middleware: Chain of Responsibility (and Decorator-like behaviour).
- HttpClientFactory: Factory. WebApplication.CreateBuilder: Builder.
- Built-in DI container: Service Locator vs DI discussion.

### Task

Students pick one .NET feature they use daily and write a short explanation of which patterns it uses and why the designers chose them.

## Solutions

- Put solutions on a separate `solutions` branch, mirroring the exercise structure, so the teacher can reveal them later.
- Each solution includes a short `NOTES.md` explaining the reasoning and alternative valid approaches.

### Answer key

Questions with fairly clear answers get an answer key on the main branch, so students can check their own work.

- Put answer keys in `/answers/`, mirroring the exercise folders (e.g. `/answers/02-spot-the-pattern/dotnet-pattern-match.md`) and `/answers/docs/` for questions in the self-study docs.
- Cover: pattern matching, "pick the pattern" scenarios, and reflection or self-study questions with a clear answer.
- For open-ended questions, give an example answer plus "things a good answer considers" instead of one right answer.
- Explain _why_ each answer is right, not just what it is.
- End each exercise README with a 🔑 link to its answer key, with a nudge to try the exercise first.
- Full code solutions stay on the `solutions` branch.

## Working instructions for Claude Code

1. Start by creating the solution, folder structure and root README.
2. Then write `docs/` content.
3. Then build exercises one type at a time, verifying each builds and tests pass.
4. Finally create the `solutions` branch.
5. Keep code idiomatic modern C# (file-scoped namespaces, records where sensible, nullable enabled).
6. Ask the teacher before adding patterns outside the core list.
