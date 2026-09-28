# Sorting benchmarks and conclusions

Results collected on 2026-09-29 for ResizeArrayT on .NET and Fable. This overview
records the comparer investigation, native JavaScript comparison, typed numeric
sorts, and comparison with FSharp.Core's `Array.sort`.

## Implementation decisions

| API | .NET implementation | Fable implementation |
| --- | --- | --- |
| `ResizeArray.sort` | Copy, then `Sort(FastGenericComparer<'T>)` | Copy, then `Sort(Operators.compare)` |
| `ResizeArray.sortInPlace` | `Sort(FastGenericComparer<'T>)` | `Sort(Operators.compare)` |
| `ResizeArray.sortInt` / `sortFloat` | Copy, then parameterless `Sort()` | Copy, then `Sort(compare)` with the numeric type known |
| `ResizeArray.sortInPlaceInt` / `sortInPlaceFloat` | Parameterless `Sort()` | `Sort(compare)` with the numeric type known |

The generic comparer change provides a large .NET improvement for primitive
types but regresses Fable performance, so the generic functions use
`#if FABLE_COMPILER`. The numeric functions also use conditional compilation:
the .NET default numeric comparer and direct typed F# comparison in Fable were
the preferred implementations. No handwritten JavaScript is needed in the API.

The numeric API docstrings highlight the benefit over generic sorting in Fable.
Some .NET gains were also measured, but they were smaller, particularly for
floats, and should not be interpreted as a universal speedup guarantee.

All timed operations below include copying. The in-place APIs use the same
chosen comparison paths, but were not separately benchmarked; the reported
timings and ratios are for copy-and-sort operations.

## 1. FastGenericComparer versus Operators.compare

The original generic implementation copied the ResizeArray and passed
`Operators.compare` as a callback. The candidate passed
`LanguagePrimitives.FastGenericComparer<'T>` directly to `Sort`.

For 10,000 random elements:

| Type | .NET 8 speedup | .NET 10 speedup | Fable candidate time versus original |
| --- | ---: | ---: | ---: |
| int | 8.55× | 8.61× | 12% longer |
| float | 8.69× | 9.06× | 13% longer |
| string | 1.62× | 1.64× | 8% longer |
| tuple | 1.03× | 1.03× | 9% longer |
| record | 1.08× | 1.07× | 11% longer |

On .NET 8, allocations for sorting 10,000 random integers fell from **7,387,528
bytes to 40,120 bytes** per operation. The optimized comparer avoids boxed
primitive comparisons. Tuples and records benefit much less.

Fable's `FastGenericComparer` creates a structural comparer object and forwards
its `Compare` method to the same generic comparison function used by the
original implementation. That extra indirection brought no specialization and
measured slower. Therefore it is used only on .NET, including in `sortInPlace`.

