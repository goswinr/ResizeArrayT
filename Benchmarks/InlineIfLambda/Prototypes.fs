/// Benchmark-only prototypes of ResizeArray functions, written like the FSharp.Core 11 Array module:
/// https://github.com/dotnet/fsharp/blob/main/src/FSharp.Core/array.fs
/// The current library implementations are benchmarked by calling ResizeArrayT.ResizeArray directly.
module Prototypes

#nowarn "44" // the hidden UtilResizeArray module, same null checks as the library
open ResizeArrayT.UtilResizeArray

/// <c>inline</c> with <c>[&lt;InlineIfLambda&gt;]</c> only.
/// For callbacks with one argument this is the complete FSharp.Core 11 pattern.
module InlineIfLambda =

    let inline iter ([<InlineIfLambda>] action: 'T -> unit) (resizeArray: ResizeArray<'T>) : unit =
        if isNull resizeArray then nullExn "iter"
        for i = 0 to resizeArray.Count - 1 do
            action resizeArray.[i]

    let inline map ([<InlineIfLambda>] mapping: 'T -> 'U) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
        if isNull resizeArray then nullExn "map"
        let res = ResizeArray<'U>(resizeArray.Count)
        for i = 0 to resizeArray.Count - 1 do
            res.Add(mapping resizeArray.[i])
        res

    let inline filter ([<InlineIfLambda>] predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "filter"
        let res = ResizeArray<'T>()
        for i = 0 to resizeArray.Count - 1 do
            let x = resizeArray.[i]
            if predicate x then res.Add x
        res

    let inline exists ([<InlineIfLambda>] predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "exists"
        let mutable state = false
        let mutable i = 0
        while not state && i < resizeArray.Count do
            state <- predicate resizeArray.[i]
            i <- i + 1
        state

    let inline sumBy ([<InlineIfLambda>] projection: 'T -> ^Key) (resizeArray: ResizeArray<'T>) : ^Key =
        if isNull resizeArray then nullExn "sumBy"
        let mutable acc = LanguagePrimitives.GenericZero< ^Key>
        for i = 0 to resizeArray.Count - 1 do
            acc <- Checked.(+) acc (projection resizeArray.[i])
        acc

    // Callbacks with two arguments. Without OptimizeClosureIfNotInlined an opaque callback
    // is called through FSharpFunc.InvokeFast for every element.

    let inline fold ([<InlineIfLambda>] folder: 'State -> 'T -> 'State) (state: 'State) (resizeArray: ResizeArray<'T>) : 'State =
        if isNull resizeArray then nullExn "fold"
        let mutable state = state
        for i = 0 to resizeArray.Count - 1 do
            state <- folder state resizeArray.[i]
        state

    let inline iteri ([<InlineIfLambda>] action: int -> 'T -> unit) (resizeArray: ResizeArray<'T>) : unit =
        if isNull resizeArray then nullExn "iteri"
        for i = 0 to resizeArray.Count - 1 do
            action i resizeArray.[i]

    let inline mapi ([<InlineIfLambda>] mapping: int -> 'T -> 'U) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
        if isNull resizeArray then nullExn "mapi"
        let res = ResizeArray<'U>(resizeArray.Count)
        for i = 0 to resizeArray.Count - 1 do
            res.Add(mapping i resizeArray.[i])
        res

    let inline map2 ([<InlineIfLambda>] mapping: 'T1 -> 'T2 -> 'U) (resizeArray1: ResizeArray<'T1>) (resizeArray2: ResizeArray<'T2>) : ResizeArray<'U> =
        if isNull resizeArray1 then nullExn "map2 first"
        if isNull resizeArray2 then nullExn "map2 second"
        if resizeArray1.Count <> resizeArray2.Count then
            fail resizeArray1 $"map2: count of resizeArray1 {resizeArray1.Count} does not match resizeArray2 {resizeArray2.Count}."
        let res = ResizeArray<'U>(resizeArray1.Count)
        for i = 0 to resizeArray1.Count - 1 do
            res.Add(mapping resizeArray1.[i] resizeArray2.[i])
        res


/// <c>inline</c> with <c>[&lt;InlineIfLambda; OptimizeClosureIfNotInlined&gt;]</c>, for callbacks with 2 to 5 arguments.
/// Needs FSharp.Core 11 and F# 11 at the declaration. The F# 11 optimizer of the calling code adapts
/// an opaque callback once with OptimizedClosures.FSharpFunc.Adapt and then calls Invoke in the loop.
/// Older compilers and Fable ignore the attribute, then this is the same as the InlineIfLambda module.
module OptimizeClosure =

    let inline fold ([<InlineIfLambda; OptimizeClosureIfNotInlined>] folder: 'State -> 'T -> 'State) (state: 'State) (resizeArray: ResizeArray<'T>) : 'State =
        if isNull resizeArray then nullExn "fold"
        let mutable state = state
        for i = 0 to resizeArray.Count - 1 do
            state <- folder state resizeArray.[i]
        state

    let inline iteri ([<InlineIfLambda; OptimizeClosureIfNotInlined>] action: int -> 'T -> unit) (resizeArray: ResizeArray<'T>) : unit =
        if isNull resizeArray then nullExn "iteri"
        for i = 0 to resizeArray.Count - 1 do
            action i resizeArray.[i]

    let inline mapi ([<InlineIfLambda; OptimizeClosureIfNotInlined>] mapping: int -> 'T -> 'U) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
        if isNull resizeArray then nullExn "mapi"
        let res = ResizeArray<'U>(resizeArray.Count)
        for i = 0 to resizeArray.Count - 1 do
            res.Add(mapping i resizeArray.[i])
        res

    let inline map2 ([<InlineIfLambda; OptimizeClosureIfNotInlined>] mapping: 'T1 -> 'T2 -> 'U) (resizeArray1: ResizeArray<'T1>) (resizeArray2: ResizeArray<'T2>) : ResizeArray<'U> =
        if isNull resizeArray1 then nullExn "map2 first"
        if isNull resizeArray2 then nullExn "map2 second"
        if resizeArray1.Count <> resizeArray2.Count then
            fail resizeArray1 $"map2: count of resizeArray1 {resizeArray1.Count} does not match resizeArray2 {resizeArray2.Count}."
        let res = ResizeArray<'U>(resizeArray1.Count)
        for i = 0 to resizeArray1.Count - 1 do
            res.Add(mapping resizeArray1.[i] resizeArray2.[i])
        res
