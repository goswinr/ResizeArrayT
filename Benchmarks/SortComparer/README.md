# ResizeArray.sort comparer benchmark

[All sorting findings and conclusions](../README.md)

Compares the original generic `Sort(Operators.compare)` implementation with
`Sort(LanguagePrimitives.FastGenericComparer<'T>)`. Both include the same
`GetRange` copy and a null check, and neither is inline. The benchmark also checks
the actual library's `ResizeArray.sort` against the baseline.

Run from the repository root:

```powershell
dotnet build Benchmarks/SortComparer/SortComparer.fsproj -c Release
dotnet Benchmarks/SortComparer/bin/Release/net8.0/SortComparer.dll
dotnet Benchmarks/SortComparer/bin/Release/net10.0/SortComparer.dll
dotnet fable Benchmarks/SortComparer/SortComparer.fsproj --outDir Benchmarks/SortComparer/_js --noCache
node Benchmarks/SortComparer/_js/Program.js
```

Run one benchmark process at a time. No additional npm dependencies are needed.

## Method

- Deterministic, identical input on both runtimes; 16, 1,024 and 10,000 elements.
- Integers, floats, strings, tuples and records; additionally sorted, reversed
  and equal integer inputs.
- 300 ms warmup per method and case, then independent batch calibration to at
  least 60 ms. Nine measured batches per method, alternating execution order.
- Reports median milliseconds per copy-and-sort, baseline/candidate speedup,
  and .NET bytes allocated per operation (ten additional operations).
- Checks ordering equivalence, input preservation and a fresh output collection,
  including empty/singleton inputs, null/ordinal strings, NaN/infinities,
  options and nested arrays. Correctness checks are outside the timed region.

This is a local microbenchmark, not a statistical performance guarantee. Results
depend on hardware, runtime, element type, size and input order. Fable results
are Node.js measurements, not browser measurements.

## Findings (2026-09-29)

Windows 11 x64, Intel Core i5-14600; .NET SDK 10.0.401, FSharp.Core 6.0.7
(matching the library), .NET 8.0.31 / 10.0.12, Fable 5.15.0, Node.js 26.7.0.

10,000 random elements, median milliseconds per operation:

| Type | .NET 8 compare → fast | .NET 10 compare → fast | Fable/Node compare → fast |
| --- | ---: | ---: | ---: |
| int | 4.166 → 0.487 (8.55×) | 4.370 → 0.508 (8.61×) | 2.350 → 2.629 (12% longer) |
| float | 4.747 → 0.546 (8.69×) | 4.801 → 0.530 (9.06×) | 2.606 → 2.939 (13% longer) |
| string | 1.964 → 1.209 (1.62×) | 1.857 → 1.134 (1.64×) | 3.975 → 4.291 (8% longer) |
| tuple | 6.513 → 6.333 (1.03×) | 6.004 → 5.850 (1.03×) | 4.788 → 5.198 (9% longer) |
| record | 2.750 → 2.548 (1.08×) | 2.373 → 2.222 (1.07×) | 7.345 → 8.176 (11% longer) |

Full measurements: [net8](results-net8.csv), [net10](results-net10.csv),
[Fable/Node](results-fable.csv). Speedup is baseline time divided by candidate
time; greater than 1 means the candidate is faster. Tiny differences for
structural values should be treated cautiously.

The .NET path benefits substantially for primitive types. For 10,000 random
integers, allocated bytes fall from 7,387,528 to 40,120 per operation on .NET 8.
Tuples and records gain much less.

Fable's generated baseline calls `result.sort((x, y) => compare(x, y) | 0)`.
The candidate calls `sortInPlace(result, LanguagePrimitives_FastGenericComparer())`.
That creates a structural comparer object and forwards each comparison through
its `Compare` method to the same generic `compare`. It has no primitive type
specialization here and measured slower.

Consequently, `ResizeArray.sort` uses `FastGenericComparer` only on .NET and
retains `Operators.compare` under `FABLE_COMPILER`. The same choice is also used
by the generic `ResizeArray.sortInPlace` function.

## Plain JavaScript versus Fable

After compiling the benchmark with Fable as above, run:

```powershell
node Benchmarks/SortComparer/native-js.mjs
```

This compares the generated library's `ResizeArray.sort`, handwritten JavaScript,
and Fable-compiled `ResizeArray.sortWith` with a typed `compare` callback. It uses
the same seeded inputs, warmup, batch calibration and nine samples, rotating the
order of the three methods. Every method copies the input before sorting, and
every result is checked for equal ordering, input preservation and a new array.

The handwritten implementations are:

```javascript
numbers.slice().sort((a, b) => a - b);
strings.slice().sort();
```

