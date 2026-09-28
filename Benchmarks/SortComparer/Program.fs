module SortComparerBenchmark

open System
open System.Diagnostics

// Keep these generic and non-inline, like the public ResizeArray.sort function.
let sortCompare<'T when 'T: comparison> (source: ResizeArray<'T>) =
    if isNull source then nullArg "source"
    let result = source.GetRange(0, source.Count)
    result.Sort(Operators.compare)
    result

let sortFast<'T when 'T: comparison> (source: ResizeArray<'T>) =
    if isNull source then nullArg "source"
    let result = source.GetRange(0, source.Count)
    result.Sort(LanguagePrimitives.FastGenericComparer<'T>)
    result

type Entry = { Key: int; Text: string }

let mutable consumed = 0

let now () =
    #if FABLE_COMPILER
    Fable.Core.JsInterop.emitJsExpr () "performance.now()"
    #else
    float (Stopwatch.GetTimestamp()) * 1000.0 / float Stopwatch.Frequency
    #endif

let batch iterations sort (source: ResizeArray<'T>) =
    let started = now ()
    for _ = 1 to iterations do
        let result: ResizeArray<'T> = sort source
        consumed <- consumed ^^^ result.Count
    now () - started

let median (values: float[]) =
    Array.sortInPlace values
    values.[values.Length / 2]

let check name (source: ResizeArray<'T>) =
    let original = source.ToArray()
    let baseline = sortCompare source
    let candidate = sortFast source
    let actual = ResizeArrayT.ResizeArray.sort source
    // Generic comparison also handles NaN, unlike equality.
    for result in [baseline; candidate; actual] do
        if result.Count <> source.Count then failwithf "%s: wrong count" name
        for i = 0 to result.Count - 1 do
            if compare result.[i] baseline.[i] <> 0 then
                failwithf "%s: result differs at %d" name i
        if obj.ReferenceEquals(source, result) then failwithf "%s: did not copy" name
    if compare original (source.ToArray()) <> 0 then failwithf "%s: mutated input" name

let bench name source =
    check name source
    // Warm both paths before calibration; alternate their order in measured rounds.
    for sort in [sortCompare; sortFast] do
        let started = now ()
        while now () - started < 300.0 do
            batch 1 sort source |> ignore
    let calibrate sort =
        let mutable count = 1
        while batch count sort source < 60.0 do
            count <- count * 2
        count
    let baselineCount = calibrate sortCompare
    let fastCount = calibrate sortFast
    let baselineTimes = Array.zeroCreate 9
    let fastTimes = Array.zeroCreate 9
    for round = 0 to 8 do
        let baseline () = baselineTimes.[round] <- batch baselineCount sortCompare source / float baselineCount
        let fast () = fastTimes.[round] <- batch fastCount sortFast source / float fastCount
        if round % 2 = 0 then baseline (); fast ()
        else fast (); baseline ()
    let baselineMs = median baselineTimes
    let fastMs = median fastTimes
    #if FABLE_COMPILER
    let allocations = "n/a,n/a"
    #else
    let allocated sort =
        let before = GC.GetAllocatedBytesForCurrentThread()
        batch 10 sort source |> ignore
        (GC.GetAllocatedBytesForCurrentThread() - before) / 10L
    let allocations = sprintf "%d,%d" (allocated sortCompare) (allocated sortFast)
    #endif
    printfn "%s,%d,%.6f,%.6f,%.3f,%s" name source.Count baselineMs fastMs (baselineMs / fastMs) allocations

let runGeneric () =
    check "empty" (ResizeArray<int>())
    check "singleton" (ResizeArray [42])
    check "ordinal/null strings" (ResizeArray ["a"; "Z"; null; "ä"; "A"; ""; "z"])
    check "NaN/infinity" (ResizeArray [nan; 1.0; -infinity; -0.0; infinity; nan; 0.0; -1.0])
    check "options" (ResizeArray [Some 3; None; Some -1; Some 3])
    check "nested arrays" (ResizeArray [ [|2; 1|]; [||]; [|1; 2|]; [|1|] ])
    printfn "Correctness checks passed."
    printfn "type,count,compare_ms,fast_ms,speedup,compare_bytes,fast_bytes"
    // Identical pseudo-random input on .NET and JS, including duplicate keys.
    let mutable state = 42
    let next () =
        state <- (state * 25173 + 13849) &&& 65535
        state - 32768
    for size in [16; 1024; 10000] do
        let ints = Array.init size (fun _ -> next ())
        bench "int/random" (ResizeArray ints)
        bench "float/random" (ResizeArray (Array.map (fun x -> float x / 7.0) ints))
        bench "string/random" (ResizeArray (Array.map string ints))
        bench "tuple/random" (ResizeArray (Array.map (fun x -> (x % 97, string x)) ints))
        bench "record/random" (ResizeArray (Array.map (fun x -> { Key = x % 97; Text = string x }) ints))
    let sorted = [|0 .. 9999|]
    bench "int/sorted" (ResizeArray sorted)
    bench "int/reversed" (ResizeArray (Array.rev sorted))
    bench "int/equal" (ResizeArray (Array.create 10000 42))
    printfn "Consumed: %d" consumed
    0

[<EntryPoint>]
let main args =
    if Array.contains "--typed" args then
        TypedBenchmark.run ()
        0
    else
        runGeneric ()
