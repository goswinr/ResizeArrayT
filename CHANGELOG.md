# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- `ResizeArray.sortInt`, `sortFloat`, `sortInPlaceInt` and `sortInPlaceFloat` for sorting with comparisons specialized for integers and double-precision floats on .NET and Fable. Float sorting puts NaN first and treats signed zeros as equal, like F# comparison.
- `ResizeArray.minNumber` and `maxNumber` skip NaN and treat -0.0 as smaller than +0.0, as the IEEE 754:2019 'minimumNumber' and 'maximumNumber' operations. See https://github.com/dotnet/fsharp/issues/13207#issuecomment-1194411950
### Changed
- ResizeArrayT now raises specific `ResizeArrayT...Exception` types for argument, null, and missing-key errors. Each inherits from its corresponding .NET exception type, so existing handlers continue to work. Index errors remain `IndexOutOfRangeException` because that .NET type is sealed.
- `ResizeArray.sort` and `sortInPlace` use `FastGenericComparer` on .NET to reduce comparison overhead and allocations, while retaining the existing comparison in Fable.
- **Breaking:** `ResizeArray.groupByDict` now requires `'Key : equality` and uses F# structural equality for grouping and dictionary lookups on both .NET and Fable, matching ArrayT. Structurally equal array keys now form one group on .NET too.
- **Breaking:** `ResizeArray.min` and `max` (and `ResizeArray.Parallel.min` and `max`) propagate NaN: if any element is NaN, NaN is returned, as the IEEE 754:2019 'minimum' and 'maximum' operations. Before, the result depended on where NaN was. Use `minNumber` and `maxNumber` to skip NaN.
- `ResizeArray.min`, `max` and `ResizeArray.Parallel.min`, `max` treat -0.0 as smaller than +0.0 for float and float32. Before, the first of them was returned.
- **Breaking:** `ResizeArray.Parallel` is now in its own file, inside the AutoOpen module `ResizeArrayParallel`. After `open ResizeArrayT`, `ResizeArray.Parallel.min` works as before, but the fully qualified `ResizeArrayT.ResizeArray.Parallel.min` does not, and code compiled against an older version needs to be recompiled.
### Fixed
- `ResizeArray.groupByDict` explicitly rejects null and `None` keys with a `ResizeArrayTArgumentNullException`, matching its documented restriction.
- `ResizeArray.minBy`, `maxBy`, `minIndexBy`, `maxIndexBy`, `min2By`, `max2By`, `min2IndicesBy`, `max2IndicesBy`, `min3By`, `max3By`, `min3IndicesBy`, `max3IndicesBy`, `Parallel.minBy` and `Parallel.maxBy` ignore NaN keys at any position, NaN keys are ranked after all other keys. Before, a NaN key at the start was returned.
- `ResizeArray.min2`, `max2`, `min3` and `max3` rank NaN after all other values. Before, the result depended on where NaN was.
- `ResizeArray.minIndexBy` and `maxIndexBy` throw an `ArgumentException` with a descriptive message on empty input, also in Fable.
- The error message of `ResizeArray.max3IndicesBy` names the function correctly.

## [0.29.0] - 2026-09-27

### Added
- `ResizeArray.mapIfResult` and `mapIfInputAndResult`, the names used in ArrayT.
- `xs.SliceNeg` and `ResizeArray.sliceNeg` slice with an inclusive end index and allow negative indices (-1 is the last item), like in ArrayT and Str.
- `ResizeArray.slice` does the same, but is marked obsolete, because in .NET the `.Slice` method of some collections takes a start index and a length instead.
- `ResizeArray.matches`, `findValue`, `findLastValue`, `findArray` and `findLastArray` from ArrayT.
- `xs.Copy()` for a shallow copy, same as `xs.Clone()`, and named like `arr.Copy()` in ArrayT.
- `ResizeArray.Parallel` has all functions of `Array.Parallel` from FSharp.Core now. New are `average`, `averageBy`, `exists`, `filter`, `forall`, `groupBy`, `max`, `maxBy`, `min`, `minBy`, `partitionWith`, `reduce`, `reduceBy`, `sort`, `sortBy`, `sortByDescending`, `sortDescending`, `sortInPlace`, `sortInPlaceBy`, `sortInPlaceWith`, `sortWith`, `sum`, `sumBy`, `tryFind`, `tryFindIndex`, `tryPick` and `zip`.

