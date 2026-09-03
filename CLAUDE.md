# CLAUDE.md

Notes for working in this repo.

## CHANGELOG.md formatting

`Src/ResizeArrayT.fsproj` runs the `Ionide.KeepAChangelog.Tasks` MSBuild task on every build
(`dotnet build`, `dotnet run` in `Tests/`, packing) to parse `CHANGELOG.md` and stamp the
package version. Its parser is strict and has no useful error message when it chokes — a bad
entry throws `System.ArgumentOutOfRangeException` from deep inside
`KeepAChangelogParser.ChangelogParser`, failing the build for every target framework at once,
which looks unrelated to the actual change.

Rules that keep it working:
- Every changelog entry must be a **single-line bullet** (`- like this`). Do **not** wrap a
  bullet onto a continuation line indented under the `-` — the parser cannot handle that and
  throws. Split long entries into multiple separate `-` bullets instead of wrapping one.
- Keep the `## [Unreleased]` section (and each `## [x.y.z] - yyyy-mm-dd` section) using the
  existing `### Added` / `### Changed` / `### Fixed` subheadings already used further down the
  file.

Before pushing a changelog edit, sanity-check it locally with `dotnet build Src/ResizeArrayT.fsproj`
(or `dotnet run` from `Tests/`) — if the changelog is malformed, this fails fast with the
`ParseChangeLogs` stack trace above, before CI does.

## Testing

- `.NET`: `cd Tests && dotnet run` (Expecto).
- `JavaScript` (Fable): `cd Tests && dotnet tool restore && npm ci && npm test` — compiles via
  Fable to `_js` and runs with mocha. Do this in addition to the .NET run before pushing changes
  to `Src/Module.fs`: Fable does not always compile generic F# code the same way .NET does. In
  particular, generic `Unchecked.defaultof<'T>` inside a library function whose type parameter
  isn't resolved at that call site compiles to `null` under Fable even for numeric `'T`, whereas
  the same expression written with a concrete type argument (e.g. in a test) can compile to the
  type's real zero value. Don't assert exact `Unchecked.defaultof<'T>`-derived values across both
  runtimes in tests; assert structural properties (length, self-consistency) instead.
