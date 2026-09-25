module Tests.Module3

open Scriptorium.Nib.Assertion
open type Scriptorium.Quill.Test

open ResizeArrayT
open Tests.Exceptions
open System

// [<Tests>]
let tests = // : TestCase in Scriptorium.Quill
    testList ("Module3 Tests", [
        // Add your tests here
        test ("zipDefault combines arrays with default values", fun _ ->
            let getDefaultVal index longerValue = longerValue + index
            let arr1 = ResizeArray [1; 2; 10]
            let arr2 = ResizeArray [4; 5]

            let result = ResizeArray.zipDefault getDefaultVal arr1 arr2 |> List.ofSeq

            assertThat (result = [(1, 4); (2, 5); (10, 12)]) (tag "zipDefault should combine arrays correctly with default values" >> isTrue)
        )

        test ("zipDefault with empty second array", fun _ ->
            let getDefaultVal _index longerValue = longerValue * 2
            let arr1 = ResizeArray [1; 2; 3]
            let arr2 = ResizeArray([])

            let result = ResizeArray.zipDefault getDefaultVal arr1 arr2 |> List.ofSeq

            assertThat (result = [(1, 2); (2, 4); (3, 6)]) (tag "zipDefault should handle empty second array correctly" >> isTrue)
        )

        test ("zipDefault with empty first array", fun _ ->
            let getDefaultVal _index longerValue = longerValue
            let arr1 = ResizeArray []
            let arr2 = ResizeArray [4; 5; 6]

            let result = ResizeArray.zipDefault getDefaultVal arr1 arr2 |> List.ofSeq

            assertThat (result = [(4, 4); (5, 5); (6, 6)]) (tag "zipDefault should handle empty first array correctly" >> isTrue)
        )

        test ("zipDefault with arrays of equal length", fun _ ->
            let getDefaultVal index longerValue = longerValue + index
            let arr1 = ResizeArray [1; 2; 3]
            let arr2 = ResizeArray [4; 5; 6]

            let result = ResizeArray.zipDefault getDefaultVal arr1 arr2 |> List.ofSeq

            assertThat (result = [(1, 4); (2, 5); (3, 6)]) (tag "zipDefault should combine arrays of equal length correctly" >> isTrue)
        )


        test ("mapPrevNext with string concatenation", fun _ ->
            let combineAdjacent prev next = prev + "-" + next
            let mergePrevAndNextCombineResults current prevResult nextResult = prevResult + ":" + current + ":" +  nextResult
            let arr = ResizeArray ["a"; "b"; "c"; "d"]

            let result =
                ResizeArray.mapPrevNext combineAdjacent mergePrevAndNextCombineResults arr
                |> List.ofSeq


            let expected =  [
                "d-a:a:a-b"
                "a-b:b:b-c"
                "b-c:c:c-d"
                "c-d:d:d-a"
            ]

            assertThat ( result = expected ) (tag "mapPrevNext should handle string concatenation correctly" >> isTrue)
        )

        test ("mapPrevNext with empty array", fun _ ->
            let combineAdjacent prev next = prev + next
            let mergePrevAndNextCombineResults current prevResult nextResult = current + prevResult + nextResult
            let arr = ResizeArray()

            let result =
                ResizeArrayT.ResizeArray.mapPrevNext combineAdjacent mergePrevAndNextCombineResults arr
                |> List.ofSeq

            let expected = []

            assertThat ( result = expected ) (tag "mapPrevNext should handle empty array correctly" >> isTrue)

        )

        test ("zeroCreate creates a ResizeArray of the given length with a uniform default value", fun _ ->
            // Checked for self-consistency (every element equal to the first) rather than against
            // a hardcoded literal: Fable compiles the generic Unchecked.defaultof<'T> inside zeroCreate
            // itself to null (even for numeric 'T), which is not the same value a locally resolved,
            // concretely-typed Unchecked.defaultof<int> in this test would compile to.
            let r = ResizeArray.zeroCreate<int> 3
            assertThat (r.Count = 3) (tag "zeroCreate should create a ResizeArray of the requested length" >> isTrue)
            assertThat (r |> Seq.forall (fun x -> x = r.[0])) (tag "zeroCreate should fill every element with the same default value" >> isTrue)

            let s = ResizeArray.zeroCreate<string> 2
            assertThat (s.Count = 2) (tag "zeroCreate should create a ResizeArray of the requested length" >> isTrue)
            assertThat s.[0] (tag "zeroCreate should fill a reference type ResizeArray with null" >> isNull)
            assertThat s.[1] (tag "zeroCreate should fill a reference type ResizeArray with null" >> isNull)

            let e = ResizeArray.zeroCreate<int> 0
            assertThat (e.Count = 0) (tag "zeroCreate should allow a count of zero" >> isTrue)

            throwsArg (fun () -> ResizeArray.zeroCreate<int> (-1) |> ignore)
        )

        test ("partitionWith behaves like partitionBy under the F# core Array module name", fun _ ->
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let classify x = if x % 2 = 0 then Choice1Of2 (x * 10) else Choice2Of2 (string x)

            let evens, odds = ResizeArray.partitionWith classify arr
            let evensBy, oddsBy = ResizeArray.partitionBy classify arr

            assertThat (List.ofSeq evens = [20; 40]) (tag "partitionWith should collect Choice1Of2 results in order" >> isTrue)
            assertThat (List.ofSeq odds = ["1"; "3"; "5"]) (tag "partitionWith should collect Choice2Of2 results in order" >> isTrue)
            assertThat (List.ofSeq evens = List.ofSeq evensBy && List.ofSeq odds = List.ofSeq oddsBy) (tag "partitionWith should agree with partitionBy" >> isTrue)

            throwsNull (fun () -> ResizeArray.partitionWith classify null |> ignore)
        )

        test ("randomChoice returns an element of the input and fails on empty input", fun _ ->
            let arr = ResizeArray [1; 2; 3; 4; 5]
            for _ = 1 to 20 do
                let x = ResizeArray.randomChoice arr
                assertThat (arr.Contains x) (tag "randomChoice should return an element from the input ResizeArray" >> isTrue)

            throwsArg (fun () -> ResizeArray.randomChoice (ResizeArray<int>()) |> ignore)
        )

        test ("randomChoiceBy uses the given randomizer function deterministically", fun _ ->
            let arr = ResizeArray [10; 20; 30; 40; 50]
            let alwaysFirst () = 0.0
            let alwaysLast () = 0.999
            assertThat (ResizeArray.randomChoiceBy alwaysFirst arr = 10) (tag "randomChoiceBy should pick the first element when the randomizer returns 0.0" >> isTrue)
            assertThat (ResizeArray.randomChoiceBy alwaysLast arr = 50) (tag "randomChoiceBy should pick the last element when the randomizer returns close to 1.0" >> isTrue)
        )

        test ("random*By functions fail when the randomizer returns a value outside [0.0, 1.0)", fun _ ->
            let arr = ResizeArray [10; 20; 30]
            throwsArg (fun () -> ResizeArray.randomChoiceBy (fun () -> 1.0) arr |> ignore)
            throwsArg (fun () -> ResizeArray.randomChoiceBy (fun () -> -0.1) arr |> ignore)
            throwsArg (fun () -> ResizeArray.randomChoiceBy (fun () -> nan) arr |> ignore)
            throwsArg (fun () -> ResizeArray.randomChoicesBy (fun () -> 2.0) 2 arr |> ignore)
            throwsArg (fun () -> ResizeArray.randomSampleBy (fun () -> 1.0) 2 arr |> ignore)
            throwsArg (fun () -> ResizeArray.randomShuffleBy (fun () -> -1.0) arr |> ignore)
            throwsArg (fun () -> ResizeArray.randomShuffleInPlaceBy (fun () -> 1.0) arr)
        )

        test ("randomChoiceWith uses the given Random instance deterministically", fun _ ->
            let arr = ResizeArray [10; 20; 30; 40; 50]
            let random = Random(42)
            let expected = arr.[random.Next(arr.Count)]
            let random2 = Random(42)
            assertThat (ResizeArray.randomChoiceWith random2 arr = expected) (tag "randomChoiceWith should use the passed in Random instance" >> isTrue)
        )

        test ("randomChoices returns count elements, each contained in source, with replacement allowed", fun _ ->
            let arr = ResizeArray [1; 2; 3]
            let res = ResizeArray.randomChoices 10 arr
            assertThat (res.Count = 10) (tag "randomChoices should return exactly count elements" >> isTrue)
            for x in res do assertThat (arr.Contains x) (tag "randomChoices should only return elements from the source" >> isTrue)

            let empty = ResizeArray.randomChoices 0 arr
            assertThat (empty.Count = 0) (tag "randomChoices should allow a count of zero" >> isTrue)

            throwsArg (fun () -> ResizeArray.randomChoices (-1) arr |> ignore)
        )

        test ("randomChoicesBy and randomChoicesWith return count elements from source", fun _ ->
            let arr = ResizeArray [1; 2; 3]
            let mutable i = 0
            let cyclic () =
                let v = float (i % 3) / 3.0
                i <- i + 1
                v
            let byRes = ResizeArray.randomChoicesBy cyclic 6 arr
            assertThat (byRes.Count = 6) (tag "randomChoicesBy should return exactly count elements" >> isTrue)
            for x in byRes do assertThat (arr.Contains x) (tag "randomChoicesBy should only return elements from the source" >> isTrue)

            let withRes = ResizeArray.randomChoicesWith (Random(7)) 6 arr
            assertThat (withRes.Count = 6) (tag "randomChoicesWith should return exactly count elements" >> isTrue)
            for x in withRes do assertThat (arr.Contains x) (tag "randomChoicesWith should only return elements from the source" >> isTrue)
        )

        test ("randomSample returns count distinct elements without replacement", fun _ ->
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let res = ResizeArray.randomSample 3 arr
            assertThat (res.Count = 3) (tag "randomSample should return exactly count elements" >> isTrue)
            let distinctCount = res |> Seq.distinct |> Seq.length
            assertThat (distinctCount = 3) (tag "randomSample should not repeat elements" >> isTrue)
            for x in res do assertThat (arr.Contains x) (tag "randomSample should only return elements from the source" >> isTrue)

            let full = ResizeArray.randomSample 5 arr
            assertThat ((full |> Seq.sort |> List.ofSeq) = [1;2;3;4;5]) (tag "randomSample with count = length should return all elements" >> isTrue)

            throwsArg (fun () -> ResizeArray.randomSample 6 arr |> ignore)
            throwsArg (fun () -> ResizeArray.randomSample (-1) arr |> ignore)
        )

        test ("randomSampleBy and randomSampleWith return distinct elements without replacement", fun _ ->
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let mutable i = 0
            let cyclic () =
                let v = float (i % 5) / 5.0
                i <- i + 1
                v
            let byRes = ResizeArray.randomSampleBy cyclic 3 arr
            assertThat (byRes.Count = 3) (tag "randomSampleBy should return exactly count elements" >> isTrue)
            assertThat ((byRes |> Seq.distinct |> Seq.length) = 3) (tag "randomSampleBy should not repeat elements" >> isTrue)

            let withRes = ResizeArray.randomSampleWith (Random(3)) 3 arr
            assertThat (withRes.Count = 3) (tag "randomSampleWith should return exactly count elements" >> isTrue)
            assertThat ((withRes |> Seq.distinct |> Seq.length) = 3) (tag "randomSampleWith should not repeat elements" >> isTrue)
        )

        test ("randomSample returns the sampled elements in random order", fun _ ->
            // a sample of all elements must not just keep the input order
            let arr = ResizeArray [0; 1; 2]
            let random = Random(1)
            let orderings = Collections.Generic.HashSet<string>()
            for _ = 1 to 600 do
                ResizeArray.randomSampleWith random 3 arr |> Seq.map string |> String.concat "" |> orderings.Add |> ignore
            assertThat (orderings.Count = 6) (tag "randomSampleWith 3 of 3 should produce all 6 orderings" >> isTrue)

            // the last element must also be able to come first in a partial sample
            let firsts = Collections.Generic.HashSet<int>()
            for _ = 1 to 300 do
                firsts.Add (ResizeArray.randomSampleWith random 2 arr).[0] |> ignore
            assertThat (firsts.Count = 3) (tag "randomSampleWith 2 of 3 should put every element first sometimes" >> isTrue)
        )

        test ("randomShuffle returns a new ResizeArray with the same elements without mutating the input", fun _ ->
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let original = List.ofSeq arr
            let shuffled = ResizeArray.randomShuffle arr

            assertThat (List.ofSeq arr = original) (tag "randomShuffle should not mutate the input ResizeArray" >> isTrue)
            assertThat ((shuffled |> Seq.sort |> List.ofSeq) = (original |> List.sort)) (tag "randomShuffle should return the same multiset of elements" >> isTrue)

            let byResult = ResizeArray.randomShuffleBy (fun () -> 0.5) arr
            assertThat ((byResult |> Seq.sort |> List.ofSeq) = (original |> List.sort)) (tag "randomShuffleBy should return the same multiset of elements" >> isTrue)

            let withResult = ResizeArray.randomShuffleWith (Random(11)) arr
            assertThat ((withResult |> Seq.sort |> List.ofSeq) = (original |> List.sort)) (tag "randomShuffleWith should return the same multiset of elements" >> isTrue)
        )

        test ("randomShuffleInPlace mutates the ResizeArray keeping the same elements", fun _ ->
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let original = arr |> List.ofSeq |> List.sort
            ResizeArray.randomShuffleInPlace arr
            assertThat ((arr |> Seq.sort |> List.ofSeq) = original) (tag "randomShuffleInPlace should keep the same elements" >> isTrue)

            let arr2 = ResizeArray [1; 2; 3; 4; 5]
            ResizeArray.randomShuffleInPlaceBy (fun () -> 0.5) arr2
            assertThat ((arr2 |> Seq.sort |> List.ofSeq) = original) (tag "randomShuffleInPlaceBy should keep the same elements" >> isTrue)

            let arr3 = ResizeArray [1; 2; 3; 4; 5]
            ResizeArray.randomShuffleInPlaceWith (Random(13)) arr3
            assertThat ((arr3 |> Seq.sort |> List.ofSeq) = original) (tag "randomShuffleInPlaceWith should keep the same elements" >> isTrue)
        )
    ])