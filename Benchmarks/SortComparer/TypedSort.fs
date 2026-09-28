module TypedSort

// These comparators have known types at the call site, allowing Fable to
// specialize compare while using the existing public sorting API.
let ints (source: ResizeArray<int>) =
    ResizeArrayT.ResizeArray.sortWith (fun (a: int) b -> compare a b) source

let floats (source: ResizeArray<float>) =
    ResizeArrayT.ResizeArray.sortWith (fun (a: float) b -> compare a b) source

let strings (source: ResizeArray<string>) =
    ResizeArrayT.ResizeArray.sortWith (fun (a: string) b -> compare a b) source

// Direct typed F# comparison, without the public sortWith callback wrapper.
let directInt (source: ResizeArray<int>) =
    if isNull source then nullArg "source"
    let result = source.GetRange(0, source.Count)
    result.Sort(fun (a: int) b -> compare a b)
    result

let directFloat (source: ResizeArray<float>) =
    if isNull source then nullArg "source"
    let result = source.GetRange(0, source.Count)
    result.Sort(fun (a: float) b -> compare a b)
    result

// Proposed typed implementations. These remain benchmark prototypes.
let sortInt (source: ResizeArray<int>) =
    if isNull source then nullArg "source"
    let result = source.GetRange(0, source.Count)
    #if FABLE_COMPILER
    // JS subtraction does not wrap at the Int32 boundary like F# int subtraction.
    Fable.Core.JsInterop.emitJsStatement result "$0.sort((a, b) => a - b)"
    #else
    result.Sort()
    #endif
    result

let sortFloat (source: ResizeArray<float>) =
    if isNull source then nullArg "source"
    let result = source.GetRange(0, source.Count)
    #if FABLE_COMPILER
    // Equal signed zeros/infinities compare equal; NaN sorts before every number.
    Fable.Core.JsInterop.emitJsStatement result
        "$0.sort((a, b) => a === b ? 0 : a !== a ? (b !== b ? 0 : -1) : b !== b ? 1 : a - b)"
    #else
    result.Sort()
    #endif
    result
