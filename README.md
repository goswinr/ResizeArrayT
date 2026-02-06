![Logo](https://raw.githubusercontent.com/goswinr/ResizeArrayT/main/Docs/img/logo128.png)
# ResizeArrayT

[![ResizeArrayT on nuget.org](https://img.shields.io/nuget/v/ResizeArrayT)](https://www.nuget.org/packages/ResizeArrayT/)
[![Build Status](https://github.com/goswinr/ResizeArrayT/actions/workflows/build.yml/badge.svg)](https://github.com/goswinr/ResizeArrayT/actions/workflows/build.yml)
[![Docs Build Status](https://github.com/goswinr/ResizeArrayT/actions/workflows/docs.yml/badge.svg)](https://github.com/goswinr/ResizeArrayT/actions/workflows/docs.yml)
[![Test Status](https://github.com/goswinr/ResizeArrayT/actions/workflows/test.yml/badge.svg)](https://github.com/goswinr/ResizeArrayT/actions/workflows/test.yml)
[![license](https://img.shields.io/github/license/goswinr/ResizeArrayT)](LICENSE.md)
![code size](https://img.shields.io/github/languages/code-size/goswinr/ResizeArrayT.svg)


ResizeArrayT is an F# extension and module library for `ResizeArray<'T>` ( = `Collection.Generic.List<'T>`).<br>
It provides all the functions from the `Array` module in `FSharp.Core` for `ResizeArray`.<br>
And more.

It also works in Javascript and Typescript with [Fable](https://fable.io/).

This library was designed for use with F# scripting.<br>
Functions and methods never return null.<br>
When a function fails on invalid input it will throw a descriptive exception.<br>
Functions starting with `try...` will return an F# option.

I was always annoyed that an `IndexOutOfRangeException` does not include the actual bad index nor the actual size of the array.<br>
This library fixes that in the `resizeArray.Get`, `resizeArray.Set`, `resizeArray.Slice` and similar instance methods for item access.<br>
I made a similar a similar library for `array<'T>`: https://github.com/goswinr/ArrayT/ .

## Why ?
Yes, F#'s array and list modules can do these kind of operations on collections too.<br>
But ResizeArray, being mutable, still offers the best performance for collections that can expand & shrink and need random access via an index.<br>
In fact FSharp.Core uses [a very similar module internally](https://github.com/dotnet/fsharp/blob/main/src/Compiler/Utilities/ResizeArray.fs).

## It Includes:

- A `ResizeArray` module that has  **all**  functions from [`Array` module from `FSharp.Core`] reimplemented (https://fsharp.github.io/fsharp-core-docs/reference/fsharp-collections-arraymodule.html).<br>
 Including the sub module for Parallel computing.

- A Computational Expressions `resizeArray` that can be used like existing ones for `seq`.

- Support for F# slicing operator and indexing from the end. e.g: `items.[ 1 .. ^1]`.

- Extension members on `ResizeArray` like `.Get` `.Set` `.First` `.Last` `.SecondLast` and more.<br>
With nicer IndexOutOfRangeExceptions that include the bad index and the actual size.

- All Tests from the from `FSharp.Core`'s `Array` module ported and adapted to run in both javascript and dotnet.

## Namespace
The main namespace is `ResizeArrayT`.<br>
It was renamed from `ResizeArray` to `ResizeArrayT` in release 0.23. When used in scripting this helps avoid name collisions with the <br>
module inside of the same name. [Reference Issue](https://github.com/dotnet/fsharp/issues/17124)<br>

Older versions of the library will still work with the old namespace `ResizeArray`.<br>
And can be found on nuget.org with the name [ResizeArray](https://www.nuget.org/packages/ResizeArray/).<br>


## Usage
Just open the namespace

```fsharp
open ResizeArrayT
```
this namespace contains:
- a module also called `ResizeArray`
- a  Computational Expressions called `resizeArray`
- this will also auto open the extension members on `ResizeArray<'T>`

then you can do:

```fsharp
let evenNumbers =
    resizeArray {  // a Computational Expression like seq
        for i = 0 to 99 do
            if i % 2 = 0 then
                i
    }

let oddNumbers = evenNumbers |> ResizeArray.map (fun x -> x + 1) // ResizeArray module

let hundred = oddNumbers.Last // Extension member to access the last item in the list

```

### Computational Expression

The `resizeArray { ... }` builder supports `for`, `while`, `yield`, `yield!`, `try/with`, `try/finally`, and `use`:

```fsharp
// Yield individual items and sequences
let mixed =
    resizeArray {
        1
        2
        yield! [3; 4; 5]   // yield from any seq
        for i in 6..10 do
            i
    }

// Filter inside the builder
let primes =
    resizeArray {
        for n = 2 to 50 do
            let mutable isPrime = true
            for d = 2 to int (sqrt (float n)) do
                if n % d = 0 then isPrime <- false
            if isPrime then n
    }
```

### Extension Members

Access items with descriptive error messages that include the bad index and the collection size:

```fsharp
let items = ResizeArray([| "a"; "b"; "c"; "d"; "e" |])

items.First        // "a"
items.Second       // "b"
items.Last         // "e"
items.SecondLast   // "d"
items.LastIndex    // 4

// Negative indexing (Python-style: -1 is last item)
items.GetNeg(-1)   // "e"
items.GetNeg(-2)   // "d"

// Looped indexing (wraps around)
items.GetLooped(7) // "c"  (index 7 wraps to index 2)

// Status checks
items.IsEmpty      // false
items.IsNotEmpty   // true
items.HasItems     // true
items.IsSingleton  // false
```

### Slicing

F# slicing notation is fully supported, including indexing from the end with `^`:

```fsharp
let nums = ResizeArray([| 10; 20; 30; 40; 50 |])

nums.[1..3]        // ResizeArray [20; 30; 40]
nums.[..2]         // ResizeArray [10; 20; 30]
nums.[2..]         // ResizeArray [30; 40; 50]
nums.[1..^1]       // ResizeArray [20; 30; 40]  (from index 1 to second-last)
nums.[^0]          // 50  (last item)
```

### Pop, Clone, and InsertAtStart

```fsharp
let xs = ResizeArray([| 1; 2; 3; 4; 5 |])

let last = xs.Pop()            // returns 5, xs is now [1; 2; 3; 4]
let second = xs.Pop(1)         // returns 2, xs is now [1; 3; 4]

xs.InsertAtStart(0)            // xs is now [0; 1; 3; 4]

let copy = xs.Clone()          // shallow copy
```

### ResizeArray Module

All functions from `FSharp.Core`'s `Array` module are available, plus many extras:

```fsharp
// Standard functional operations
let doubled = items |> ResizeArray.map (fun x -> x * 2)
let evens   = items |> ResizeArray.filter (fun x -> x % 2 = 0)
let total   = items |> ResizeArray.sum

// Positional access
let first     = items |> ResizeArray.first
let last      = items |> ResizeArray.last
let secLast   = items |> ResizeArray.secondLast

// Grouping and counting
let grouped  = items |> ResizeArray.groupBy (fun x -> x % 3)
let counts   = items |> ResizeArray.countBy (fun x -> x % 3)

// Finding duplicates
let dupes    = items |> ResizeArray.duplicates
let dupesBy  = items |> ResizeArray.duplicatesBy (fun x -> x % 10)

// Partitioning into multiple groups
let trueOnes, falseOnes = items |> ResizeArray.partition (fun x -> x > 3)

let small, medium, large =
    items |> ResizeArray.partition3
        (fun x -> x < 10)
        (fun x -> x < 50)

// Windowed iteration (useful for geometry/polyline processing)
items |> ResizeArray.windowed2 |> Seq.iter (fun (a, b) -> printfn "%A -> %A" a b)
items |> ResizeArray.prevThisNext |> Seq.iter (fun (prev, this', next) -> printfn "%A %A %A" prev this' next)
```

### Construction and Conversion

```fsharp
let empty  = ResizeArray.empty<int>
let single = ResizeArray.singleton 42
let filled = ResizeArray.create 10 0         // 10 zeros
let inited = ResizeArray.init 5 (fun i -> i * i) // [0; 1; 4; 9; 16]

// From other collections
let fromList  = ResizeArray.ofList [1; 2; 3]
let fromArray = ResizeArray.ofArray [|1; 2; 3|]
let fromSeq   = ResizeArray.ofSeq (seq { 1..10 })

// To other collections
let asArray = items |> ResizeArray.toArray
let asList  = items |> ResizeArray.toList
let asSeq   = items |> ResizeArray.toSeq
```

### Error Messages

When an index is out of range, you get a descriptive exception including the bad index and the collection content:

```
System.IndexOutOfRangeException:
ResizeArray.Get: Can't get index 5 from:
ResizeArray<String> with 3 items:
  0: "a"
  1: "b"
  2: "c"
```

### Operators

Open the `Operators` module to combine collections with `++` and `+++`:

```fsharp
open ResizeArrayT.Operators

let combined = xs ++ ys         // from two ICollection<'T>, preallocates capacity
let combined' = xs +++ ys       // from two seq<'T>
```

### Parallel

A sub module for parallel operations (on .NET only):

```fsharp
let results = items |> ResizeArray.Parallel.map (fun x -> expensiveComputation x)
let chosen  = items |> ResizeArray.Parallel.choose (fun x -> tryProcess x)
```

## Use of AI and LLMs
All core function are are written by hand to ensure performance and correctness.<br>
However, AI tools have been used for code review, typo and grammar checking in documentation<br>
and to generate not all but many of the tests.

## Full API Documentation

[goswinr.github.io/ResizeArrayT](https://goswinr.github.io/ResizeArrayT/reference/ResizeArrayT.html)


## Tests
All Tests run in both javascript and dotnet.
Successful Fable compilation to typescript is verified too.
Go to the tests folder:

```bash
cd Tests
```

For testing with .NET using Expecto:

```bash
dotnet run
```

for JS testing with Fable.Mocha and TS verification:

```bash
npm test
```

## License
[MIT](https://github.com/goswinr/ResizeArrayT/blob/main/LICENSE.md)

## Changelog
see [CHANGELOG.md](https://github.com/goswinr/ResizeArrayT/blob/main/CHANGELOG.md)

