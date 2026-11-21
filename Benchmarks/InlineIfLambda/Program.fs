module InlineIfLambdaBenchmark

open System
open System.Diagnostics
open System.Runtime.CompilerServices
open ResizeArrayT

module L = Prototypes.InlineIfLambda
module O = Prototypes.OptimizeClosure

// Short on purpose: about 0.2 s per method, about half a minute per runtime for all functions.
let sharedWarmupMs = 1000.0
let warmupMs = 30.0
let batchMs = 10.0
let samples = 11

let now () =
    #if FABLE_COMPILER
    Fable.Core.JsInterop.emitJsExpr () "performance.now()"
    #else
    float (Stopwatch.GetTimestamp()) * 1000.0 / float Stopwatch.Frequency
    #endif

let mutable consumed = 0

/// Written by the iter and iteri callbacks. A static field, so the 'lambda' callbacks capture nothing.
let mutable sink = 0

/// Opaque call sites: the callback is a parameter of a function that is not inlined,
/// like library or application code that receives a function and passes it on.
/// NoCompilerInlining stops the F# optimizer from seeing the lambda at the outer call site.
module Opaque =
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let iterCurrent (f: int -> unit) xs = ResizeArray.iter f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let iterInline (f: int -> unit) xs = L.iter f xs

    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let mapCurrent (f: int -> int) xs = ResizeArray.map f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let mapInline (f: int -> int) xs = L.map f xs

    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let filterCurrent (f: int -> bool) xs = ResizeArray.filter f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let filterInline (f: int -> bool) xs = L.filter f xs

    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let existsCurrent (f: int -> bool) xs = ResizeArray.exists f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let existsInline (f: int -> bool) xs = L.exists f xs

    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let sumByCurrent (f: int -> int) xs = ResizeArray.sumBy f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let sumByInline (f: int -> int) xs = L.sumBy f xs

    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let foldCurrent (f: int -> int -> int) xs = ResizeArray.fold f 0 xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let foldInline (f: int -> int -> int) xs = L.fold f 0 xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let foldOptimized (f: int -> int -> int) xs = O.fold f 0 xs

    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let iteriCurrent (f: int -> int -> unit) xs = ResizeArray.iteri f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let iteriInline (f: int -> int -> unit) xs = L.iteri f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let iteriOptimized (f: int -> int -> unit) xs = O.iteri f xs

    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let mapiCurrent (f: int -> int -> int) xs = ResizeArray.mapi f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let mapiInline (f: int -> int -> int) xs = L.mapi f xs
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let mapiOptimized (f: int -> int -> int) xs = O.mapi f xs

    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let map2Current (f: int -> int -> int) xs ys = ResizeArray.map2 f xs ys
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let map2Inline (f: int -> int -> int) xs ys = L.map2 f xs ys
    [<NoCompilerInlining; MethodImpl(MethodImplOptions.NoInlining)>]
    let map2Optimized (f: int -> int -> int) xs ys = O.map2 f xs ys

type Case = { Fn: string; CallSite: string; Count: int; Methods: (string * (unit -> int))[] }

