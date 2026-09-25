module Tests.FableCompat

// Tests for functions that have FABLE_COMPILER_JAVASCRIPT compiler directives.
// These tests ensure the JS and .NET runtimes behave the same way.

open ResizeArrayT

open Scriptorium.Nib.Assertion
open type Scriptorium.Quill.Test

open System
open System.Collections.Generic
open Exceptions
open ExtensionOnArray

// --- Array.asResizeArray tests ---

let asResizeArrayTests =
    testList ("Array.asResizeArray", [

        test ("converts string array to ResizeArray", fun _ ->
            let arr = [| "a"; "b"; "c" |]
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "count" >> isEqualTo 3)
            assertThat ra.[0] (tag "first" >> isEqualTo "a")
            assertThat ra.[1] (tag "second" >> isEqualTo "b")
            assertThat ra.[2] (tag "third" >> isEqualTo "c")
        )

        test ("converts single-element string array", fun _ ->
            let arr = [| "only" |]
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "count" >> isEqualTo 1)
            assertThat ra.[0] (tag "element" >> isEqualTo "only")
        )

        test ("converts empty string array", fun _ ->
            let arr : string[] = [||]
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "empty" >> isEqualTo 0)
        )

        test ("preserves null elements in string array", fun _ ->
            let arr = [| "a"; null; "c" |]
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "count" >> isEqualTo 3)
            assertThat ra.[0] (tag "first" >> isEqualTo "a")
            assertThat ra.[1] (tag "null element" >> isNull)
            assertThat ra.[2] (tag "third" >> isEqualTo "c")
        )

        test ("preserves duplicate elements", fun _ ->
            let arr = [| "x"; "x"; "y"; "x" |]
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "count" >> isEqualTo 4)
            assertThat ra.[0] (tag "0" >> isEqualTo "x")
            assertThat ra.[1] (tag "1" >> isEqualTo "x")
            assertThat ra.[2] (tag "2" >> isEqualTo "y")
            assertThat ra.[3] (tag "3" >> isEqualTo "x")
        )

        test ("works with obj array", fun _ ->
            let arr : obj[] = [| box 1; box "hello"; box 3.14 |]
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "count" >> isEqualTo 3)
            assertThat (unbox<int> ra.[0]) (tag "int" >> isEqualTo 1)
            assertThat (unbox<string> ra.[1]) (tag "string" >> isEqualTo "hello")
        )

        test ("throws on null array", fun _ ->
            let nullArr : string[] = null
            throwsNull (fun () -> Array.asResizeArray nullArr |> ignore)
        )

        test ("large string array", fun _ ->
            let arr = Array.init 1000 (fun i -> $"item{i}")
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "count" >> isEqualTo 1000)
            assertThat ra.[0] (tag "first" >> isEqualTo "item0")
            assertThat ra.[999] (tag "last" >> isEqualTo "item999")
        )

        test ("all null elements", fun _ ->
            let arr : string[] = [| null; null; null |]
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "count" >> isEqualTo 3)
            assertThat ra.[0] (tag "0" >> isNull)
            assertThat ra.[1] (tag "1" >> isNull)
            assertThat ra.[2] (tag "2" >> isNull)
        )

        test ("works with record type array", fun _ ->
            // Using a tuple of strings as a reference-type proxy
            let arr = [| ("a","b"); ("c","d") |]
            let ra = Array.asResizeArray arr
            assertThat ra.Count (tag "count" >> isEqualTo 2)
            assertThat ra.[0] (tag "first" >> isEqualTo ("a","b"))
            assertThat ra.[1] (tag "second" >> isEqualTo ("c","d"))
        )
    ])


// --- ResizeArray.asArray tests ---

