namespace Tests
open ResizeArrayT

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open System
open System.Collections.Generic

type Assert =

    static member AreEqual(expected : ResizeArray<'T>, actual : ResizeArray<'T>, message : string) =
        if not <| expected.IsEqualTo actual then
            failwithf  "%s: AreEqual Expected \r\n%A \r\nbut got \r\n%A" message expected actual

    static member AreNotEqual(expected : ResizeArray<'T>, actual : ResizeArray<'T>, message : string) =
        if expected.IsEqualTo actual then
            failwithf "%s: AreNotEqual expected \r\n%A \r\n NOT to equal \r\n%A" message expected actual
            Exception message |> raise

    static member AreEqual(expected : ResizeArray<'T>, actual : ResizeArray<'T>) =
        if not <| expected.IsEqualTo actual then
            failwithf "AreEqual expected \r\n%A \r\nbut got \r\n%A" expected actual


    static member AreEqual(expected : 'T option, actual : 'T option) =
        match expected, actual with
        | None, None -> ()
        | None, Some _
        | Some _, None -> failwithf "AreEqual expected \r\n%A \r\nbut got \r\n%A" expected actual
        | Some e, Some a ->
            if not <| e.Equals(a) then failwithf "AreEqual expected \r\n%A \r\nbut got \r\n%A" expected actual

    static member AreEqual(expected : float, actual : float) =
        //use a tolerance the first 12 digits for float comparisons
        let tol = abs expected * 1e-12 + abs actual * 1e-12
        if abs(expected-actual) > tol then
            failwithf "AreEqual expected \r\n%A \r\nbut got \r\n%A" expected actual

    static member AreEqual(expected : string, actual : string) =
        if expected <> actual then
            failwithf "AreEqual expected \r\n%A \r\nbut got \r\n%A" expected actual


    static member Fail(message : string) = Exception(message) |> raise

    static member Fail() : unit = Assert.Fail("")

    static member True(condition : bool) =
        if not condition then
            Exception("Assertion failed: Expected true but got false") |> raise

    static member False(condition) =
        if condition then
            Exception("Assertion failed: Expected false but got true") |> raise


module Exceptions =

    /// Check that the lambda throws an exception of the given type. Otherwise
    /// calls Assert.Fail()
    let CheckThrowsExn<'a when 'a :> exn> (f : unit -> unit) =
        #if FABLE_COMPILER
            Expect.throws f "CheckThrowsExn"
        #else
            try
                let _ = f ()
                sprintf "Expected %O exception, got no exception" typeof<'a> |> Assert.Fail
            with
            | :? 'a -> ()
            | e -> sprintf "Expected %O exception, got: %O" typeof<'a> e |> Assert.Fail
        #endif

    let private CheckThrowsExn2<'a when 'a :> exn> _s (f : unit -> unit) =

        #if FABLE_COMPILER
            Expect.throws f "CheckThrowsExn2"
        #else

            let funcThrowsAsExpected =
                try
                    let _ = f ()
                    false // Did not throw!
                with
                | :? 'a
                    -> true   // Thew null ref, OK
                | _ -> false  // Did now throw a null ref exception!
            if funcThrowsAsExpected
            then ()
            else Assert.Fail(_s)
        #endif


    let throwsArg   f = CheckThrowsExn<ArgumentException>    f
    let throwsIdx  f = CheckThrowsExn<IndexOutOfRangeException>    f

    let throwsNull f = CheckThrowsExn<ArgumentNullException>    f
    let throwsKey f = CheckThrowsExn<KeyNotFoundException>    f



