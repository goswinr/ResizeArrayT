module Tests.Module3

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open ResizeArrayT

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
    ]