let asArrayTests =
    testList ("ResizeArray.asArray", [

        test ("converts string ResizeArray to array", fun _ ->
            let ra = ResizeArray([ "a"; "b"; "c" ])
            let arr = ResizeArray.asArray ra
            assertThat arr.Length (tag "length" >> isEqualTo 3)
            assertThat arr.[0] (tag "first" >> isEqualTo "a")
            assertThat arr.[1] (tag "second" >> isEqualTo "b")
            assertThat arr.[2] (tag "third" >> isEqualTo "c")
        )

        test ("converts single-element ResizeArray", fun _ ->
            let ra = ResizeArray([ "only" ])
            let arr = ResizeArray.asArray ra
            assertThat arr.Length (tag "length" >> isEqualTo 1)
            assertThat arr.[0] (tag "element" >> isEqualTo "only")
        )

        test ("converts empty ResizeArray", fun _ ->
            let ra = ResizeArray<string>()
            let arr = ResizeArray.asArray ra
            assertThat arr.Length (tag "empty" >> isEqualTo 0)
        )

        test ("preserves null elements", fun _ ->
            let ra = ResizeArray([ "a"; null; "c" ])
            let arr = ResizeArray.asArray ra
            assertThat arr.Length (tag "length" >> isEqualTo 3)
            assertThat arr.[0] (tag "first" >> isEqualTo "a")
            assertThat arr.[1] (tag "null element" >> isNull)
            assertThat arr.[2] (tag "third" >> isEqualTo "c")
        )

        test ("preserves duplicate elements", fun _ ->
            let ra = ResizeArray([ "x"; "x"; "y"; "x" ])
            let arr = ResizeArray.asArray ra
            assertThat arr.Length (tag "length" >> isEqualTo 4)
            assertThat arr.[0] (tag "0" >> isEqualTo "x")
            assertThat arr.[1] (tag "1" >> isEqualTo "x")
            assertThat arr.[2] (tag "2" >> isEqualTo "y")
            assertThat arr.[3] (tag "3" >> isEqualTo "x")
        )

        test ("works with obj ResizeArray", fun _ ->
            let ra = ResizeArray<obj>([ box 1; box "hello"; box 3.14 ])
            let arr = ResizeArray.asArray ra
            assertThat arr.Length (tag "length" >> isEqualTo 3)
            assertThat (unbox<int> arr.[0]) (tag "int" >> isEqualTo 1)
            assertThat (unbox<string> arr.[1]) (tag "string" >> isEqualTo "hello")
        )

        test ("throws on null ResizeArray", fun _ ->
            let nullArr : ResizeArray<string> = null
            throwsNull (fun () -> ResizeArray.asArray nullArr |> ignore)
        )

        test ("large ResizeArray", fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> $"item{i}" })
            let arr = ResizeArray.asArray ra
            assertThat arr.Length (tag "length" >> isEqualTo 1000)
            assertThat arr.[0] (tag "first" >> isEqualTo "item0")
            assertThat arr.[999] (tag "last" >> isEqualTo "item999")
        )

        test ("all null elements", fun _ ->
            let ra = ResizeArray<string>([ null; null; null ])
            let arr = ResizeArray.asArray ra
            assertThat arr.Length (tag "length" >> isEqualTo 3)
            assertThat arr.[0] (tag "0" >> isNull)
            assertThat arr.[1] (tag "1" >> isNull)
            assertThat arr.[2] (tag "2" >> isNull)
        )
    ])


// --- ResizeArray.map tests ---

