module Tests.FableCompat

// Tests for functions that have FABLE_COMPILER_JAVASCRIPT compiler directives.
// These tests ensure the JS and .NET runtimes behave the same way.

open ResizeArrayT

#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
open Fable.Mocha
#else
open Expecto
#endif

open System
open System.Collections.Generic
open Exceptions
open ExtensionOnArray

// --- Array.asResizeArray tests ---

let asResizeArrayTests =
    testList "Array.asResizeArray" [

        testCase "converts string array to ResizeArray" <| fun _ ->
            let arr = [| "a"; "b"; "c" |]
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 3 "count"
            Expect.equal ra.[0] "a" "first"
            Expect.equal ra.[1] "b" "second"
            Expect.equal ra.[2] "c" "third"

        testCase "converts single-element string array" <| fun _ ->
            let arr = [| "only" |]
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 1 "count"
            Expect.equal ra.[0] "only" "element"

        testCase "converts empty string array" <| fun _ ->
            let arr : string[] = [||]
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 0 "empty"

        testCase "preserves null elements in string array" <| fun _ ->
            let arr = [| "a"; null; "c" |]
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 3 "count"
            Expect.equal ra.[0] "a" "first"
            Expect.isTrue (isNull ra.[1]) "null element"
            Expect.equal ra.[2] "c" "third"

        testCase "preserves duplicate elements" <| fun _ ->
            let arr = [| "x"; "x"; "y"; "x" |]
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 4 "count"
            Expect.equal ra.[0] "x" "0"
            Expect.equal ra.[1] "x" "1"
            Expect.equal ra.[2] "y" "2"
            Expect.equal ra.[3] "x" "3"

        testCase "works with obj array" <| fun _ ->
            let arr : obj[] = [| box 1; box "hello"; box 3.14 |]
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 3 "count"
            Expect.equal (unbox<int> ra.[0]) 1 "int"
            Expect.equal (unbox<string> ra.[1]) "hello" "string"

        testCase "throws on null array" <| fun _ ->
            let nullArr : string[] = null
            throwsNull (fun () -> Array.asResizeArray nullArr |> ignore)

        testCase "large string array" <| fun _ ->
            let arr = Array.init 1000 (fun i -> $"item{i}")
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 1000 "count"
            Expect.equal ra.[0] "item0" "first"
            Expect.equal ra.[999] "item999" "last"

        testCase "all null elements" <| fun _ ->
            let arr : string[] = [| null; null; null |]
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 3 "count"
            Expect.isTrue (isNull ra.[0]) "0"
            Expect.isTrue (isNull ra.[1]) "1"
            Expect.isTrue (isNull ra.[2]) "2"

        testCase "works with record type array" <| fun _ ->
            // Using a tuple of strings as a reference-type proxy
            let arr = [| ("a","b"); ("c","d") |]
            let ra = Array.asResizeArray arr
            Expect.equal ra.Count 2 "count"
            Expect.equal ra.[0] ("a","b") "first"
            Expect.equal ra.[1] ("c","d") "second"
    ]


// --- ResizeArray.asArray tests ---

