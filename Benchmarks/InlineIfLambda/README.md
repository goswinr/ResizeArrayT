# InlineIfLambda and OptimizeClosureIfNotInlined

Would ResizeArray functions get faster if they were written like the
[FSharp.Core 11 Array module](https://github.com/dotnet/fsharp/blob/main/src/FSharp.Core/array.fs)?
There, functions such as `Array.fold`, `Array.iteri` and `Array.exists` are `inline` and
mark their callback with `[<InlineIfLambda>]`. Callbacks with 2 to 5 arguments are also
marked with `[<OptimizeClosureIfNotInlined>]`, which is new in F# 11
([dotnet/fsharp#20422](https://github.com/dotnet/fsharp/pull/20422)).

- `[<InlineIfLambda>]`: if the argument at the call site is a lambda, its body is inlined into
  the loop. There is no closure allocation and no virtual call per element.
- `[<OptimizeClosureIfNotInlined>]`: if the argument is *not* a lambda (opaque), the F# 11
  optimizer of the calling code adapts it once with `OptimizedClosures.FSharpFunc.Adapt` before
  the loop. Without it, an inlined `f a b` of an opaque function goes through
  `FSharpFunc.InvokeFast` for every element.

## What is compared

- `current`: the ResizeArrayT library as it was when measured, see the table below.
- `inlineIfLambda`: `inline` with `[<InlineIfLambda>]`, a plain F# loop, see [Prototypes.fs](Prototypes.fs).
- `optimizeClosure`: as `inlineIfLambda`, plus `[<OptimizeClosureIfNotInlined>]`. Only for the
  functions with two-argument callbacks: `fold`, `iteri`, `mapi` and `map2`.

| Function | Current .NET implementation | Current Fable implementation |
| --- | --- | --- |
| `iter`, `sumBy` | `inline` loop, no attribute | same |
| `map` | `inline`, `ConvertAll` with a `Converter` delegate | `inline`, native JS `map` |
| `filter` | `inline`, `FindAll` with a `Predicate` delegate | JS `filter` via Fable's `findAll` |
| `exists` | not inline, `Exists` with a `Predicate` delegate | JS `findIndex` |
| `fold`, `mapi`, `map2` | not inline, `Adapt` once, then `Invoke` | not inline, loop |
| `iteri` | `inline`, explicit `Adapt` | same |

Each is called from three kinds of call sites:

- `lambda`: a small lambda that captures nothing, e.g. `fold (fun acc x -> acc + x) 0 xs`.
- `closure`: a lambda that captures a local value, e.g. `fold (fun acc x -> acc + x + k) 0 xs`.
- `opaque`: a function value received as a parameter by a function that is not inlined
  (`[<NoCompilerInlining>]`), as in code that passes a callback on. The closure is created once.

## Run

The F# code must be compiled with the .NET 11 SDK (F# 11). An older compiler silently ignores
`OptimizeClosureIfNotInlined`, so the project fails the build if the SDK is older. The benchmark
references FSharp.Core 11.0.101, which defines the attribute, and runs on .NET 8 and .NET 10.
From the repository root:

```powershell
dotnet build Benchmarks/InlineIfLambda/InlineIfLambda.fsproj -c Release
dotnet Benchmarks/InlineIfLambda/bin/Release/net8.0/InlineIfLambda.dll
dotnet Benchmarks/InlineIfLambda/bin/Release/net10.0/InlineIfLambda.dll
$env:DOTNET_TieredPGO=0; dotnet Benchmarks/InlineIfLambda/bin/Release/net10.0/InlineIfLambda.dll; Remove-Item Env:DOTNET_TieredPGO
dotnet fable Benchmarks/InlineIfLambda/InlineIfLambda.fsproj --outDir Benchmarks/InlineIfLambda/_js --noCache
node Benchmarks/InlineIfLambda/_js/Program.js
```

A full run takes about 30 seconds. Function names as arguments select a subset, for example
`dotnet Benchmarks/InlineIfLambda/bin/Release/net10.0/InlineIfLambda.dll fold map2`.
Run one benchmark process at a time. No npm installation is needed.
The CSV files here combine three runs each:

```powershell
node Benchmarks/InlineIfLambda/combine.mjs run1.csv run2.csv run3.csv > Benchmarks/InlineIfLambda/results-net10.csv
```

## Method

- Integers from 0 to 999, deterministic, in ResizeArrays of 10 and 10,000 elements.
- First all methods of all cases are called in turn for 1 second. Without this, the JIT
  (dynamic PGO in .NET, inline caches in V8) would optimize a shared, not inlined library function
  like `ResizeArray.fold` for the callback of whichever case runs first. In an application such a
  function is called with many different callbacks. The inlined variants have their own copy of
  the loop at each call site.
- Then per case and method: 30 ms warmup, batches calibrated to at least 10 ms, 11 measured
  batches with the method order rotated. The result is the median time per call in nanoseconds.
- Bytes allocated per call are measured on .NET with `GC.GetAllocatedBytesForCurrentThread`.
- Every method is checked against the FSharp.Core `Array` function outside the timed section.
- On .NET the program inspects the IL of the opaque helpers and prints whether they call
  `OptimizedClosures.FSharpFunc.Adapt`: `fold=false/true` means the `inlineIfLambda` helper does
  not and the `optimizeClosure` helper does. It prints a warning if the attribute had no effect.
- Each CSV row is the median of three separate processes. `ns_min` and `ns_max` are the fastest
  and slowest of the three. `relative_to_current` is the median time divided by that of `current`;
  less than 1 is faster.

For `lambda` and `closure` call sites, `inlineIfLambda` and `optimizeClosure` compile to the same
code. Their differences, and the spread between runs, show the noise. It is largest for `iter` and
`iteri`, whose callbacks write a static field for each element. Differences below about 1.3× are
within noise on this machine.

## Results (2026-11-21)

Windows 11 x64, Intel Core i5-1135G7 (laptop), .NET SDK 11.0.100 (F# 11), FSharp.Core 11.0.101,
.NET 8.0.34 and 10.0.15, Fable 5.27.0, Node.js 26.12.1. "No PGO" is .NET 10 with
`DOTNET_TieredPGO=0`, similar to runtimes without dynamic PGO, such as .NET Framework.

Time relative to `current` for 10,000 elements, less than 1 is faster.
For two-argument callbacks the cells are `inlineIfLambda / optimizeClosure`.

| Function | Call site | .NET 8 | .NET 10 | .NET 10, no PGO | Fable |
| --- | --- | ---: | ---: | ---: | ---: |
| fold | lambda | 0.27 / 0.29 | 0.21 / 0.19 | 0.40 / 0.41 | 0.67 / 0.71 |
| fold | closure | 0.46 / 0.46 | 0.29 / 0.29 | 0.44 / 0.45 | 0.57 / 0.60 |
| fold | opaque | **4.60** / 1.01 | **4.62** / 1.02 | **2.36** / 0.91 | 0.84 / 0.83 |
| iteri | lambda | 0.84 / 0.72 | 1.04 / 0.70 | 0.65 / 0.73 | 1.18 / 1.04 |
| iteri | closure | 0.82 / 0.67 | 1.18 / 1.29 | 0.67 / 0.49 | 0.99 / 0.88 |
| iteri | opaque | **3.69** / 1.56 | **2.27** / 0.91 | **1.87** / 0.99 | 0.88 / 1.03 |
| mapi | lambda | 0.79 / 0.81 | 0.71 / 0.72 | 0.94 / 0.94 | 0.94 / 0.84 |
| mapi | closure | 0.74 / 0.77 | 0.74 / 0.73 | 0.86 / 0.83 | 0.73 / 0.71 |
| mapi | opaque | **2.24** / 1.02 | **2.35** / 0.98 | **1.97** / 0.98 | 0.80 / 0.80 |
| map2 | lambda | 0.73 / 0.74 | 0.77 / 0.75 | 0.87 / 0.87 | 0.78 / 0.78 |
| map2 | closure | 0.77 / 0.77 | 0.78 / 0.77 | 0.91 / 0.85 | 0.72 / 0.68 |
| map2 | opaque | **2.31** / 0.91 | **1.88** / 0.89 | **1.72** / 1.01 | 0.81 / 0.75 |

| Function | Call site | .NET 8 | .NET 10 | .NET 10, no PGO | Fable |
| --- | --- | ---: | ---: | ---: | ---: |
| map | lambda | 0.79 | 0.64 | 0.66 | **3.40** |
| map | closure | 0.89 | 0.87 | 0.84 | **3.16** |
| map | opaque | 0.69 | 0.66 | 0.85 | 1.35 |
| filter | lambda | 0.47 | 0.38 | 0.52 | 1.53 |
| filter | closure | 0.61 | 0.49 | 0.62 | 1.05 |
| filter | opaque | 0.60 | 0.43 | 0.72 | 1.22 |
| exists | lambda | 0.49 | 0.38 | 0.35 | **7.05** |
| exists | closure | 0.45 | 0.35 | 0.34 | **4.96** |
| exists | opaque | 0.50 | 0.40 | 0.66 | 1.25 |
| iter | lambda | 0.97 | 0.94 | 1.00 | 0.95 |
| iter | closure | 0.92 | 1.07 | 0.80 | 1.00 |
| iter | opaque | 0.93 | 0.90 | 1.09 | 0.80 |
| sumBy | lambda | 0.93 | 1.02 | 1.03 | 0.91 |
| sumBy | closure | 0.95 | 0.95 | 0.96 | 1.15 |
| sumBy | opaque | 0.94 | 0.99 | 0.82 | 1.00 |

The 10-element results point the same way, with `fold` opaque at 2.7× (1.7× without PGO) for
`inlineIfLambda` and 0.92 to 0.96 for `optimizeClosure`. Allocations per call on .NET 8, 10 elements:

| Function | Call site | current | inlined |
| --- | --- | ---: | ---: |
| exists | lambda / closure | 88 / 112 B | 0 B |
| filter | lambda / closure | 96 / 120 B | 32 B |
| map | lambda / closure | 160 / 184 B | 96 B |
| fold, iteri | closure | 24 B | 0 B |
| mapi, map2 | closure | 120 B | 96 B |

The inlined bytes are the result ResizeArray. The `Predicate` and `Converter` delegates for
`Exists`, `FindAll` and `ConvertAll` cost 64 bytes, 88 bytes when they wrap a function parameter.
A capturing closure costs 24 bytes. .NET 10 without PGO allocates the same. .NET 10 with PGO can
avoid some of the delegates by escape analysis, for example 24 instead of 88 bytes for `exists`,
but never allocates less than the inlined versions.
Full results: [.NET 8](results-net8.csv), [.NET 10](results-net10.csv),
[.NET 10 without PGO](results-net10-nopgo.csv), [Fable](results-fable.csv).

## Conclusions

1. **Two-argument callbacks need both attributes.** With `[<InlineIfLambda>]` alone, opaque
   callbacks get 1.5× to 4.6× slower than today, because each element goes through
   `InvokeFast`. With `[<OptimizeClosureIfNotInlined>]` added, opaque callbacks are as fast as
   today (0.9 to 1.1; the 1.56 for `iteri` on .NET 8 is within its noise), and lambda call sites
   keep the gains of inlining: `fold` 2.2× to 5×, `mapi` and `map2` 1.1× to 1.4×.
2. **Delegate-based functions gain on .NET for every call site.** `exists`, `filter` and `map` as
   inlined F# loops are 1.1× to 3.4× faster than `Exists`, `FindAll` and `ConvertAll`, also for
   opaque callbacks, and allocate no delegate or closure.
3. **Functions that are already inline gain nothing measurable.** For `iter` and `sumBy` the F# 11
   optimizer already inlines a lambda that the inline function calls once (0 bytes allocated).
   This was also checked with larger, multi-statement lambdas. The attribute only guarantees it.
4. **Fable ignores `OptimizeClosureIfNotInlined`.** Fable 5.27 does not know the attribute, and JS
   functions take all arguments at once anyway. Inlining makes F# loops faster in Fable as well:
   `fold` 1.4× to 1.75×, `mapi` and `map2` 1.1× to 1.7×. But F# loops are up to 7× slower than
   the native JS array methods that `map`, `filter` and `exists` use today, because Fable compiles
   `xs.[i]` to a bounds-checked `item(i, xs)` call. The Fable implementations of these should stay.
5. **The gains do not depend on dynamic PGO.** With PGO the JIT can devirtualize the callback of a
   shared, not inlined function for one dominant closure type. Without PGO, inlining still gains,
   for example 2.2× to 3× for `fold`, and `optimizeClosure` still keeps opaque callbacks as fast
   as today.

## Requirements for using OptimizeClosureIfNotInlined in the library

- The attribute must come from FSharp.Core: the compiler recognizes it only as an FSharp.Core
  type, so the library cannot define its own copy. The library's FSharp.Core reference would rise
  from 6.0.7 to at least 11.0.100, and that becomes the minimum for every consumer.
- The library must be built with F# 11 (.NET 11 SDK). The feature is gated by language version.
- Consumers whose compiler is older than F# 11 get only the `InlineIfLambda` behavior. For them,
  opaque two-argument callbacks would be 1.5× to 4.6× slower than today.
- `[<InlineIfLambda>]` alone needs FSharp.Core 6.0, the current minimum.
- As with every `inline` function, the code grows at each call site, and consumers must recompile
  to get changes of the function body.

## Recommendation

1. **Without a dependency change (done, see the changelog):** make the .NET implementations of the
   functions with one-argument callbacks that use delegates `inline` loops with `[<InlineIfLambda>]`:
   `exists`, `filter` (and its alias `where`) and `map`. Keep their Fable implementations with
   native JS array methods. The results above were measured before this change; `current` for
   these functions now runs the same code as `inlineIfLambda`.
   `forall`, `find`, `findIndex` and `tryFind` follow the same pattern and likely gain the same
   way, but were not measured.
2. **When FSharp.Core 11 can be the minimum:** make the functions with two-argument callbacks
   (`fold`, `iteri`, `mapi`, `map2` and similar) `inline` with both
   `[<InlineIfLambda; OptimizeClosureIfNotInlined>]`, as FSharp.Core 11 does. Until then keep them
   as they are: `[<InlineIfLambda>]` alone makes opaque callbacks 1.5× to 4.6× slower.

These are local microbenchmarks with integers, not performance guarantees. Node.js results are not
browser measurements.