[<AutoOpen>]
module ExtensionOnArray =
    type ``[]``<'T> with
        member a.asRarr = ResizeArray(a)

    //let inline (==) a b = ResizeArray.equals a b
    //let inline (=+=) a b = ResizeArray.equals2 a b
    //let inline (=++=) (a:ResizeArray<'T>*ResizeArray<'T>) (b:ResizeArray<'T>*ResizeArray<'T>) = ResizeArray.equals (fst a) (fst b) && ResizeArray.equals (snd a) (snd b)
    //let inline (<!!>) (a:ResizeArray<'T>*ResizeArray<'T>) (b:ResizeArray<'T>*ResizeArray<'T>) = not <| ResizeArray.equals (fst a) (fst b) && not <| ResizeArray.equals (snd a) (snd b)
    //let inline (<!!!>) (a:ResizeArray<'T>*ResizeArray<'T>*ResizeArray<'T>) (b:ResizeArray<'T>*ResizeArray<'T>*ResizeArray<'T>) = not <| ResizeArray.equals (t1 a) (t1 b) && not <| ResizeArray.equals (t2 a) (t2 b) && not <| ResizeArray.equals (t3 a) (t3 b)
    //let inline (<!>) a b = not <| ResizeArray.equals a b

    /// shallow structural equality
    let inline (==) (a:ResizeArray<'T>) b =  ResizeArray.equals a b
    let inline (<!>) (a:ResizeArray<'T>) b =  not (ResizeArray.equals a b)
    let inline (=++=) (a:ResizeArray<'T>*ResizeArray<'T>) (b:ResizeArray<'T>*ResizeArray<'T>) = (fst a == fst b) && (snd a == snd b)
    let inline (<!!>) (a:ResizeArray<'T>*ResizeArray<'T>) (b:ResizeArray<'T>*ResizeArray<'T>) = (fst a <!> fst b) || (snd a <!> snd b)
    let inline (<!!!>) (aaa:ResizeArray<'T>*ResizeArray<'T>*ResizeArray<'T>) (bbb:ResizeArray<'T>*ResizeArray<'T>*ResizeArray<'T>) =
        let a,b,c = aaa
        let x,y,z = bbb
        (a <!> x) || (b <!> y) || (c <!> z)

    let inline (=+=) (aa:ResizeArray<ResizeArray<'T>>) (bb:ResizeArray<ResizeArray<'T>>) =
        let rec eq i =
            if i < aa.Count then
                let a = aa.[i]
                let b = bb.[i]
                if ResizeArray.equals a b then eq (i+1)
                else false
            else
                aa.Count=bb.Count
        eq 0


    let eqi (rarr1: ResizeArray<'K*ResizeArray<'T>>) (rarr2: ResizeArray<'K*ResizeArray<'T>>) =
        if rarr1.Count <> rarr2.Count then false
        else
            let rec eq i =
                if i < rarr1.Count then
                    let i1,r1 = rarr1.[i]
                    let i2,r2 = rarr2.[i]
                    if ResizeArray.equals r1 r2 && i1=i2 then eq (i+1)
                    else false
                else
                    true
            eq 0

    let inline (<*>) a b = not <| eqi a b
    //let inline (=*=) a b = eqi a b

module Module =
 open Exceptions

 let tests =
  testList "Module.fs Tests" [



    testCase "equals3" <| fun _ ->
        let a = [|[|[|1;4 |].asRarr; [|2;5 |].asRarr; [|3;6 |].asRarr |].asRarr; [||].asRarr|].asRarr
        let b = [|[|[|1;4 |].asRarr; [|2;5 |].asRarr; [|3;6 |].asRarr |].asRarr; [||].asRarr|].asRarr
        Assert.True (ResizeArray.equals3 a b)
        let c = [|[|[|1;4 |].asRarr; [|2;5 |].asRarr; [|3;0 |].asRarr |].asRarr; [||].asRarr|].asRarr
        Assert.False (ResizeArray.equals3 a c)

    testCase "ResizeArray<_>() // () " <| fun _ ->
        let emptyArray = ResizeArray<_>() // <int>
        if ResizeArray.length emptyArray <> 0 then Assert.Fail()

        let c : int ResizeArray   = ResizeArray<_>() // <int>
        Assert.True( (c == [|   |].asRarr) )

        let d = ResizeArray<_>() // <string>
        Assert.True( (d == [|   |].asRarr) )
        d.Add("a")
        Assert.True( (d == [| "a"  |].asRarr) )



    testCase "ResizeArray.AllPairs() " <| fun _ ->
        // integer array
        let resultInt =  ResizeArray.allPairs [|1..3  |].asRarr [|2..2..6  |].asRarr
        if resultInt <!> [|(1,2);(1,4);(1,6)
                           (2,2);(2,4);(2,6)
                           (3,2);(3,4);(3,6)  |].asRarr then Assert.Fail()

        // string array
        let resultStr = ResizeArray.allPairs [|"A"; "B"; "C" ; "D"   |].asRarr [|"a";"b";"c";"d"  |].asRarr
        if resultStr <!> [|("A","a");("A","b");("A","c");("A","d")
                           ("B","a");("B","b");("B","c");("B","d")
                           ("C","a");("C","b");("C","c");("C","d")
                           ("D","a");("D","b");("D","c");("D","d")  |].asRarr then Assert.Fail()

        // empty array
        if ResizeArray.allPairs [| |].asRarr     [| |].asRarr <!> [| |].asRarr  then Assert.Fail()
        if ResizeArray.allPairs [|1..3 |].asRarr [| |].asRarr <!> [| |].asRarr  then Assert.Fail()
        if ResizeArray.allPairs [| |].asRarr [|1..3 |].asRarr <!> [| |].asRarr  then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.allPairs nullArr nullArr  |> ignore)
        throwsNull (fun () -> ResizeArray.allPairs [| |].asRarr    nullArr  |> ignore)
        throwsNull (fun () -> ResizeArray.allPairs nullArr [| |].asRarr     |> ignore)



    testCase "ResizeArray.Append() " <| fun _ ->
        // integer array
        let intArray = ResizeArray.append [| 1; 2  |].asRarr [| 3; 4  |].asRarr
        Assert.True( (intArray == [| 1; 2; 3; 4  |].asRarr) )

        // string array
        let strArray = ResizeArray.append [| "a"; "b"  |].asRarr [| "C"; "D"  |].asRarr
        Assert.True( (strArray == [| "a"; "b"; "C"; "D"  |].asRarr) )

        // empty array
        let emptyArray : int ResizeArray  = [|    |].asRarr
        let singleArray : int ResizeArray = [| 1  |].asRarr

        let appEmptySingle = ResizeArray.append emptyArray singleArray
        let appSingleEmpty = ResizeArray.append singleArray emptyArray

        Assert.True( (appEmptySingle == [| 1  |].asRarr) )
        Assert.True( (appSingleEmpty == [| 1  |].asRarr) )

        // null array
        let nullArray = null:int ResizeArray
        let validArray = [| 1  |].asRarr
        throwsNull (fun () -> ResizeArray.append validArray nullArray |> ignore)
        throwsNull (fun () -> ResizeArray.append nullArray validArray |> ignore)



    testCase "ResizeArray.Average()1 " <| fun _ ->

        // empty float32 array
        let emptyFloatArray = ResizeArray<float32>() // <float32>
        throwsArg(fun () -> ResizeArray.average emptyFloatArray |> ignore)

        // empty double array
        let emptyDoubleArray = ResizeArray<float>() // <System.Double>
        throwsArg(fun () -> ResizeArray.average emptyDoubleArray |> ignore)

        // empty decimal array
        let emptyDecimalArray = ResizeArray<decimal>() // <System.Decimal>
        throwsArg (fun () -> ResizeArray.average emptyDecimalArray |>ignore )

    testCase "ResizeArray.Average()2 " <| fun _ ->
        // float32 array
        let floatArray: float32 ResizeArray = [| 1.2f; 3.5f; 6.7f  |].asRarr
        let averageOfFloat = ResizeArray.average floatArray
        if averageOfFloat <> 3.8000000000000003f then Assert.Fail()

        // double array
        let doubleArray: System.Double ResizeArray = [| 1.0;8.0  |].asRarr
        let averageOfDouble = ResizeArray.average doubleArray
        if averageOfDouble <> 4.5 then Assert.Fail()

    testCase "ResizeArray.Average()3 " <| fun _ ->
        // decimal array
        let decimalArray: decimal ResizeArray = [| 0M; 19M; 19.03M  |].asRarr
        let averageOfDecimal = ResizeArray.average decimalArray
        if abs (averageOfDecimal - 12.6766666666667M) > 0.0001M then Assert.Fail($"averageOfDecimal:12.676666666666667M <> {averageOfDecimal}")

    testCase "ResizeArray.Average()4 " <| fun _ ->
        // null array
        let nullArr = null : double ResizeArray
        throwsNull (fun () -> ResizeArray.average nullArr |> ignore)



    testCase "ResizeArray.AverageBy() " <| fun _ ->

        // empty double array
        let emptyDouArray = ResizeArray<_>() // <System.Double>
        throwsArg(fun () -> ResizeArray.averageBy (fun x -> x + 6.7) emptyDouArray |> ignore)

        // empty float32 array
        let emptyFloat32Array: float32 ResizeArray = [| |].asRarr
        throwsArg(fun () -> ResizeArray.averageBy (fun x -> x + 9.8f) emptyFloat32Array |> ignore)

        // empty decimal array
        let emptyDecimalArray = ResizeArray<_>() // <System.Decimal>
        throwsArg(fun () -> ResizeArray.averageBy (fun x -> x + 9.8M) emptyDecimalArray |> ignore)

        // float32 array
        let floatArray: float32 ResizeArray = [| 1.5f; 2.5f; 3.5f; 4.5f  |].asRarr // using values that behave nicely with IEEE floats
        let averageOfFloat = ResizeArray.averageBy (fun x -> x + 1.0f) floatArray
        Assert.AreEqual(float 4.0f, float averageOfFloat)

        // double array
        let doubleArray: System.Double ResizeArray = [| 1.0; 8.0  |].asRarr // using values that behave nicely with IEEE doubles
        let averageOfDouble = ResizeArray.averageBy (fun x -> x + 1.0) doubleArray
        Assert.AreEqual(float 5.5, float averageOfDouble)

        // decimal array
        let decimalArray: decimal ResizeArray = [| 0M;19M;19.03M  |].asRarr
        let averageOfDecimal = ResizeArray.averageBy (fun x -> x + 9.8M) decimalArray
        Assert.AreEqual(float 22.476666666666666666666666667M, float averageOfDecimal)

        // null array
        let nullArr : double ResizeArray = null
        throwsNull (fun () -> ResizeArray.averageBy (fun x -> x + 6.7) nullArr |> ignore)



    testCase "ResizeArray.ChunkBySize() " <| fun _ ->

        // int Seq
        Assert.True([| [|1..4 |].asRarr; [|5..8 |].asRarr  |].asRarr =+= ResizeArray.chunkBySize 4 [|1..8 |].asRarr)
        Assert.True([| [|1..4 |].asRarr; [|5..8 |].asRarr; [|9..10 |].asRarr  |].asRarr =+= ResizeArray.chunkBySize 4 [|1..10 |].asRarr)
        Assert.True([| [|1 |].asRarr; [|2 |].asRarr; [|3 |].asRarr; [|4 |].asRarr  |].asRarr =+= ResizeArray.chunkBySize 1 [|1..4 |].asRarr)
        Assert.True([| [|1..3 |].asRarr; [|4 |].asRarr  |].asRarr =+= ResizeArray.chunkBySize 3 [|1..4 |].asRarr)
        Assert.True([| [|1..5 |].asRarr; [|6..10 |].asRarr; [|11..12 |].asRarr  |].asRarr =+= ResizeArray.chunkBySize 5 [|1..12 |].asRarr)

        // string Seq
        Assert.True([| [|"a"; "b" |].asRarr; [|"c";"d" |].asRarr; [|"e" |].asRarr  |].asRarr =+= ResizeArray.chunkBySize 2 [|"a";"b";"c";"d";"e" |].asRarr)

        // empty Seq
        Assert.True([| |].asRarr =+= ResizeArray.chunkBySize 3 [| |].asRarr)

        // null Seq
        let nullArr:_ ResizeArray = null
        throwsNull (fun () -> ResizeArray.chunkBySize 3 nullArr |> ignore)

        // invalidArg
        throwsArg (fun () -> ResizeArray.chunkBySize 0 [|1..10 |].asRarr |> ignore)
        throwsArg (fun () -> ResizeArray.chunkBySize -1 [|1..10 |].asRarr |> ignore)



    testCase "ResizeArray.SplitInto() " <| fun _ ->

        // int array
        Assert.True([| [|1..4 |].asRarr; [|5..7 |].asRarr; [|8..10 |].asRarr  |].asRarr =+= ResizeArray.splitInto 3 [|1..10 |].asRarr)
        Assert.True([| [|1..4 |].asRarr; [|5..8 |].asRarr; [|9..11 |].asRarr  |].asRarr =+= ResizeArray.splitInto 3 [|1..11 |].asRarr)
        Assert.True([| [|1..4 |].asRarr; [|5..8 |].asRarr; [|9..12 |].asRarr  |].asRarr =+= ResizeArray.splitInto 3 [|1..12 |].asRarr)

        Assert.True([| [|1..2 |].asRarr; [|3 |].asRarr; [|4 |].asRarr; [|5 |].asRarr  |].asRarr =+= ResizeArray.splitInto 4 [|1..5 |].asRarr)
        Assert.True([| [|1 |].asRarr; [|2 |].asRarr; [|3 |].asRarr; [|4 |].asRarr  |].asRarr =+= ResizeArray.splitInto 20 [|1..4 |].asRarr)

        // string array
        Assert.True([| [|"a"; "b" |].asRarr; [|"c";"d" |].asRarr; [|"e" |].asRarr  |].asRarr =+= ResizeArray.splitInto 3 [|"a";"b";"c";"d";"e" |].asRarr)

        // empty array
        Assert.True([|  |].asRarr =+= ResizeArray.splitInto 3 [|  |].asRarr)

        // null array
        let nullArr:_ ResizeArray = null
        throwsNull (fun () -> ResizeArray.splitInto 3 nullArr |> ignore)

        // invalidArg
        throwsArg (fun () -> ResizeArray.splitInto 0 [|1..10 |].asRarr |> ignore)
        throwsArg (fun () -> ResizeArray.splitInto -1 [|1..10 |].asRarr |> ignore)



    testCase "ResizeArray.distinct() " <| fun _ ->
        // distinct should work on empty array
        Assert.AreEqual([| |].asRarr, ResizeArray.distinct [| |].asRarr)

        // distinct not should work on null
        throwsNull (fun () -> ResizeArray.distinct null |> ignore)

        // distinct should filter out simple duplicates
        Assert.AreEqual([|1 |].asRarr, ResizeArray.distinct [|1 |].asRarr)
        Assert.AreEqual([|1 |].asRarr, ResizeArray.distinct [|1; 1 |].asRarr)
        Assert.AreEqual([|1; 2; 3 |].asRarr, ResizeArray.distinct [|1; 2; 3; 1 |].asRarr)
        Assert.AreEqual([|[1;2]; [1;3] |].asRarr, ResizeArray.distinct [|[1;2]; [1;3]; [1;2]; [1;3] |].asRarr)
        Assert.AreEqual([|[1;1]; [1;2]; [1;3]; [1;4] |].asRarr, ResizeArray.distinct [|[1;1]; [1;2]; [1;3]; [1;4] |].asRarr)
        Assert.AreEqual([|[1;1]; [1;4] |].asRarr, ResizeArray.distinct [|[1;1]; [1;1]; [1;1]; [1;4] |].asRarr)

        Assert.AreEqual([|null |].asRarr, ResizeArray.distinct [|null |].asRarr)
        let list = new System.Collections.Generic.List<int>()
        Assert.AreEqual([|null, list |].asRarr, ResizeArray.distinct [|null, list |].asRarr)

    testCase "ResizeArray.distinctBy() " <| fun _ ->
        // distinctBy should work on empty array
        Assert.AreEqual([| |].asRarr, ResizeArray.distinctBy (fun _ -> failwith "should not be executed") [| |].asRarr)

        // distinctBy should not work on null
        throwsNull (fun () -> ResizeArray.distinctBy (fun _ -> failwith "should not be executed") null |> ignore)

        // distinctBy should filter out simple duplicates
        Assert.AreEqual([|1 |].asRarr, ResizeArray.distinctBy id [|1 |].asRarr)
        Assert.AreEqual([|1 |].asRarr, ResizeArray.distinctBy id [|1; 1 |].asRarr)
        Assert.AreEqual([|1; 2; 3 |].asRarr, ResizeArray.distinctBy id [|1; 2; 3; 1 |].asRarr)

        // distinctBy should use the given projection to filter out simple duplicates
        Assert.AreEqual([|1 |].asRarr, ResizeArray.distinctBy (fun x -> x / x) [|1; 2 |].asRarr)
        Assert.AreEqual([|1; 2 |].asRarr, ResizeArray.distinctBy (fun x -> if x < 3 then x else 1) [|1; 2; 3; 4 |].asRarr)
        Assert.AreEqual([| [1;2]; [1;3] |].asRarr, ResizeArray.distinctBy (fun x -> List.sum x) [| [1;2]; [1;3]; [2;1] |].asRarr)

        Assert.AreEqual([|null |].asRarr, ResizeArray.distinctBy id [|null |].asRarr)
        let list = new System.Collections.Generic.List<int>()
        Assert.AreEqual([|null, list |].asRarr, ResizeArray.distinctBy id [|null, list |].asRarr)

    testCase "ResizeArray.Except() " <| fun _ ->
        // integer array
        let intArr1 = [| yield! seq{1..100}
                         yield! seq{1..100}  |].asRarr
        let intArr2 = [| 1 .. 10  |].asRarr
        let expectedIntArr = [| 11 .. 100  |].asRarr

        Assert.AreEqual(expectedIntArr, ResizeArray.except intArr2 intArr1)

        // string array
        let strArr1 = [| "a"; "b"; "c"; "d"; "a"  |].asRarr
        let strArr2 = [| "b"; "c"  |].asRarr
        let expectedStrArr = [| "a"; "d"  |].asRarr

        Assert.AreEqual(expectedStrArr, ResizeArray.except strArr2 strArr1)

        // empty array
        let emptyIntArr = [|  |].asRarr
        Assert.AreEqual([|1..100 |].asRarr, ResizeArray.except emptyIntArr intArr1)
        Assert.AreEqual(emptyIntArr, ResizeArray.except intArr1 emptyIntArr)
        Assert.AreEqual(emptyIntArr, ResizeArray.except emptyIntArr emptyIntArr)
        Assert.AreEqual(emptyIntArr, ResizeArray.except intArr1 intArr1)

        // null array
        let nullArr : int  ResizeArray = null
        throwsNull(fun () -> ResizeArray.except nullArr emptyIntArr |> ignore)
        throwsNull(fun () -> ResizeArray.except emptyIntArr nullArr |> ignore)
        throwsNull(fun () -> ResizeArray.except nullArr nullArr |> ignore)



    testCase "ResizeArray.Take() " <| fun _ ->
        Assert.AreEqual([| |].asRarr, ResizeArray.take 0 [| |].asRarr)
        Assert.AreEqual([| |].asRarr, ResizeArray.take 0 [|"str1";"str2";"str3";"str4" |].asRarr)
        Assert.AreEqual([|1;2;4 |].asRarr, ResizeArray.take 3 [|1;2;4;5;7 |].asRarr)
        Assert.AreEqual([|"str1";"str2" |].asRarr, ResizeArray.take 2 [|"str1";"str2";"str3";"str4" |].asRarr)
        Assert.AreEqual( [|"str1";"str2";"str3";"str4" |].asRarr, ResizeArray.take 4 [|"str1";"str2";"str3";"str4" |].asRarr)

        throwsArg (fun () -> ResizeArray.take 1 [| |].asRarr |> ignore)
        throwsArg (fun () -> ResizeArray.take -1 [|0;1 |].asRarr |> ignore)
        throwsArg (fun () -> ResizeArray.take 5 [|"str1";"str2";"str3";"str4" |].asRarr |> ignore)
        throwsNull (fun () -> ResizeArray.take 5 null |> ignore)

    testCase "ResizeArray.takeWhile() " <| fun _ ->
        Assert.AreEqual([| |].asRarr, ResizeArray.takeWhile (fun _ -> failwith "should not be used") [| |].asRarr)
        Assert.AreEqual([|1;2;4;5 |].asRarr, ResizeArray.takeWhile (fun x -> x < 6) [|1;2;4;5;6;7 |].asRarr)
        Assert.AreEqual([|"a"; "ab"; "abc" |].asRarr, ResizeArray.takeWhile (fun (x:string) -> x.Length < 4) [|"a"; "ab"; "abc"; "abcd"; "abcde" |].asRarr)
        Assert.AreEqual([|"a"; "ab"; "abc"; "abcd"; "abcde" |].asRarr, ResizeArray.takeWhile (fun _ -> true) [|"a"; "ab"; "abc"; "abcd"; "abcde" |].asRarr)
        Assert.AreEqual([| |].asRarr, ResizeArray.takeWhile (fun _ -> false) [|"a"; "ab"; "abc"; "abcd"; "abcde" |].asRarr)
        Assert.AreEqual([| |].asRarr, ResizeArray.takeWhile (fun _ -> false) [|"a" |].asRarr)
        Assert.AreEqual([|"a" |].asRarr, ResizeArray.takeWhile (fun _ -> true) [|"a" |].asRarr)
        Assert.AreEqual([|"a" |].asRarr, ResizeArray.takeWhile (fun x -> x <> "ab") [|"a"; "ab"; "abc"; "abcd"; "abcde" |].asRarr)

        throwsNull (fun () -> ResizeArray.takeWhile (fun _ -> failwith "should not be used") null |> ignore)

    testCase "ResizeArray.splitAt() " <| fun _ ->
        Assert.AreEqual([| |].asRarr, ResizeArray.splitAt 0 [| |].asRarr |> fst)
        Assert.AreEqual([| |].asRarr, ResizeArray.splitAt 0 [| |].asRarr |> snd)

        Assert.AreEqual([|1..4 |].asRarr, ResizeArray.splitAt 4 [|1..10 |].asRarr |> fst)
        Assert.AreEqual([|5..10 |].asRarr, ResizeArray.splitAt 4 [|1..10 |].asRarr |> snd)

        Assert.AreEqual([| |].asRarr, ResizeArray.splitAt 0 [|1..2 |].asRarr |> fst)
        Assert.AreEqual([|1..2 |].asRarr, ResizeArray.splitAt 0 [|1..2 |].asRarr |> snd)

        Assert.AreEqual([|1 |].asRarr, ResizeArray.splitAt 1 [|1..2 |].asRarr |> fst)
        Assert.AreEqual([|2 |].asRarr, ResizeArray.splitAt 1 [|1..2 |].asRarr |> snd)

        Assert.AreEqual([|1..2 |].asRarr, ResizeArray.splitAt 2 [|1..2 |].asRarr |> fst)
        Assert.AreEqual([| |].asRarr, ResizeArray.splitAt 2 [|1..2 |].asRarr |> snd)

        Assert.AreEqual([|"a" |].asRarr, ResizeArray.splitAt 1 [|"a";"b";"c" |].asRarr |> fst)
        Assert.AreEqual([|"b";"c" |].asRarr, ResizeArray.splitAt 1 [|"a";"b";"c" |].asRarr |> snd)

        // split should fail if index exceeds bounds
        throwsArg (fun () -> ResizeArray.splitAt 1 [| |].asRarr |> ignore)
        throwsArg (fun () -> ResizeArray.splitAt -1 [|0;1 |].asRarr |> ignore)
        throwsArg (fun () -> ResizeArray.splitAt 5 [|"str1";"str2";"str3";"str4" |].asRarr |> ignore)

        throwsNull (fun () -> ResizeArray.splitAt 0 null |> ignore)
        throwsNull (fun () -> ResizeArray.splitAt 1 null |> ignore)

    testCase "ResizeArray.replicate() " <| fun _ ->
        // replicate should create multiple copies of the given value
        Assert.AreEqual([| |].asRarr, ResizeArray.replicate 0 null)
        Assert.AreEqual([| |].asRarr, ResizeArray.replicate 0 1)
        Assert.AreEqual([|null |].asRarr, ResizeArray.replicate 1 null)
        Assert.AreEqual([|"1";"1" |].asRarr, ResizeArray.replicate 2 "1")

        throwsArg (fun () ->  ResizeArray.replicate -1 null |> ignore)

    testCase "ResizeArray.Blit() " <| fun _ ->
        // int array
        let intSrc = [| 1..10  |].asRarr
        let intDes:int ResizeArray = ResizeArray.create 10 Unchecked.defaultof<_>
        ResizeArray.blit intSrc 0 intDes 0 5

        if intDes.[4] <> 5 then Assert.Fail()
        if intDes.[5] <> 0 then Assert.Fail()

        // string array
        let strSrc = [| "a";"b";"c";"d";"e";"j" |].asRarr
        let strDes = ResizeArray.create 10 "w"
        ResizeArray.blit strSrc 1 strDes 2 3
        if strDes.[3] <> "c" || ResizeArray.get 4 strDes = "w" then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.blit nullArr 1 strDes 2 3 |> ignore)

        // bounds check
        throwsArg (fun () -> ResizeArray.blit intSrc -1 intDes 1 3 |> ignore)
        throwsArg (fun () -> ResizeArray.blit intSrc 1 intDes -1 3 |> ignore)
        throwsArg (fun () -> ResizeArray.blit intSrc 1 intDes 1 -3 |> ignore)
        throwsArg (fun () -> ResizeArray.blit intSrc 1 intDes 1 300 |> ignore)
        throwsArg (fun () -> ResizeArray.blit intSrc 1 intDes 5 8 |> ignore)



    testCase "ResizeArray.BlitExtend() " <| fun _ ->
        // int array
        let input = [| 10..19  |].asRarr
        let insert1  = [| 0..19 |].asRarr
        // let insert2 = [| 0..11 |].asRarr
        let expected  = [| 0..19 |].asRarr
        ResizeArray.blitExtend input 0 insert1 10 10
        Assert.AreEqual(expected,expected)

        ResizeArray.blitExtend input 0 insert1 10 10
        Assert.AreEqual(expected,expected)
        // TODO add more



    //member private this.
    let ChooseTester chooseInt chooseString =
            // int array
            let intSrc:int  ResizeArray = [| 1..100  |].asRarr
            let funcInt x = if (x%5=0) then Some x else None
            let intChosen : int ResizeArray = chooseInt funcInt intSrc
            if intChosen.[1] <> 10 then Assert.Fail()

            // string array
            let stringSrc: string  ResizeArray = "Lists are a commonly used data structure. They are not mutable, i.e., you can't delete an element of a list  instead you create a new list with the element deleted. List values often share storage under the hood, i.e., a list value only allocate more memory when you actually execute construction operations.".Split([|' ' |], System.StringSplitOptions.RemoveEmptyEntries).asRarr
            let funcString x =
                match x with
                | "list"-> Some x
                | "List" -> Some x
                | _ -> None
            let strChosen : string ResizeArray  = chooseString funcString stringSrc
            if strChosen.[1].ToLower() <> "list" then Assert.Fail()

            // empty array
            let emptySrc :int ResizeArray = [|  |].asRarr
            let emptyChosen = chooseInt funcInt emptySrc
            Assert.True( (emptyChosen == [|  |].asRarr) )

            // null array
            let nullArr = null:int ResizeArray
            throwsNull (fun () -> chooseInt funcInt nullArr |> ignore)



    testCase "ResizeArray.Choose() " <| fun _ ->
        ChooseTester ResizeArray.choose ResizeArray.choose

    #if FABLE_COMPILER
    #else
    testCase "ResizeArray.Parallel.Choose() " <| fun _ ->
        ChooseTester ResizeArray.Parallel.choose ResizeArray.Parallel.choose
    #endif

    // member private this.
    let CollectTester collectInt collectString =

            // int array - checking ordering
            let intSrc  = [| 1..3  |].asRarr
            //let func : int-> #seq<int> = fun i -> [| 1..i  |].asRarr
            let func  = fun i -> [| 1..i  |].asRarr
            let result : int ResizeArray = collectInt func intSrc
            Assert.AreEqual ([| 1; 1; 2; 1; 2; 3  |].asRarr, result)

            // string array
            let stringSrc = [| "foo"; "bar"  |].asRarr
            let func = fun s -> [| s  |].asRarr
            let result : string ResizeArray = collectString func stringSrc
            Assert.AreEqual(stringSrc, result)

            // empty array
            let emptyArray : string  ResizeArray = [|  |].asRarr
            let result = collectString func emptyArray
            Assert.AreEqual(emptyArray,result)

            // null array
            let nullArr = null:int ResizeArray
            throwsNull (fun () -> collectInt func nullArr |> ignore)



    testCase "ResizeArray.Collect () " <| fun _ ->

        CollectTester ResizeArray.collect ResizeArray.collect

    testCase "ResizeArray.CollectWithSideEffects () " <| fun _ ->
        let stamp = ref 0
        let f x =
            stamp.Value <-  stamp.Value + 1
            [| x  |].asRarr

        ResizeArray.collect f [|  |].asRarr |> ignore
        Assert.AreEqual(0, stamp.Value)

        stamp.Value <-  0
        ResizeArray.collect f [|1;2;3 |].asRarr |> ignore
        Assert.AreEqual(3,stamp.Value)

    #if FABLE_COMPILER
    #else
    testCase "ResizeArray.Parallel.Collect" <| fun _ ->
        CollectTester ResizeArray.Parallel.collect ResizeArray.Parallel.collect
    #endif

    testCase "ResizeArray.compareWith() " <| fun _ ->
        // compareWith should work on empty arrays
        Assert.AreEqual(0, ResizeArray.compareWith (fun _ -> failwith "should not be executed")  [| |].asRarr [| |].asRarr)
        Assert.AreEqual(-1, ResizeArray.compareWith (fun _ -> failwith "should not be executed") [| |].asRarr [|1 |].asRarr)
        Assert.AreEqual(1, ResizeArray.compareWith (fun _ -> failwith "should not be executed")  [|"1" |].asRarr [| |].asRarr)

        // compareWith should not work on null arrays
        throwsNull(fun () -> ResizeArray.compareWith (fun _ -> failwith "should not be executed") null [| |].asRarr |> ignore)
        throwsNull(fun () -> ResizeArray.compareWith (fun _ -> failwith "should not be executed") [| |].asRarr null |> ignore)

        // compareWith should work on longer arrays
        Assert.AreEqual(-1, ResizeArray.compareWith compare [|"1";"2" |].asRarr [|"1";"3" |].asRarr)
        Assert.AreEqual(1, ResizeArray.compareWith compare [|1;2;43 |].asRarr [|1;2;1 |].asRarr)
        Assert.AreEqual(1, ResizeArray.compareWith compare [|1;2;3;4 |].asRarr [|1;2;3 |].asRarr)
        Assert.AreEqual(0, ResizeArray.compareWith compare [|1;2;3;4 |].asRarr [|1;2;3;4 |].asRarr)
        Assert.AreEqual(-1, ResizeArray.compareWith compare [|1;2;3 |].asRarr [|1;2;3;4 |].asRarr)
        Assert.AreEqual(1, ResizeArray.compareWith compare [|1;2;3 |].asRarr [|1;2;2;4 |].asRarr)
        Assert.AreEqual(-1, ResizeArray.compareWith compare [|1;2;2 |].asRarr [|1;2;3;4 |].asRarr)

        // compareWith should use the comparer
        Assert.AreEqual(0, ResizeArray.compareWith  (fun _ _ ->  0) [|"1";"2" |].asRarr [|"1";"3" |].asRarr)
        Assert.AreEqual(1, ResizeArray.compareWith  (fun _ _ ->  1) [|"1";"2" |].asRarr [|"1";"3" |].asRarr)
        Assert.AreEqual(-1, ResizeArray.compareWith (fun _ _ -> -1) [|"1";"2" |].asRarr [|"1";"3" |].asRarr)

    testCase "ResizeArray.Concat() " <| fun _ ->
        // integer array
        let seqInt =
            seq { for i in 1..10 do
                    yield [|i; i*10 |].asRarr } |> ResizeArray.ofSeq

        let conIntArr = ResizeArray.concat seqInt
        if ResizeArray.length conIntArr <> 20 then Assert.Fail()

        // string array
        let strSeq =
            seq { for a in 'a'..'c' do
                    for b in 'a'..'c' do
                        yield [|a.ToString();b.ToString()  |].asRarr} |> ResizeArray.ofSeq

        let conStrArr = ResizeArray.concat strSeq|> ResizeArray.ofSeq
        if ResizeArray.length conStrArr <> 18 then Assert.Fail()

        // Empty array
        let emptyArrays = [| [|  |].asRarr; [| 0  |].asRarr; [| 1  |].asRarr; [|  |].asRarr; [|  |].asRarr  |].asRarr
        let result2 = ResizeArray.concat emptyArrays
        Assert.True(result2.[0] = 0 && result2.[1] = 1)
        if result2.[0] <> 0 && result2.[1] <> 1 then Assert.Fail()

        // null array
        let nullArray = null:int ResizeArray
        let nullArrays = ResizeArray.create 2 nullArray
        throwsNull (fun () -> ResizeArray.concat nullArrays |> ignore)



    testCase "ResizeArray.countBy() " <| fun _ ->
        // countBy should work on empty array
        Assert.AreEqual(0, ResizeArray.countBy (fun _ -> failwith "should not be executed") [| |].asRarr |> ResizeArray.length)

        // countBy should not work on null
        throwsNull(fun () -> ResizeArray.countBy (fun _ -> failwith "should not be executed") null |> ignore)

        // countBy should count by the given key function
        Assert.AreEqual([| 5,1; 2,2; 3,2  |].asRarr, ResizeArray.countBy id [|5;2;2;3;3 |].asRarr)
        Assert.AreEqual([| 3,3; 2,2; 1,3  |].asRarr, ResizeArray.countBy (fun x -> if x < 3 then x else 3) [|5;2;1;2;3;3;1;1 |].asRarr)

    testCase "ResizeArray.Copy() " <| fun _ ->
        // int array
        let intSrc:int  ResizeArray = [| 3;5;7  |].asRarr
        let intCopyed = ResizeArray.copy  intSrc
        if intCopyed <!> [| 3;5;7  |].asRarr then Assert.Fail()

        // string array
        let stringSrc: string  ResizeArray = [|"Lists"; "are";  "commonly"   |].asRarr

        let strCopyed = ResizeArray.copy  stringSrc
        if strCopyed <!> [|"Lists"; "are";  "commonly"   |].asRarr then Assert.Fail()

        // empty array
        let emptySrc :int ResizeArray = [|  |].asRarr
        let emptyCopyed = ResizeArray.copy emptySrc
        if emptyCopyed <!> [|  |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:int ResizeArray
        throwsNull (fun () -> ResizeArray.copy nullArr |> ignore)



    testCase "ResizeArray.Create() " <| fun _ ->
        // int array
        let intArr = ResizeArray.create 3 8
        if intArr <!> [| 8; 8; 8 |].asRarr then Assert.Fail()

        // string array
        let strArr = ResizeArray.create 3 "good"
        Assert.True( (strArr == [|"good"; "good";  "good" |].asRarr) )

        // empty array
        let emptyArr = ResizeArray.create 0 "empty"
        if emptyArr <!> [|  |].asRarr then Assert.Fail()

        // array with null elements
        // let nullStr = null:string
        // let nullArr = ResizeArray.create 3 nullStr
        // ResizeArray cant be null:Assert.True( (nullArr = [|null; null; null |].asRarr) )




    testCase "ResizeArray.TryHead() " <| fun _ ->
        // integer array
        let resultInt = ResizeArray.tryHead  [|2..2..20 |].asRarr
        Assert.AreEqual(2, resultInt.Value)

        // string array
        let resultStr = ResizeArray.tryHead  [|"a";"b";"c";"d" |].asRarr
        Assert.True("a" = resultStr.Value)

        // empty array
        let resultNone = ResizeArray.tryHead [| |].asRarr
        Assert.True(resultNone.IsNone)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.tryHead nullArr |> ignore)


    testCase "ResizeArray.Exists() " <| fun _ ->
        // integer array
        let intArr = [| 2;4;6;8  |].asRarr
        let funcInt x = if (x%2 = 0) then true else false
        let resultInt = ResizeArray.exists funcInt intArr
        if resultInt <> true then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly"  |].asRarr
        let funcStr (x:string) = if (x.Length >15) then true else false
        let resultStr = ResizeArray.exists funcStr strArr
        if resultStr <> false then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.exists funcInt emptyArr
        if resultEpt <> false then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.exists funcStr nullArr |> ignore)



    testCase "ResizeArray.Exists2() " <| fun _ ->
        // integer array
        let intFir = [| 2;4;6;8  |].asRarr
        let intSec = [| 1;2;3;4  |].asRarr
        let funcInt x y = if (x%y = 0) then true else false
        let resultInt = ResizeArray.exists2 funcInt intFir intSec
        if resultInt <> true then Assert.Fail()

        // string array
        let strFir = [|"Lists"; "are";  "commonly"  |].asRarr
        let strSec = [|"good"; "good";  "good"   |].asRarr
        let funcStr (x:string) (y:string) = if (x = y) then true else false
        let resultStr = ResizeArray.exists2 funcStr strFir strSec
        if resultStr <> false then Assert.Fail()

        // empty array
        let eptFir:int ResizeArray = [|  |].asRarr
        let eptSec:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.exists2 funcInt eptFir eptSec
        if resultEpt <> false then Assert.Fail()

        // null array
        let nullFir = null:string ResizeArray
        let validArray = [| "a"  |].asRarr
        throwsNull (fun () -> ResizeArray.exists2 funcStr nullFir validArray |> ignore)
        throwsNull (fun () -> ResizeArray.exists2 funcStr validArray nullFir |> ignore)

        // len1 <> len2
        throwsArg(fun () -> ResizeArray.exists2 funcInt [|1..10 |].asRarr [|2..20 |].asRarr |> ignore)



    testCase "ResizeArray.Fill() " <| fun _ ->
        // integer array
        let intArr = [|1..5 |].asRarr
        ResizeArray.fill intArr 0 3 21
        if intArr <!> [|21;21;21;4;5 |].asRarr then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "data";"structor"  |].asRarr
        ResizeArray.fill strArr 1 5 "a"

        if strArr <!> [|"Lists"; "a"; "a"; "a"; "a";"a"  |].asRarr then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        ResizeArray.fill emptyArr 0 0 8
        if emptyArr <!> [|  |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.fill nullArr 0 1 "good" |> ignore)

        // start < 0
        throwsArg(fun () -> ResizeArray.fill intArr -1 3 21)

        // len < 0
        throwsArg(fun () -> ResizeArray.fill intArr 1 -2 21)




    testCase "ResizeArray.Filter() " <| fun _ ->
        // integer array
        let intArr = [| 1..20  |].asRarr
        let funcInt x = if (x%5 = 0) then true else false
        let resultInt = ResizeArray.filter funcInt intArr
        if resultInt <!> [|5;10;15;20 |].asRarr then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "data";"structor"  |].asRarr
        let funcStr (x:string) = if (x.Length > 4) then true else false
        let resultStr = ResizeArray.filter funcStr strArr
        if resultStr <!> [|"Lists";  "commonly"; "structor"  |].asRarr then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.filter funcInt emptyArr
        if resultEpt <!> [|  |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () ->  ResizeArray.filter funcStr nullArr |> ignore)



    testCase "ResizeArray.Filter2 () " <| fun _ ->
        // The ResizeArray.filter algorithm uses a bitmask as a temporary storage mechanism
        // for which elements to filter. This introduces some possible error conditions
        // around how the filter is filled and subsequently used, so filter test
        // does a pretty exhaustive test suite.
        // It works by first generating arrays which consist of sequences of unique
        // positive and negative numbers, as per arguments, it then filters for the
        // positive values, and then compares the results against the original array.

        (*
        let makeTestArray size posLength negLength startWithPos startFromEnd =
            let array = Array.zeroCreate size

            let mutable sign  = if startWithPos then 1         else -1
            let mutable count = if startWithPos then posLength else negLength
            for i = 1 to size do
                let idx = if startFromEnd then size-i else i-1
                array.[idx] <- (idx+1) * sign
                count <- count - 1
                if count <= 0 then
                    sign <- sign * -1
                    count <- if sign > 0 then posLength else negLength

            array

        let checkFilter filter (array:array<_>) =
            let filtered = array |> filter (fun n -> n > 0)

            let mutable idx = 0
            for item in filtered do
                while array.[idx] < item do
                    idx <- idx + 1
                if item <> array.[idx] then
                    Assert.Fail ()
            idx <- idx + 1
            while idx < array.Length do
                if array.[idx] > 0 then
                    Assert.Fail ()
                idx <- idx + 1

        let checkCombinations filter maxSize =
            for size = 0 to maxSize do
                for posLength = 1 to size do
                    for negLength = 1 to size do
                        for startWithPos in [true; false] do
                            for startFromEnd in [true; false] do
                                let testArray = makeTestArray size posLength negLength startWithPos startFromEnd
                                checkFilter filter testArray

        // this could probably be a bit smaller, but needs to at least be > 64 to test chunk copying
        // of data, and > 96 gives a safer feel, so settle on a nice decimal rounding of one hundred
        // to appease those with digits.
        let suitableTestMaxLength = 100

        checkCombinations ResizeArray.filter suitableTestMaxLength
        *)
        ()


    testCase "ResizeArray.Where() " <| fun _ ->
        // integer array
        let intArr = [| 1..20  |].asRarr
        let funcInt x = if (x%5 = 0) then true else false
        let resultInt = ResizeArray.where funcInt intArr
        if resultInt <!> [|5;10;15;20 |].asRarr then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "data";"structor"  |].asRarr
        let funcStr (x:string) = if (x.Length > 4) then true else false
        let resultStr = ResizeArray.where funcStr strArr
        if resultStr <!> [|"Lists";  "commonly"; "structor"  |].asRarr then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.where funcInt emptyArr
        if resultEpt <!> [|  |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () ->  ResizeArray.where funcStr nullArr |> ignore)



    testCase "ResizeArray.where should work like filter" <| fun _ ->
        Assert.AreEqual([| |].asRarr, ResizeArray.where (fun x -> x % 2 = 0) [| |].asRarr)
        Assert.AreEqual([|0;2;4;6;8 |].asRarr, ResizeArray.where (fun x -> x % 2 = 0) [|0..9 |].asRarr)
        Assert.AreEqual([|"a";"b";"c" |].asRarr, ResizeArray.where (fun _ -> true) [|"a";"b";"c" |].asRarr)

    testCase "ResizeArray.Find() " <| fun _ ->
        // integer array
        let intArr = [| 1..20  |].asRarr
        let funcInt x = if (x%5 = 0) then true else false
        let resultInt = ResizeArray.find funcInt intArr
        if resultInt <> 5 then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "data";"structor"  |].asRarr
        let funcStr (x:string) = if (x.Length >7) then true else false
        let resultStr = ResizeArray.find funcStr strArr
        if resultStr <> "commonly" then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        throwsKey (fun () -> ResizeArray.find (fun _ -> true) emptyArr |> ignore)

        // not found
        throwsKey (fun () -> ResizeArray.find (fun _ -> false) intArr |> ignore)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.find funcStr nullArr |> ignore)



    testCase "ResizeArray.FindBack() " <| fun _ ->
        // integer array
        let funcInt x = if (x%5 = 0) then true else false
        Assert.AreEqual(20, ResizeArray.findBack funcInt [| 1..20  |].asRarr)
        Assert.AreEqual(15, ResizeArray.findBack funcInt [| 1..19  |].asRarr)
        Assert.AreEqual(5, ResizeArray.findBack funcInt [| 5..9  |].asRarr)

        // string array
        let strArr = [| "Lists"; "are"; "a"; "commonly"; "data"; "12345678" ; "1234567" |].asRarr
        let funcStr (x:string) = x.Length > 7
        let resultStr = ResizeArray.findBack funcStr strArr
        Assert.AreEqual("12345678", resultStr)

        // empty array
        throwsKey (fun () -> ResizeArray.findBack (fun _ -> true) [|  |].asRarr |> ignore)

        // not found
        throwsKey (fun () -> ResizeArray.findBack (fun _ -> false) [| 1..20  |].asRarr |> ignore)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.findBack funcStr nullArr |> ignore)



    testCase "ResizeArray.FindIndex() " <| fun _ ->
        // integer array
        let intArr = [| 1..20  |].asRarr
        let funcInt x = if (x%5 = 0) then true else false
        let resultInt = ResizeArray.findIndex funcInt intArr
        if resultInt <> 4 then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "data";"structor"  |].asRarr
        let funcStr (x:string) = if (x.Length >7) then true else false
        let resultStr = ResizeArray.findIndex funcStr strArr
        if resultStr <> 3 then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        throwsKey(fun() -> ResizeArray.findIndex (fun _ -> true) emptyArr |> ignore)

        // not found
        throwsKey(fun() -> ResizeArray.findIndex (fun _ -> false) intArr |> ignore)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.findIndex funcStr nullArr |> ignore)



    testCase "ResizeArray.FindIndexBack() " <| fun _ ->
        // integer array
        let funcInt x = if (x%5 = 0) then true else false
        Assert.AreEqual(19, ResizeArray.findIndexBack funcInt [| 1..20  |].asRarr)
        Assert.AreEqual(14, ResizeArray.findIndexBack funcInt [| 1..19  |].asRarr)
        Assert.AreEqual(0, ResizeArray.findIndexBack funcInt [| 5..9  |].asRarr)

        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "data";"structor"  |].asRarr
        let funcStr (x:string) = if (x.Length >7) then true else false
        let resultStr = ResizeArray.findIndexBack funcStr strArr
        Assert.AreEqual(5, resultStr)

        // empty array
        throwsKey(fun() -> ResizeArray.findIndexBack (fun _ -> true) [|  |].asRarr |> ignore)

        // not found
        throwsKey(fun() -> ResizeArray.findIndexBack (fun _ -> false) [| 1..20  |].asRarr |> ignore)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.findIndexBack funcStr nullArr |> ignore)



    testCase "ResizeArray.Pick() " <| fun _ ->
        // integers
        let intArr = [| 1..10  |].asRarr
        let matchFunc n =
            if n = 3 then Some(n.ToString())
            else None
        let resultInt = ResizeArray.pick matchFunc intArr
        Assert.AreEqual("3", resultInt)

        // make it not found
        throwsKey (fun () -> ResizeArray.pick (fun _ -> None) intArr |> ignore)

    testCase "ResizeArray.last() " <| fun _ ->
        // last should fail on empty array
        throwsArg(fun () -> ResizeArray.last [| |].asRarr |> ignore)

        // last should fail on null
        throwsNull(fun () -> ResizeArray.last null |> ignore)

        // last should return the last element from arrays
        Assert.AreEqual(1, ResizeArray.last [|1 |].asRarr)
        Assert.AreEqual("2", ResizeArray.last [|"1"; "3"; "2" |].asRarr)
        Assert.AreEqual([|"4"|].asRarr, ResizeArray.last [|  [||].asRarr; [|"4"|].asRarr  |].asRarr)

    testCase "ResizeArray.TryLast() " <| fun _ ->
        // integers array
        let IntSeq = [| 1..9  |].asRarr
        let intResult = ResizeArray.tryLast IntSeq
        Assert.AreEqual(9, intResult.Value)

        // string array
        let strResult = ResizeArray.tryLast [|"first"; "second";  "third" |].asRarr
        Assert.AreEqual("third", strResult.Value)

        // Empty array
        let emptyResult = ResizeArray.tryLast (ResizeArray<int>()) // <int>
        Assert.True(emptyResult.IsNone)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () ->ResizeArray.tryLast nullArr |> ignore)


    testCase "ResizeArray.ToSeq() " <| fun _ ->
        let intArr = [| 1..10  |].asRarr
        let seq = ResizeArray.toSeq intArr
        let sum = Seq.sum seq
        Assert.AreEqual(55, sum)

    testCase "ResizeArray.TryPick() " <| fun _ ->
        // integer array
        let intArr = [| 1..10  |].asRarr
        let funcInt x =
                match x with
                | _ when x % 3 = 0 -> Some (x.ToString())
                | _ -> None
        let resultInt = ResizeArray.tryPick funcInt intArr
        if resultInt <> Some "3" then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        let funcStr x =
                match x with
                | "good" -> Some (x.ToString())
                | _ -> None
        let resultStr = ResizeArray.tryPick funcStr strArr
        if resultStr <> None then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.tryPick funcInt emptyArr
        if resultEpt <> None then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.tryPick funcStr nullArr |> ignore)



    testCase "ResizeArray.Pick()2 " <| fun _ ->
        // integer array
        let intArr = [| 1..10  |].asRarr
        let funcInt x =
                match x with
                | _ when x % 3 = 0 -> Some (x.ToString())
                | _ -> None
        let resultInt = ResizeArray.pick funcInt intArr
        if resultInt <> "3" then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        let funcStr x =
                match x with
                | "good" -> Some (x.ToString())
                | _ -> None
        throwsKey (fun () -> ResizeArray.pick funcStr strArr |> ignore)

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        throwsKey (fun () -> ResizeArray.pick funcInt emptyArr |> ignore)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.pick funcStr nullArr |> ignore)




    testCase "ResizeArray.TryPickBack() " <| fun _ ->
        // integer array
        let intArr = [| 1..10  |].asRarr
        let funcInt x =
                match x with
                | _ when x % 3 = 0 -> Some (x.ToString())
                | _ -> None
        let resultInt = ResizeArray.tryPickBack funcInt intArr
        if resultInt <> Some "9" then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        let funcStr x =
                match x with
                | "good" -> Some (x.ToString())
                | _ -> None
        let resultStr = ResizeArray.tryPickBack funcStr strArr
        if resultStr <> None then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.tryPickBack funcInt emptyArr
        if resultEpt <> None then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.tryPickBack funcStr nullArr |> ignore)





    testCase "ResizeArray.Fold() " <| fun _ ->
        // integer array
        let intArr = [| 1..5  |].asRarr
        let funcInt x y = x+"+"+y.ToString()
        let resultInt = ResizeArray.fold funcInt "x" intArr
        if resultInt <> "x+1+2+3+4+5" then Assert.Fail()

        // string array
        let strArr = [|"A"; "B";  "C" ; "D"  |].asRarr
        let funcStr x y = x+y

        let resultStr = ResizeArray.fold funcStr "X" strArr
        if resultStr <> "XABCD" then Assert.Fail()

        // empty array
        let emptyArr : int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.fold funcInt "x" emptyArr
        if resultEpt <> "x" then Assert.Fail()

        // null array
        let nullArr = null : string ResizeArray
        throwsNull (fun () -> ResizeArray.fold funcStr "begin" nullArr |> ignore)



    testCase "ResizeArray.Fold2() " <| fun _ ->
        // integer array
        let funcInt x y z = x + y.ToString() + z.ToString()
        let resultInt = ResizeArray.fold2 funcInt "x" [| 1;3;5  |].asRarr  [|2;4;6 |].asRarr
        if resultInt <> "x123456" then Assert.Fail()

        // string array
        let funcStr x y z= x + y + z
        let resultStr = ResizeArray.fold2 funcStr "X" [|"A"; "B";  "C" ; "D"  |].asRarr [|"H"; "I";  "J" ; "K"  |].asRarr
        if resultStr <> "XAHBICJDK" then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.fold2 funcInt "x" emptyArr emptyArr
        if resultEpt <> "x" then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        let validArray = [| "a"  |].asRarr
        throwsNull (fun () -> ResizeArray.fold2 funcStr "begin" validArray nullArr |> ignore)
        throwsNull (fun () -> ResizeArray.fold2 funcStr "begin" nullArr validArray |> ignore)

        // len1 <> len2
        throwsArg(fun () -> ResizeArray.fold2 funcInt "x" [| 1;3;5  |].asRarr  [|2;4;6;8 |].asRarr |> ignore)



    testCase "ResizeArray.FoldBack() " <| fun _ ->
        // integer array
        let intArr = [| 1..5  |].asRarr
        let funcInt x y = x.ToString()+y
        let resultInt = ResizeArray.foldBack funcInt intArr "x"
        if resultInt <> "12345x" then Assert.Fail()

        // string array
        let strArr = [|"A"; "B";  "C" ; "D"  |].asRarr
        let funcStr x y = x+y

        let resultStr = ResizeArray.foldBack funcStr strArr "X"
        if resultStr <> "ABCDX" then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.foldBack funcInt emptyArr "x"
        if resultEpt <> "x" then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.foldBack funcStr nullArr "begin" |> ignore)



    testCase "ResizeArray.FoldBack2() " <| fun _ ->
        // integer array
        let funcInt x y z = x.ToString() + y.ToString() + z
        let resultInt = ResizeArray.foldBack2 funcInt  [| 1;3;5  |].asRarr  [|2;4;6 |].asRarr "x"
        if resultInt <> "123456x" then Assert.Fail()

        // string array
        let funcStr x y z= x + y + z
        let resultStr = ResizeArray.foldBack2 funcStr [|"A"; "B";  "C" ; "D"  |].asRarr [|"H"; "I";  "J" ; "K"  |].asRarr "X"
        if resultStr <> "AHBICJDKX" then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.foldBack2 funcInt emptyArr emptyArr "x"
        if resultEpt <> "x" then Assert.Fail()

        // null array
        let nullArr = null : string ResizeArray
        let validArray = [| "a"  |].asRarr
        throwsNull (fun () -> ResizeArray.foldBack2 funcStr nullArr validArray "begin" |> ignore)
        throwsNull (fun () -> ResizeArray.foldBack2 funcStr validArray nullArr "begin" |> ignore)

        // len1 <> len2
        throwsArg(fun () -> ResizeArray.foldBack2 funcInt [|1..10 |].asRarr [|2..20 |].asRarr "x" |> ignore)



    testCase "ResizeArray.ForAll() " <| fun _ ->
        // integer array
        let resultInt = ResizeArray.forall (fun x -> x > 2) [| 3..2..10  |].asRarr
        if resultInt <> true then Assert.Fail()

        // string array
        let resultStr = ResizeArray.forall (fun (x:string) -> x.Contains("a")) [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        if resultStr <> false then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.forall (fun (x:string) -> x.Contains("a")) [| |].asRarr
        if resultEpt <> true then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.forall (fun _ -> true) nullArr |> ignore)



    testCase "ResizeArray.ForAll2() " <| fun _ ->
        // integer array
        let resultInt = ResizeArray.forall2 (fun x y -> x < y) [| 1..10  |].asRarr [|2..2..20 |].asRarr
        if resultInt <> true then Assert.Fail()

        // string array
        let resultStr = ResizeArray.forall2 (fun (x:string) (y:string) -> x.Length < y.Length) [|"Lists"; "are";  "commonly" ; "list"  |].asRarr [|"Listslong"; "arelong";  "commonlylong" ; "listlong"  |].asRarr
        if resultStr <> true then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.forall2 (fun x y -> x>y) [| |].asRarr [| |].asRarr
        if resultEpt <> true then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        let validArray = [| "a"  |].asRarr
        throwsNull (fun () -> ResizeArray.forall2 (fun _ _ -> true) nullArr validArray |> ignore)
        throwsNull (fun () -> ResizeArray.forall2 (fun _ _ -> true) validArray nullArr |> ignore)

        // len1 <> len2
        throwsArg(fun () -> ResizeArray.forall2 (fun x y -> x < y) [|1..10 |].asRarr [|2..20 |].asRarr |> ignore)



    testCase "ResizeArray.Get() " <| fun _ ->
        // integer array
        let intArr = [| 3;4;7;8;10  |].asRarr
        let resultInt = ResizeArray.get 3 intArr
        if resultInt <> 8 then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr

        let resultStr = ResizeArray.get 2 strArr
        if resultStr <> "commonly" then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        throwsIdx (fun () -> ResizeArray.get -1 emptyArr |> ignore)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.get 0 nullArr |> ignore)



    testCase "ResizeArray.exactlyOne should return the element from singleton arrays" <| fun _ ->
        Assert.AreEqual(1, ResizeArray.exactlyOne [|1 |].asRarr)
        Assert.AreEqual("2", ResizeArray.exactlyOne [|"2" |].asRarr)


    testCase "ResizeArray.exactlyOne should fail on empty array" <| fun _ ->
        throwsArg(fun () -> ResizeArray.exactlyOne [| |].asRarr |> ignore)

    testCase "ResizeArray.exactlyOne should fail on null array" <| fun _ ->
        throwsNull(fun () -> ResizeArray.exactlyOne null |> ignore)


    testCase "ResizeArray.exactlyOne should fail on arrays with more than one element" <| fun _ ->
        throwsArg(fun () -> ResizeArray.exactlyOne [|"1"; "2" |].asRarr |> ignore)

    testCase "ResizeArray.tryExactlyOne should return the element from singleton arrays" <| fun _ ->
        Assert.AreEqual(Some 1, ResizeArray.tryExactlyOne [|1 |].asRarr)
        Assert.AreEqual(Some "2", ResizeArray.tryExactlyOne [|"2" |].asRarr)


    testCase "ResizeArray.tryExactlyOne should return None on empty array" <| fun _ ->
        Assert.AreEqual(None, ResizeArray.tryExactlyOne [| |].asRarr)

    testCase "ResizeArray.tryExactlyOne should return None for arrays with more than one element" <| fun _ ->
        Assert.AreEqual(None, ResizeArray.tryExactlyOne [|"1"; "2" |].asRarr)

    testCase "ResizeArray.tryExactlyOne should fail on null array" <| fun _ ->
        throwsNull(fun () -> ResizeArray.tryExactlyOne null |> ignore)


    testCase "ResizeArray.GroupBy() " <| fun _ ->
        let funcInt x = x%5

        let IntArray = [| 0 .. 9  |].asRarr

        let group_byInt = ResizeArray.groupBy funcInt IntArray

        let expectedIntArray =
            [| for i in 0..4 -> i, [|i; i+5 |].asRarr  |].asRarr

        if group_byInt <*> expectedIntArray then Assert.Fail()

        // string array
        let funcStr (x:string) = x.Length
        let strArray = [|"l1ngth7"; "length 8";  "l2ngth7" ; "length  9" |].asRarr

        let group_byStr = ResizeArray.groupBy funcStr strArray
        let expectedStrArray =
            [|
                7, [|"l1ngth7"; "l2ngth7" |].asRarr
                8, [|"length 8" |].asRarr
                9, [|"length  9" |].asRarr
             |].asRarr

        if group_byStr <*> expectedStrArray then Assert.Fail()

        // Empty array
        let emptyArray = [| |].asRarr
        // let group_byEmpty = ResizeArray.groupBy funcInt emptyArray
        let expectedEmptyArray = [| |].asRarr

        if emptyArray <!> expectedEmptyArray then Assert.Fail()

        throwsNull(fun () -> ResizeArray.groupBy funcInt (null : int ResizeArray) |> ignore)


    //member private this.
    let InitTester initInt initString =
            // integer array
            let resultInt : int ResizeArray = initInt 3 (fun x -> x + 3)
            if resultInt <!> [|3;4;5 |].asRarr then Assert.Fail()

            // string array
            let funStr (x:int) =
                match x with
                | 0 -> "Lists"
                | 1 -> "are"
                | 2 -> "commonly"
                | _ -> "end"
            let resultStr = initString 3 funStr
            if resultStr <!> [|"Lists"; "are";  "commonly"   |].asRarr then Assert.Fail()

            // empty array
            let resultEpt = initInt 0 (fun x -> x+1)
            if resultEpt <!> [|  |].asRarr then Assert.Fail()



    testCase "ResizeArray.Hd() " <| fun _ ->
        // integer array
        let resultInt = ResizeArray.head [|2..2..20 |].asRarr
        Assert.AreEqual(2, resultInt)

        // string array
        let resultStr = ResizeArray.head [|"a";"b";"c";"d" |].asRarr
        Assert.AreEqual("a", resultStr)

        throwsArg(fun () -> ResizeArray.head [| |].asRarr |> ignore)
        throwsNull(fun () -> ResizeArray.head null |> ignore)


    testCase "ResizeArray.Init() " <| fun _ ->
        InitTester ResizeArray.init ResizeArray.init

    testCase "ResizeArray.InitWithSideEffects () " <| fun _ ->
        let stamp = ref 0
        let f i =
            stamp.Value <-  stamp.Value + 1;
            i
        ResizeArray.init 0 f |> ignore
        Assert.AreEqual (0, stamp.Value)

        stamp.Value <-  0
        ResizeArray.init 10 f |> ignore
        Assert.AreEqual (10, stamp.Value)

    #if FABLE_COMPILER
    #else
    testCase "ResizeArray.Parallel.Init" <| fun _ ->
        InitTester ResizeArray.Parallel.init ResizeArray.Parallel.init
    #endif

    testCase "ResizeArray.IsEmpty() " <| fun _ ->
        // integer array
        let intArr = [| 3;4;7;8;10  |].asRarr
        let resultInt = ResizeArray.isEmpty intArr
        if resultInt <> false then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        let resultStr = ResizeArray.isEmpty strArr
        if resultStr <> false then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.isEmpty emptyArr
        if resultEpt <> true then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.isEmpty nullArr |> ignore)



    testCase "ResizeArray.Iter() " <| fun _ ->
        // integer array
        let intArr = [| 1..10  |].asRarr
        let resultInt = ref 0
        let funInt (x:int) =
            resultInt.Value <-  resultInt.Value + x

        ResizeArray.iter funInt intArr
        if resultInt.Value <> 55 then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        let resultStr = ref ""
        let funStr (x : string) =
            resultStr.Value <-  (resultStr.Value) + x

        ResizeArray.iter funStr strArr
        if resultStr.Value <> "Listsarecommonlylist" then Assert.Fail()

        // empty array
        let emptyArr : int ResizeArray = [|  |].asRarr
        let resultEpt = ref 0
        ResizeArray.iter funInt emptyArr
        if resultEpt.Value <> 0 then Assert.Fail()

        // null array
        let nullArr = null : string ResizeArray
        throwsNull (fun () -> ResizeArray.iter funStr nullArr |> ignore)



    testCase "ResizeArray.Iter2() " <| fun _ ->
        // integer array
        let resultInt = ref 0
        let funInt (x:int) (y:int) =
            resultInt.Value <-  resultInt.Value + x + y

        ResizeArray.iter2 funInt [| 1..10  |].asRarr [|2..2..20 |].asRarr
        if resultInt.Value <> 165 then Assert.Fail()

        // string array
        let resultStr = ref ""
        let funStr (x:string) (y:string) =
            resultStr.Value <-  (resultStr.Value) + x  + y

        ResizeArray.iter2 funStr [|"A"; "B";  "C" ; "D"  |].asRarr [|"a"; "b"; "c"; "d" |].asRarr
        if resultStr.Value <> "AaBbCcDd" then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ref 0
        ResizeArray.iter2 funInt emptyArr emptyArr
        if resultEpt.Value <> 0 then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        let validArray = [| "a"  |].asRarr
        throwsNull (fun () -> ResizeArray.iter2 funStr nullArr validArray |> ignore)
        throwsNull (fun () -> ResizeArray.iter2 funStr validArray nullArr |> ignore)

        // len1 <> len2
        throwsArg(fun () -> ResizeArray.iter2 funInt [| 1..10  |].asRarr [|2..20 |].asRarr)




    testCase "ResizeArray.Iteri() " <| fun _ ->
        // integer array
        let intArr = [| 1..10  |].asRarr
        let resultInt = ref 0
        let funInt (x:int) y =
            resultInt.Value <-  resultInt.Value + x + y

        ResizeArray.iteri funInt intArr
        if resultInt.Value <> 100 then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        let resultStr = ref 0
        let funStr (x:int) (y:string) =
            resultStr.Value <-  (resultStr.Value) + x + y.Length

        ResizeArray.iteri funStr strArr
        if resultStr.Value <> 26 then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ref 0
        ResizeArray.iteri funInt emptyArr
        if resultEpt.Value <> 0 then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.iteri funStr nullArr |> ignore)



    testCase "ResizeArray.Iteri2() " <| fun _ ->
        // integer array
        let resultInt = ref 0
        let funInt (x:int) (y:int) (z:int) =
            resultInt.Value <-  resultInt.Value + x + y + z

        ResizeArray.iteri2 funInt [| 1..10  |].asRarr [|2..2..20 |].asRarr
        if resultInt.Value <> 210 then Assert.Fail()

        // string array
        let resultStr = ref ""
        let funStr (x:int) (y:string) (z:string) =
            resultStr.Value <-  (resultStr.Value) + x.ToString()  + y + z

        ResizeArray.iteri2 funStr [|"A"; "B";  "C" ; "D"  |].asRarr [|"a"; "b"; "c"; "d" |].asRarr
        if resultStr.Value <> "0Aa1Bb2Cc3Dd" then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ref 0
        ResizeArray.iteri2 funInt emptyArr emptyArr
        if resultEpt.Value <> 0 then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        let validArray = [| "a"  |].asRarr
        throwsNull (fun () -> ResizeArray.iteri2 funStr nullArr validArray |> ignore)
        throwsNull (fun () -> ResizeArray.iteri2 funStr validArray nullArr |> ignore)

        // len1 <> len2
        throwsArg(fun () -> ResizeArray.iteri2 funInt [| 1..10  |].asRarr [|2..20 |].asRarr  |> ignore)



    testCase "ResizeArray.pairwise should return pairs of the input array" <| fun _ ->
        Assert.AreEqual([| |].asRarr, ResizeArray.pairwise [| |].asRarr)
        Assert.AreEqual([| |].asRarr, ResizeArray.pairwise [|1 |].asRarr)
        Assert.AreEqual([|1,2 |].asRarr, ResizeArray.pairwise [|1;2 |].asRarr)
        Assert.AreEqual([|1,2; 2,3 |].asRarr, ResizeArray.pairwise [|1;2;3 |].asRarr)
        Assert.AreEqual([|"H","E"; "E","L"; "L","L"; "L","O" |].asRarr, ResizeArray.pairwise [|"H";"E";"L";"L";"O" |].asRarr)

    testCase "ResizeArray.pairwise should not work on null" <| fun _ ->
        throwsNull(fun () -> ResizeArray.pairwise null |> ignore)



    testCase "ResizeArray.mapToArray" <| fun _ ->
        let arr: string[] = ResizeArray.mapToArray (fun x -> string x) [| 1 ; 2 |].asRarr
        Assert.True([| "1";"2" |] = arr)


    //member private this.
    let MapTester mapInt (mapString : (string -> int) -> ResizeArray<string> -> ResizeArray<int>) =
            // empty array
            let f x = x + 1
            let result = mapInt f [|  |].asRarr
            if result <!> [|  |].asRarr then Assert.Fail ()

            // int array
            let result = mapInt f [| 1..100  |].asRarr
            if result <!> [| 2..101  |].asRarr then Assert.Fail ()

            // string array
            let result = [| "a"; "aa"; "aaa"  |].asRarr |> mapString (fun s -> s.Length)
            if result <!> [| 1..3  |].asRarr then Assert.Fail ()

            // null array
            let nullArg : int  ResizeArray = null
            throwsNull (fun () -> mapInt f nullArg |> ignore)



    testCase "ResizeArray.Map () " <| fun _ ->
        MapTester ResizeArray.map ResizeArray.map

    testCase "ResizeArray.MapWithSideEffects () " <| fun _ ->
        let stamp = ref 0
        let f x = stamp.Value <-  stamp.Value + 1; x + 1

        ResizeArray.map f [|  |].asRarr |> ignore
        Assert.AreEqual(0,stamp.Value)

        stamp.Value <-  0
        ResizeArray.map f [| 1..100  |].asRarr |> ignore
        Assert.AreEqual(100,stamp.Value)

    #if FABLE_COMPILER
    #else
    testCase "ResizeArray.Parallel.Map" <| fun _ ->
        MapTester ResizeArray.Parallel.map ResizeArray.Parallel.map
    #endif

    // member private this.
    let MapiTester mapiInt mapiString =
            // empty array
            let f i x = (i, x + 1)
            let result = mapiInt f [|  |].asRarr
            if result <!> [|  |].asRarr then Assert.Fail ()

            // int array
            let result : ResizeArray<int*int> = mapiInt f [| 1..2  |].asRarr
            if result <!> [| (0,2); (1,3)  |].asRarr then Assert.Fail ()

            // string array
            let result : ResizeArray<int*int> = [| "a"; "aa"; "aaa"  |].asRarr |> mapiString (fun i (s:string) -> i, s.Length)
            if result <!> [| (0,1); (1,2); (2,3)  |].asRarr then Assert.Fail ()

            // null array
            let nullArg : int  ResizeArray = null
            throwsNull (fun () -> mapiInt f nullArg |> ignore)


    testCase "ResizeArray.Mapi () " <| fun _ ->
        MapiTester ResizeArray.mapi ResizeArray.mapi


    testCase "ResizeArray.MapiWithSideEffects () " <| fun _ ->
        let stamp = ref 0
        let f i x = stamp.Value <-  stamp.Value + 1; (i, x + 1)

        ResizeArray.mapi f [|  |].asRarr |> ignore
        Assert.AreEqual(0,stamp.Value)

        stamp.Value <-  0
        ResizeArray.mapi f [| 1..100  |].asRarr |> ignore
        Assert.AreEqual(100,stamp.Value)



    #if FABLE_COMPILER
    #else
    testCase "ResizeArray.Parallel.Mapi" <| fun _ ->
        MapiTester ResizeArray.Parallel.mapi ResizeArray.Parallel.mapi


    testCase "ResizeArray.Parallel.Iter" <| fun _ ->
        // integer array
        let intArr = [| 1..10  |].asRarr
        let resultInt = ref 0
        let funInt (x:int) =
            lock resultInt (fun () -> resultInt.Value <-  resultInt.Value + x)

        ResizeArray.Parallel.iter funInt intArr
        if resultInt.Value <> 55 then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        let resultStr = ref 0
        let funStr (x : string) =
            lock resultStr (fun () -> resultStr.Value <-  (resultStr.Value) + x.Length)

        ResizeArray.Parallel.iter funStr strArr
        if resultStr.Value <> 20 then Assert.Fail()

        // empty array
        let emptyArr : int ResizeArray = [|  |].asRarr
        let resultEpt = ref 0
        ResizeArray.Parallel.iter funInt emptyArr
        if resultEpt.Value <> 0 then Assert.Fail()

        // null array
        let nullArr = null : string ResizeArray
        throwsNull (fun () -> ResizeArray.Parallel.iter funStr nullArr |> ignore)



    testCase "ResizeArray.Parallel.Iteri" <| fun _ ->
        // integer array
        let intArr = [| 1..10  |].asRarr

        let resultInt = ref 0
        let funInt (x:int) y =
            lock resultInt (fun () -> resultInt.Value <-  resultInt.Value + x + y)

        ResizeArray.Parallel.iteri funInt intArr
        if resultInt.Value <> 100 then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        let resultStr = ref 0
        let funStr (x:int) (y:string) =
            lock resultStr (fun () -> resultStr.Value <-  (resultStr.Value) + x + y.Length)

        ResizeArray.Parallel.iteri funStr strArr
        if resultStr.Value <> 26 then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ref 0
        ResizeArray.Parallel.iteri funInt emptyArr
        if resultEpt.Value <> 0 then Assert.Fail()

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.Parallel.iteri funStr nullArr |> ignore)


    #endif


    //member private this.
    let PartitionTester partInt partString =
            // int array
            let intSrc:int  ResizeArray = [| 1..100  |].asRarr
            let funcInt x = if (x%2=1) then true else false
            let intPartitioned : int ResizeArray * int ResizeArray = partInt funcInt intSrc
            if ([|1..2..100 |].asRarr,[|2..2..100 |].asRarr) <!!> intPartitioned then Assert.Fail ()

            let allLeft = partInt (fun _ -> true) intSrc
            if (intSrc, [| |].asRarr) <!!> allLeft then Assert.Fail()
            let allRight = partInt (fun _ -> false) intSrc
            if ([| |].asRarr, intSrc) <!!> allRight then Assert.Fail()


            // string array
            let stringSrc: string  ResizeArray = "List 1 list 2 3 4 5".Split([|' ' |], System.StringSplitOptions.RemoveEmptyEntries).asRarr
            let funcString x =  match x with
                                | "list"-> true
                                | "List" -> true
                                | _ -> false
            let strPartitioned : string ResizeArray * string ResizeArray  = partString funcString stringSrc
            if strPartitioned <!!> ([|"List";"list" |].asRarr, [| "1";"2"; "3"; "4"; "5" |].asRarr) then Assert.Fail ()

            // empty array
            let emptySrc :int ResizeArray = [|  |].asRarr
            let emptyPartitioned = partInt funcInt emptySrc
            if emptyPartitioned <!!> ([|  |].asRarr, [|  |].asRarr) then Assert.Fail()

            // null array
            let nullArr = null:string ResizeArray
            throwsNull (fun () -> partString funcString nullArr |> ignore)


    testCase "ResizeArray.Partition () " <| fun _ ->
        PartitionTester ResizeArray.partition ResizeArray.partition

    testCase "ResizeArray.Singleton() " <| fun _ ->
        Assert.AreEqual([|null |].asRarr, ResizeArray.singleton null)
        Assert.AreEqual([|"1" |].asRarr, ResizeArray.singleton "1")
        Assert.AreEqual([| []  |].asRarr, ResizeArray.singleton [])
        Assert.True([| [|  |].asRarr  |].asRarr =+= ResizeArray.singleton [|  |].asRarr)

    #if FABLE_COMPILER
    #else
    testCase "ResizeArray.Parallel.Partition" <| fun _ ->
        PartitionTester ResizeArray.Parallel.partition ResizeArray.Parallel.partition
    #endif

    testCase "ResizeArray.Contains() " <| fun _ ->
        // integer array
        let intArr = [| 2;4;6;8  |].asRarr
        let resultInt = ResizeArray.contains 6 intArr
        Assert.True(resultInt)

        // string array
        let strArr = [|"Lists"; "are"; "commonly" |].asRarr
        let resultStr = ResizeArray.contains "not" strArr
        Assert.False(resultStr)

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.contains 4 emptyArr
        Assert.False(resultEpt)

        // null array
        let nullArr = null:string ResizeArray
        throwsNull (fun () -> ResizeArray.contains "empty" nullArr |> ignore)

    testCase "ResizeArray.Slicing with first index reverse behaves as expected" <| fun _ ->
        let arr = [| 1;2;3;4;5  |].asRarr

        Assert.AreEqual(arr.[^3..], arr.[1..])


    testCase "ResizeArray.Slicing with second index reverse behaves as expected" <| fun _ ->
        let arr = [| 1;2;3;4;5  |].asRarr

        Assert.AreEqual(arr.[..^1], arr.[..3])


    testCase "ResizeArray.Slicing with both index reverse behaves as expected" <| fun _ ->
        let arr = [| 1;2;3;4;5  |].asRarr

        Assert.AreEqual(arr.[^3..^1], arr.[1..3])

    testCase "ResizeArray.Slicing with first index reverse and second index non reverse behaves as expected" <| fun _ ->
        let arr = [|1;2;3;4;5 |].asRarr

        Assert.AreEqual(arr.[^3..4], arr.[1..4])

    testCase "ResizeArray.Slicing with first index non reverse and second index reverse behaves as expected" <| fun _ ->
        let arr = [|1;2;3;4;5 |].asRarr

        Assert.AreEqual(arr.[3..^0], arr.[3..4])

    testCase "ResizeArray.Set slice with first index reverse behaves as expected" <| fun _ ->
        let arr1 = [| 1;2;3;4;5  |].asRarr
        let arr2 = [| 1;2;3;4;5  |].asRarr

        arr1.[^3..] <- [| 9;8;7;6  |].asRarr
        arr2.[1..] <- [| 9;8;7;6  |].asRarr

        Assert.AreEqual(arr1, arr2)

    testCase "ResizeArray.Set slice with second index reverse behaves as expected" <| fun _ ->
        let arr1 = [| 1;2;3;4;5  |].asRarr
        let arr2 = [| 1;2;3;4;5  |].asRarr

        arr1.[..^1] <- [| 9;8;7;6  |].asRarr
        arr2.[..3] <- [| 9;8;7;6  |].asRarr

        Assert.AreEqual(arr1, arr2)

    testCase "ResizeArray.Set slice with both index reverse behaves as expected" <| fun _ ->
        let arr1 = [| 1;2;3;4;5  |].asRarr
        let arr2 = [| 1;2;3;4;5  |].asRarr

        arr1.[^3..^1] <- [| 8;7;6  |].asRarr
        arr2.[1..3] <- [| 8;7;6  |].asRarr

        Assert.AreEqual(arr1, arr2)

    testCase "ResizeArray.Get item with reverse index behaves as expected" <| fun _ ->
        let arr = [|1;2;3;4;5 |].asRarr
        Assert.AreEqual(arr.[^1], 4)

    testCase "ResizeArray.Set item with reverse index behaves as expected" <| fun _ ->
        let arr = [|1;2;3;4;5 |].asRarr

        arr.[^0] <- 9
        Assert.AreEqual(arr.[4], 9)

    testCase "ResizeArray.SlicingUnboundedEnd() " <| fun _ ->
        let arr = [|1;2;3;4;5;6 |].asRarr

        // throwsIdx   (fun () -> arr.[-1..]|> ignore )
        Assert.AreEqual(arr.[-1..], arr)
        Assert.AreEqual(arr.[-99..], arr)
        Assert.AreEqual(arr.[-99..99], arr)
        Assert.AreEqual(arr.[..99], arr)
        Assert.AreEqual(arr.[0..], arr)
        Assert.AreEqual(arr.[1..], [|2;3;4;5;6|].asRarr)
        Assert.AreEqual(arr.[2..], [|3;4;5;6|].asRarr)
        Assert.AreEqual(arr.[5..], [|6|].asRarr)


        //Assert.AreEqual(arr.[6..], ([| |].asRarr: int ResizeArray))
        //Assert.AreEqual(arr.[7..], ([| |].asRarr: int ResizeArray))
        // throwsIdx   (fun () -> arr.[..7]  |> ignore )

    testCase "ResizeArray.SlicingUnboundedStart()  -1" <| fun _ ->
        let arr = [|1; 2 |].asRarr
        Assert.AreEqual(arr.[ .. -1], [||].asRarr)
        // throwsIdx   (fun () -> arr.[..(-1)]|> ignore )

    testCase "ResizeArray.SlicingUnboundedStart() " <| fun _ ->
        let arr = [|1;2;3;4;5;6 |].asRarr
        Assert.AreEqual(arr.[..1], [|1;2 |].asRarr)
        Assert.AreEqual(arr.[..2], [|1;2;3 |].asRarr)
        Assert.AreEqual(arr.[..3], [|1;2;3;4 |].asRarr)
        Assert.AreEqual(arr.[..4], [|1;2;3;4;5 |].asRarr)
        Assert.AreEqual(arr.[..5], [|1;2;3;4;5;6 |].asRarr)
        //Assert.AreEqual(arr.[..6], [|1;2;3;4;5;6 |].asRarr)
        //Assert.AreEqual(arr.[..7], [|1;2;3;4;5;6 |].asRarr)

    // testCase "ResizeArray.SlicingUnboundedStart() +6" <| fun _ ->
    //     let arr = [|1;2;3;4;5;6 |].asRarr
    //     throwsIdx   (fun () -> arr.[..6]  |> ignore )
    //     throwsIdx   (fun () -> arr.[..7]  |> ignore )



    testCase "ResizeArray.SlicingBoundedStartEnd() " <| fun _ ->
        let arr = [|1;2;3;4;5;6 |].asRarr

        Assert.AreEqual(arr.[*], arr)

        Assert.AreEqual(arr.[0..0], [|1 |].asRarr)
        Assert.AreEqual(arr.[0..1], [|1;2 |].asRarr)
        Assert.AreEqual(arr.[0..2], [|1;2;3 |].asRarr)
        Assert.AreEqual(arr.[0..3], [|1;2;3;4 |].asRarr)
        Assert.AreEqual(arr.[0..4], [|1;2;3;4;5 |].asRarr)
        Assert.AreEqual(arr.[0..5], [|1;2;3;4;5;6 |].asRarr)

        Assert.AreEqual(arr.[1..1], [|2 |].asRarr)
        Assert.AreEqual(arr.[1..2], [|2;3 |].asRarr)
        Assert.AreEqual(arr.[1..3], [|2;3;4 |].asRarr)
        Assert.AreEqual(arr.[1..4], [|2;3;4;5 |].asRarr)
        Assert.AreEqual(arr.[1..5], [|2;3;4;5;6 |].asRarr)

        Assert.AreEqual(arr.[0..1], [|1;2 |].asRarr)
        Assert.AreEqual(arr.[1..1], [|2 |].asRarr)

        //Assert.AreEqual(arr.[4..1], ([| |].asRarr: int ResizeArray))
        //Assert.AreEqual(arr.[3..1], ([| |].asRarr: int ResizeArray))
        //Assert.AreEqual(arr.[2..1], ([| |].asRarr: int ResizeArray))

    testCase "ResizeArray.SlicingBoundedStartEnd() bad order" <| fun _ ->
        let arr = [|1;2;3;4;5;6 |].asRarr
        Assert.AreEqual(arr.[2..1], [||].asRarr)
    //     throwsIdx   (fun () -> arr.[2..1]  |> ignore )
    //     throwsIdx   (fun () -> arr.[3..1]  |> ignore )
    //     throwsIdx   (fun () -> arr.[4..3]  |> ignore )

    testCase "ResizeArray.SlicingEmptyArray() " <| fun _ ->

        let empty = ResizeArray<_>() // <int>
        Assert.AreEqual(empty.[*], ([| |].asRarr: ResizeArray<int>))
        // throwsIdx   (fun () -> empty.[5..3] |> ignore )
        // throwsIdx   (fun () -> empty.[0..]  |> ignore )
        // throwsIdx   (fun () -> empty.[0..0] |> ignore )
        // throwsIdx   (fun () -> empty.[0..1] |> ignore )
        // throwsIdx   (fun () -> empty.[3..5] |> ignore )
        Assert.AreEqual(empty.[-1..1], [||].asRarr)


    testCase "ResizeArray.SlicingOutOfBounds() [-1..1] " <| fun _ ->
        let arr = [|1;2;3|].asRarr
        Assert.AreEqual(arr.[-1..1], [|1;2|].asRarr)

    testCase "ResizeArray.SlicingOutOfBounds() [2..1] " <| fun _ ->
        let arr = [|1;2;3|].asRarr
        Assert.AreEqual(arr.[2..1], [||].asRarr)

    testCase "ResizeArray.SlicingOutOfBounds() [2..9] " <| fun _ ->
        let arr = [|1;2;3|].asRarr
        Assert.AreEqual(arr.[2..9], [|3|].asRarr)

    testCase "ResizeArray.SlicingOutOfBounds() [10..8] " <| fun _ ->
        let arr = [|1;2;3|].asRarr
        Assert.AreEqual(arr.[10 .. 8 ], [||].asRarr)

    testCase "ResizeArray.SlicingOutOfBounds() [-1 2] " <| fun _ ->
        let arr = [|1;2;3|].asRarr
        throwsIdx   (fun () -> arr.SliceIdx(-1,2)|> ignore )

    testCase "ResizeArray.SlicingOutOfBounds() [0 9] " <| fun _ ->
        let arr = [|1;2;3|].asRarr
        throwsIdx   (fun () -> arr.SliceIdx(0,9)|> ignore )

    testCase "ResizeArray.SlicingOutOfBounds() [..] " <| fun _ ->
        let arr = [|1;2;3|].asRarr
        Assert.AreEqual(arr.SliceLooped(-1,0), [||].asRarr)
        Assert.AreEqual(arr.SliceLooped(-1,-1), [|3|].asRarr)
        Assert.AreEqual(arr.SliceLooped(3,4), [|1;2|].asRarr)

    testCase "ResizeArray.equals" <| fun _ ->
        let a1 = [|1;2;3|].asRarr
        let a2 = [|1;2;3|].asRarr
        let a3 = [|1;2;4|].asRarr
        Assert.True(ResizeArray.equals a1 a1)
        Assert.True(ResizeArray.equals a1 a2)
        Assert.False(ResizeArray.equals a1 a3)
        Assert.False(ResizeArray.equals null a1)
        Assert.True(ResizeArray.equals null null)



    testCase "ResizeArray.equals2" <| fun _ ->
        let a1 = [|1;2;3|].asRarr
        let a2 = [|1;2;3|].asRarr
        // let a3 = [|1;2;4|].asRarr

        let aa1 = [|a1;a1|].asRarr
        let aa2 = [|a1;a2|].asRarr
        // let aa3 = [|a1;a3|].asRarr
        let aa4 = [|a1;a1;a2|].asRarr
        let aa4n = [|a1;a1;null|].asRarr
        let aa5 = [|a1;null|].asRarr
        let aa6 = [|a1;null|].asRarr
        Assert.True(ResizeArray.equals2 aa1 aa1)
        Assert.True(ResizeArray.equals2 aa1 aa2)
        Assert.True(ResizeArray.equals2 aa5 aa6)
        Assert.False(ResizeArray.equals2 aa1 aa4)
        Assert.False(ResizeArray.equals2 aa4n aa4)

  ]