let mapTests =
    testList ("ResizeArray.map", [

        test ("maps int to int", fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map (fun x -> x * 2) ra
            assertThat (result == [| 2; 4; 6 |].asRarr) (tag "doubled" >> isTrue)
        )

        test ("maps int to string", fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map (fun x -> $"v{x}") ra
            assertThat (result == [| "v1"; "v2"; "v3" |].asRarr) (tag "to string" >> isTrue)
        )

        test ("maps string to int (length)", fun _ ->
            let ra = [| "a"; "bb"; "ccc" |].asRarr
            let result = ResizeArray.map (fun (s:string) -> s.Length) ra
            assertThat (result == [| 1; 2; 3 |].asRarr) (tag "lengths" >> isTrue)
        )

        test ("maps string to string", fun _ ->
            let ra = [| "hello"; "world" |].asRarr
            let result = ResizeArray.map (fun (s:string) -> s.ToUpper()) ra
            assertThat (result == [| "HELLO"; "WORLD" |].asRarr) (tag "upper" >> isTrue)
        )

        test ("maps empty ResizeArray", fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.map (fun x -> x * 2) ra
            assertThat result.Count (tag "empty result" >> isEqualTo 0)
        )

        test ("maps single element", fun _ ->
            let ra = [| 42 |].asRarr
            let result = ResizeArray.map (fun x -> x + 1) ra
            assertThat result.Count (tag "count" >> isEqualTo 1)
            assertThat result.[0] (tag "value" >> isEqualTo 43)
        )

        test ("throws on null", fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.map (fun x -> x) nullArr |> ignore)
        )

        test ("maps float to float", fun _ ->
            let ra = [| 1.5; 2.5; 3.5 |].asRarr
            let result = ResizeArray.map (fun x -> x * 2.0) ra
            assertThat (result == [| 3.0; 5.0; 7.0 |].asRarr) (tag "doubled floats" >> isTrue)
        )

        test ("maps with identity", fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map id ra
            assertThat (result == ra) (tag "identity" >> isTrue)
        )

        test ("result is independent of source", fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map (fun x -> x * 2) ra
            ra.[0] <- 99
            assertThat result.[0] (tag "result unchanged after source mutation" >> isEqualTo 2)
        )

        test ("maps with index-dependent function", fun _ ->
            let ra = [| 10; 20; 30 |].asRarr
            let mutable idx = 0
            let result = ResizeArray.map (fun x -> let r = x + idx in idx <- idx + 1; r) ra
            assertThat result.[0] (tag "0" >> isEqualTo 10)
            assertThat result.[1] (tag "1" >> isEqualTo 21)
            assertThat result.[2] (tag "2" >> isEqualTo 32)
        )

        test ("maps large ResizeArray", fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i })
            let result = ResizeArray.map (fun x -> x * x) ra
            assertThat result.Count (tag "count" >> isEqualTo 1000)
            assertThat result.[0] (tag "first" >> isEqualTo 0)
            assertThat result.[999] (tag "last" >> isEqualTo (999*999))
        )

        test ("maps bool to bool", fun _ ->
            let ra = [| true; false; true |].asRarr
            let result = ResizeArray.map not ra
            assertThat (result == [| false; true; false |].asRarr) (tag "negated" >> isTrue)
        )

        test ("maps to option type", fun _ ->
            let ra = [| 1; 2; 3 |].asRarr
            let result = ResizeArray.map (fun x -> if x % 2 = 0 then Some x else None) ra
            assertThat result.[0] (tag "odd" >> isEqualTo None)
            assertThat result.[1] (tag "even" >> isEqualTo (Some 2))
            assertThat result.[2] (tag "odd2" >> isEqualTo None)
        )

        test ("maps with constant function", fun _ ->
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let result = ResizeArray.map (fun _ -> 0) ra
            assertThat result.Count (tag "count" >> isEqualTo 5)
            assertThat (result == [| 0; 0; 0; 0; 0 |].asRarr) (tag "all zeros" >> isTrue)
        )

        test ("maps duplicates", fun _ ->
            let ra = [| 1; 1; 1 |].asRarr
            let result = ResizeArray.map (fun x -> x + 1) ra
            assertThat (result == [| 2; 2; 2 |].asRarr) (tag "all twos" >> isTrue)
        )
    ])


// --- ResizeArray.forall tests ---

