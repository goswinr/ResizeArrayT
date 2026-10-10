# Project guidelines

ResizeArrayT is an F# module and extensions library for `ResizeArray` (`System.Collections.Generic.List<'T>`) for .NET and JavaScript/TypeScript via Fable. Keep changes small, targeted, and consistent with the F# style in `Src/`. See `README.md` for usage and API examples.

## Architecture and code style

Preserve the compilation order in `Src/ResizeArrayT.fsproj`:

1. `Src/Exceptions.fs`: `ResizeArrayTArgumentException`, `ResizeArrayTArgumentNullException` and `ResizeArrayTKeyNotFoundException`. On .NET they inherit from the matching .NET exception types; in Fable from `Exception`.
2. `Src/Util.fs`: the `Operators` module (`+++`, `++`) and the hidden `UtilResizeArray` module with index normalization (`negIdx`, `negIdxLooped`), float min/max helpers, and the exception helpers `nullExn`, `badGetExn`, `badSetExn`, `badCountExn`, `fail` and `failSimple`.
3. `Src/Extensions.fs`: auto-opened extension members on `ResizeArray` (`AutoOpenResizeArrayExtensions`).
4. `Src/ComputationalExpression.fs`: the auto-opened `resizeArray { ... }` computation expression builder.
5. `Src/MinMax.fs`: internal min/max helpers with the NaN rules described in its header comment.
6. `Src/Module.fs`: `Array.asResizeArray` and the main `ResizeArray` module with all `FSharp.Core` `Array` module functions plus extras.
7. `Src/Parallel.fs`: `ResizeArray.Parallel` inside the auto-opened `ResizeArrayParallel` module. .NET only, wrapped in `#if !FABLE_COMPILER`.

Opening the `ResizeArrayT` namespace exposes the `ResizeArray` module, `ResizeArray.Parallel`, the `resizeArray` computation expression, and the extension members.

- Do not reorder source files unless the change requires it. All `.fs` files in `Src/` are packed into the NuGet package for Fable.
- Keep public API names and behavior aligned with the `FSharp.Core` `Array` and `Array.Parallel` modules where applicable.
- Preserve public API behavior across .NET and Fable targets. Fable-specific code uses `#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT` (or `#if FABLE_COMPILER`). Array/ResizeArray casts such as `Array.asResizeArray` are only valid in Fable, where both are JavaScript arrays.
- Check inputs for null and raise errors through the `UtilResizeArray` helpers, not ad-hoc exceptions. Messages start with `ResizeArray.<function>:`; index and bounds errors include the index and the collection content. Index errors stay `IndexOutOfRangeException`.
- Do not return null from library functions. Raise an exception or offer a `try*` function returning an option.
- Add XML docs to every public API. The build enables warning 3390 and level 5 warnings, and the docs build runs with `--strict`.
- Edit only sources in `Src/` and `Tests/`, never generated output in `bin/`, `obj/`, `Tests/_js/`, `Tests/_ts/`, `Tests/_tscBuild/` or `Docs/index.md` (copied from `README.md` on build).
- The package version comes from `CHANGELOG.md` via `Ionide.KeepAChangelog.Tasks`. Keep every changelog bullet on a single line and keep the file's line endings uniform, or the build fails.

## Build and test

Run builds from the repository root with the .NET 10 SDK. The library targets `net8.0`, `net10.0` and `net472`; tests target `net8.0` and `net10.0`. Install the .NET 8 runtime too. FsDocs uses the first library target, `net8.0`. Building the library also creates the NuGet package.

```bash
dotnet build ResizeArray.sln
dotnet build Src/ResizeArrayT.fsproj --configuration Release
```

Run tests from `Tests/`:

```bash
dotnet run --framework net8.0   # .NET 8 tests
dotnet run --framework net10.0  # .NET 10 tests
npm test                        # JavaScript tests via Fable and Node.js, then TypeScript compilation of the library
npm run buildTS                 # TypeScript compilation of the library only
npm run watchTS                 # Watch mode for TypeScript development
```

For a first JavaScript test run or a clean environment, run `dotnet tool restore` from the repository root and `npm ci` in `Tests/`. CI (`.github/workflows/test.yml`) runs exactly these test steps; `build.yml` runs `dotnet restore` and `dotnet build --configuration Release --no-restore`.

To build the docs as CI does:

```bash
dotnet build Src/ResizeArrayT.fsproj -c Release --framework net8.0 -p:GeneratePackageOnBuild=false
dotnet fsdocs build --clean --strict --properties Configuration=Release TargetFramework=net8.0 --input Docs --output DocsGenerated
```

## Test conventions

The .NET and JavaScript targets share the test definitions in `Tests/`. Their single entry point is `Tests/Main.fs`, which calls `runTests` once with all test lists; add new test lists there. `Tests/ParallelTests.fs` is .NET only. Tests use `<LangVersion>preview</LangVersion>` for `^` indexing from the end; the library does not.

Tests use [Scriptorium](https://fable-hub.github.io/Scriptorium/):

- `Scriptorium.Quill` (`open type Scriptorium.Quill.Test`) supplies `testList ("name", [ ... ])`, `test ("name", fun _ -> ...)`, and `Runner.runTests`.
- `Scriptorium.Nib` (`open Scriptorium.Nib.Assertion`) supplies assertions such as `assertThat result (tag "message" >> isEqualTo expected)`, `assertThat (fun () -> ...) (tag "message" >> throws)`, and `assertThat flag isTrue`.
- `Tests/Module.fs` defines shared helpers: an `Assert` shim, the `Exceptions` module (`throwsArg`, `throwsIdx`, `throwsNull`, `throwsKey`, `throwsWith`, `CheckThrowsExn`), and `.asRarr` on arrays. `CheckThrowsExn` checks the exception type only on .NET; in Fable it only checks that something is thrown.
- Test names must be unique within a list; duplicate paths are rejected.
- There is no CLI `--filter`. To run a subset temporarily, use `ftest` / `ftestList` or `xtest` / `xtestList`. Focused tests fail the CI run, so remove focus markers afterward.
- When changing behavior in Fable-specific branches, add or update shared tests and run both the .NET and JavaScript tests.