/// Checks every method outside the timed section, then wraps it to return an int for consumption.
let mkCase fn callSite count (check: 'R -> bool) (consume: 'R -> int) (methods: (string * (unit -> 'R)) list) =
    for name, f in methods do
        if not (check (f ())) then failwithf "%s/%s/%d/%s: wrong result" fn callSite count name
    { Fn = fn; CallSite = callSite; Count = count; Methods = [| for name, f in methods -> name, (fun () -> consume (f ())) |] }

let sameAs (expected: int[]) = fun (r: ResizeArray<int>) -> r.ToArray() = expected
let countOf (r: ResizeArray<int>) = r.Count
let inline sinkAfter ([<InlineIfLambda>] f: unit -> unit) = sink <- 0; f (); sink

/// Not inline: 'k' is a runtime value here, so the 'closure' lambdas really capture a value.
let buildCases (k: int) (xs: ResizeArray<int>) (ys: ResizeArray<int>) : Case list =
    let a, b, n = xs.ToArray(), ys.ToArray(), xs.Count
    let isInt (expected: int) = fun (r: int) -> r = expected
    // The opaque callbacks are created once, outside the timed call, and capture 'k' like the 'closure' ones.
    let iterF = fun x -> sink <- sink + x + k
    let mapF = fun x -> x * 3 + k
    let filterF = fun x -> x % k = 0
    let existsF = fun x -> x + k = -1
    let sumByF = fun x -> x * k
    let foldF = fun acc x -> acc + x + k
    let iteriF = fun i x -> sink <- sink + i + x + k
    let mapiF = fun i x -> i + x + k
    let map2F = fun x y -> x + y + k
    [
        mkCase "iter" "lambda" n (isInt (sinkAfter (fun () -> Array.iter (fun x -> sink <- sink + x) a))) id [
            "current",         fun () -> sinkAfter (fun () -> ResizeArray.iter (fun x -> sink <- sink + x) xs)
            "inlineIfLambda",  fun () -> sinkAfter (fun () -> L.iter (fun x -> sink <- sink + x) xs) ]
        mkCase "iter" "closure" n (isInt (sinkAfter (fun () -> Array.iter iterF a))) id [
            "current",         fun () -> sinkAfter (fun () -> ResizeArray.iter (fun x -> sink <- sink + x + k) xs)
            "inlineIfLambda",  fun () -> sinkAfter (fun () -> L.iter (fun x -> sink <- sink + x + k) xs) ]
        mkCase "iter" "opaque" n (isInt (sinkAfter (fun () -> Array.iter iterF a))) id [
            "current",         fun () -> sinkAfter (fun () -> Opaque.iterCurrent iterF xs)
            "inlineIfLambda",  fun () -> sinkAfter (fun () -> Opaque.iterInline iterF xs) ]

        mkCase "map" "lambda" n (sameAs (Array.map (fun x -> x * 3) a)) countOf [
            "current",         fun () -> ResizeArray.map (fun x -> x * 3) xs
            "inlineIfLambda",  fun () -> L.map (fun x -> x * 3) xs ]
        mkCase "map" "closure" n (sameAs (Array.map mapF a)) countOf [
            "current",         fun () -> ResizeArray.map (fun x -> x * 3 + k) xs
            "inlineIfLambda",  fun () -> L.map (fun x -> x * 3 + k) xs ]
        mkCase "map" "opaque" n (sameAs (Array.map mapF a)) countOf [
            "current",         fun () -> Opaque.mapCurrent mapF xs
            "inlineIfLambda",  fun () -> Opaque.mapInline mapF xs ]

        mkCase "filter" "lambda" n (sameAs (Array.filter (fun x -> x % 3 = 0) a)) countOf [
            "current",         fun () -> ResizeArray.filter (fun x -> x % 3 = 0) xs
            "inlineIfLambda",  fun () -> L.filter (fun x -> x % 3 = 0) xs ]
        mkCase "filter" "closure" n (sameAs (Array.filter filterF a)) countOf [
            "current",         fun () -> ResizeArray.filter (fun x -> x % k = 0) xs
            "inlineIfLambda",  fun () -> L.filter (fun x -> x % k = 0) xs ]
        mkCase "filter" "opaque" n (sameAs (Array.filter filterF a)) countOf [
            "current",         fun () -> Opaque.filterCurrent filterF xs
            "inlineIfLambda",  fun () -> Opaque.filterInline filterF xs ]

        // The predicate is never true, so every element is tested.
        let existsFalse = (fun r -> not r), (fun r -> if r then 1 else 0)
        mkCase "exists" "lambda" n (fst existsFalse) (snd existsFalse) [
            "current",         fun () -> ResizeArray.exists (fun x -> x = -1) xs
            "inlineIfLambda",  fun () -> L.exists (fun x -> x = -1) xs ]
        mkCase "exists" "closure" n (fst existsFalse) (snd existsFalse) [
            "current",         fun () -> ResizeArray.exists (fun x -> x + k = -1) xs
            "inlineIfLambda",  fun () -> L.exists (fun x -> x + k = -1) xs ]
        mkCase "exists" "opaque" n (fst existsFalse) (snd existsFalse) [
            "current",         fun () -> Opaque.existsCurrent existsF xs
            "inlineIfLambda",  fun () -> Opaque.existsInline existsF xs ]

        mkCase "sumBy" "lambda" n (isInt (Array.sumBy (fun x -> x * 2) a)) id [
            "current",         fun () -> ResizeArray.sumBy (fun x -> x * 2) xs
            "inlineIfLambda",  fun () -> L.sumBy (fun x -> x * 2) xs ]
        mkCase "sumBy" "closure" n (isInt (Array.sumBy sumByF a)) id [
            "current",         fun () -> ResizeArray.sumBy (fun x -> x * k) xs
            "inlineIfLambda",  fun () -> L.sumBy (fun x -> x * k) xs ]
        mkCase "sumBy" "opaque" n (isInt (Array.sumBy sumByF a)) id [
            "current",         fun () -> Opaque.sumByCurrent sumByF xs
            "inlineIfLambda",  fun () -> Opaque.sumByInline sumByF xs ]

        mkCase "fold" "lambda" n (isInt (Array.fold (fun acc x -> acc + x) 0 a)) id [
            "current",         fun () -> ResizeArray.fold (fun acc x -> acc + x) 0 xs
            "inlineIfLambda",  fun () -> L.fold (fun acc x -> acc + x) 0 xs
            "optimizeClosure", fun () -> O.fold (fun acc x -> acc + x) 0 xs ]
        mkCase "fold" "closure" n (isInt (Array.fold foldF 0 a)) id [
            "current",         fun () -> ResizeArray.fold (fun acc x -> acc + x + k) 0 xs
            "inlineIfLambda",  fun () -> L.fold (fun acc x -> acc + x + k) 0 xs
            "optimizeClosure", fun () -> O.fold (fun acc x -> acc + x + k) 0 xs ]
        mkCase "fold" "opaque" n (isInt (Array.fold foldF 0 a)) id [
            "current",         fun () -> Opaque.foldCurrent foldF xs
            "inlineIfLambda",  fun () -> Opaque.foldInline foldF xs
            "optimizeClosure", fun () -> Opaque.foldOptimized foldF xs ]

        mkCase "iteri" "lambda" n (isInt (sinkAfter (fun () -> Array.iteri (fun i x -> sink <- sink + i + x) a))) id [
            "current",         fun () -> sinkAfter (fun () -> ResizeArray.iteri (fun i x -> sink <- sink + i + x) xs)
            "inlineIfLambda",  fun () -> sinkAfter (fun () -> L.iteri (fun i x -> sink <- sink + i + x) xs)
            "optimizeClosure", fun () -> sinkAfter (fun () -> O.iteri (fun i x -> sink <- sink + i + x) xs) ]
        mkCase "iteri" "closure" n (isInt (sinkAfter (fun () -> Array.iteri iteriF a))) id [
            "current",         fun () -> sinkAfter (fun () -> ResizeArray.iteri (fun i x -> sink <- sink + i + x + k) xs)
            "inlineIfLambda",  fun () -> sinkAfter (fun () -> L.iteri (fun i x -> sink <- sink + i + x + k) xs)
            "optimizeClosure", fun () -> sinkAfter (fun () -> O.iteri (fun i x -> sink <- sink + i + x + k) xs) ]
        mkCase "iteri" "opaque" n (isInt (sinkAfter (fun () -> Array.iteri iteriF a))) id [
            "current",         fun () -> sinkAfter (fun () -> Opaque.iteriCurrent iteriF xs)
            "inlineIfLambda",  fun () -> sinkAfter (fun () -> Opaque.iteriInline iteriF xs)
            "optimizeClosure", fun () -> sinkAfter (fun () -> Opaque.iteriOptimized iteriF xs) ]

        mkCase "mapi" "lambda" n (sameAs (Array.mapi (fun i x -> i + x) a)) countOf [
            "current",         fun () -> ResizeArray.mapi (fun i x -> i + x) xs
            "inlineIfLambda",  fun () -> L.mapi (fun i x -> i + x) xs
            "optimizeClosure", fun () -> O.mapi (fun i x -> i + x) xs ]
        mkCase "mapi" "closure" n (sameAs (Array.mapi mapiF a)) countOf [
            "current",         fun () -> ResizeArray.mapi (fun i x -> i + x + k) xs
            "inlineIfLambda",  fun () -> L.mapi (fun i x -> i + x + k) xs
            "optimizeClosure", fun () -> O.mapi (fun i x -> i + x + k) xs ]
        mkCase "mapi" "opaque" n (sameAs (Array.mapi mapiF a)) countOf [
            "current",         fun () -> Opaque.mapiCurrent mapiF xs
            "inlineIfLambda",  fun () -> Opaque.mapiInline mapiF xs
            "optimizeClosure", fun () -> Opaque.mapiOptimized mapiF xs ]

        mkCase "map2" "lambda" n (sameAs (Array.map2 (fun x y -> x + y) a b)) countOf [
            "current",         fun () -> ResizeArray.map2 (fun x y -> x + y) xs ys
            "inlineIfLambda",  fun () -> L.map2 (fun x y -> x + y) xs ys
            "optimizeClosure", fun () -> O.map2 (fun x y -> x + y) xs ys ]
        mkCase "map2" "closure" n (sameAs (Array.map2 map2F a b)) countOf [
            "current",         fun () -> ResizeArray.map2 (fun x y -> x + y + k) xs ys
            "inlineIfLambda",  fun () -> L.map2 (fun x y -> x + y + k) xs ys
            "optimizeClosure", fun () -> O.map2 (fun x y -> x + y + k) xs ys ]
        mkCase "map2" "opaque" n (sameAs (Array.map2 map2F a b)) countOf [
            "current",         fun () -> Opaque.map2Current map2F xs ys
            "inlineIfLambda",  fun () -> Opaque.map2Inline map2F xs ys
            "optimizeClosure", fun () -> Opaque.map2Optimized map2F xs ys ]
    ]

let batch count (action: unit -> int) =
    let started = now ()
    for _ = 1 to count do consumed <- consumed + action ()
    now () - started

let bytesPerCall (action: unit -> int) : string =
    #if FABLE_COMPILER
    ignore action
    "n/a"
    #else
    let calls = 32
    let before = GC.GetAllocatedBytesForCurrentThread()
    for _ = 1 to calls do consumed <- consumed + action ()
    string ((GC.GetAllocatedBytesForCurrentThread() - before) / int64 calls)
    #endif

/// Calls all methods of all cases in turn. Without this, the JIT (with dynamic PGO on .NET, or V8 in Node.js)
/// would optimize a shared non-inline library function like ResizeArray.fold for the callback of the
/// first case that runs. In an application such a function is called with many different callbacks.
/// The inlined variants have a separate copy of the loop at every call site.
let warmupShared (cases: Case list) =
    let all = [| for case in cases do for _, action in case.Methods -> action |]
    let started = now ()
    while now () - started < sharedWarmupMs do
        for action in all do consumed <- consumed + action ()

/// Warmup, independent batch calibration, then rotated samples. Prints median nanoseconds per call.
let run (case: Case) =
    let methods = case.Methods
    for _, action in methods do
        let started = now ()
        while now () - started < warmupMs do batch 8 action |> ignore
    let counts = methods |> Array.map (fun (_, action) ->
        let mutable count = 1
        while batch count action < batchMs do count <- count * 2
        count)
    let times = Array.init methods.Length (fun _ -> Array.zeroCreate samples)
    for round = 0 to samples - 1 do
        for step = 0 to methods.Length - 1 do
            let m = (round + step) % methods.Length
            times.[m].[round] <- batch counts.[m] (snd methods.[m]) * 1e6 / float counts.[m]
    let medians = times |> Array.map (fun xs -> Array.sortInPlace xs; xs.[samples / 2])
    for m = 0 to methods.Length - 1 do
        printfn "%s,%s,%d,%s,%.1f,%.3f,%s" case.Fn case.CallSite case.Count (fst methods.[m]) medians.[m] (medians.[m] / medians.[0]) (bytesPerCall (snd methods.[m]))

#if !FABLE_COMPILER
/// True if the compiled method calls OptimizedClosures.FSharpFunc.Adapt,
/// i.e. if the F# compiler adapted the opaque callback once before the loop.
let callsAdapt (methodName: string) =
    let m = Reflection.Assembly.GetExecutingAssembly().GetType("InlineIfLambdaBenchmark+Opaque").GetMethod methodName
    let il = m.GetMethodBody().GetILAsByteArray()
    let isAdapt i =
        try
            let callee = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1))
            callee.Name = "Adapt" && callee.DeclaringType.FullName.StartsWith "Microsoft.FSharp.Core.OptimizedClosures"
        with _ -> false
    Seq.exists (fun i -> il.[i] = 0x28uy && isAdapt i) (seq { 0 .. il.Length - 5 }) // 0x28 = call
#endif

[<EntryPoint>]
let main argv =
    // Optional arguments select functions by name, e.g. "fold map2".
    let selected (case: Case) = argv.Length = 0 || Array.contains case.Fn argv
    #if FABLE_COMPILER
    printfn "# Fable on Node.js %s" (Fable.Core.JsInterop.emitJsExpr () "process.version")
    #else
    printfn "# %s, FSharp.Core %O" Runtime.InteropServices.RuntimeInformation.FrameworkDescription (typeof<int option>.Assembly.GetName().Version)
    let adapts = [ for fn in [ "fold"; "iteri"; "mapi"; "map2" ] -> sprintf "%s=%b/%b" fn (callsAdapt (fn + "Inline")) (callsAdapt (fn + "Optimized")) ]
    printfn "# Opaque callback adapted once (inlineIfLambda/optimizeClosure): %s" (String.Join(" ", adapts))
    if not (callsAdapt "foldOptimized") then
        printfn "# WARNING: OptimizeClosureIfNotInlined had no effect. Build with the .NET 11 SDK (F# 11)."
    try Process.GetCurrentProcess().PriorityClass <- ProcessPriorityClass.High with _ -> ()
    #endif
    let mutable state = 42
    let next () =
        state <- (state * 25173 + 13849) &&& 65535
        state % 1000
    printfn "function,call_site,count,method,ns,relative_to_current,bytes"
    let started = now ()
    let cases = [
        for size in [ 10; 10000 ] do
            let xs = ResizeArray(Array.init size (fun _ -> next ()))
            let ys = ResizeArray(Array.init size (fun _ -> next ()))
            yield! buildCases 3 xs ys |> List.filter selected ]
    warmupShared cases
    for case in cases do run case
    printfn "# All correctness checks passed. Total %.0f s. Consumed: %d" ((now () - started) / 1000.0) consumed
    0