let forallTests =
    testList ("ResizeArray.forall", [

        test ("all satisfy predicate", fun _ ->
            let ra = [| 2; 4; 6; 8 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            assertThat result (tag "all even" >> isTrue)
        )

        test ("none satisfy predicate", fun _ ->
            let ra = [| 1; 3; 5; 7 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            assertThat result (tag "none even" >> isFalse)
        )

        test ("first fails predicate", fun _ ->
            let ra = [| 1; 2; 4; 6 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            assertThat result (tag "first is odd" >> isFalse)
        )

        test ("last fails predicate", fun _ ->
            let ra = [| 2; 4; 6; 7 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            assertThat result (tag "last is odd" >> isFalse)
        )

        test ("middle fails predicate", fun _ ->
            let ra = [| 2; 4; 5; 6 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            assertThat result (tag "middle is odd" >> isFalse)
        )

        test ("empty array returns true", fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.forall (fun x -> x > 0) ra
            assertThat result (tag "empty is vacuously true" >> isTrue)
        )

        test ("single element satisfies", fun _ ->
            let ra = [| 2 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            assertThat result (tag "single even" >> isTrue)
        )

        test ("single element fails", fun _ ->
            let ra = [| 1 |].asRarr
            let result = ResizeArray.forall (fun x -> x % 2 = 0) ra
            assertThat result (tag "single odd" >> isFalse)
        )

        test ("throws on null", fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.forall (fun x -> x > 0) nullArr |> ignore)
        )

        test ("short-circuits on false", fun _ ->
            let mutable count = 0
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let _ = ResizeArray.forall (fun x -> count <- count + 1; x < 3) ra
            assertThat count (tag "should stop at third element" >> isEqualTo 3)
        )

        test ("with string predicate", fun _ ->
            let ra = [| "abc"; "def"; "ghi" |].asRarr
            let result = ResizeArray.forall (fun (s:string) -> s.Length = 3) ra
            assertThat result (tag "all length 3" >> isTrue)
        )

        test ("with string predicate failing", fun _ ->
            let ra = [| "abc"; "de"; "ghi" |].asRarr
            let result = ResizeArray.forall (fun (s:string) -> s.Length = 3) ra
            assertThat result (tag "not all length 3" >> isFalse)
        )

        test ("with float predicate", fun _ ->
            let ra = [| 1.0; 2.0; 3.0 |].asRarr
            let result = ResizeArray.forall (fun x -> x > 0.0) ra
            assertThat result (tag "all positive" >> isTrue)
        )

        test ("with always true predicate", fun _ ->
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let result = ResizeArray.forall (fun _ -> true) ra
            assertThat result (tag "always true" >> isTrue)
        )

        test ("with always false predicate on non-empty", fun _ ->
            let ra = [| 1 |].asRarr
            let result = ResizeArray.forall (fun _ -> false) ra
            assertThat result (tag "always false" >> isFalse)
        )

        test ("with always false predicate on empty", fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.forall (fun _ -> false) ra
            assertThat result (tag "vacuously true even with false predicate" >> isTrue)
        )

        test ("large array all true", fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i })
            let result = ResizeArray.forall (fun x -> x >= 0) ra
            assertThat result (tag "all non-negative" >> isTrue)
        )

        test ("large array with single false at end", fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i })
            ra.[999] <- -1
            let result = ResizeArray.forall (fun x -> x >= 0) ra
            assertThat result (tag "last element is negative" >> isFalse)
        )

        test ("with duplicates all satisfying", fun _ ->
            let ra = [| 2; 2; 2; 2 |].asRarr
            let result = ResizeArray.forall (fun x -> x = 2) ra
            assertThat result (tag "all twos" >> isTrue)
        )

        test ("with bool values", fun _ ->
            let ra = [| true; true; true |].asRarr
            let result = ResizeArray.forall id ra
            assertThat result (tag "all true" >> isTrue)
        )

        test ("with bool values one false", fun _ ->
            let ra = [| true; false; true |].asRarr
            let result = ResizeArray.forall id ra
            assertThat result (tag "one false" >> isFalse)
        )
    ])


// --- ResizeArray.countBy tests ---

let countByTests =
    testList ("ResizeArray.countBy", [

        test ("counts by int identity", fun _ ->
            let ra = [| 1; 2; 1; 3; 2; 1 |].asRarr
            let result = ResizeArray.countBy id ra
            // Order should match first occurrence
            assertThat result.Count (tag "3 unique keys" >> isEqualTo 3)
            assertThat (result.[0]) (tag "1 appears 3 times" >> isEqualTo (1, 3))
            assertThat (result.[1]) (tag "2 appears 2 times" >> isEqualTo (2, 2))
            assertThat (result.[2]) (tag "3 appears 1 time" >> isEqualTo (3, 1))
        )

        test ("counts by string identity", fun _ ->
            let ra = [| "a"; "b"; "a"; "c"; "b"; "a" |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "3 unique keys" >> isEqualTo 3)
            assertThat (result.[0]) (tag "a appears 3 times" >> isEqualTo ("a", 3))
            assertThat (result.[1]) (tag "b appears 2 times" >> isEqualTo ("b", 2))
            assertThat (result.[2]) (tag "c appears 1 time" >> isEqualTo ("c", 1))
        )

        test ("counts by string length", fun _ ->
            let ra = [| "a"; "bb"; "c"; "ddd"; "ee" |].asRarr
            let result = ResizeArray.countBy (fun (s:string) -> s.Length) ra
            assertThat result.Count (tag "3 unique lengths" >> isEqualTo 3)
            assertThat (result.[0]) (tag "length 1: 2 items" >> isEqualTo (1, 2))
            assertThat (result.[1]) (tag "length 2: 2 items" >> isEqualTo (2, 2))
            assertThat (result.[2]) (tag "length 3: 1 item" >> isEqualTo (3, 1))
        )

        test ("counts by even/odd", fun _ ->
            let ra = [| 1; 2; 3; 4; 5; 6 |].asRarr
            let result = ResizeArray.countBy (fun x -> x % 2 = 0) ra
            assertThat result.Count (tag "2 groups" >> isEqualTo 2)
            assertThat (result.[0]) (tag "3 odd" >> isEqualTo (false, 3))
            assertThat (result.[1]) (tag "3 even" >> isEqualTo (true, 3))
        )

        test ("empty array", fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "empty" >> isEqualTo 0)
        )

        test ("single element", fun _ ->
            let ra = [| 42 |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "one group" >> isEqualTo 1)
            assertThat (result.[0]) (tag "42 once" >> isEqualTo (42, 1))
        )

        test ("all same value int", fun _ ->
            let ra = [| 5; 5; 5; 5 |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "one group" >> isEqualTo 1)
            assertThat (result.[0]) (tag "5 four times" >> isEqualTo (5, 4))
        )

        test ("all same value string", fun _ ->
            let ra = [| "x"; "x"; "x" |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "one group" >> isEqualTo 1)
            assertThat (result.[0]) (tag "x three times" >> isEqualTo ("x", 3))
        )

        test ("throws on null", fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.countBy id nullArr |> ignore)
        )

        test ("counts by float key", fun _ ->
            let ra = [| 1.0; 2.0; 1.0; 3.0 |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "3 unique" >> isEqualTo 3)
            assertThat (result.[0]) (tag "1.0 twice" >> isEqualTo (1.0, 2))
            assertThat (result.[1]) (tag "2.0 once" >> isEqualTo (2.0, 1))
            assertThat (result.[2]) (tag "3.0 once" >> isEqualTo (3.0, 1))
        )

        test ("counts by bool key", fun _ ->
            let ra = [| true; false; true; true; false |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "2 groups" >> isEqualTo 2)
            assertThat (result.[0]) (tag "true 3 times" >> isEqualTo (true, 3))
            assertThat (result.[1]) (tag "false 2 times" >> isEqualTo (false, 2))
        )

        test ("counts by projection to string", fun _ ->
            let ra = [| 1; 2; 11; 22; 111 |].asRarr
            let result = ResizeArray.countBy (fun x -> (string x).Length) ra
            assertThat result.Count (tag "3 digit lengths" >> isEqualTo 3)
            assertThat (result.[0]) (tag "1-digit: 2" >> isEqualTo (1, 2))
            assertThat (result.[1]) (tag "2-digit: 2" >> isEqualTo (2, 2))
            assertThat (result.[2]) (tag "3-digit: 1" >> isEqualTo (3, 1))
        )

        test ("large array", fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i % 10 })
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "10 unique values" >> isEqualTo 10)
            for i = 0 to result.Count - 1 do
                assertThat (snd result.[i]) (tag $"each value appears 100 times" >> isEqualTo 100)
        )

        test ("counts with null string key", fun _ ->
            let ra = [| "a"; null; "a"; null |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "2 groups" >> isEqualTo 2)
            assertThat (result.[0]) (tag "a twice" >> isEqualTo ("a", 2))
            assertThat (result.[1]) (tag "null twice" >> isEqualTo (null, 2))
        )

        test ("counts with option key None", fun _ ->
            let ra = [| Some 1; None; Some 1; None; Some 2 |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "3 groups" >> isEqualTo 3)
            assertThat (result.[0]) (tag "Some 1 twice" >> isEqualTo (Some 1, 2))
            assertThat (result.[1]) (tag "None twice" >> isEqualTo (None, 2))
            assertThat (result.[2]) (tag "Some 2 once" >> isEqualTo (Some 2, 1))
        )

        test ("unique elements", fun _ ->
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let result = ResizeArray.countBy id ra
            assertThat result.Count (tag "5 unique" >> isEqualTo 5)
            for i = 0 to result.Count - 1 do
                assertThat (snd result.[i]) (tag "each once" >> isEqualTo 1)
        )
    ])


