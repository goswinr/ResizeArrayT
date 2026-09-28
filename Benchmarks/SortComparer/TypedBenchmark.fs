module TypedBenchmark

open System
open System.Diagnostics

let private now () =
    #if FABLE_COMPILER
    Fable.Core.JsInterop.emitJsExpr () "performance.now()"
    #else
    float (Stopwatch.GetTimestamp()) * 1000.0 / float Stopwatch.Frequency
    #endif

let mutable private consumed = 0

let private batch count sort source =
    let started = now ()
    for _ = 1 to count do
        let result: ResizeArray<'T> = sort source
        consumed <- consumed ^^^ result.Count
    now () - started

let private check (source: ResizeArray<'T>) methods =
    let original = source.ToArray()
    let expected = Array.sortWith compare original
    for name, sort in methods do
        let result: ResizeArray<'T> = sort source
        if result.Count <> expected.Length then failwithf "%s: count" name
        for i = 0 to result.Count - 1 do
            if compare result.[i] expected.[i] <> 0 then failwithf "%s: ordering at %d" name i
        if compare original (source.ToArray()) <> 0 then failwithf "%s: changed input" name
        if obj.ReferenceEquals(source, result) then failwithf "%s: no copy" name

let private bench kind source methods =
    check source methods
    for _, sort in methods do
        let started = now ()
        while now () - started < 300.0 do batch 1 sort source |> ignore
    let counts = methods |> Array.map (fun (_, sort) ->
        let mutable n = 1
        while batch n sort source < 60.0 do n <- n * 2
        n)
    let samples = Array.init methods.Length (fun _ -> Array.zeroCreate 9)
    for round = 0 to 8 do
        for step = 0 to methods.Length - 1 do
            let m = (round + step) % methods.Length
            samples.[m].[round] <- batch counts.[m] (snd methods.[m]) source / float counts.[m]
    let medians = samples |> Array.map (fun xs -> Array.sortInPlace xs; xs.[4])
    for m = 0 to methods.Length - 1 do
        printfn "%s,%d,%s,%.6f,%.3f" kind source.Count (fst methods.[m]) medians.[m] (medians.[0] / medians.[m])

let run () =
    let intMethods = [|
        "generic", ResizeArrayT.ResizeArray.sort<int>
        "sortWith", TypedSort.ints
        "directTyped", TypedSort.directInt
        "specialized", TypedSort.sortInt
    |]
    let floatMethods = [|
        "generic", ResizeArrayT.ResizeArray.sort<float>
        "sortWith", TypedSort.floats
        "directTyped", TypedSort.directFloat
        "specialized", TypedSort.sortFloat
    |]
    check (ResizeArray<int>()) intMethods
    check (ResizeArray<float>()) floatMethods
    check (ResizeArray [42]) intMethods
    check (ResizeArray [nan]) floatMethods
    for methods in [intMethods |> Array.map (fun (n, f) -> n, fun () -> f null |> ignore)
                    floatMethods |> Array.map (fun (n, f) -> n, fun () -> f null |> ignore)] do
        for name, call in methods do
            let mutable threw = false
            try call () with _ -> threw <- true
            if not threw then failwithf "%s: accepted null" name
    let intEdges = [|Int32.MinValue; Int32.MaxValue; 0; -1; 1; 0|]
    let floatEdges = [|nan; -infinity; -Double.MaxValue; -1.0; -Double.Epsilon; -0.0;
                       0.0; Double.Epsilon; 1.0; Double.MaxValue; infinity; nan|]
    // Every pair, including extreme integer subtraction, NaNs and signed zeros.
    for a in intEdges do
        for b in intEdges do check (ResizeArray [a; b]) intMethods
    for a in floatEdges do
        for b in floatEdges do check (ResizeArray [a; b]) floatMethods
    check (ResizeArray floatEdges) floatMethods
    check (ResizeArray (Array.rev floatEdges)) floatMethods
    printfn "Correctness checks passed."
    printfn "type,count,method,ms,speedup_vs_generic"
    let mutable state = 42
    let next () =
        state <- (state * 25173 + 13849) &&& 65535
        state - 32768
    for size in [16; 1024; 10000] do
        let values = Array.init size (fun _ -> next ())
        bench "int/random" (ResizeArray values) intMethods
        bench "float/random" (ResizeArray (Array.map (fun x -> float x / 7.0) values)) floatMethods
    let mixed = Array.init 10000 (fun i -> if i % 10 = 0 then nan else float (next ()) / 7.0)
    bench "float/10pct-NaN" (ResizeArray mixed) floatMethods
    printfn "Consumed: %d" consumed
