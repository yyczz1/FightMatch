## Architecture and Design Rules

This is a Unity project. Prefer simple, maintainable, testable code over speculative architecture.

### Core principles

- Follow SOLID as a review standard, not as a reason to add unnecessary abstractions.
- Prefer the existing project architecture over introducing a new architecture.
- If the project already uses QFramework, follow QFramework conventions.
- If the project already uses MVP/MVC-style organization, preserve that style.
- Do not mix QFramework, MVP, MVC, MVVM, ECS, or custom architecture layers unless there is a clear reason.
- Do not introduce a framework or design pattern just because it is generally “good”.
- Every abstraction must solve a real current problem in this project.

### Simplicity rule

Before adding an interface, base class, manager, service, factory, event bus, or design pattern, check:

1. Is there more than one implementation now, or very likely in the current task?
2. Does this reduce coupling or make testing significantly easier?
3. Does this match the existing project style?
4. Would the code be harder to understand without this abstraction?
5. Can the same result be achieved with a simpler class or method?

If the answer is mostly no, do not add the abstraction.

### Existing architecture priority

When editing existing code:

1. First inspect nearby files and naming conventions.
2. Match the existing folder structure, namespace style, lifecycle pattern, and dependency direction.
3. Do not refactor unrelated code.
4. Do not rename public classes, serialized fields, assets, prefabs, or scene objects unless required.
5. Keep changes surgical and directly tied to the task.

### QFramework guidance

If QFramework is already present and used:

- Put state/data in Models.
- Put business/domain operations in Systems.
- Put reusable stateless helpers in Utilities.
- Use Commands for user actions, editor actions, undoable operations, and state-changing workflows.
- Use Queries for read-only access.
- Use Events for decoupled notifications.
- Avoid direct View-to-Model mutation when a Command/System should handle it.
- Avoid using MonoBehaviour as a global god object.

If QFramework is not already used, do not add it unless explicitly requested.

### MVP/MVC guidance

If the project uses MVP or MVC:

- View should be passive and mainly handle Unity UI references, rendering, and user input forwarding.
- Presenter/Controller should coordinate view input, model updates, validation, and UI refresh.
- Model should contain data and domain rules, independent from Unity UI.
- Avoid putting business logic directly into MonoBehaviour views.
- Avoid having Model depend on View.

### Design pattern decision rules

Use design patterns only when they fit the current problem:

- Use State Pattern when behavior changes across clear states and if/else state logic is growing.
- Use Strategy Pattern when multiple interchangeable algorithms are needed, such as different solvers, generators, scoring rules, or difficulty evaluators.
- Use Command Pattern for undo/redo, editor actions, generation steps, user operations, or batch operations.
- Use Observer/Event when multiple independent systems need to react to a change.
- Use Factory only when object creation has meaningful variants or construction complexity.
- Use Adapter when integrating third-party APIs or framework-specific APIs behind a stable project interface.
- Use Service/Repository only for persistence, external APIs, or data access boundaries.

Avoid:

- Interfaces with only one implementation unless required for tests or architecture consistency.
- Abstract base classes for one-off behavior.
- Global Manager classes that own unrelated responsibilities.
- Singleton by default.
- Event bus for simple direct calls.
- CQRS-style separation unless QFramework already uses it naturally or read/write flows are genuinely complex.

### Required design note

For any non-trivial architectural decision, briefly state:

- What existing pattern or architecture was found.
- Whether a design pattern was used.
- Why it was used or why it was intentionally avoided.
- How the change was verified.

Keep the note short. Do not write long design documents unless requested.