### Changed
- `ResizeArray.applyIfResult` and `applyIfInputAndResult` are marked obsolete, use `mapIfResult` and `mapIfInputAndResult` instead.
- `xs.Duplicate()` is marked obsolete, use `xs.Copy()` or `xs.Clone()` instead.
- The exception message of `xs.FirstAndOnly` and `ResizeArray.firstAndOnly` says that exactly one item is expected, like in ArrayT, instead of reporting a bad index.
- `ResizeArray.sliceIdx` and `sliceLooped` call `xs.SliceIdx` and `xs.SliceLooped`, like in ArrayT, so their exception messages name `SliceIdx`.
- `ResizeArray.sliceIdx` and `xs.SliceIdx` fail with a "Can't slice an empty ResizeArray" message on empty input, like ArrayT and Str.
- The docs of the slicing functions match those in ArrayT and Str.

### Fixed
- `xs.ToString(Int32.MaxValue)` printed "..." and the last item twice because of an integer overflow.
- `ResizeArray.failIfEmpty`, `failIfLessThan`, `sliceIdx` and `sliceLooped` raise an `ArgumentNullException` for null input, instead of a `NullReferenceException`.

## [0.28.0] - 2026-09-25

### Changed
- Tests: migrate the test project from Fable.Mocha and Expecto to Scriptorium (Scriptorium.Quill and Scriptorium.Nib) on both .NET and JS
- `ResizeArray.skip` treats a negative count as zero, like `Array.skip`, instead of throwing.
- The library (and its Fable source package) is compiled with F# `LangVersion` latest instead of preview.

### Fixed
- `randomSample`, `randomSampleBy` and `randomSampleWith` now return the sampled elements in random order. Before, e.g. a sample of all elements always kept the input order.
- Slicing from the end with an offset beyond the start, e.g. `xs.[..^5]` on three items, now returns a clamped result like F# array slicing instead of throwing.
- The `random*By` functions fail with a descriptive exception when the randomizer returns a value outside the [0.0, 1.0) range.
- `removeManyAt` fails with a descriptive exception on a negative count and `insertManyAt` on null values.
- Fix garbled or incomplete error messages of `.Pop(index)`, `.[a..b] <- values` and `permute`.
- In Fable, opening `ResizeArrayT` no longer brings an internal `isEqualTo` function into scope that shadowed other functions of that name.

## [0.27.0] - 2026-09-07