let asArrayTests =
    testList "ResizeArray.asArray" [

        testCase "converts string ResizeArray to array" <| fun _ ->
            let ra = ResizeArray([ "a"; "b"; "c" ])
            let arr = ResizeArray.asArray ra
            Expect.equal arr.Length 3 "length"
            Expect.equal arr.[0] "a" "first"
            Expect.equal arr.[1] "b" "second"
            Expect.equal arr.[2] "c" "third"

        testCase "converts single-element ResizeArray" <| fun _ ->
            let ra = ResizeArray([ "only" ])
            let arr = ResizeArray.asArray ra
            Expect.equal arr.Length 1 "length"
            Expect.equal arr.[0] "only" "element"

        testCase "converts empty ResizeArray" <| fun _ ->
            let ra = ResizeArray<string>()
            let arr = ResizeArray.asArray ra
            Expect.equal arr.Length 0 "empty"

        testCase "preserves null elements" <| fun _ ->
            let ra = ResizeArray([ "a"; null; "c" ])
            let arr = ResizeArray.asArray ra
            Expect.equal arr.Length 3 "length"
            Expect.equal arr.[0] "a" "first"
            Expect.isTrue (isNull arr.[1]) "null element"
            Expect.equal arr.[2] "c" "third"

        testCase "preserves duplicate elements" <| fun _ ->
            let ra = ResizeArray([ "x"; "x"; "y"; "x" ])
            let arr = ResizeArray.asArray ra
            Expect.equal arr.Length 4 "length"
            Expect.equal arr.[0] "x" "0"
            Expect.equal arr.[1] "x" "1"
            Expect.equal arr.[2] "y" "2"
            Expect.equal arr.[3] "x" "3"

        testCase "works with obj ResizeArray" <| fun _ ->
            let ra = ResizeArray<obj>([ box 1; box "hello"; box 3.14 ])
            let arr = ResizeArray.asArray ra
            Expect.equal arr.Length 3 "length"
            Expect.equal (unbox<int> arr.[0]) 1 "int"
            Expect.equal (unbox<string> arr.[1]) "hello" "string"

        testCase "throws on null ResizeArray" <| fun _ ->
            let nullArr : ResizeArray<string> = null
            throwsNull (fun () -> ResizeArray.asArray nullArr |> ignore)

        testCase "large ResizeArray" <| fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> $"item{i}" })
            let arr = ResizeArray.asArray ra
            Expect.equal arr.Length 1000 "length"
            Expect.equal arr.[0] "item0" "first"
            Expect.equal arr.[999] "item999" "last"

        testCase "all null elements" <| fun _ ->
            let ra = ResizeArray<string>([ null; null; null ])
            let arr = ResizeArray.asArray ra
            Expect.equal arr.Length 3 "length"
            Expect.isTrue (isNull arr.[0]) "0"
            Expect.isTrue (isNull arr.[1]) "1"
            Expect.isTrue (isNull arr.[2]) "2"
    ]


// --- ResizeArray.map tests ---

let mapTests =
    testList "ResizeArray.map" [

        testCase "maps int to int" <| fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map (fun x -> x * 2) ra
            Expect.isTrue (result == [| 2; 4; 6 |].asRarr) "doubled"

        testCase "maps int to string" <| fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map (fun x -> $"v{x}") ra
            Expect.isTrue (result == [| "v1"; "v2"; "v3" |].asRarr) "to string"

        testCase "maps string to int (length)" <| fun _ ->
            let ra = [| "a"; "bb"; "ccc" |].asRarr
            let result = ResizeArray.map (fun (s:string) -> s.Length) ra
            Expect.isTrue (result == [| 1; 2; 3 |].asRarr) "lengths"

        testCase "maps string to string" <| fun _ ->
            let ra = [| "hello"; "world" |].asRarr
            let result = ResizeArray.map (fun (s:string) -> s.ToUpper()) ra
            Expect.isTrue (result == [| "HELLO"; "WORLD" |].asRarr) "upper"

        testCase "maps empty ResizeArray" <| fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.map (fun x -> x * 2) ra
            Expect.equal result.Count 0 "empty result"

        testCase "maps single element" <| fun _ ->
            let ra = [| 42 |].asRarr
            let result = ResizeArray.map (fun x -> x + 1) ra
            Expect.equal result.Count 1 "count"
            Expect.equal result.[0] 43 "value"

        testCase "throws on null" <| fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.map (fun x -> x) nullArr |> ignore)

        testCase "maps float to float" <| fun _ ->
            let ra = [| 1.5; 2.5; 3.5 |].asRarr
            let result = ResizeArray.map (fun x -> x * 2.0) ra
            Expect.isTrue (result == [| 3.0; 5.0; 7.0 |].asRarr) "doubled floats"

        testCase "maps with identity" <| fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map id ra
            Expect.isTrue (result == ra) "identity"

        testCase "result is independent of source" <| fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map (fun x -> x * 2) ra
            ra.[0] <- 99
            Expect.equal result.[0] 2 "result unchanged after source mutation"

        testCase "maps with index-dependent function" <| fun _ ->
            let ra = [| 10; 20; 30 |].asRarr
            let mutable idx = 0
            let result = ResizeArray.map (fun x -> let r = x + idx in idx <- idx + 1; r) ra
            Expect.equal result.[0] 10 "0"
            Expect.equal result.[1] 21 "1"
            Expect.equal result.[2] 32 "2"

        testCase "maps large ResizeArray" <| fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i })
            let result = ResizeArray.map (fun x -> x * x) ra
            Expect.equal result.Count 1000 "count"
            Expect.equal result.[0] 0 "first"
            Expect.equal result.[999] (999*999) "last"

        testCase "maps bool to bool" <| fun _ ->
            let ra = [| true; false; true |].asRarr
            let result = ResizeArray.map not ra
            Expect.isTrue (result == [| false; true; false |].asRarr) "negated"

        testCase "maps to option type" <| fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map (fun x -> if x % 2 = 0 then Some x else None) ra
            Expect.equal result.[0] None "odd"
            Expect.equal result.[1] (Some 2) "even"
            Expect.equal result.[2] None "odd2"

        testCase "maps with constant function" <| fun _ ->
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let result = ResizeArray.map (fun _ -> 0) ra
            Expect.equal result.Count 5 "count"
            Expect.isTrue (result == [| 0; 0; 0; 0; 0 |].asRarr) "all zeros"

        testCase "maps duplicates" <| fun _ ->
            let ra = [| 1; 1; 1 |].asRarr
            let result = ResizeArray.map (fun x -> x + 1) ra
            Expect.isTrue (result == [| 2; 2; 2 |].asRarr) "all twos"
    ]