// --- ResizeArray.groupBy tests ---

let groupByTests =
    testList ("ResizeArray.groupBy", [

        test ("groups by int identity", fun _ ->
            let ra = [| 1; 2; 1; 3; 2; 1 |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "3 groups" >> isEqualTo 3)
            let k0, v0 = result.[0]
            assertThat k0 (tag "key 0" >> isEqualTo 1)
            assertThat (v0 == [|1;1;1|].asRarr) (tag "values 0" >> isTrue)
            let k1, v1 = result.[1]
            assertThat k1 (tag "key 1" >> isEqualTo 2)
            assertThat (v1 == [|2;2|].asRarr) (tag "values 1" >> isTrue)
            let k2, v2 = result.[2]
            assertThat k2 (tag "key 2" >> isEqualTo 3)
            assertThat (v2 == [|3|].asRarr) (tag "values 2" >> isTrue)
        )

        test ("groups by string identity", fun _ ->
            let ra = [| "a"; "b"; "a"; "c" |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "3 groups" >> isEqualTo 3)
            let k0, v0 = result.[0]
            assertThat k0 (tag "key a" >> isEqualTo "a")
            assertThat (v0 == [|"a";"a"|].asRarr) (tag "values a" >> isTrue)
            let k1, v1 = result.[1]
            assertThat k1 (tag "key b" >> isEqualTo "b")
            assertThat (v1 == [|"b"|].asRarr) (tag "values b" >> isTrue)
        )

        test ("groups by string length", fun _ ->
            let ra = [| "a"; "bb"; "c"; "ddd"; "ee" |].asRarr
            let result = ResizeArray.groupBy (fun (s:string) -> s.Length) ra
            assertThat result.Count (tag "3 groups" >> isEqualTo 3)
            let k0, v0 = result.[0]
            assertThat k0 (tag "key 1" >> isEqualTo 1)
            assertThat (v0 == [|"a";"c"|].asRarr) (tag "len 1" >> isTrue)
            let k1, v1 = result.[1]
            assertThat k1 (tag "key 2" >> isEqualTo 2)
            assertThat (v1 == [|"bb";"ee"|].asRarr) (tag "len 2" >> isTrue)
            let k2, v2 = result.[2]
            assertThat k2 (tag "key 3" >> isEqualTo 3)
            assertThat (v2 == [|"ddd"|].asRarr) (tag "len 3" >> isTrue)
        )

        test ("groups by even/odd", fun _ ->
            let ra = [| 1; 2; 3; 4; 5; 6 |].asRarr
            let result = ResizeArray.groupBy (fun x -> x % 2 = 0) ra
            assertThat result.Count (tag "2 groups" >> isEqualTo 2)
            let k0, v0 = result.[0]
            assertThat k0 (tag "odd group" >> isEqualTo false)
            assertThat (v0 == [|1;3;5|].asRarr) (tag "odd values" >> isTrue)
            let k1, v1 = result.[1]
            assertThat k1 (tag "even group" >> isEqualTo true)
            assertThat (v1 == [|2;4;6|].asRarr) (tag "even values" >> isTrue)
        )

        test ("empty array", fun _ ->
            let ra = ResizeArray<int>()
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "empty" >> isEqualTo 0)
        )

        test ("single element", fun _ ->
            let ra = [| 42 |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "one group" >> isEqualTo 1)
            let k, v = result.[0]
            assertThat k (tag "key" >> isEqualTo 42)
            assertThat (v == [|42|].asRarr) (tag "value" >> isTrue)
        )

        test ("all same value", fun _ ->
            let ra = [| 5; 5; 5; 5 |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "one group" >> isEqualTo 1)
            let k, v = result.[0]
            assertThat k (tag "key" >> isEqualTo 5)
            assertThat (v == [|5;5;5;5|].asRarr) (tag "values" >> isTrue)
        )

        test ("throws on null", fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.groupBy id nullArr |> ignore)
        )

        test ("groups with null string key", fun _ ->
            let ra = [| "a"; null; "a"; null |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "2 groups" >> isEqualTo 2)
            let k0, v0 = result.[0]
            assertThat k0 (tag "key a" >> isEqualTo "a")
            assertThat (v0 == [|"a";"a"|].asRarr) (tag "values a" >> isTrue)
            let k1, v1 = result.[1]
            assertThat k1 (tag "key null" >> isNull)
            assertThat v1.Count (tag "null group count" >> isEqualTo 2)
        )

        test ("groups with option key None", fun _ ->
            let ra = [| Some 1; None; Some 1; None; Some 2 |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "3 groups" >> isEqualTo 3)
            let k0, v0 = result.[0]
            assertThat k0 (tag "key Some 1" >> isEqualTo (Some 1))
            assertThat v0.Count (tag "Some 1 count" >> isEqualTo 2)
            let k1, v1 = result.[1]
            assertThat k1 (tag "key None" >> isEqualTo None)
            assertThat v1.Count (tag "None count" >> isEqualTo 2)
            let k2, v2 = result.[2]
            assertThat k2 (tag "key Some 2" >> isEqualTo (Some 2))
            assertThat v2.Count (tag "Some 2 count" >> isEqualTo 1)
        )

        test ("groups preserve element order", fun _ ->
            let ra = [| 3; 1; 4; 1; 5; 9; 2; 6; 5; 3 |].asRarr
            let result = ResizeArray.groupBy id ra
            // first occurrence order: 3, 1, 4, 5, 9, 2, 6
            assertThat (fst result.[0]) (tag "first key is 3" >> isEqualTo 3)
            assertThat (fst result.[1]) (tag "second key is 1" >> isEqualTo 1)
            assertThat (fst result.[2]) (tag "third key is 4" >> isEqualTo 4)
        )

        test ("groups by float key", fun _ ->
            let ra = [| 1.0; 2.0; 1.0; 3.0 |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "3 groups" >> isEqualTo 3)
            let k0, v0 = result.[0]
            assertThat k0 (tag "key 1.0" >> isEqualTo 1.0)
            assertThat (v0 == [|1.0;1.0|].asRarr) (tag "values 1.0" >> isTrue)
        )

        test ("groups by bool key", fun _ ->
            let ra = [| true; false; true; true; false |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "2 groups" >> isEqualTo 2)
            let k0, v0 = result.[0]
            assertThat k0 (tag "key true" >> isEqualTo true)
            assertThat v0.Count (tag "true count" >> isEqualTo 3)
            let k1, v1 = result.[1]
            assertThat k1 (tag "key false" >> isEqualTo false)
            assertThat v1.Count (tag "false count" >> isEqualTo 2)
        )

        test ("unique elements", fun _ ->
            let ra = [| 1; 2; 3; 4; 5 |].asRarr
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "5 groups" >> isEqualTo 5)
            for i = 0 to result.Count - 1 do
                let _, v = result.[i]
                assertThat v.Count (tag "each group has 1 element" >> isEqualTo 1)
        )

        test ("large array", fun _ ->
            let ra = ResizeArray(seq { for i in 0..999 -> i % 10 })
            let result = ResizeArray.groupBy id ra
            assertThat result.Count (tag "10 groups" >> isEqualTo 10)
            for i = 0 to result.Count - 1 do
                let _, v = result.[i]
                assertThat v.Count (tag "each group has 100 elements" >> isEqualTo 100)
        )

        test ("groups by projection to tuple", fun _ ->
            let ra = [| 1; 2; 3; 4; 5; 6 |].asRarr
            let result = ResizeArray.groupBy (fun x -> (x % 2, x % 3)) ra
            // (1,1)->1  (0,2)->2  (1,0)->3  (0,1)->4  (1,2)->5  (0,0)->6
            assertThat result.Count (tag "6 unique (mod2, mod3) pairs" >> isEqualTo 6)
        )
    ])


// --- Combined test list ---

let tests =
    testList ("FableCompat Tests", [
        asResizeArrayTests
        asArrayTests
        mapTests
        forallTests
        countByTests
        groupByTests
    ])
