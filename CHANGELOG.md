# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.25.0] - 2025-09-30
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

[Unreleased]: https://github.com/goswinr/ResizeArrayT/compare/v0.25.0...HEAD
[0.25.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.24.0...v0.25.0
[0.24.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.23.0...v0.24.0
[0.23.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.22.0...v0.23.0
[0.22.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.21.0...v0.22.0
[0.21.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.20.0...v0.21.0
[0.20.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.19.0...v0.20.0
[0.19.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.18.0...v0.19.0
[0.18.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.17.0...v0.18.0
[0.17.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.16.0...v0.17.0
[0.16.0]: https://github.com/goswinr/ResizeArrayT/compare/v0.15.0...v0.16.0
[0.15.0]: https://github.com/goswinr/ResizeArrayT/releases/tag/v0.15.0

<!--
use to get tag dates:
git log --tags --simplify-by-decoration --pretty="format:%ci %d"
-->

