namespace Tests

open ResizeArrayT

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
// open System.Linq
// open System.Runtime.InteropServices
#endif

open System
open System.Collections.Generic

type RarrWindowedTestInput<'t> =
    {
        InputArray : 't ResizeArray
        WindowSize : int
        ExpectedArray : 't ResizeArray ResizeArray
        Exception : Type option
    }

module Module2 =
 open Exceptions

 let tests =
  testList "Module2.fs Tests" [


    testCase "ResizeArray.Length" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.length [|1..8 |].asRarr
        if resultInt <> 8 then Assert.Fail()

        // string array
        let resultStr = ResizeArray.length [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        if resultStr <> 4 then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.length [|  |].asRarr
        if resultEpt <> 0 then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.length  nullArr  |> ignore)

        // null array, argument name showing up
        //try
        //    ResizeArray.length nullArr |> ignore
        //with
        //| :? ArgumentNullException as e -> Assert.Equal("array", e.ParamName) |> ignore

        ()


    testCase "ResizeArray.Indexed" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.indexed [|10..2..20 |].asRarr
        Assert.AreEqual([|(0,10);(1,12);(2,14);(3,16);(4,18);(5,20) |].asRarr, resultInt)

        // string array
        // let funcStr (x:int) (y:string) =  x+ y.Length
        let resultStr = ResizeArray.indexed [| "Lists"; "Are"; "Commonly"; "List"  |].asRarr
        Assert.AreEqual([| (0,"Lists");(1,"Are");(2,"Commonly");(3,"List")  |].asRarr, resultStr)

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.indexed emptyArr
        Assert.AreEqual([|  |].asRarr, resultEpt)

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.indexed nullArr |> ignore)

        ()


    testCase "ResizeArray.Map" <| fun _ ->
        // integer array
        let funcInt x =
                match x with
                | _ when x % 2 = 0 -> 10*x
                | _ -> x
        let resultInt = ResizeArray.map funcInt [| 1..10  |].asRarr
        if resultInt <!> [|1;20;3;40;5;60;7;80;9;100 |].asRarr then Assert.Fail()

        // string array
        let funcStr (x:string) = x.ToLower()
        let resultStr = ResizeArray.map funcStr [|"Lists"; "Are";  "Commonly" ; "List"  |].asRarr
        if resultStr <!> [|"lists"; "are";  "commonly" ; "list"  |].asRarr then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.map funcInt [|  |].asRarr
        if resultEpt <!> [|  |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.map funcStr nullArr |> ignore)

        ()


    testCase "ResizeArray.Map2" <| fun _ ->
        // integer array
        let funcInt x y = x+y
        let resultInt = ResizeArray.map2 funcInt [|1..10 |].asRarr [|2..2..20 |].asRarr
        if resultInt <!> [|3;6;9;12;15;18;21;24;27;30 |].asRarr then Assert.Fail()

        // string array
        let funcStr (x:int) (y:string) =  x+ y.Length
        let resultStr = ResizeArray.map2 funcStr [|3;6;9;11 |].asRarr [|"Lists"; "Are";  "Commonly" ; "List"  |].asRarr
        if resultStr <!> [|8;9;17;15 |].asRarr then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.map2 funcInt emptyArr emptyArr
        if resultEpt <!> [|  |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<int>
        let validArray = [| 1  |].asRarr
        throwsNull (fun () -> ResizeArray.map2 funcInt nullArr validArray |> ignore)
        throwsNull (fun () -> ResizeArray.map2 funcInt validArray nullArr |> ignore)

        // len1 <!> len2
        throwsArg(fun () -> ResizeArray.map2 funcInt [|1..10 |].asRarr [|2..20 |].asRarr |> ignore)

        ()


    testCase "ResizeArray.Map3" <| fun _ ->
        // Integer array
        let funcInt a b c = (a + b) * c
        let resultInt = ResizeArray.map3 funcInt [| 1..8  |].asRarr [| 2..9  |].asRarr [| 3..10  |].asRarr
        if resultInt <!> [| 9; 20; 35; 54; 77; 104; 135; 170  |].asRarr then Assert.Fail()

        // First array is shorter
        throwsArg (fun () -> ResizeArray.map3 funcInt [| 1..2  |].asRarr [| 2..9  |].asRarr [| 3..10  |].asRarr |> ignore)
        // Second array is shorter
        throwsArg (fun () -> ResizeArray.map3 funcInt [| 1..8  |].asRarr [| 2..6  |].asRarr [| 3..10  |].asRarr |> ignore)
        // Third array is shorter
        throwsArg (fun () -> ResizeArray.map3 funcInt [| 1..8  |].asRarr [| 2..9  |].asRarr [| 3..6  |].asRarr |> ignore)

        // String array
        let funcStr a b c = a + b + c
        let resultStr = ResizeArray.map3 funcStr [| "A";"B";"C";"D"  |].asRarr [| "a";"b";"c";"d"  |].asRarr [| "1";"2";"3";"4"  |].asRarr
        if resultStr <!> [| "Aa1";"Bb2";"Cc3";"Dd4"  |].asRarr then Assert.Fail()

        // Empty array
        let resultEmpty = ResizeArray.map3 funcStr [| |].asRarr [| |].asRarr [| |].asRarr
        if resultEmpty <!> [| |].asRarr then Assert.Fail()

        // Null array
        let nullArray = null:ResizeArray<int>
        let nonNullArray = [|1 |].asRarr
        throwsNull (fun () -> ResizeArray.map3 funcInt nullArray nonNullArray nonNullArray |> ignore)
        throwsNull (fun () -> ResizeArray.map3 funcInt nonNullArray nullArray nonNullArray |> ignore)
        throwsNull (fun () -> ResizeArray.map3 funcInt nonNullArray nonNullArray nullArray |> ignore)

        ()


    testCase "ResizeArray.MapFold" <| fun _ ->
        // integer array
        let funcInt acc x = if x % 2 = 0 then 10*x, acc + 1 else x, acc
        let resultInt,resultIntAcc = ResizeArray.mapFold funcInt 100 [| 1..10  |].asRarr
        if resultInt <!> [| 1;20;3;40;5;60;7;80;9;100  |].asRarr then Assert.Fail()
        Assert.AreEqual(105, resultIntAcc)

        // string array
        let funcStr acc (x:string) = match x.Length with 0 -> "empty", acc | _ -> x.ToLower(), sprintf "%s%s" acc x
        let resultStr,resultStrAcc = ResizeArray.mapFold funcStr "" [| "";"BB";"C";""  |].asRarr
        if resultStr <!> [| "empty";"bb";"c";"empty"  |].asRarr then Assert.Fail()
        Assert.AreEqual("BBC", resultStrAcc)

        // empty array
        let resultEpt,resultEptAcc = ResizeArray.mapFold funcInt 100 [|  |].asRarr
        if resultEpt <!> [|  |].asRarr then Assert.Fail()
        Assert.AreEqual(100, resultEptAcc)

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.mapFold funcStr "" nullArr |> ignore)

        ()


    testCase "ResizeArray.MapFoldBack" <| fun _ ->
        // integer array
        let funcInt x acc = if acc < 105 then 10*x, acc + 2 else x, acc
        let resultInt,resultIntAcc = ResizeArray.mapFoldBack funcInt [| 1..10  |].asRarr 100
        if resultInt <!> [| 1;2;3;4;5;6;7;80;90;100  |].asRarr then Assert.Fail()
        Assert.AreEqual(106, resultIntAcc)

        // string array
        let funcStr (x:string) acc = match x.Length with 0 -> "empty", acc | _ -> x.ToLower(), sprintf "%s%s" acc x
        let resultStr,resultStrAcc = ResizeArray.mapFoldBack funcStr [| "";"BB";"C";""  |].asRarr ""
        if resultStr <!> [| "empty";"bb";"c";"empty"  |].asRarr then Assert.Fail()
        Assert.AreEqual("CBB", resultStrAcc)

        // empty array
        let resultEpt,resultEptAcc = ResizeArray.mapFoldBack funcInt [|  |].asRarr 100
        if resultEpt <!> [|  |].asRarr then Assert.Fail()
        Assert.AreEqual(100, resultEptAcc)

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.mapFoldBack funcStr nullArr "" |> ignore)

        ()


    testCase "ResizeArray.Mapi" <| fun _ ->
        // integer array
        let funcInt x y = x+y
        let resultInt = ResizeArray.mapi funcInt [|10..2..20 |].asRarr
        if resultInt <!> [|10;13;16;19;22;25 |].asRarr then Assert.Fail()

        // string array
        let funcStr (x:int) (y:string) =  x+ y.Length
        let resultStr = ResizeArray.mapi funcStr  [|"Lists"; "Are";  "Commonly" ; "List"  |].asRarr
        if resultStr <!> [|5;4;10;7 |].asRarr then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.mapi funcInt emptyArr
        if resultEpt <!> [|  |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.mapi funcStr nullArr |> ignore)

        ()


    testCase "ResizeArray.mapi2" <| fun _ ->
        // integer array
        let funcInt x y z = x+y+z
        let resultInt = ResizeArray.mapi2 funcInt [|1..10 |].asRarr [|2..2..20 |].asRarr
        if resultInt <!> [|3;7;11;15;19;23;27;31;35;39 |].asRarr then Assert.Fail()

        // string array
        let funcStr  z (x:int) (y:string)  =z + x+ y.Length
        let resultStr = ResizeArray.mapi2 funcStr [|3;6;9;11 |].asRarr [|"Lists"; "Are";  "Commonly" ; "List"  |].asRarr
        if resultStr <!> [|8;10;19;18 |].asRarr then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEpt = ResizeArray.mapi2 funcInt emptyArr emptyArr
        if resultEpt <!> [|  |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<int>
        let validArray = [| 1  |].asRarr
        throwsNull (fun () -> ResizeArray.mapi2 funcInt validArray  nullArr  |> ignore)
        throwsNull (fun () -> ResizeArray.mapi2 funcInt  nullArr validArray |> ignore)

        // len1 <!> len2
        throwsArg(fun () -> ResizeArray.mapi2 funcInt [|1..10 |].asRarr [|2..20 |].asRarr |> ignore)

        ()


    testCase "ResizeArray.Max" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.max  [|2..2..20 |].asRarr
        if resultInt <> 20 then Assert.Fail()

        // string array
        let resultStr = ResizeArray.max [|"t"; "ahe"; "Lists"; "Are";  "Commonly" ; "List";"a"  |].asRarr
        if resultStr <> "t" then Assert.Fail()

        // empty array -- argumentexception

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.max   nullArr  |> ignore)

        // len = 0
        throwsArg(fun() -> ResizeArray.max  [| |].asRarr |> ignore)

        ()


    testCase "ResizeArray.MaxBy" <| fun _ ->
        // integer array
        let funcInt x = x%8
        let resultInt = ResizeArray.maxBy funcInt [|2..2..20 |].asRarr
        if resultInt <> 6 then Assert.Fail()

        // string array
        let funcStr (x:string) = x.Length
        let resultStr = ResizeArray.maxBy funcStr  [|"Lists"; "Are";  "Commonly" ; "List" |].asRarr
        if resultStr <> "Commonly" then Assert.Fail()

        // empty array -- argumentexception

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.maxBy funcStr   nullArr  |> ignore)

        // len = 0
        throwsArg(fun() -> ResizeArray.maxBy funcInt (ResizeArray<int>()) |> ignore)// <int>) |> ignore)

        ()


    testCase "ResizeArray.Min" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.min  [|3;7;8;9;4;1;1;2 |].asRarr
        if resultInt <> 1 then Assert.Fail()

        // string array
        let resultStr = ResizeArray.min [|"a"; "Lists";  "Commonly" ; "List"   |].asRarr
        if resultStr <> "Commonly" then Assert.Fail()

        // empty array -- argument exception

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.min   nullArr  |> ignore)

        // len = 0
        throwsArg(fun () -> ResizeArray.min  [| |].asRarr |> ignore)

        ()


    testCase "ResizeArray.MinBy" <| fun _ ->
        // integer array
        let funcInt x = x%8
        let resultInt = ResizeArray.minBy funcInt [|3;7;9;4;8;1;1;2 |].asRarr
        if resultInt <> 8 then Assert.Fail()

        // string array
        let funcStr (x:string) = x.Length
        let resultStr = ResizeArray.minBy funcStr  [|"Lists"; "Are";  "Commonly" ; "List" |].asRarr
        if resultStr <> "Are" then Assert.Fail()

        // empty array -- argumentexception

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.minBy funcStr  nullArr |> ignore)

        // len = 0
        throwsArg(fun () -> ResizeArray.minBy funcInt (ResizeArray<int>()) |> ignore) // <int>) |> ignore)

        ()



    testCase "ResizeArray.Of_List" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.ofList [1..10]
        if resultInt <!> [|1..10 |].asRarr then Assert.Fail()

        // string array
        let resultStr = ResizeArray.ofList ["Lists"; "are";  "commonly" ; "list" ]
        if resultStr <!> [| "Lists"; "are";  "commonly" ; "list"  |].asRarr then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.ofList []
        if resultEpt <!> [| |].asRarr then Assert.Fail()

        // null array

        ()


    testCase "ResizeArray.Of_Seq" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.ofSeq (seq{1..10})
        if resultInt <!> [|1..10 |].asRarr then Assert.Fail()

        // string array
        let resultStr = ResizeArray.ofSeq (seq {for x in 'a'..'f' -> x.ToString()})
        if resultStr <!> [| "a";"b";"c";"d";"e";"f"  |].asRarr then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.ofSeq []
        if resultEpt <!> [|  |].asRarr then Assert.Fail()

        // null array

        ()


    testCase "ResizeArray.Partition" <| fun _ ->
        // integer array
        let resultInt,ri = ResizeArray.partition (fun x -> x%3 = 0) [|1..10 |].asRarr
        if resultInt <!> [|3;6;9 |].asRarr then Assert.Fail()
        if ri <!>  [|1;2;4;5;7;8;10 |].asRarr then Assert.Fail()

        // string array
        let resultStr,rs = ResizeArray.partition (fun (x:string) -> x.Length >4) [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        if resultStr<!> [|"Lists";"commonly" |].asRarr then Assert.Fail()
        if rs <!> [|"are"; "list" |].asRarr then Assert.Fail()

        // empty array
        let resultEpt,re = ResizeArray.partition (fun x -> x%3 = 0) [| |].asRarr
        if resultEpt <!> [| |].asRarr then Assert.Fail()
        if re <!> [| |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.partition (fun (x:string) -> x.Length >4)  nullArr  |> ignore)

        ()


    testCase "ResizeArray.Permute" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.permute (fun i -> (i+1) % 4) [|1;2;3;4 |].asRarr
        if resultInt <!> [|4;1;2;3 |].asRarr then Assert.Fail()

        // string array
        let resultStr = ResizeArray.permute (fun i -> (i+1) % 4) [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        if resultStr <!> [|"list";"Lists"; "are";  "commonly"  |].asRarr then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.permute (fun i -> (i+1) % 4) [| |].asRarr
        if resultEpt <!> [| |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.permute (fun i -> (i+1) % 4)  nullArr  |> ignore)

        ()


    testCase "ResizeArray.Reduce" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.reduce (fun x y -> x/y) [|5*4*3*2; 4;3;2;1 |].asRarr
        if resultInt <> 5 then Assert.Fail()

        // string array
        let resultStr = ResizeArray.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length)) [|"ABCDE";"A"; "B";  "C" ; "D"  |].asRarr
        if resultStr <> "E" then  Assert.Fail()

        // empty array
        throwsArg (fun () -> ResizeArray.reduce (fun x y -> x/y)  [| |].asRarr |> ignore)

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.reduce (fun (x:string) (y:string) -> x.Remove(0,y.Length))  nullArr  |> ignore)

        ()



    testCase "ResizeArray.ReduceBack" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.reduceBack (fun x y -> x/y) [|5*4*3*2; 4;3;2;1 |].asRarr
        if resultInt <> 30 then Assert.Fail()

        // string array
        let resultStr = ResizeArray.reduceBack (fun (x:string) (y:string) -> x.Remove(0,y.Length)) [|"ABCDE";"A"; "B";  "C" ; "D"  |].asRarr
        if resultStr <> "ABCDE" then  Assert.Fail()

        // string array
        let funcStr x y = x+y
        let resultStr2 = ResizeArray.reduceBack funcStr [|"A"; "B";  "C" ; "D"  |].asRarr
        if resultStr2 <> "ABCD" then  Assert.Fail()

        // empty array
        throwsArg (fun () -> ResizeArray.reduceBack (fun x y -> x/y) [| |].asRarr |> ignore)

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.reduceBack (fun (x:string) (y:string) -> x.Remove(0,y.Length))  nullArr  |> ignore)

        ()



    testCase "ResizeArray.Rev" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.rev  [|1..10 |].asRarr
        if resultInt <!> [|10;9;8;7;6;5;4;3;2;1 |].asRarr then Assert.Fail()

        // string array
        let resultStr = ResizeArray.rev  [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        if resultStr <!> [|"list"; "commonly"; "are"; "Lists"  |].asRarr then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.rev  [| |].asRarr
        if resultEpt <!> [| |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.rev  nullArr  |> ignore)
        ()


    testCase "ResizeArray.Scan" <| fun _ ->
        // integer array
        let funcInt x y = x+y
        let resultInt = ResizeArray.scan funcInt 9 [| 1..10  |].asRarr
        if resultInt <!> [|9;10;12;15;19;24;30;37;45;54;64 |].asRarr then Assert.Fail()

        // string array
        let funcStr x y = x+y
        let resultStr = ResizeArray.scan funcStr "x" [|"A"; "B";  "C" ; "D"  |].asRarr
        if resultStr <!> [|"x";"xA";"xAB";"xABC";"xABCD" |].asRarr then Assert.Fail()



        // empty array
        let resultEpt = ResizeArray.scan funcInt 5 [|  |].asRarr
        if resultEpt <!> [|5 |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.scan funcStr "begin"  nullArr  |> ignore)

        ()


    testCase "ResizeArray.ScanBack" <| fun _ ->
        // integer array
        let funcInt x y = x+y
        let resultInt = ResizeArray.scanBack funcInt [| 1..10  |].asRarr 9
        if resultInt <!> [|64;63;61;58;54;49;43;36;28;19;9 |].asRarr then Assert.Fail()

        // string array
        let funcStr x y = x+y
        let resultStr = ResizeArray.scanBack funcStr [|"A"; "B";  "C" ; "D"  |].asRarr "X"
        if resultStr <!> [|"ABCDX";"BCDX";"CDX";"DX";"X" |].asRarr then Assert.Fail()



        // empty array
        let resultEpt = ResizeArray.scanBack funcInt [|  |].asRarr 5
        if resultEpt <!> [|5 |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.scanBack funcStr nullArr "begin"  |> ignore)

        ()


    testCase "ResizeArray.Skip" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.skip 2 [|1..10 |].asRarr
        if resultInt <!> [|3..10 |].asRarr then Assert.Fail()

        let resultInt2 = ResizeArray.skip 0 [|1..10 |].asRarr
        if resultInt2 <!> [|1..10 |].asRarr then Assert.Fail()

        throwsArg (fun () ->  ResizeArray.skip -5 [|1..10 |].asRarr |> ignore)
        //if resultInt3 <!> [|1..10 |].asRarr then Assert.Fail()

        // string List
        let resultStr = ResizeArray.skip 2 [|"str1";"str2";"str3";"str4" |].asRarr
        if resultStr <!> [|"str3";"str4" |].asRarr then Assert.Fail()

        // empty List
        let resultEpt = ResizeArray.skip 0 [| |].asRarr
        if resultEpt <!> [| |].asRarr then Assert.Fail()

        // exceptions
        throwsNull (fun () -> ResizeArray.skip 0 (null:ResizeArray<string>) |> ignore)
        throwsNull (fun () -> ResizeArray.skip -3 (null:ResizeArray<string>) |> ignore)
        throwsArg (fun () -> ResizeArray.skip 1 [| |].asRarr |> ignore)
        throwsArg (fun () -> ResizeArray.skip 4 [|1; 2; 3 |].asRarr |> ignore)


    testCase "ResizeArray.SkipWhile" <| fun _ ->
        // integer array
        let funcInt x = (x < 4)
        let intArr = [|1..10 |].asRarr
        let resultInt = ResizeArray.skipWhile funcInt intArr
        if resultInt <!> [|4..10 |].asRarr then Assert.Fail()

        // string array
        let funcStr (s:string) = s.Length < 8
        let strArr = [| "Lists"; "are";  "commonly" ; "list"  |].asRarr
        let resultStr = ResizeArray.skipWhile funcStr strArr
        if resultStr <!> [| "commonly" ; "list"  |].asRarr then Assert.Fail()

        // empty array
        let resultEmpt = ResizeArray.skipWhile (fun _ -> failwith "unexpected error") [|  |].asRarr
        if resultEmpt <!> [|  |].asRarr then Assert.Fail()

        // null array
        throwsNull (fun () -> ResizeArray.skipWhile (fun _ -> failwith "unexpected error") null |> ignore)

        // skip all
        let resultAll = ResizeArray.skipWhile (fun _ -> true) intArr
        if resultAll <!> [|  |].asRarr then Assert.Fail()

        // skip none
        let resultNone = ResizeArray.skipWhile (fun _ -> false) intArr
        if resultNone <!> intArr then Assert.Fail()

        ()


    testCase "ResizeArray.Set" <| fun _ ->
        // integer array
        let intArr = [|10;9;8;7 |].asRarr
        ResizeArray.set 3 600 intArr
        if intArr <!> [|10;9;8;600 |].asRarr then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        ResizeArray.set 2 "always" strArr
        if strArr <!> [|"Lists"; "are";  "always" ; "list"  |].asRarr     then Assert.Fail()

        // empty array -- outofbundaryexception

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.set 0 "null"  nullArr )

        ()


    testCase "ResizeArray.sortInPlaceWith" <| fun _ ->
        // integer array
        let intArr = [|3;5;7;2;4;8 |].asRarr
        ResizeArray.sortInPlaceWith compare intArr
        if intArr <!> [|2;3;4;5;7;8 |].asRarr then Assert.Fail()

        // Sort backwards
        let intArr = [|3;5;7;2;4;8 |].asRarr
        ResizeArray.sortInPlaceWith (fun a b -> -1 * compare a b) intArr
        if intArr <!> [|8;7;5;4;3;2 |].asRarr then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "used"; "data"; "structure" |].asRarr
        ResizeArray.sortInPlaceWith compare strArr
        if strArr <!> [| "Lists"; "a"; "are"; "commonly"; "data"; "structure"; "used" |].asRarr     then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        ResizeArray.sortInPlaceWith compare emptyArr
        if emptyArr <!> [| |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.sortInPlaceWith compare nullArr  |> ignore)

        // len = 2
        let len2Arr = [|8;3 |].asRarr
        ResizeArray.sortInPlaceWith compare len2Arr
        Assert.AreEqual([|3;8 |].asRarr, len2Arr)

        // Equal elements
        let eights = [|8; 8;8 |].asRarr
        ResizeArray.sortInPlaceWith compare eights
        Assert.AreEqual([|8;8;8 |].asRarr, eights)

        ()



    testCase "ResizeArray.sortInPlaceBy" <| fun _ ->
        // integer array
        let intArr = [|3;5;7;2;4;8 |].asRarr
        ResizeArray.sortInPlaceBy int intArr
        if intArr <!> [|2;3;4;5;7;8 |].asRarr then Assert.Fail()

        // string array
        let strArr = [|"Lists"; "are"; "a"; "commonly"; "used"; "data"; "structure" |].asRarr
        ResizeArray.sortInPlaceBy (fun (x:string) -> x.Length)  strArr
        // note: ResizeArray.sortInPlaceBy is not stable, so we allow 2 results.
        if strArr <!> [| "a"; "are";"data"; "used";"Lists"; "commonly";"structure" |].asRarr && strArr <!> [| "a"; "are"; "used"; "data"; "Lists"; "commonly";"structure" |].asRarr  then Assert.Fail()

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        ResizeArray.sortInPlaceBy int emptyArr
        if emptyArr <!> [| |].asRarr then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.sortInPlaceBy (fun (x:string) -> x.Length) nullArr |> ignore)

        // len = 2
        let len2Arr = [|8;3 |].asRarr
        ResizeArray.sortInPlaceBy int len2Arr
        if len2Arr <!> [|3;8 |].asRarr then Assert.Fail()
        Assert.AreEqual([|3;8 |].asRarr,len2Arr)

        ()


    testCase "ResizeArray.SortDescending" <| fun _ ->
        // integer array
        let intArr = [|3;5;7;2;4;8 |].asRarr
        let resultInt = ResizeArray.sortDescending intArr
        Assert.AreEqual([|8;7;5;4;3;2 |].asRarr, resultInt)

        // string Array
        let strArr = [|"Z";"a";"d"; ""; "Y"; null; "c";"b";"X" |].asRarr
        let resultStr = ResizeArray.sortDescending strArr
        Assert.AreEqual([|"d"; "c"; "b"; "a"; "Z"; "Y"; "X"; ""; null |].asRarr, resultStr)

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEmpty = ResizeArray.sortDescending emptyArr
        if resultEmpty <!> [| |].asRarr then Assert.Fail()

        // tuple array
        let tupArr = [|(2,"a");(1,"d");(1,"b");(1,"a");(2,"x");(2,"b");(1,"x") |].asRarr
        let resultTup = ResizeArray.sortDescending tupArr
        Assert.AreEqual([|(2,"x");(2,"b");(2,"a");(1,"x");(1,"d");(1,"b");(1,"a") |].asRarr, resultTup)

        // date array
        let dateArr = [|DateTime(2014,12,31);DateTime(2014,1,1);DateTime(2015,1,1);DateTime(2013,12,31);DateTime(2014,1,1) |].asRarr
        let resultDate = ResizeArray.sortDescending dateArr

        if [|DateTime(2014,12,31);DateTime(2014,1,1);DateTime(2015,1,1);DateTime(2013,12,31);DateTime(2014,1,1) |].asRarr <!> dateArr then  Assert.Fail()
        if [|DateTime(2015,1,1);DateTime(2014,12,31);DateTime(2014,1,1);DateTime(2014,1,1);DateTime(2013,12,31) |].asRarr <!> resultDate then  Assert.Fail()


        //Assert.AreEqual([|DateTime(2014,12,31);DateTime(2014,1,1);DateTime(2015,1,1);DateTime(2013,12,31);DateTime(2014,1,1) |].asRarr, dateArr)
        //Assert.AreEqual([|DateTime(2015,1,1);DateTime(2014,12,31);DateTime(2014,1,1);DateTime(2014,1,1);DateTime(2013,12,31) |].asRarr, resultDate)

        // float array
        let minFloat,maxFloat,epsilon = System.Double.MinValue,System.Double.MaxValue,System.Double.Epsilon
        let floatArr = [| 0.0; 0.5; 2.0; 1.5; 1.0; minFloat; maxFloat; epsilon; -epsilon  |].asRarr
        let resultFloat = ResizeArray.sortDescending floatArr
        Assert.AreEqual([| maxFloat; 2.0; 1.5; 1.0; 0.5; epsilon; 0.0; -epsilon; minFloat;  |].asRarr, resultFloat)

        ()


    testCase "ResizeArray.SortByDescending" <| fun _ ->
        // integer array
        let intArr = [|3;5;7;2;4;8 |].asRarr
        let resultInt = ResizeArray.sortByDescending int intArr
        Assert.AreEqual([|3;5;7;2;4;8 |].asRarr, intArr)
        Assert.AreEqual([|8;7;5;4;3;2 |].asRarr, resultInt)

        // string array
        let strArr = [|".."; ""; "..."; "."; "...." |].asRarr
        let resultStr = ResizeArray.sortByDescending (fun (x:string) -> x.Length)  strArr
        Assert.AreEqual([|".."; ""; "..."; "."; "...." |].asRarr, strArr)
        Assert.AreEqual([|"....";"...";"..";"."; "" |].asRarr, resultStr)

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEmpty = ResizeArray.sortByDescending int emptyArr
        if resultEmpty <!> [| |].asRarr then Assert.Fail()

        // tuple array
        let tupArr = [|(2,"a");(1,"d");(1,"b");(2,"x") |].asRarr
        let sndTup = ResizeArray.sortByDescending snd tupArr
        Assert.AreEqual( [|(2,"a");(1,"d");(1,"b");(2,"x") |].asRarr , tupArr)
        Assert.AreEqual( [|(2,"x");(1,"d");(1,"b");(2,"a") |].asRarr , sndTup)

        // date array
        let dateArr = [|DateTime(2013,12,31);DateTime(2014,2,1);DateTime(2015,1,1);DateTime(2014,3,1) |].asRarr
        let resultDate = ResizeArray.sortByDescending (fun (d:DateTime) -> d.Month) dateArr
        if [|DateTime(2013,12,31);DateTime(2014,2,1);DateTime(2015,1,1);DateTime(2014,3,1) |].asRarr <!> dateArr then Assert.Fail()
        if [|DateTime(2013,12,31);DateTime(2014,3,1);DateTime(2014,2,1);DateTime(2015,1,1) |].asRarr <!> resultDate then Assert.Fail()

        // float array
        let minFloat,maxFloat,epsilon = System.Double.MinValue,System.Double.MaxValue,System.Double.Epsilon
        let floatArr = [| 0.0; 0.5; 2.0; 1.5; 1.0; minFloat; maxFloat; epsilon; -epsilon  |].asRarr
        let resultFloat = ResizeArray.sortByDescending id floatArr
        Assert.AreEqual([| maxFloat; 2.0; 1.5; 1.0; 0.5; epsilon; 0.0; -epsilon; minFloat;  |].asRarr, resultFloat)

        ()


    testCase "ResizeArray.Sub2" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.sub 3 3 [|1..8 |].asRarr
        if resultInt <!> [|4;5;6 |].asRarr then Assert.Fail()


    testCase "ResizeArray.Sub3" <| fun _ ->
        // string array
        let resultStr = ResizeArray.sub 1 2 [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        if resultStr <!> [|"are";  "commonly"  |].asRarr then Assert.Fail()

    testCase "ResizeArray.Sub4" <| fun _ ->
        // empty array
        let resultEpt = ResizeArray.sub 0 0 [|  |].asRarr
        if resultEpt <!> [| |].asRarr then Assert.Fail()

    testCase "ResizeArray.Sub5" <| fun _ ->
        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.sub 1 1  nullArr|> ignore)

    testCase "ResizeArray.Sub6" <| fun _ ->
        // bounds
        let resultInt = ResizeArray.sub 3 3 [|1..8 |].asRarr
        throwsArg (fun () -> ResizeArray.sub  -1 2 resultInt |> ignore)

    testCase "ResizeArray.Sub7" <| fun _ ->
        let resultInt = ResizeArray.sub 3 3 [|1..8 |].asRarr
        throwsArg (fun () -> ResizeArray.sub  1 -2 resultInt |> ignore)

    testCase "ResizeArray.Sub8" <| fun _ ->
        let resultInt = ResizeArray.sub 3 3 [|1..8 |].asRarr
        throwsArg (fun () -> ResizeArray.sub  1 20 resultInt |> ignore)

        ()


    testCase "ResizeArray.Sum" <| fun _ ->
        // empty integer array
        let resultEptInt = ResizeArray.sum ([| |].asRarr:int ResizeArray)
        if resultEptInt <> 0 then Assert.Fail()

        // empty float32 array
        let emptyFloatArray = ResizeArray<_>() // <System.Single>
        let resultEptFloat = ResizeArray.sum emptyFloatArray
        if resultEptFloat <> 0.0f then Assert.Fail()

        // empty double array
        let emptyDoubleArray = ResizeArray<_>() // <System.Double>
        let resultDouEmp = ResizeArray.sum emptyDoubleArray
        if resultDouEmp <> 0.0 then Assert.Fail()

        // empty decimal array
        let emptyDecimalArray = ResizeArray<_>() // <System.Decimal>
        let resultDecEmp = ResizeArray.sum emptyDecimalArray
        if resultDecEmp <> 0M then Assert.Fail()

        // integer array
        let resultInt = ResizeArray.sum [|1..10 |].asRarr
        if resultInt <> 55 then Assert.Fail()

        // float32 array
        let floatArray: float32 ResizeArray = [| 1.1f; 1.1f; 1.1f  |].asRarr
        let resultFloat = ResizeArray.sum floatArray
        if resultFloat < 3.3f - 0.001f || resultFloat > 3.3f + 0.001f then
            Assert.Fail()

        // double array
        let doubleArray: System.Double ResizeArray = [| 1.0; 8.0  |].asRarr
        let resultDouble = ResizeArray.sum doubleArray
        if resultDouble <> 9.0 then Assert.Fail()

        // decimal array
        let decimalArray: decimal ResizeArray = [| 0M; 19M; 19.03M  |].asRarr
        let resultDecimal = ResizeArray.sum decimalArray
        if resultDecimal <> 38.03M then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<float>
        throwsNull (fun () -> ResizeArray.sum  nullArr  |> ignore)
        ()

    testCase "ResizeArray.SumBy 1" <| fun _ ->
        // float32 array
        let floatArray: string ResizeArray = [| "1.2"; "3.5"; "6.7"  |].asRarr
        let resultFloat = ResizeArray.sumBy float32 floatArray
        if abs (resultFloat - 11.4f) > 0.00001f then Assert.Fail($"{resultFloat}<>0.000001")

    testCase "ResizeArray.SumBy 2" <| fun _ ->
        // float array
        let floatArray: string ResizeArray = [| "1.2"; "3.5"; "6.7"  |].asRarr
        let resultFloat = ResizeArray.sumBy float floatArray
        if abs (resultFloat - 11.4) > 0.000001 then Assert.Fail($"{resultFloat}<>0.000001")

    testCase "ResizeArray.SumBy 3" <| fun _ ->
        // empty integer array
        let resultEptInt = ResizeArray.sumBy int ([| |].asRarr:int ResizeArray)
        if resultEptInt <> 0 then Assert.Fail()
    testCase "ResizeArray.SumBy 4" <| fun _ ->
        // empty float32 array
        let emptyFloatArray = ResizeArray<_>() // <System.Single>
        let resultEptFloat = ResizeArray.sumBy float32 emptyFloatArray
        if resultEptFloat <> 0.0f then Assert.Fail()
    testCase "ResizeArray.SumBy 5" <| fun _ ->
        // empty double array
        let emptyDoubleArray = ResizeArray<_>() // <System.Double>
        let resultDouEmp = ResizeArray.sumBy float emptyDoubleArray
        if resultDouEmp <> 0.0 then Assert.Fail()
    testCase "ResizeArray.SumBy 6" <| fun _ ->
        // empty decimal array
        let emptyDecimalArray = ResizeArray<_>() // <System.Decimal>
        let resultDecEmp = ResizeArray.sumBy decimal emptyDecimalArray
        if resultDecEmp <> 0M then Assert.Fail()
    testCase "ResizeArray.SumBy 7" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.sumBy int [|1..10 |].asRarr
        if resultInt <> 55 then Assert.Fail()
    testCase "ResizeArray.SumBy 8" <| fun _ ->
        // float32 array
        let floatArray: string ResizeArray = [| "1.2";"3.5";"6.7"  |].asRarr
        let resultFloat = ResizeArray.sumBy float32 floatArray
        if abs (resultFloat - 11.4f) > 0.0001f then Assert.Fail($"{resultFloat}<>0.0000001")
    testCase "ResizeArray.SumBy 9" <| fun _ ->
        // double array
        let doubleArray: System.Double ResizeArray = [| 1.0;8.0  |].asRarr
        let resultDouble = ResizeArray.sumBy float doubleArray
        if resultDouble <> 9.0 then Assert.Fail()
    testCase "ResizeArray.SumBy 10" <| fun _ ->
        // decimal array
        let decimalArray: decimal ResizeArray = [| 0M;19M;19.03M  |].asRarr
        let resultDecimal = ResizeArray.sumBy decimal decimalArray
        if resultDecimal <> 38.03M then Assert.Fail()
    testCase "ResizeArray.SumBy 11" <| fun _ ->
        // null array
        let nullArr = null:ResizeArray<float>
        throwsNull (fun () -> ResizeArray.sumBy float32  nullArr  |> ignore)
        ()


    testCase "ResizeArray.Tail" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.tail [|1..10 |].asRarr
        Assert.AreEqual([|2..10 |].asRarr, resultInt)

        // string array
        let resultStr = ResizeArray.tail [| "a"; "b"; "c"; "d"  |].asRarr
        Assert.AreEqual([| "b";  "c" ; "d"  |].asRarr, resultStr)

        // 1-element array
        let resultStr2 = ResizeArray.tail [| "a"  |].asRarr
        Assert.AreEqual([|  |].asRarr, resultStr2)

        throwsArg(fun () -> ResizeArray.tail [| |].asRarr |> ignore)

        throwsNull(fun () -> ResizeArray.tail null |> ignore)
        ()


    testCase "ResizeArray.To_List" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.toList [|1..10 |].asRarr
        if resultInt <> [1..10] then Assert.Fail()


        // string array
        let resultStr = ResizeArray.toList [|"Lists"; "are";  "commonly" ; "list"  |].asRarr
        if resultStr <> ["Lists"; "are";  "commonly" ; "list"] then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.toList  [| |].asRarr
        if resultEpt <> [] then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.toList   nullArr  |> ignore)

        ()


    testCase "ResizeArray.To_Seq" <| fun _ ->
        // integer array
        let resultInt = [|1..10 |].asRarr |> ResizeArray.toSeq  |> ResizeArray.ofSeq
        if resultInt <!> [|1..10 |].asRarr then Assert.Fail()

        // string array
        let resultStr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr |> ResizeArray.toSeq |> ResizeArray.ofSeq
        if resultStr <!> [|"Lists"; "are";  "commonly" ; "list"  |].asRarr then Assert.Fail()

        // empty array
        let resultEpt =[| |].asRarr |> ResizeArray.toSeq  |> ResizeArray.ofSeq
        if resultEpt <!> [| |].asRarr  then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> nullArr  |> ResizeArray.toSeq   |> ignore)

        ()


    testCase "ResizeArray.Transpose" <| fun _ ->

        let areSame(x,y)= Assert.True(ResizeArray.equals2 x y)



        // integer array
        areSame ([|[|1;4 |].asRarr; [|2;5 |].asRarr; [|3;6 |].asRarr |].asRarr, ResizeArray.transpose (resizeArray{ [|1..3 |].asRarr; [|4..6 |].asRarr} ))
        areSame([|[|1;4 |].asRarr; [|2;5 |].asRarr; [|3;6 |].asRarr |].asRarr, ResizeArray.transpose (resizeArray{ [|1..3 |].asRarr; [|4..6 |].asRarr} ))
        areSame([|[|1 |].asRarr; [|2 |].asRarr; [|3 |].asRarr |].asRarr, ResizeArray.transpose [|[|1..3 |].asRarr |].asRarr)
        areSame([|[|1..2 |].asRarr |].asRarr, ResizeArray.transpose [|[|1 |].asRarr; [|2 |].asRarr |].asRarr)

        // string array
        areSame([|[|"a";"d" |].asRarr; [|"b";"e" |].asRarr; [|"c";"f" |].asRarr |].asRarr, ResizeArray.transpose (resizeArray{[|"a";"b";"c" |].asRarr; [|"d";"e";"f" |].asRarr}))

        // empty array
        areSame([|  |].asRarr, ResizeArray.transpose [|  |].asRarr)

        // array of empty arrays - m x 0 array transposes to 0 x m (i.e. empty)
        areSame([|  |].asRarr, ResizeArray.transpose [| [| |].asRarr  |].asRarr)
        areSame([|  |].asRarr, ResizeArray.transpose [| [| |].asRarr; [| |].asRarr  |].asRarr)

        // null array
        let nullArr = null: ResizeArray<ResizeArray<string>>
        throwsNull (fun () -> nullArr |> ResizeArray.transpose |> ignore)

        // jagged arrays
        throwsArg (fun () -> ResizeArray.transpose [| [|1; 2 |].asRarr; [|3 |].asRarr  |].asRarr |> ignore)
        throwsArg (fun () -> ResizeArray.transpose [| [|1 |].asRarr; [|2; 3 |].asRarr  |].asRarr |> ignore)


    testCase "ResizeArray.Truncate" <| fun _ ->
        // integer array
        Assert.AreEqual([|1..3 |].asRarr, ResizeArray.truncate 3 [|1..5 |].asRarr)
        Assert.AreEqual([|1..5 |].asRarr, ResizeArray.truncate 10 [|1..5 |].asRarr)
        Assert.AreEqual([|  |].asRarr, ResizeArray.truncate 0 [|1..5 |].asRarr)

        // string array
        Assert.AreEqual([|"str1";"str2" |].asRarr, ResizeArray.truncate 2 [|"str1";"str2";"str3" |].asRarr)

        // empty array
        Assert.AreEqual([|  |].asRarr, ResizeArray.truncate 0 [|  |].asRarr)
        Assert.AreEqual([|  |].asRarr, ResizeArray.truncate 1 [|  |].asRarr)

        // null array
        throwsNull(fun() -> ResizeArray.truncate 1 null |> ignore)

        // negative count
        Assert.AreEqual([|  |].asRarr, ResizeArray.truncate -1 [|1..5 |].asRarr)
        Assert.AreEqual([|  |].asRarr, ResizeArray.truncate System.Int32.MinValue [|1..5 |].asRarr)

        ()


    testCase "ResizeArray.TryFind" <| fun _ ->
        // integer array
        let resultInt = [|1..10 |].asRarr |> ResizeArray.tryFind (fun x -> x%7 = 0)
        if resultInt <> Some 7 then Assert.Fail()

        // string array
        let resultStr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr |> ResizeArray.tryFind (fun (x:string) -> x.Length > 4)
        if resultStr <> Some "Lists" then Assert.Fail()

        // empty array
        let resultEpt =[| |].asRarr |> ResizeArray.tryFind  (fun x -> x%7 = 0)
        if resultEpt <> None  then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.tryFind (fun (x:string) -> x.Length > 4)  nullArr  |> ignore)

        ()


    testCase "ResizeArray.TryFindBack" <| fun _ ->
        // integer array
        let funcInt x = x%5 = 0
        Assert.AreEqual(Some 20, [| 1..20  |].asRarr |> ResizeArray.tryFindBack funcInt)
        Assert.AreEqual(Some 15, [| 1..19  |].asRarr |> ResizeArray.tryFindBack funcInt)
        Assert.AreEqual(Some 5, [| 5..9  |].asRarr |> ResizeArray.tryFindBack funcInt)

        // string array
        let resultStr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr |> ResizeArray.tryFindBack (fun (x:string) -> x.Length > 4)
        Assert.AreEqual(Some "commonly", resultStr)

        // empty array
        Assert.AreEqual(None, [|  |].asRarr |> ResizeArray.tryFindBack (fun _ -> failwith "error"))

        // not found
        Assert.AreEqual(None, [| 1..20  |].asRarr |> ResizeArray.tryFindBack (fun _ -> false))

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.tryFindBack (fun _ -> failwith "error") nullArr |> ignore)

        ()


    testCase "ResizeArray.TryFindIndex" <| fun _ ->
        // integer array
        let resultInt = [|1..10 |].asRarr |> ResizeArray.tryFindIndex (fun x -> x%7 = 0)
        if resultInt <> Some 6 then Assert.Fail()

        // string array
        let resultStr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr |> ResizeArray.tryFindIndex (fun (x:string) -> x.Length > 4)
        if resultStr <> Some 0 then Assert.Fail()

        // empty array
        let resultEpt =[| |].asRarr |> ResizeArray.tryFindIndex  (fun x -> x % 7 = 0)
        if resultEpt <> None  then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.tryFindIndex (fun (x:string) -> x.Length > 4)  nullArr  |> ignore)

        ()


    testCase "ResizeArray.TryFindIndexBack" <| fun _ ->
        // integer array
        let funcInt x = x%5 = 0
        Assert.AreEqual(Some 19, [| 1..20  |].asRarr |> ResizeArray.tryFindIndexBack funcInt)
        Assert.AreEqual(Some 14, [| 1..19  |].asRarr |> ResizeArray.tryFindIndexBack funcInt)
        Assert.AreEqual(Some 0, [| 5..9  |].asRarr |> ResizeArray.tryFindIndexBack funcInt)

        // string array
        let resultStr = [|"Lists"; "are";  "commonly" ; "list"  |].asRarr |> ResizeArray.tryFindIndexBack (fun (x:string) -> x.Length > 4)
        Assert.AreEqual(Some 2, resultStr)

        // empty array
        Assert.AreEqual(None, [|  |].asRarr |> ResizeArray.tryFindIndexBack (fun _ -> true))

        // not found
        Assert.AreEqual(None, [| 1..20  |].asRarr |> ResizeArray.tryFindIndexBack (fun _ -> false))

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.tryFindIndexBack (fun (x:string) -> x.Length > 4) nullArr |> ignore)

        ()


    testCase "ResizeArray.Unfold" <| fun _ ->
        // integer Seq
        let resultInt = ResizeArray.unfold (fun x -> if x < 20 then Some (x+1,x*2) else None) 1
        Assert.AreEqual([|2;3;5;9;17 |].asRarr, resultInt)

        // string Seq
        let resultStr = ResizeArray.unfold (fun (x:string) -> if x.Contains("unfold") then Some("a","b") else None) "unfold"
        Assert.AreEqual([|"a" |].asRarr, resultStr)

        // empty seq
        let resultEpt = ResizeArray.unfold (fun _ -> None) 1
        Assert.AreEqual([|  |].asRarr, resultEpt)

        ()


    testCase "ResizeArray.Unzip" <| fun _ ->
        // integer array
        let resultInt =  ResizeArray.unzip [|(1,2);(2,4);(3,6) |].asRarr
        if resultInt <!!>  ([|1..3 |].asRarr, [|2..2..6 |].asRarr) then Assert.Fail()

        // string array
        let resultStr = ResizeArray.unzip [|("A","a");("B","b");("C","c");("D","d") |].asRarr
        // let str = resultStr.ToString()
        if resultStr <!!> ([|"A"; "B";  "C" ; "D"  |].asRarr,[|"a";"b";"c";"d" |].asRarr) then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.unzip  [| |].asRarr
        if resultEpt <!!> ([| |].asRarr,[| |].asRarr)  then Assert.Fail()

        // null array

        ()


    testCase "ResizeArray.Unzip3" <| fun _ ->
        // integer array
        let resultInt =  ResizeArray.unzip3 [|(1,2,3);(2,4,6);(3,6,9) |].asRarr
        if resultInt <!!!> ([|1;2;3 |].asRarr, [|2;4;6 |].asRarr, [|3;6;9 |].asRarr) then Assert.Fail()

        // string array
        let resultStr = ResizeArray.unzip3 [|("A","1","a");("B","2","b");("C","3","c");("D","4","d") |].asRarr
        if resultStr <!!!> ([|"A"; "B";  "C" ; "D"  |].asRarr, [|"1";"2";"3";"4" |].asRarr, [|"a"; "b"; "c"; "d" |].asRarr) then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.unzip3  [| |].asRarr
        if resultEpt <!!!>  ([| |].asRarr, [| |].asRarr, [| |].asRarr) then Assert.Fail()

        // null array

        ()

    testCase "ResizeArray.Windowed" <| fun _ ->
        let testWindowed config =
            try
                config.InputArray
                |> ResizeArray.windowed config.WindowSize
                |> (fun actual -> Assert.True(ResizeArray.equals2 config.ExpectedArray  actual))
            with
            | e when Option.isNone config.Exception -> raise e
            #if FABLE_COMPILER
            | _ -> ()
            #else
            | e when e.GetType() = (Option.get config.Exception) -> ()
            | _ -> Assert.Fail()
            #endif

        {
          InputArray = [|1..10 |].asRarr
          WindowSize = 1
          ExpectedArray =  [| for i in 1..10 do yield [| i  |].asRarr  |].asRarr
          Exception = None
        } |> testWindowed
        {
          InputArray = [|1..10 |].asRarr
          WindowSize = 5
          ExpectedArray =  [| for i in 1..6 do yield [| i; i+1; i+2; i+3; i+4  |].asRarr  |].asRarr
          Exception = None
        } |> testWindowed
        {
          InputArray = [|1..10 |].asRarr
          WindowSize = 10
          ExpectedArray =  [| yield [| 1 .. 10  |].asRarr  |].asRarr
          Exception = None
        } |> testWindowed
        {
          InputArray = [|1..10 |].asRarr
          WindowSize = 25
          ExpectedArray = [|  |].asRarr
          Exception = None
        } |> testWindowed
        {
          InputArray = [|"str1";"str2";"str3";"str4" |].asRarr
          WindowSize = 2
          ExpectedArray =  [| [|"str1";"str2" |].asRarr; [|"str2";"str3" |].asRarr; [|"str3";"str4" |].asRarr  |].asRarr
          Exception = None
        } |> testWindowed
        {
          InputArray = [|  |].asRarr
          WindowSize = 2
          ExpectedArray = [|  |].asRarr
          Exception = None
        } |> testWindowed
        {
         InputArray = null
         WindowSize = 2
         ExpectedArray = [|  |].asRarr
         Exception = Some typeof<ArgumentNullException>
        } |> testWindowed
        {
          InputArray = [|1..10 |].asRarr
          WindowSize = 0
          ExpectedArray =  [|  |].asRarr
          Exception = Some typeof<ArgumentException>
        } |> testWindowed

        // expectedArrays indexed by arraySize,windowSize
        #if FABLE_COMPILER
        #else
        let expectedArrays = Array2D.zeroCreate 6 6
        expectedArrays.[1,1] <- [| [|1 |].asRarr  |].asRarr
        expectedArrays.[2,1] <- [| [|1 |].asRarr; [|2 |].asRarr  |].asRarr
        expectedArrays.[2,2] <- [| [|1; 2 |].asRarr  |].asRarr
        expectedArrays.[3,1] <- [| [|1 |].asRarr; [|2 |].asRarr; [|3 |].asRarr  |].asRarr
        expectedArrays.[3,2] <- [| [|1; 2 |].asRarr; [|2; 3 |].asRarr  |].asRarr
        expectedArrays.[3,3] <- [| [|1; 2; 3 |].asRarr  |].asRarr
        expectedArrays.[4,1] <- [| [|1 |].asRarr; [|2 |].asRarr; [|3 |].asRarr; [|4 |].asRarr  |].asRarr
        expectedArrays.[4,2] <- [| [|1; 2 |].asRarr; [|2; 3 |].asRarr; [|3; 4 |].asRarr  |].asRarr
        expectedArrays.[4,3] <- [| [|1; 2; 3 |].asRarr; [|2; 3; 4 |].asRarr  |].asRarr
        expectedArrays.[4,4] <- [| [|1; 2; 3; 4 |].asRarr  |].asRarr
        expectedArrays.[5,1] <- [| [|1 |].asRarr; [|2 |].asRarr; [|3 |].asRarr; [|4 |].asRarr; [|5 |].asRarr  |].asRarr
        expectedArrays.[5,2] <- [| [|1; 2 |].asRarr; [|2; 3 |].asRarr; [|3; 4 |].asRarr; [|4; 5 |].asRarr  |].asRarr
        expectedArrays.[5,3] <- [| [|1; 2; 3 |].asRarr; [|2; 3; 4 |].asRarr; [|3; 4; 5 |].asRarr  |].asRarr
        expectedArrays.[5,4] <- [| [|1; 2; 3; 4 |].asRarr; [|2; 3; 4; 5 |].asRarr  |].asRarr
        expectedArrays.[5,5] <- [| [|1; 2; 3; 4; 5 |].asRarr  |].asRarr

        for arraySize = 0 to 5 do
            for windowSize = -1 to 5 do
                if windowSize <= 0 then
                    throwsArg (fun () -> ResizeArray.windowed windowSize [|1..arraySize |].asRarr |> ignore)
                elif arraySize < windowSize then
                    Assert.True(([| |].asRarr) =+= ( ResizeArray.windowed windowSize [|1..arraySize |].asRarr))
                else
                    Assert.True(( expectedArrays.[arraySize, windowSize]) =+= (ResizeArray.windowed windowSize [|1..arraySize |].asRarr))
        #endif
        ()



    testCase "ResizeArray.Zero_Create" <| fun _ ->
        (*
        // Check for bogus input
        throwsRange(fun () -> ResizeArray.zeroCreate -1 |> ignore)

        // integer array
        let resultInt =  ResizeArray.zeroCreate 8
        if resultInt <> [|0;0;0;0;0;0;0;0 |].asRarr then Assert.Fail()

        // string array
        let resultStr = ResizeArray.zeroCreate 3
        if resultStr <> [|null;null;null |].asRarr then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.zeroCreate  0
        if resultEpt <> [| |].asRarr  then Assert.Fail()
        *)

        ()


    testCase "ResizeArray.BadCreateArguments" <| fun _ ->
        // negative number
        throwsArg (fun () -> ResizeArray.create -1 0 |> ignore)


    testCase "ResizeArray.Zip" <| fun _ ->
        // integer array
        let resultInt =  ResizeArray.zip [|1..3 |].asRarr [|2..2..6 |].asRarr
        if resultInt <!> [|(1,2);(2,4);(3,6)|].asRarr then Assert.Fail()

        // string array
        let resultStr = ResizeArray.zip [|"A"; "B";  "C" ; "D"  |].asRarr [|"a";"b";"c";"d" |].asRarr
        if resultStr <!> [|("A","a");("B","b");("C","c");("D","d") |].asRarr then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.zip  [| |].asRarr [| |].asRarr
        if resultEpt <!> [| |].asRarr  then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.zip nullArr   nullArr  |> ignore)

        // len1 <!> len2
        throwsArg(fun () -> ResizeArray.zip [|1..10 |].asRarr [|2..20 |].asRarr |> ignore)

        ()


    testCase "ResizeArray.Zip3" <| fun _ ->
        // integer array
        let resultInt =  ResizeArray.zip3 [|1..3 |].asRarr [|2..2..6 |].asRarr [|3;6;9 |].asRarr
        if resultInt <!> [|(1,2,3);(2,4,6);(3,6,9) |].asRarr then Assert.Fail()

        // string array
        let resultStr = ResizeArray.zip3 [|"A"; "B";  "C" ; "D"  |].asRarr  [|"1";"2";"3";"4" |].asRarr  [|"a"; "b"; "c"; "d" |].asRarr
        // let str = resultStr.ToString()
        if resultStr <!> [|("A","1","a");("B","2","b");("C","3","c");("D","4","d") |].asRarr then Assert.Fail()

        // empty array
        let resultEpt = ResizeArray.zip3  [| |].asRarr [| |].asRarr [| |].asRarr
        if resultEpt <!> [| |].asRarr  then Assert.Fail()

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.zip3 nullArr  nullArr  nullArr  |> ignore)

        // len1 <> len2
        throwsArg(fun () -> ResizeArray.zip3 [|1..10 |].asRarr [|2..20 |].asRarr [|1..10 |].asRarr |> ignore)
        // len1 <> len3
        throwsArg(fun () -> ResizeArray.zip3 [|1..10 |].asRarr [|1..10 |].asRarr [|2..20 |].asRarr |> ignore)

        ()


    testCase "ResizeArray.Item" <| fun _ ->
        // integer array
        let resultInt = ResizeArray.item 3 [|1..8 |].asRarr
        Assert.AreEqual(4, resultInt)

        // string array
        let resultStr = ResizeArray.item 2 [|"Arrays"; "are"; "commonly"; "array"  |].asRarr
        Assert.AreEqual("commonly", resultStr)

        // empty array
        throwsIdx(fun () -> ResizeArray.item 0 ([|  |].asRarr : decimal ResizeArray) |> ignore)

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.item 0 nullArr |> ignore)

        // Negative index
        for i = -1 downto -10 do
            throwsIdx (fun () -> ResizeArray.item i [|1..8 |].asRarr |> ignore)

        // Out of range
        for i = 11 to 20 do
           throwsIdx (fun () -> ResizeArray.item i [|1..8 |].asRarr |> ignore)


    testCase "ResizeArray.tryItem" <| fun _ ->
        // integer array
        let intArr = [| 3;4;7;8;10  |].asRarr
        let resultInt = ResizeArray.tryItem 3 intArr
        Assert.AreEqual(Some(8), resultInt)

        // string array
        let strArr = [| "Lists"; "are"; "commonly"; "list"  |].asRarr
        let resultStr = ResizeArray.tryItem 1 strArr
        Assert.AreEqual(Some("are"), resultStr)

        // empty array
        let emptyArr:int ResizeArray = [|  |].asRarr
        let resultEmpty = ResizeArray.tryItem 1 emptyArr
        Assert.AreEqual(None, resultEmpty)

        // null array
        let nullArr = null:ResizeArray<string>
        throwsNull (fun () -> ResizeArray.tryItem 0 nullArr |> ignore)

        // Negative index
        let resultNegativeIndex = ResizeArray.tryItem -1 [| 3;1;6;2  |].asRarr
        Assert.AreEqual(None, resultNegativeIndex)

        // Index greater than length
        let resultIndexGreater = ResizeArray.tryItem 14 [| 3;1;6;2  |].asRarr
        Assert.AreEqual(None, resultIndexGreater)




    testCase "ResizeArray.RemoveAt" <| fun _ ->
        // integer list
        Assert.AreEqual([|2; 3; 4; 5|].asRarr , (ResizeArray.removeAt 0 [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 4; 5|].asRarr , (ResizeArray.removeAt 2 [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 3; 4|].asRarr , (ResizeArray.removeAt 4 [|1..5|].asRarr ))

        //string list
        Assert.AreEqual([|"2"; "3"; "4"; "5"|].asRarr , (ResizeArray.removeAt 0 [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "4"; "5"|].asRarr , (ResizeArray.removeAt 2 [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "3"; "4"|].asRarr , (ResizeArray.removeAt 4 [|"1"; "2"; "3"; "4"; "5"|].asRarr ))

        // empty list & out of bounds
        throwsArg (fun () -> ResizeArray.removeAt 0 [||].asRarr  |> ignore)
        throwsArg (fun () -> ResizeArray.removeAt -1 [|1|].asRarr  |> ignore)
        throwsArg (fun () -> ResizeArray.removeAt 2 [|1|].asRarr  |> ignore)


    testCase "ResizeArray.RemoveManyAt" <| fun _ ->
        // integer list
        Assert.AreEqual([|3; 4; 5|].asRarr , (ResizeArray.removeManyAt 0 2 [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 5|].asRarr , (ResizeArray.removeManyAt 2 2 [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 3|].asRarr , (ResizeArray.removeManyAt 3 2 [|1..5|].asRarr ))

        //string list
        Assert.AreEqual([|"3"; "4"; "5"|].asRarr , (ResizeArray.removeManyAt 0 2 [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "5"|].asRarr , (ResizeArray.removeManyAt 2 2 [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "3"|].asRarr , (ResizeArray.removeManyAt 3 2 [|"1"; "2"; "3"; "4"; "5"|].asRarr ))

        // empty list & out of bounds
        throwsArg (fun () -> ResizeArray.removeManyAt 0 2 [||].asRarr  |> ignore)
        throwsArg (fun () -> ResizeArray.removeManyAt -1 2 [|1|].asRarr  |> ignore)
        throwsArg (fun () -> ResizeArray.removeManyAt 2 2 [|1|].asRarr  |> ignore)


    testCase "ResizeArray.UpdateAt" <| fun _ ->
        // integer list
        Assert.AreEqual([|0; 2; 3; 4; 5|].asRarr , (ResizeArray.updateAt 0 0 [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 0; 4; 5|].asRarr , (ResizeArray.updateAt 2 0 [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 3; 4; 0|].asRarr , (ResizeArray.updateAt 4 0 [|1..5|].asRarr ))

        //string list
        Assert.AreEqual([|"0"; "2"; "3"; "4"; "5"|].asRarr , (ResizeArray.updateAt 0 "0" [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "0"; "4"; "5"|].asRarr , (ResizeArray.updateAt 2 "0" [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "3"; "4"; "0"|].asRarr , (ResizeArray.updateAt 4 "0" [|"1"; "2"; "3"; "4"; "5"|].asRarr ))

        // empty list & out of bounds
        throwsIdx (fun () -> ResizeArray.updateAt 0 0 [||].asRarr  |> ignore)
        throwsIdx (fun () -> ResizeArray.updateAt -1 0 [|1|].asRarr  |> ignore)
        throwsIdx (fun () -> ResizeArray.updateAt 2 0 [|1|].asRarr  |> ignore)


    testCase "ResizeArray.InsertAt" <| fun _ ->
        // integer list
        Assert.AreEqual([|0; 1; 2; 3; 4; 5|].asRarr , (ResizeArray.insertAt 0 0 [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 0; 3; 4; 5|].asRarr , (ResizeArray.insertAt 2 0 [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 3; 4; 0; 5|].asRarr , (ResizeArray.insertAt 4 0 [|1..5|].asRarr ))

        //string list
        Assert.AreEqual([|"0"; "1"; "2"; "3"; "4"; "5"|].asRarr , (ResizeArray.insertAt 0 "0" [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "0"; "3"; "4"; "5"|].asRarr , (ResizeArray.insertAt 2 "0" [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "3"; "4"; "0"; "5"|].asRarr , (ResizeArray.insertAt 4 "0" [|"1"; "2"; "3"; "4"; "5"|].asRarr ))

        // empty list & out of bounds
        Assert.AreEqual([|0|].asRarr, ResizeArray.insertAt 0 0 [||].asRarr )
        throwsArg (fun () -> ResizeArray.insertAt -1 0 [|1|].asRarr  |> ignore)
        throwsArg (fun () -> ResizeArray.insertAt 2 0 [|1|].asRarr  |> ignore)


    testCase "ResizeArray.InsertManyAt" <| fun _ ->
        // integer list
        Assert.AreEqual([|0; 0; 1; 2; 3; 4; 5|].asRarr , (ResizeArray.insertManyAt 0 [|0; 0|] [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 0; 0; 3; 4; 5|].asRarr , (ResizeArray.insertManyAt 2 [|0; 0|] [|1..5|].asRarr ))
        Assert.AreEqual([|1; 2; 3; 4; 0; 0; 5|].asRarr , (ResizeArray.insertManyAt 4 [|0; 0|] [|1..5|].asRarr ))

        //string list
        Assert.AreEqual([|"0"; "0"; "1"; "2"; "3"; "4"; "5"|].asRarr , (ResizeArray.insertManyAt 0 [|"0"; "0"|] [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "0"; "0"; "3"; "4"; "5"|].asRarr , (ResizeArray.insertManyAt 2 [|"0"; "0"|] [|"1"; "2"; "3"; "4"; "5"|].asRarr ))
        Assert.AreEqual([|"1"; "2"; "3"; "4"; "0"; "0"; "5"|].asRarr , (ResizeArray.insertManyAt 4 [|"0"; "0"|] [|"1"; "2"; "3"; "4"; "5"|].asRarr ))

        // empty list & out of bounds
        Assert.AreEqual( [|0; 0|].asRarr, ResizeArray.insertManyAt 0 [|0; 0|] [||].asRarr )
        throwsArg (fun () -> ResizeArray.insertManyAt -1 [|0; 0|] [|1|].asRarr  |> ignore)
        throwsArg (fun () -> ResizeArray.insertManyAt  2 [|0; 0|] [|1|].asRarr  |> ignore)


    //--------------------------------------------------------------------------------------------------------------------
    //------------------------------------------tests vor custom functions-------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------


    testCase "ResizeArray.rotate" <| fun _ ->
        let xs = [|0; 1; 2; 3; 4; 5|].asRarr
        Assert.AreEqual(xs |> ResizeArray.rotate  2 , [|4; 5; 0; 1; 2; 3 |].asRarr)
        Assert.AreEqual(xs |> ResizeArray.rotate  1 , [|5; 0; 1; 2; 3; 4 |].asRarr)
        Assert.AreEqual(xs |> ResizeArray.rotate -2 , [|2; 3; 4; 5; 0; 1|].asRarr)
        Assert.AreEqual(xs |> ResizeArray.rotate -1 , [|1; 2; 3; 4; 5; 0|].asRarr)
        Assert.AreEqual(xs |> ResizeArray.rotate -6 , xs)
        Assert.AreEqual(xs |> ResizeArray.rotate  12 , xs)
        Assert.AreEqual(xs |> ResizeArray.rotate -12 , xs)
        Assert.AreEqual(xs |> ResizeArray.rotate -13 , xs|> ResizeArray.rotate -1)
        Assert.AreEqual(xs |> ResizeArray.rotate  13 , xs|> ResizeArray.rotate  1)


    testCase "ResizeArray.rotateDownTill" <| fun _ ->
        let xs = [|0; 7; 2; 3; 7; 5|].asRarr
        Assert.AreEqual(xs |> ResizeArray.rotateDownTill(fun i -> i = 7)     , [|7; 2; 3; 7; 5; 0 |].asRarr)
        throwsArg (fun () -> xs |> ResizeArray.rotateDownTill (fun i -> i = 99) |> ignore  )



    testCase "ResizeArray.rotateDownTillLast" <| fun _ ->
        let xs = [|0; 7; 2; 3; 7; 5|].asRarr
        Assert.AreEqual(xs |> ResizeArray.rotateDownTillLast(fun i -> i = 7) , [|2; 3; 7; 5; 0; 7 |].asRarr)
        throwsArg (fun () -> xs |> ResizeArray.rotateDownTillLast (fun i -> i = 99) |> ignore  )



    testCase "ResizeArray.rotateUpTill" <| fun _ ->
        let xs = [|0; 7; 2; 3; 7; 5|].asRarr
        Assert.AreEqual(xs |> ResizeArray.rotateUpTill(fun i -> i = 7)       , [|7; 5; 0; 7; 2; 3 |].asRarr)
        throwsArg (fun () -> xs |> ResizeArray.rotateUpTill (fun i -> i = 99) |> ignore  )



    testCase "ResizeArray.rotateUpTillLast" <| fun _ ->
        let xs = [|0; 7; 2; 3; 7; 5|].asRarr
        Assert.AreEqual(xs |> ResizeArray.rotateUpTillLast(fun i -> i = 7)   , [|5; 0; 7; 2; 3; 7 |].asRarr)
        throwsArg (fun () -> xs |> ResizeArray.rotateUpTillLast (fun i -> i = 99) |> ignore  )


    testCase "ResizeArray.filteri " <| fun _ ->
        let arr = [|'a';'b';'c'|].asRarr
        let result = arr|> ResizeArray.filteri (fun i _ -> i % 2 = 0)
        Assert.AreEqual([|'a';'c'|].asRarr , result)
        Assert.AreNotEqual(arr, result,"input array should not be modified")

    testCase "slice2" <| fun _ ->
        let arr = [|0;1;2;3;4;5;6;7;8;9;10|].asRarr
        Assert.AreEqual([|1;2;3;4;5|].asRarr, arr |> ResizeArray.sliceIdx 1 5)
        Assert.AreEqual([|1;2;3;4;5;6;7;8;9;10|].asRarr, arr |> ResizeArray.sliceIdx 1 10)
        Assert.AreEqual([|8;9;10|].asRarr, arr |> ResizeArray.sliceLooped -3 -1)
        throwsIdx ( fun () -> arr |> ResizeArray.sliceIdx -3 -1  |> ignore<_>)

    testCase "slice" <| fun _ ->
        let arr = [|0;1;2;3;4;5;6;7;8;9;10|].asRarr
        Assert.AreEqual([|1;2;3;4;5|].asRarr, arr.[1 .. 5])
        Assert.AreEqual([|1;2;3;4;5;6;7;8;9;10|].asRarr, arr.[1 .. 10])
        Assert.AreEqual([|1;2;3;4;5;6;7;8;9;10|].asRarr, arr.[1 .. ])
        Assert.AreEqual([|0;1;2;3;4;5;6;7;8;9|].asRarr, arr.[ .. 9])
        Assert.AreEqual([|8;9;10|].asRarr, arr.[^2 .. ^0])



  ]