# Project Guidelines

## Code Style
- Language: F# with preview language features enabled in project files.
- Preserve existing module/file structure in Src: Util -> Extensions -> ComputationalExpression -> Module.
- Keep public API names and behavior aligned with FSharp.Core Array module semantics where applicable.
- Keep functions and extension members null-safe: use existing utility exception helpers instead of ad-hoc errors.
- Prefer existing conditional compilation symbols used in this repo:
  - FABLE_COMPILER_JAVASCRIPT
  - FABLE_COMPILER_TYPESCRIPT
- Follow existing style: concise inline helpers, XML docs on public APIs, and descriptive exception messages.

## Architecture
- Main library lives in Src and targets net6.0 + net472.
- Core boundaries:
  - Src/Util.fs: shared helpers, index normalization, and exception formatting.
  - Src/Extensions.fs: AutoOpen extension members on ResizeArray/List.
  - Src/ComputationalExpression.fs: resizeArray computation expression builder.
  - Src/Module.fs: main ResizeArray module functions (Array-module-compatible API + extras).
- Tests live in Tests and are cross-runtime:
  - .NET path uses Expecto.
  - JS/TS path uses Fable.Mocha.
  - Entry dispatch is controlled in Tests/Main.fs with compile-time directives.

## Build And Test
- Use CI-verified commands by default.

### Repository root
- dotnet restore
- dotnet build --configuration Release --no-restore

### .NET tests
- cd Tests
- dotnet run

### JS/TS tests
- cd Tests
- dotnet tool restore
- npm ci
- npm test

## Conventions
- Do not return null from library functions. Use exceptions or option-returning try* APIs.
- Keep rich error diagnostics for indexing and bounds errors (include index and collection context).
- Maintain Fable compatibility when adding/changing APIs:
  - Keep conditional branches for .NET vs Fable where performance/representation differs.
  - Be careful with array/list casting optimizations that are valid only in Fable JS/TS.
- When adding tests, ensure both .NET and Fable/JS paths still compile and run.
- Prefer editing source files under Src and Tests; do not modify generated outputs under bin, obj, Tests/js, or Tests/ts unless the task explicitly requires generated artifacts.


