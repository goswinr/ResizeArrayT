# FSharp.Core Array.sort versus ResizeArray sorting

[All sorting findings and conclusions](../README.md)

This benchmark uses fully qualified `Microsoft.FSharp.Collections.Array.sort`
calls, not ArrayT. It compares:

- `Array.sort` called with a known element type.
- `Array.sort` inside a non-inline generic wrapper.
- The current `ResizeArrayT.ResizeArray.sort`.
- The public `ResizeArray.sortInt` or `sortFloat` for numeric inputs.

All operations include their normal copy and return a new collection. Each
receives its native input representation, prepared before timing: arrays for
`Array.sort`, and ResizeArrays for the other functions. Converting an existing
ResizeArray to an array or back is not included. No in-place sorts are measured.

## Run

From the repository root:

```powershell
dotnet build Benchmarks/ArraySort/ArraySort.fsproj -c Release
dotnet Benchmarks/ArraySort/bin/Release/net8.0/ArraySort.dll
dotnet Benchmarks/ArraySort/bin/Release/net10.0/ArraySort.dll
dotnet fable Benchmarks/ArraySort/ArraySort.fsproj --outDir Benchmarks/ArraySort/_js --noCache
node Benchmarks/ArraySort/_js/Program.js
```

Run only one benchmark process at a time. No npm installation is necessary.

## Method and environment

Deterministic random values; 16, 1,024 and 10,000 integers/floats, plus 10,000
strings. Each method gets 300 ms warmup, independently calibrated batches lasting
at least 60 ms, then nine measured batches with method order rotated. Results are
median milliseconds per operation. Input preservation, fresh copies and ordering
are checked outside the timed section, including NaN, infinities and integer
extremes. These are local microbenchmark measurements, not performance guarantees.

Windows 11 x64, Intel Core i5-14600, .NET SDK 10.0.401, .NET 8.0.31 / 10.0.12,
FSharp.Core package 10.0.101 (matching the tests), Fable 5.15.0, Node.js 26.7.0.
The benchmark prints the loaded FSharp.Core assembly version on .NET and the
actual numeric array representations in Fable. Earlier comparer experiments in
`../SortComparer` used FSharp.Core 6.0.7; compare methods within this experiment,
not absolute times between experiments.

Fable compiles the typed Array.sort calls with `comparePrimitives`; the generic
wrapper uses its generic `compare` function. Both use the Fable array library's
copy-and-sort helper and a comparer object. ResizeArray's typed methods directly
call native sorting with `comparePrimitives` as the callback.

## Results (2026-09-29)

10,000 random elements, median milliseconds per copy-and-sort. `Array.sort`
below is the normal call with the element type known at the call site.

| Runtime | Type | Array.sort | ResizeArray.sort | ResizeArray.sortInt / sortFloat |
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

The Fable numeric inputs were confirmed to be `Int32Array` and `Float64Array`;
ResizeArray inputs were ordinary JS arrays. With a generic Array.sort wrapper,
the Fable timings rose to 1.681 ms (int), 1.783 ms (float) and 3.710 ms (string).
This illustrates the importance of retaining type information at the call site
in Fable. The .NET typed/generic Array.sort calls were approximately equivalent.

The typed ResizeArray functions are effectively tied with Array.sort for the
large numeric cases on .NET and take about 6% less time on Fable/Node. Generic
ResizeArray.sort takes about 22–23% longer for integers and 7–12% longer for
floats on .NET. Small differences should be treated as approximate.

All correctness checks passed. Full measurements (including smaller inputs):
[.NET 8](results-net8.csv), [.NET 10](results-net10.csv),
[Fable/Node](results-fable.csv). `time_relative_to_Array_sort` is method time
divided by typed-call Array.sort time; less than 1 means faster than Array.sort.

These numbers compare already-prepared native collections. If the input is a
ResizeArray, switching to Array.sort would add a conversion before sorting,
and potentially another conversion if a ResizeArray result is required.
