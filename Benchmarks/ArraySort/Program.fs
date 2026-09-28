module ArraySortBenchmark

open System.Diagnostics

// Explicit qualification ensures this benchmarks FSharp.Core, not ArrayT.
let sortArrayInt (xs: int[]) = Microsoft.FSharp.Collections.Array.sort xs
let sortArrayFloat (xs: float[]) = Microsoft.FSharp.Collections.Array.sort xs
let sortArrayString (xs: string[]) = Microsoft.FSharp.Collections.Array.sort xs
let sortArrayGeneric (xs: 'T[]) = Microsoft.FSharp.Collections.Array.sort xs

let now () =
    #if FABLE_COMPILER
    Fable.Core.JsInterop.emitJsExpr () "performance.now()"
    #else
    float (Stopwatch.GetTimestamp()) * 1000.0 / float Stopwatch.Frequency
    #endif

let mutable consumed = 0

let batch count action =
    let started = now ()
    for _ = 1 to count do consumed <- consumed ^^^ action ()
    now () - started

let bench kind count (methods: (string * (unit -> int))[]) =
    for _, action in methods do
        let start = now ()
        while now () - start < 300.0 do batch 1 action |> ignore
    let counts = methods |> Array.map (fun (_, action) ->
        let mutable count = 1
        while batch count action < 60.0 do count <- count * 2
        count)
    let samples = Array.init methods.Length (fun _ -> Array.zeroCreate 9)
    for round = 0 to 8 do
        for step = 0 to methods.Length - 1 do
            let m = (round + step) % methods.Length
            samples.[m].[round] <- batch counts.[m] (snd methods.[m]) / float counts.[m]
    let medians = samples |> Array.map (fun xs -> Array.sortInPlace xs; xs.[4])
    for m = 0 to methods.Length - 1 do
        printfn "%s,%d,%s,%.6f,%.3f" kind count (fst methods.[m]) medians.[m] (medians.[m] / medians.[0])

let prepare (values: 'T[]) (arraySort: 'T[] -> 'T[]) (resizeSort: ResizeArray<'T> -> ResizeArray<'T>) =
    // Each API receives its native input representation. Conversions are setup only.
    let resizeInput = ResizeArray values
    let original = Array.copy values
    let expected = Array.sortWith compare values
    let check (actual: 'T[]) =
        if compare actual expected <> 0 then failwith "Ordering differs"
    let result = arraySort values
    if obj.ReferenceEquals(result, values) then failwith "Array.sort did not copy"
    check result
    check (sortArrayGeneric values)
    let genericResult = ResizeArrayT.ResizeArray.sort resizeInput
    let typedResult = resizeSort resizeInput
    if obj.ReferenceEquals(genericResult, resizeInput) || obj.ReferenceEquals(typedResult, resizeInput) then
        failwith "ResizeArray sort did not copy"
    check (genericResult.ToArray())
    check (typedResult.ToArray())
    if compare original values <> 0 || compare original (resizeInput.ToArray()) <> 0 then
        failwith "Input changed"
    [|
        "Array.sort/typed-call", fun () -> (arraySort values).Length
        "Array.sort/generic-wrapper", fun () -> (sortArrayGeneric values).Length
        "ResizeArray.sort", fun () -> (ResizeArrayT.ResizeArray.sort resizeInput).Count
        "ResizeArray.sortTyped", fun () -> (resizeSort resizeInput).Count
    |]

[<EntryPoint>]
let main _ =
    prepare [||] sortArrayInt ResizeArrayT.ResizeArray.sortInt |> ignore
    prepare [|System.Int32.MaxValue; 0; System.Int32.MinValue; -1|] sortArrayInt ResizeArrayT.ResizeArray.sortInt |> ignore
    prepare [||] sortArrayFloat ResizeArrayT.ResizeArray.sortFloat |> ignore
    prepare [|nan; infinity; -infinity; -0.0; 0.0; nan; System.Double.Epsilon|] sortArrayFloat ResizeArrayT.ResizeArray.sortFloat |> ignore
    let mutable state = 42
    let next () =
        state <- (state * 25173 + 13849) &&& 65535
        state - 32768
    printfn "type,count,method,ms,time_relative_to_Array_sort"
    for size in [16; 1024; 10000] do
        let values = Array.init size (fun _ -> next ())
        let floats = Array.map (fun x -> float x / 7.0) values
        #if FABLE_COMPILER
        if size = 16 then
            let arrayKind: string = Fable.Core.JsInterop.emitJsExpr values "$0.constructor.name"
            let floatKind: string = Fable.Core.JsInterop.emitJsExpr floats "$0.constructor.name"
            printfn "# Fable input representations: int=%s float=%s ResizeArray=Array" arrayKind floatKind
        #else
        if size = 16 then printfn "# FSharp.Core=%s" (typeof<int option>.Assembly.GetName().Version.ToString())
        #endif
        bench "int/random" size (prepare values sortArrayInt ResizeArrayT.ResizeArray.sortInt)
        bench "float/random" size (prepare floats sortArrayFloat ResizeArrayT.ResizeArray.sortFloat)
    let strings = Array.init 10000 (fun _ -> string (next ()))
    let methods = prepare strings sortArrayString ResizeArrayT.ResizeArray.sort
    // There is no typed string ResizeArray API; avoid timing the generic method twice.
    bench "string/random" strings.Length methods.[0..2]
    printfn "# All correctness checks passed. Consumed: %d" consumed
    0
