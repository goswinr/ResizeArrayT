module Tests.Module3

#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
open Fable.Mocha
#else
open Expecto
#endif

open ResizeArrayT
open Tests.Exceptions
open System

// [<Tests>]
let tests = // : Test in Expect , TestCase in Mocha
    testList "Module3 Tests" [
        // Add your tests here
        test "zipDefault combines arrays with default values" {
            let getDefaultVal index longerValue = longerValue + index
            let arr1 = ResizeArray [1; 2; 10]
            let arr2 = ResizeArray [4; 5]

            let result = ResizeArray.zipDefault getDefaultVal arr1 arr2 |> List.ofSeq

            Expect.isTrue  (result = [(1, 4); (2, 5); (10, 12)]) "zipDefault should combine arrays correctly with default values"
        }

        test "zipDefault with empty second array" {
            let getDefaultVal _index longerValue = longerValue * 2
            let arr1 = ResizeArray [1; 2; 3]
            let arr2 = ResizeArray([])

            let result = ResizeArray.zipDefault getDefaultVal arr1 arr2 |> List.ofSeq

            Expect.isTrue (result = [(1, 2); (2, 4); (3, 6)]) "zipDefault should handle empty second array correctly"
        }

        test "zipDefault with empty first array" {
            let getDefaultVal _index longerValue = longerValue
            let arr1 = ResizeArray []
            let arr2 = ResizeArray [4; 5; 6]

            let result = ResizeArray.zipDefault getDefaultVal arr1 arr2 |> List.ofSeq

            Expect.isTrue (result = [(4, 4); (5, 5); (6, 6)]) "zipDefault should handle empty first array correctly"
        }

        test "zipDefault with arrays of equal length" {
            let getDefaultVal index longerValue = longerValue + index
            let arr1 = ResizeArray [1; 2; 3]
            let arr2 = ResizeArray [4; 5; 6]

            let result = ResizeArray.zipDefault getDefaultVal arr1 arr2 |> List.ofSeq

            Expect.isTrue (result = [(1, 4); (2, 5); (3, 6)]) "zipDefault should combine arrays of equal length correctly"
        }


        test "mapPrevNext with string concatenation" {
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

            Expect.isTrue ( result = expected ) "mapPrevNext should handle string concatenation correctly"
        }

        test "mapPrevNext with empty array" {
            let combineAdjacent prev next = prev + next
            let mergePrevAndNextCombineResults current prevResult nextResult = current + prevResult + nextResult
            let arr = ResizeArray()

            let result =
                ResizeArrayT.ResizeArray.mapPrevNext combineAdjacent mergePrevAndNextCombineResults arr
                |> List.ofSeq

            let expected = []

            Expect.isTrue ( result = expected ) "mapPrevNext should handle empty array correctly"

        }

        test "zeroCreate creates a ResizeArray of the given length with default values" {
            let r = ResizeArray.zeroCreate<int> 3
            Expect.isTrue (r.Count = 3) "zeroCreate should create a ResizeArray of the requested length"
            Expect.isTrue (List.ofSeq r = [0; 0; 0]) "zeroCreate should fill an int ResizeArray with zeros"

            let s = ResizeArray.zeroCreate<string> 2
            Expect.isTrue (List.ofSeq s = [null; null]) "zeroCreate should fill a reference type ResizeArray with nulls"

            let e = ResizeArray.zeroCreate<int> 0
            Expect.isTrue (e.Count = 0) "zeroCreate should allow a count of zero"

            throwsArg (fun () -> ResizeArray.zeroCreate<int> (-1) |> ignore)
        }

        test "partitionWith behaves like partitionBy under the F# core Array module name" {
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let classify x = if x % 2 = 0 then Choice1Of2 (x * 10) else Choice2Of2 (string x)

            let evens, odds = ResizeArray.partitionWith classify arr
            let evensBy, oddsBy = ResizeArray.partitionBy classify arr

            Expect.isTrue (List.ofSeq evens = [20; 40]) "partitionWith should collect Choice1Of2 results in order"
            Expect.isTrue (List.ofSeq odds = ["1"; "3"; "5"]) "partitionWith should collect Choice2Of2 results in order"
            Expect.isTrue (List.ofSeq evens = List.ofSeq evensBy && List.ofSeq odds = List.ofSeq oddsBy) "partitionWith should agree with partitionBy"

            throwsNull (fun () -> ResizeArray.partitionWith classify null |> ignore)
        }

        test "randomChoice returns an element of the input and fails on empty input" {
            let arr = ResizeArray [1; 2; 3; 4; 5]
            for _ = 1 to 20 do
                let x = ResizeArray.randomChoice arr
                Expect.isTrue (arr.Contains x) "randomChoice should return an element from the input ResizeArray"

            throwsArg (fun () -> ResizeArray.randomChoice (ResizeArray<int>()) |> ignore)
        }

        test "randomChoiceBy uses the given randomizer function deterministically" {
            let arr = ResizeArray [10; 20; 30; 40; 50]
            let alwaysFirst () = 0.0
            let alwaysLast () = 0.999
            Expect.isTrue (ResizeArray.randomChoiceBy alwaysFirst arr = 10) "randomChoiceBy should pick the first element when the randomizer returns 0.0"
            Expect.isTrue (ResizeArray.randomChoiceBy alwaysLast arr = 50) "randomChoiceBy should pick the last element when the randomizer returns close to 1.0"
        }

        test "randomChoiceWith uses the given Random instance deterministically" {
            let arr = ResizeArray [10; 20; 30; 40; 50]
            let random = Random(42)
            let expected = arr.[random.Next(arr.Count)]
            let random2 = Random(42)
            Expect.isTrue (ResizeArray.randomChoiceWith random2 arr = expected) "randomChoiceWith should use the passed in Random instance"
        }

        test "randomChoices returns count elements, each contained in source, with replacement allowed" {
            let arr = ResizeArray [1; 2; 3]
            let res = ResizeArray.randomChoices 10 arr
            Expect.isTrue (res.Count = 10) "randomChoices should return exactly count elements"
            for x in res do Expect.isTrue (arr.Contains x) "randomChoices should only return elements from the source"

            let empty = ResizeArray.randomChoices 0 arr
            Expect.isTrue (empty.Count = 0) "randomChoices should allow a count of zero"

            throwsArg (fun () -> ResizeArray.randomChoices (-1) arr |> ignore)
        }

        test "randomChoicesBy and randomChoicesWith return count elements from source" {
            let arr = ResizeArray [1; 2; 3]
            let mutable i = 0
            let cyclic () =
                let v = float (i % 3) / 3.0
                i <- i + 1
                v
            let byRes = ResizeArray.randomChoicesBy cyclic 6 arr
            Expect.isTrue (byRes.Count = 6) "randomChoicesBy should return exactly count elements"
            for x in byRes do Expect.isTrue (arr.Contains x) "randomChoicesBy should only return elements from the source"

            let withRes = ResizeArray.randomChoicesWith (Random(7)) 6 arr
            Expect.isTrue (withRes.Count = 6) "randomChoicesWith should return exactly count elements"
            for x in withRes do Expect.isTrue (arr.Contains x) "randomChoicesWith should only return elements from the source"
        }

        test "randomSample returns count distinct elements without replacement" {
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let res = ResizeArray.randomSample 3 arr
            Expect.isTrue (res.Count = 3) "randomSample should return exactly count elements"
            let distinctCount = res |> Seq.distinct |> Seq.length
            Expect.isTrue (distinctCount = 3) "randomSample should not repeat elements"
            for x in res do Expect.isTrue (arr.Contains x) "randomSample should only return elements from the source"

            let full = ResizeArray.randomSample 5 arr
            Expect.isTrue ((full |> Seq.sort |> List.ofSeq) = [1;2;3;4;5]) "randomSample with count = length should return all elements"

            throwsArg (fun () -> ResizeArray.randomSample 6 arr |> ignore)
            throwsArg (fun () -> ResizeArray.randomSample (-1) arr |> ignore)
        }

        test "randomSampleBy and randomSampleWith return distinct elements without replacement" {
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let mutable i = 0
            let cyclic () =
                let v = float (i % 5) / 5.0
                i <- i + 1
                v
            let byRes = ResizeArray.randomSampleBy cyclic 3 arr
            Expect.isTrue (byRes.Count = 3) "randomSampleBy should return exactly count elements"
            Expect.isTrue ((byRes |> Seq.distinct |> Seq.length) = 3) "randomSampleBy should not repeat elements"

            let withRes = ResizeArray.randomSampleWith (Random(3)) 3 arr
            Expect.isTrue (withRes.Count = 3) "randomSampleWith should return exactly count elements"
            Expect.isTrue ((withRes |> Seq.distinct |> Seq.length) = 3) "randomSampleWith should not repeat elements"
        }

        test "randomShuffle returns a new ResizeArray with the same elements without mutating the input" {
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let original = List.ofSeq arr
            let shuffled = ResizeArray.randomShuffle arr

            Expect.isTrue (List.ofSeq arr = original) "randomShuffle should not mutate the input ResizeArray"
            Expect.isTrue ((shuffled |> Seq.sort |> List.ofSeq) = (original |> List.sort)) "randomShuffle should return the same multiset of elements"

            let byResult = ResizeArray.randomShuffleBy (fun () -> 0.5) arr
            Expect.isTrue ((byResult |> Seq.sort |> List.ofSeq) = (original |> List.sort)) "randomShuffleBy should return the same multiset of elements"

            let withResult = ResizeArray.randomShuffleWith (Random(11)) arr
            Expect.isTrue ((withResult |> Seq.sort |> List.ofSeq) = (original |> List.sort)) "randomShuffleWith should return the same multiset of elements"
        }

        test "randomShuffleInPlace mutates the ResizeArray keeping the same elements" {
            let arr = ResizeArray [1; 2; 3; 4; 5]
            let original = arr |> List.ofSeq |> List.sort
            ResizeArray.randomShuffleInPlace arr
            Expect.isTrue ((arr |> Seq.sort |> List.ofSeq) = original) "randomShuffleInPlace should keep the same elements"

            let arr2 = ResizeArray [1; 2; 3; 4; 5]
            ResizeArray.randomShuffleInPlaceBy (fun () -> 0.5) arr2
            Expect.isTrue ((arr2 |> Seq.sort |> List.ofSeq) = original) "randomShuffleInPlaceBy should keep the same elements"

            let arr3 = ResizeArray [1; 2; 3; 4; 5]
            ResizeArray.randomShuffleInPlaceWith (Random(13)) arr3
            Expect.isTrue ((arr3 |> Seq.sort |> List.ofSeq) = original) "randomShuffleInPlaceWith should keep the same elements"
        }
    ]