### Added
- add ResizeArray.zeroCreate
- add ResizeArray.partitionWith (alias of partitionBy under the F# core Array module name)
- add ResizeArray.randomChoice, randomChoiceBy and randomChoiceWith
- add ResizeArray.randomChoices, randomChoicesBy and randomChoicesWith
- add ResizeArray.randomSample, randomSampleBy and randomSampleWith
- add ResizeArray.randomShuffle, randomShuffleBy and randomShuffleWith
- add ResizeArray.randomShuffleInPlace, randomShuffleInPlaceBy and randomShuffleInPlaceWith

### Fixed
- Packaging: the Fable content glob is no longer recursive, so the package no longer ships generated obj AssemblyInfo files, only the real source files.

## [0.26.1] - 2026-07-12

### Added
- ResizeArray.headAndTail
- ResizeArray.popOff to just remove the last element without returning it

### Changed
- Correct and complete the public API documentation and hide implementation-only utilities from generated documentation.

### Fixed
- Make `ResizeArray.sliceIdx` and `.SliceIdx` use an inclusive end index and reject invalid bounds consistently.
- Interpolate the actual collection lengths in the `ResizeArray.zip` length-mismatch error message.
- Correct the zero-based index passed to and returned from `ResizeArray.tryFindIndexi` and `findIndexi`.
- Fix `rotateDownTill` and `rotateDownTillLast` boundary handling and ensure the latter tests the last element as documented.
- Return fresh empty ResizeArrays from operations documented to return new collections.

## [0.26.0] - 2026-03-07
### Changed
- allow ResizeArray.asArray only on reference types

## [0.25.0] - 2025-10-11
### Changed
- BREAKING CHANGE: reversed order of arguments in ResizeArray.set and ResizeArray.get.
### Added
- add mapPrevNext function
- add zipDefault function

## [0.24.0] - 2025-06-17
### Changed
- equals does not fail for null input
### Added
- tryPickBack


## [0.23.0] - 2025-05-24
### Changed
- renamed main namespace an nuget from `ResizeArray` to `ResizeArrayT`
- .[startIdx .. endIdx] slicing error cases aligned with F# array slicing
- added .SliceLooped function with any integer as valid index
- added .SliceIdx function to distinguish from List<'T> built in .Slice(start,length)

## [0.22.0] - 2025-02-21
### Added
- add findIndexi
- add DebugIdx member

## [0.21.0] - 2024-11-02
### Added
- add docs with fsdocs
### Changed
- rename ToNiceString to AsString

## [0.20.0] - 2024-09-15
### Added
- add filteri

## [0.19.0] - 2024-05-07
### Added
- ad TS build check
- rename minIndBy to minIndexBy
- add asArray (for casting in Fable)

## [0.18.0] - 2024-02-25
### Added
- add mapToArray
- add failIfEmpty

## [0.17.0] - 2024-01-28
### Fixed
- don't fail on LastIndex when empty

## [0.16.0] - 2024-01-21
### Added
- add null checks
- add 'partitionBy' functions
- add equality checks for nested ResizeArrays
- flip arg order of 'sub' function

## [0.15.0] - 2024-01-21
### Added
- implementation ported from `Rarr` type in https://github.com/goswinr/FsEx/blob/main/Src/RarrModule.fs

[Unreleased]: https://github.com/goswinr/ResizeArrayT/compare/0.29.0...HEAD
[0.29.0]: https://github.com/goswinr/ResizeArrayT/compare/0.28.0...0.29.0
[0.28.0]: https://github.com/goswinr/ResizeArrayT/compare/0.27.0...0.28.0
[0.27.0]: https://github.com/goswinr/ResizeArrayT/compare/0.26.1...0.27.0
[0.26.1]: https://github.com/goswinr/ResizeArrayT/compare/0.26.0...0.26.1
[0.26.0]: https://github.com/goswinr/ResizeArrayT/compare/0.25.0...0.26.0
[0.25.0]: https://github.com/goswinr/ResizeArrayT/compare/0.24.0...0.25.0
[0.24.0]: https://github.com/goswinr/ResizeArrayT/compare/0.23.0...0.24.0
[0.23.0]: https://github.com/goswinr/ResizeArrayT/compare/0.22.0...0.23.0
[0.22.0]: https://github.com/goswinr/ResizeArrayT/compare/0.21.0...0.22.0
[0.21.0]: https://github.com/goswinr/ResizeArrayT/compare/0.20.0...0.21.0
[0.20.0]: https://github.com/goswinr/ResizeArrayT/compare/0.19.0...0.20.0
[0.19.0]: https://github.com/goswinr/ResizeArrayT/compare/0.18.0...0.19.0
[0.18.0]: https://github.com/goswinr/ResizeArrayT/compare/0.17.0...0.18.0
[0.17.0]: https://github.com/goswinr/ResizeArrayT/compare/0.16.0...0.17.0
[0.16.0]: https://github.com/goswinr/ResizeArrayT/compare/0.15.0...0.16.0
[0.15.0]: https://github.com/goswinr/ResizeArrayT/releases/tag/0.15.0