// --- ResizeArray.forall tests ---

let forallTests =
    testList "ResizeArray.forall" [

        testCase "all satisfy predicate" <| fun _ ->
            let ra = [| 2; 4; 6; 8 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            Expect.isTrue result "all even"

        testCase "none satisfy predicate" <| fun _ ->
            let ra = [| 1; 3; 5; 7 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            Expect.isFalse result "none even"

        testCase "first fails predicate" <| fun _ ->
            let ra = [| 1; 2; 4; 6 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            Expect.isFalse result "first is odd"

        testCase "last fails predicate" <| fun _ ->
            let ra = [| 2; 4; 6; 7 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            Expect.isFalse result "last is odd"

        testCase "middle fails predicate" <| fun _ ->
            let ra = [| 2; 4; 5; 6 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            Expect.isFalse result "middle is odd"

        testCase "empty array returns true" <| fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.forall (fun x -> x > 0) ra
            Expect.isTrue result "empty is vacuously true"

        testCase "single element satisfies" <| fun _ ->
            let ra = [| 2 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            Expect.isTrue result "single even"

        testCase "single element fails" <| fun _ ->
            let ra = [| 1 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            Expect.isFalse result "single odd"

        testCase "throws on null" <| fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.forall (fun x -> x > 0) nullArr |> ignore)

        testCase "short-circuits on false" <| fun _ ->
            let mutable count = 0
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let _ = ResizeArray.forall (fun x -> count <- count + 1; x < 3) ra
            Expect.equal count 3 "should stop at third element"

        testCase "with string predicate" <| fun _ ->
            let ra = [| "abc"; "def"; "ghi" |].asRarr
            let result = ResizeArray.forall (fun (s:string) -> s.Length = 3) ra
            Expect.isTrue result "all length 3"

        testCase "with string predicate failing" <| fun _ ->
            let ra = [| "abc"; "de"; "ghi" |].asRarr
            let result = ResizeArray.forall (fun (s:string) -> s.Length = 3) ra
            Expect.isFalse result "not all length 3"

        testCase "with float predicate" <| fun _ ->
            let ra = [| 1.0; 2.0; 3.0 |].asRarr
            let result = ResizeArray.forall (fun x -> x > 0.0) ra
            Expect.isTrue result "all positive"

        testCase "with always true predicate" <| fun _ ->
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let result = ResizeArray.forall (fun _ -> true) ra
            Expect.isTrue result "always true"

        testCase "with always false predicate on non-empty" <| fun _ ->
            let ra = [| 1 |].asRarr
            let result = ResizeArray.forall (fun _ -> false) ra
            Expect.isFalse result "always false"

        testCase "with always false predicate on empty" <| fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.forall (fun _ -> false) ra
            Expect.isTrue result "vacuously true even with false predicate"

        testCase "large array all true" <| fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i })
            let result = ResizeArray.forall (fun x -> x >= 0) ra
            Expect.isTrue result "all non-negative"

        testCase "large array with single false at end" <| fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i })
            ra.[999] <- -1
            let result = ResizeArray.forall (fun x -> x >= 0) ra
            Expect.isFalse result "last element is negative"

        testCase "with duplicates all satisfying" <| fun _ ->
            let ra = [| 2; 2; 2; 2 |].asRarr
            let result = ResizeArray.forall (fun x -> x = 2) ra
            Expect.isTrue result "all twos"

        testCase "with bool values" <| fun _ ->
            let ra = [| true; true; true |].asRarr
            let result = ResizeArray.forall id ra
            Expect.isTrue result "all true"

        testCase "with bool values one false" <| fun _ ->
            let ra = [| true; false; true |].asRarr
            let result = ResizeArray.forall id ra
            Expect.isFalse result "one false"
    ]


// --- ResizeArray.countBy tests ---

let countByTests =
    testList "ResizeArray.countBy" [

        testCase "counts by int identity" <| fun _ ->
            let ra = [| 1; 2; 1; 3; 2; 1 |].asRarr
            let result = ResizeArray.countBy id ra
            // Order should match first occurrence
            Expect.equal result.Count 3 "3 unique keys"
            Expect.equal (result.[0]) (1, 3) "1 appears 3 times"
            Expect.equal (result.[1]) (2, 2) "2 appears 2 times"
            Expect.equal (result.[2]) (3, 1) "3 appears 1 time"

        testCase "counts by string identity" <| fun _ ->
            let ra = [| "a"; "b"; "a"; "c"; "b"; "a" |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 3 "3 unique keys"
            Expect.equal (result.[0]) ("a", 3) "a appears 3 times"
            Expect.equal (result.[1]) ("b", 2) "b appears 2 times"
            Expect.equal (result.[2]) ("c", 1) "c appears 1 time"

        testCase "counts by string length" <| fun _ ->
            let ra = [| "a"; "bb"; "c"; "ddd"; "ee" |].asRarr
            let result = ResizeArray.countBy (fun (s:string) -> s.Length) ra
            Expect.equal result.Count 3 "3 unique lengths"
            Expect.equal (result.[0]) (1, 2) "length 1: 2 items"
            Expect.equal (result.[1]) (2, 2) "length 2: 2 items"
            Expect.equal (result.[2]) (3, 1) "length 3: 1 item"

        testCase "counts by even/odd" <| fun _ ->
            let ra = [| 1; 2; 3; 4; 5; 6 |].asRarr
            let result = ResizeArray.countBy (fun x -> x % 2 = 0) ra
            Expect.equal result.Count 2 "2 groups"
            Expect.equal (result.[0]) (false, 3) "3 odd"
            Expect.equal (result.[1]) (true, 3) "3 even"

        testCase "empty array" <| fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 0 "empty"

        testCase "single element" <| fun _ ->
            let ra = [| 42 |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 1 "one group"
            Expect.equal (result.[0]) (42, 1) "42 once"

        testCase "all same value int" <| fun _ ->
            let ra = [| 5; 5; 5; 5 |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 1 "one group"
            Expect.equal (result.[0]) (5, 4) "5 four times"

        testCase "all same value string" <| fun _ ->
            let ra = [| "x"; "x"; "x" |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 1 "one group"
            Expect.equal (result.[0]) ("x", 3) "x three times"

        testCase "throws on null" <| fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.countBy id nullArr |> ignore)

        testCase "counts by float key" <| fun _ ->
            let ra = [| 1.0; 2.0; 1.0; 3.0 |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 3 "3 unique"
            Expect.equal (result.[0]) (1.0, 2) "1.0 twice"
            Expect.equal (result.[1]) (2.0, 1) "2.0 once"
            Expect.equal (result.[2]) (3.0, 1) "3.0 once"

        testCase "counts by bool key" <| fun _ ->
            let ra = [| true; false; true; true; false |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 2 "2 groups"
            Expect.equal (result.[0]) (true, 3) "true 3 times"
            Expect.equal (result.[1]) (false, 2) "false 2 times"

        testCase "counts by projection to string" <| fun _ ->
            let ra = [| 1; 2; 11; 22; 111 |].asRarr
            let result = ResizeArray.countBy (fun x -> (string x).Length) ra
            Expect.equal result.Count 3 "3 digit lengths"
            Expect.equal (result.[0]) (1, 2) "1-digit: 2"
            Expect.equal (result.[1]) (2, 2) "2-digit: 2"
            Expect.equal (result.[2]) (3, 1) "3-digit: 1"

        testCase "large array" <| fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i % 10 })
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 10 "10 unique values"
            for i = 0 to result.Count - 1 do
                Expect.equal (snd result.[i]) 100 $"each value appears 100 times"

        testCase "counts with null string key" <| fun _ ->
            let ra = [| "a"; null; "a"; null |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 2 "2 groups"
            Expect.equal (result.[0]) ("a", 2) "a twice"
            Expect.equal (result.[1]) (null, 2) "null twice"

        testCase "counts with option key None" <| fun _ ->
            let ra = [| Some 1; None; Some 1; None; Some 2 |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 3 "3 groups"
            Expect.equal (result.[0]) (Some 1, 2) "Some 1 twice"
            Expect.equal (result.[1]) (None, 2) "None twice"
            Expect.equal (result.[2]) (Some 2, 1) "Some 2 once"

        testCase "unique elements" <| fun _ ->
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let result = ResizeArray.countBy id ra
            Expect.equal result.Count 5 "5 unique"
            for i = 0 to result.Count - 1 do
                Expect.equal (snd result.[i]) 1 "each once"
    ]


// --- ResizeArray.groupBy tests ---

let groupByTests =
    testList "ResizeArray.groupBy" [

        testCase "groups by int identity" <| fun _ ->
            let ra = [| 1; 2; 1; 3; 2; 1 |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 3 "3 groups"
            let k0, v0 = result.[0]
            Expect.equal k0 1 "key 0"
            Expect.isTrue (v0 == [|1;1;1|].asRarr) "values 0"
            let k1, v1 = result.[1]
            Expect.equal k1 2 "key 1"
            Expect.isTrue (v1 == [|2;2|].asRarr) "values 1"
            let k2, v2 = result.[2]
            Expect.equal k2 3 "key 2"
            Expect.isTrue (v2 == [|3|].asRarr) "values 2"

        testCase "groups by string identity" <| fun _ ->
            let ra = [| "a"; "b"; "a"; "c" |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 3 "3 groups"
            let k0, v0 = result.[0]
            Expect.equal k0 "a" "key a"
            Expect.isTrue (v0 == [|"a";"a"|].asRarr) "values a"
            let k1, v1 = result.[1]
            Expect.equal k1 "b" "key b"
            Expect.isTrue (v1 == [|"b"|].asRarr) "values b"

        testCase "groups by string length" <| fun _ ->
            let ra = [| "a"; "bb"; "c"; "ddd"; "ee" |].asRarr
            let result = ResizeArray.groupBy (fun (s:string) -> s.Length) ra
            Expect.equal result.Count 3 "3 groups"
            let k0, v0 = result.[0]
            Expect.equal k0 1 "key 1"
            Expect.isTrue (v0 == [|"a";"c"|].asRarr) "len 1"
            let k1, v1 = result.[1]
            Expect.equal k1 2 "key 2"
            Expect.isTrue (v1 == [|"bb";"ee"|].asRarr) "len 2"
            let k2, v2 = result.[2]
            Expect.equal k2 3 "key 3"
            Expect.isTrue (v2 == [|"ddd"|].asRarr) "len 3"

        testCase "groups by even/odd" <| fun _ ->
            let ra = [| 1; 2; 3; 4; 5; 6 |].asRarr
            let result = ResizeArray.groupBy (fun x -> x % 2 = 0) ra
            Expect.equal result.Count 2 "2 groups"
            let k0, v0 = result.[0]
            Expect.equal k0 false "odd group"
            Expect.isTrue (v0 == [|1;3;5|].asRarr) "odd values"
            let k1, v1 = result.[1]
            Expect.equal k1 true "even group"
            Expect.isTrue (v1 == [|2;4;6|].asRarr) "even values"

        testCase "empty array" <| fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 0 "empty"

        testCase "single element" <| fun _ ->
            let ra = [| 42 |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 1 "one group"
            let k, v = result.[0]
            Expect.equal k 42 "key"
            Expect.isTrue (v == [|42|].asRarr) "value"

        testCase "all same value" <| fun _ ->
            let ra = [| 5; 5; 5; 5 |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 1 "one group"
            let k, v = result.[0]
            Expect.equal k 5 "key"
            Expect.isTrue (v == [|5;5;5;5|].asRarr) "values"

        testCase "throws on null" <| fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.groupBy id nullArr |> ignore)

        testCase "groups with null string key" <| fun _ ->
            let ra = [| "a"; null; "a"; null |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 2 "2 groups"
            let k0, v0 = result.[0]
            Expect.equal k0 "a" "key a"
            Expect.isTrue (v0 == [|"a";"a"|].asRarr) "values a"
            let k1, v1 = result.[1]
            Expect.isTrue (isNull k1) "key null"
            Expect.equal v1.Count 2 "null group count"

        testCase "groups with option key None" <| fun _ ->
            let ra = [| Some 1; None; Some 1; None; Some 2 |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 3 "3 groups"
            let k0, v0 = result.[0]
            Expect.equal k0 (Some 1) "key Some 1"
            Expect.equal v0.Count 2 "Some 1 count"
            let k1, v1 = result.[1]
            Expect.equal k1 None "key None"
            Expect.equal v1.Count 2 "None count"
            let k2, v2 = result.[2]
            Expect.equal k2 (Some 2) "key Some 2"
            Expect.equal v2.Count 1 "Some 2 count"

        testCase "groups preserve element order" <| fun _ ->
            let ra = [| 3; 1; 4; 1; 5; 9; 2; 6; 5; 3 |].asRarr
            let result = ResizeArray.groupBy id ra
            // first occurrence order: 3, 1, 4, 5, 9, 2, 6
            Expect.equal (fst result.[0]) 3 "first key is 3"
            Expect.equal (fst result.[1]) 1 "second key is 1"
            Expect.equal (fst result.[2]) 4 "third key is 4"

        testCase "groups by float key" <| fun _ ->
            let ra = [| 1.0; 2.0; 1.0; 3.0 |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 3 "3 groups"
            let k0, v0 = result.[0]
            Expect.equal k0 1.0 "key 1.0"
            Expect.isTrue (v0 == [|1.0;1.0|].asRarr) "values 1.0"

        testCase "groups by bool key" <| fun _ ->
            let ra = [| true; false; true; true; false |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 2 "2 groups"
            let k0, v0 = result.[0]
            Expect.equal k0 true "key true"
            Expect.equal v0.Count 3 "true count"
            let k1, v1 = result.[1]
            Expect.equal k1 false "key false"
            Expect.equal v1.Count 2 "false count"

        testCase "unique elements" <| fun _ ->
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 5 "5 groups"
            for i = 0 to result.Count - 1 do
                let _, v = result.[i]
                Expect.equal v.Count 1 "each group has 1 element"

        testCase "large array" <| fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i % 10 })
            let result = ResizeArray.groupBy id ra
            Expect.equal result.Count 10 "10 groups"
            for i = 0 to result.Count - 1 do
                let _, v = result.[i]
                Expect.equal v.Count 100 "each group has 100 elements"

        testCase "groups by projection to tuple" <| fun _ ->
            let ra = [| 1; 2; 3; 4; 5; 6 |].asRarr
            let result = ResizeArray.groupBy (fun x -> (x % 2, x % 3)) ra
            // (1,1)->1  (0,2)->2  (1,0)->3  (0,1)->4  (1,2)->5  (0,0)->6
            Expect.equal result.Count 6 "6 unique (mod2, mod3) pairs"
    ]


// --- Combined test list ---

let tests =
    testList "FableCompat Tests" [
        asResizeArrayTests
        asArrayTests
        mapTests
        forallTests
        countByTests
        groupByTests
    ]