Measured numbers are finite and strings are non-null. These implementations are
not replacements for arbitrary F# structural comparison: default JS sorting is
lexicographic even for numbers, subtraction does not order NaN like F# comparison,
and default string sorting would stringify null. See
[JavaScript sort semantics](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/sort).

Fable's generic sort already uses native `Array.sort`, with its generic comparison
function as the callback. That callback checks for null, `IComparable`, arrays
and other value types before reaching primitive comparison. A typed F# callback
compiles to `comparePrimitives`, avoiding that generic dispatch, although
`sortWith` still adds a callback wrapper. The handwritten comparators need less
work for these restricted input types.

Same machine and Node/Fable versions as above, 10,000 random elements, milliseconds
per copy-and-sort (a separate run; compare methods within this table):

| Type | Fable generic | Plain JS | Fable typed callback | Plain JS speedup |
| --- | ---: | ---: | ---: | ---: |
| int | 1.886 | 1.392 | 1.710 | 1.35× |
| float | 2.665 | 1.945 | 2.021 | 1.37× |
| string | 3.539 | 1.713 | 3.006 | 2.07× |

All ordering and copy checks passed. Full measurements:
[native JS comparison](results-native-js.csv). The typed F# float callback was
1.32× faster than generic sorting, and within 4% of the handwritten JS time:

```fsharp
ResizeArray.sortWith (fun (a: float) b -> compare a b) values
```

The library implementation is unchanged by this additional experiment.

## Dedicated sortInt and sortFloat prototypes

`TypedSort.fs` contains benchmark-only prototypes with explicit numeric types.
Run after building/compiling as above:

```powershell
dotnet Benchmarks/SortComparer/bin/Release/net8.0/SortComparer.dll --typed
dotnet Benchmarks/SortComparer/bin/Release/net10.0/SortComparer.dll --typed
node Benchmarks/SortComparer/_js/Program.js --typed
```

The four methods are the current generic library sort (already optimized with
`FastGenericComparer` on .NET), typed `sortWith`, a direct typed F# comparison,
and the specialized prototype. The prototype uses `List<int/double>.Sort()` on
.NET and emitted JS comparators in Fable:

```javascript
// int: the subtraction is JS number arithmetic, avoiding Int32 overflow.
result.sort((a, b) => a - b);
// float: NaN first; NaNs, equal infinities and signed zeros compare equal.
result.sort((a, b) =>
    a === b ? 0 : a !== a ? (b !== b ? 0 : -1) : b !== b ? 1 : a - b);
```

Every method includes copying. The same warmup/calibration/nine-sample procedure
is used, rotating all four methods. Checks include null input rejection,
empty/singleton arrays, every pair of numeric boundary values, and mixed float
inputs containing NaN, infinities, signed zeros and subnormals. A timed case with
10% NaN values supplements the finite random-input cases. Results are compared
using F# comparison, since NaN does not compare equal with ordinary equality.

Results from this run, 10,000 random finite elements, milliseconds per
copy-and-sort. The recommended typed path is `Sort()` on .NET and direct typed
F# `compare` on Fable (`directTyped` in the raw results):

| Type | .NET 8 generic → typed | .NET 10 generic → typed | Fable generic → typed |
| --- | ---: | ---: | ---: |
| int | 0.424 → 0.333 (1.27×) | 0.389 → 0.315 (1.24×) | 1.593 → 1.385 (1.15×) |
| float | 0.394 → 0.354 (1.11×) | 0.379 → 0.363 (1.05×) | 1.739 → 1.460 (1.19×) |

With 10% NaNs, float speedups were 1.33× on .NET 8, 1.23× on .NET 10 and
1.22× on Fable. The handwritten JS float comparator with NaN handling was
approximately as fast as generic Fable sorting (1.753 ms for finite values),
so it offers no advantage over the simpler direct typed F# comparison.
For integers, direct typed F# comparison (1.385 ms) was effectively tied with
emitted JS subtraction (1.381 ms).

Simply forwarding these functions to typed `sortWith` is not equivalent to
these implementations: it adds comparator indirection and performed worse,
particularly on .NET. The production implementation for each numeric
type copies the collection, then uses `Sort(compare)` with the type already
known under `FABLE_COMPILER`, and parameterless `Sort()` on .NET.

All edge-case checks passed on all three runtimes. Full results:
[.NET 8](results-typed-net8.csv), [.NET 10](results-typed-net10.csv),
[Fable](results-typed-fable.csv). These are measurements of benchmark prototypes.
The public API now includes `ResizeArray.sortInt`, `sortFloat`, `sortInPlaceInt`
and `sortInPlaceFloat`, using parameterless `Sort()` on .NET and direct typed
F# comparison on Fable. The handwritten-JS prototypes remain here for comparison.
Small gains such as 1.05× should be
treated cautiously, and absolute timings should not be compared between the
separate benchmark experiments: JIT context and machine conditions vary.