[Detailed method and results](SortComparer/README.md#findings-2026-09-29):
[.NET 8 CSV](SortComparer/results-net8.csv),
[.NET 10 CSV](SortComparer/results-net10.csv),
[Fable CSV](SortComparer/results-fable.csv).

## 2. Plain JavaScript versus Fable

Fable already uses native JavaScript sorting. The difference is primarily in
the comparator, not access to a different sorting algorithm. Generic Fable
comparison supports nulls, structural values and custom comparison, requiring
more work than a comparator restricted to numbers or non-null strings.

This experiment compared generic ResizeArray sorting against:

```javascript
numbers.slice().sort((a, b) => a - b);
strings.slice().sort();
```

For 10,000 random finite numbers or non-null strings, median milliseconds:

| Type | Fable generic | Plain JS | Fable typed sortWith callback | Plain JS speedup |
| --- | ---: | ---: | ---: | ---: |
| int | 1.886 | 1.392 | 1.710 | 1.35× |
| float | 2.665 | 1.945 | 2.021 | 1.37× |
| string | 3.539 | 1.713 | 3.006 | 2.07× |

A typed callback such as `ResizeArray.sortWith (fun (a: float) b -> compare a b)`
lets Fable use primitive comparison. It recovered much of the difference in
this experiment, although `sortWith` still adds callback indirection.

These native JS comparators are **not replacements for generic F# comparison**:

- Default `.sort()` orders numbers lexicographically and stringifies objects
  and nulls.
- Subtraction does not preserve F# NaN ordering or support structural values.
- Generic sorting must support tuples, records, options and custom comparisons.
- Sorting the original JS array would violate `ResizeArray.sort`'s copy semantics.

Emitting native JS with the same generic comparator and copy would reproduce
what Fable already generates, without removing the important overhead. Making
the F# function `inline` is a different possible experiment; it was not evaluated.

[Detailed results](SortComparer/README.md#plain-javascript-versus-fable),
[CSV](SortComparer/results-native-js.csv).

## 3. Dedicated sortInt and sortFloat

The next experiment compared the optimized generic sort, typed `sortWith`,
direct typed F# comparison and emitted numeric JS comparators. The preferred
combination was parameterless `Sort()` on .NET and direct typed F# comparison
on Fable. At 10,000 random finite elements:

| Type | .NET 8 generic → typed, ms | .NET 10 generic → typed, ms | Fable generic → typed, ms |
| --- | ---: | ---: | ---: |
| int | 0.424 → 0.333 (1.27×) | 0.389 → 0.315 (1.24×) | 1.593 → 1.385 (1.15×) |
| float | 0.394 → 0.354 (1.11×) | 0.379 → 0.363 (1.05×) | 1.739 → 1.460 (1.19×) |

With 10% NaNs, float speedups were 1.33× on .NET 8, 1.23× on .NET 10 and
1.22× on Fable. NaNs sort before other numbers, NaNs compare equal to each other,
and signed zeros compare equal. The algorithms do not promise stable sorting.

Handwritten JS subtraction for integers was effectively tied with direct typed
F# comparison. For floats, a handwritten comparator with explicit NaN handling
was about as fast as generic Fable sorting and slower than direct typed F#
comparison. There was no reason to add that JS code to the public API.

Simply wrapping typed `sortWith` did not deliver the same results: its extra
comparator indirection was particularly costly on .NET. The four typed public
functions use the comparison paths directly.

[Detailed results and prototypes](SortComparer/README.md#dedicated-sortint-and-sortfloat-prototypes):
[.NET 8 CSV](SortComparer/results-typed-net8.csv),
[.NET 10 CSV](SortComparer/results-typed-net10.csv),
[Fable CSV](SortComparer/results-typed-fable.csv).

## 4. FSharp.Core Array.sort

This experiment explicitly calls `Microsoft.FSharp.Collections.Array.sort`,
not the ArrayT library. Inputs are already in the appropriate collection type;
conversion between arrays and ResizeArrays is not timed.

For 10,000 random elements, median milliseconds:

| Runtime | Type | Core Array.sort | ResizeArray.sort | ResizeArray.sortInt / sortFloat |
| --- | --- | ---: | ---: | ---: |
| .NET 8 | int | 0.349 | 0.428 | 0.347 |
| .NET 8 | float | 0.361 | 0.404 | 0.369 |
| .NET 8 | string | 1.058 | 1.071 | — |
| .NET 10 | int | 0.333 | 0.406 | 0.332 |
| .NET 10 | float | 0.365 | 0.391 | 0.366 |
| .NET 10 | string | 1.125 | 1.126 | — |
| Fable / Node | int | 1.508 | 1.633 | 1.423 |
| Fable / Node | float | 1.515 | 1.731 | 1.429 |
| Fable / Node | string | 3.094 | 3.451 | — |

The typed ResizeArray sorts were effectively tied with Core Array.sort on .NET
and took about **6% less time in Fable/Node**. Generic ResizeArray sorting took
22–23% longer for integers and 7–12% longer for floats on .NET. String sorting
was approximately equal on .NET.

In Fable, numeric F# arrays were `Int32Array` and `Float64Array`, while
ResizeArrays were ordinary JS arrays. The table uses Array.sort calls with
known element types. Through a non-inline generic wrapper, Fable Array.sort
instead took 1.681 ms for integers, 1.783 ms for floats and 3.710 ms for strings.
Keeping type information at the call site matters in Fable; on .NET these
typed and generic Array.sort calls performed approximately equally.

For data already in a ResizeArray, converting to an array solely to use
Array.sort adds overhead not shown here. The typed ResizeArray APIs provide
comparable sorting performance without that conversion.

[Detailed method and results](ArraySort/README.md):
[.NET 8 CSV](ArraySort/results-net8.csv),
[.NET 10 CSV](ArraySort/results-net10.csv),
[Fable CSV](ArraySort/results-fable.csv).

## Reproducing and interpreting the results

Use the build and run commands in [SortComparer](SortComparer/README.md)
and [ArraySort](ArraySort/README.md#run). SortComparer supports its original
comparer benchmark, `--typed` for numeric prototypes, and `native-js.mjs` for
the plain-JS comparison. Run one benchmark process at a time.

The common setup used deterministic inputs, 300 ms warmup per method and case,
batch calibration to at least 60 ms, and nine measured batches with method order
alternated or rotated. Reported times are medians per copy-and-sort. Smaller
16- and 1,024-element cases and additional input distributions are in the CSVs.
Correctness checks run outside the timed section.

Environment: Windows 11 x64, Intel Core i5-14600, .NET SDK 10.0.401, .NET 8.0.31
and 10.0.12, Fable 5.15.0, Node.js 26.7.0. SortComparer uses FSharp.Core 6.0.7,
matching the library's minimum dependency; ArraySort uses 10.0.101, matching the
test project. In Fable, calls are compiled to the Fable runtime implementation.

Compare methods **within each experiment**, not absolute timings across the
tables: FSharp.Core versions, JIT context and machine conditions differ.
Small differences such as 1.05× should be treated cautiously. These are local
microbenchmarks, not statistical performance guarantees, and Node.js results
are not browser measurements.

Checks covered ordering, copying versus mutation, empty inputs, null rejection,
integer extremes, NaNs, infinities, signed zeros, structural values and ordinal
string ordering as appropriate to each experiment/API. After the API changes,
402 .NET tests and 380 Fable tests passed, along with TypeScript compilation
and both library target builds (`net8.0` and `net472`). The .NET Framework target
was built but not separately benchmarked.
