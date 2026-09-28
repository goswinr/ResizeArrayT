module Tests.Module3

open Scriptorium.Nib.Assertion
open type Scriptorium.Quill.Test

open ResizeArrayT
open Tests.Exceptions
open System

#nowarn "44" // to test the obsolete applyIf and slice aliases
let private obsoleteApplyIfResult pr f (xs: ResizeArray<int>) = ResizeArray.applyIfResult pr f xs
let private obsoleteApplyIfInputAndResult pi pr f (xs: ResizeArray<int>) = ResizeArray.applyIfInputAndResult pi pr f xs
let private obsoleteSlice startIdx endIdx (xs: ResizeArray<int>) = ResizeArray.slice startIdx endIdx xs
#warnon "44"

/// Compares by Value only, but Equals also checks the Name.
/// So items that compare as equal are not equal by '='.
[<CustomEquality; CustomComparison>]
type TieItem =
    { Value: int; Name: string }
    override this.Equals(o) =
        match o with
        | :? TieItem as i -> i.Value = this.Value && i.Name = this.Name
        | _ -> false
    override this.GetHashCode() = hash (this.Value, this.Name)
    interface IComparable with
        member this.CompareTo(o) = compare this.Value (o :?> TieItem).Value

/// Shows a float so that NaN, -0.0 and +0.0 can be told apart, since -0.0 = +0.0 is true.
let private show (x: float) =
    if Double.IsNaN x then "NaN"
    elif x = 0.0 then (if 1.0 / x < 0.0 then "-0" else "+0")
    else string x

let private show32 (x: float32) = show (float x)

/// For a stable sort: NaN comes last, other values ascending. -0.0 and +0.0 are equal.
let private nanLastAsc (a: float) (b: float) =
    match Double.IsNaN a, Double.IsNaN b with
    | true, true -> 0
    | true, false -> 1
    | false, true -> -1
    | false, false -> compare a b

/// For a stable sort: NaN comes last, other values descending. -0.0 and +0.0 are equal.
let private nanLastDesc (a: float) (b: float) =
    match Double.IsNaN a, Double.IsNaN b with
    | true, true -> 0
    | true, false -> 1
    | false, true -> -1
    | false, false -> compare b a

/// Reference for the IEEE 754:2019 'minimumNumber': NaN is skipped and -0.0 is smaller than +0.0.
let private refMinNumber (vs: float list) =
    match vs |> List.filter (fun v -> not (Double.IsNaN v)) with
    | [] -> nan
    | ns -> ns |> List.reduce (fun a b -> if b < a || (b = a && show b = "-0") then b else a)

/// Reference for the IEEE 754:2019 'maximumNumber': NaN is skipped and +0.0 is bigger than -0.0.
let private refMaxNumber (vs: float list) =
    match vs |> List.filter (fun v -> not (Double.IsNaN v)) with
    | [] -> nan
    | ns -> ns |> List.reduce (fun a b -> if b > a || (b = a && show b = "+0") then b else a)

/// All lists of length 1 to 4 made of NaN, -0.0, +0.0, -1.0 and 1.0.
let private nanInputs =
    let values = [nan; -0.0; 0.0; -1.0; 1.0]
    [ for a in values do
        [a]
        for b in values do
            [a; b]
            for c in values do
                [a; b; c]
                for d in values do
                    [a; b; c; d] ]

// [<Tests>]
let tests = // : TestCase in Scriptorium.Quill
    testList ("Module3 Tests", [
        test ("groupByDict uses structural keys and preserves group order", fun _ ->
            let xs = ResizeArray [1; 2; 3; 4; 5]
            let mutable calls = 0
            let d = ResizeArray.groupByDict (fun x -> calls <- calls + 1; [|x % 2|]) xs
            assertThat d.Count (tag "structurally equal arrays form one group" >> isEqualTo 2)
            assertThat (List.ofSeq d.[[|1|]]) (tag "fresh key lookup and input order" >> isEqualTo [1; 3; 5])
            assertThat (List.ofSeq d.[[|0|]]) (tag "other group" >> isEqualTo [2; 4])
            assertThat calls (tag "projection runs once per item" >> isEqualTo xs.Count)
            let nested = ResizeArray.groupByDict (fun x -> Some ([|x % 2|], "key")) xs
            assertThat (List.ofSeq nested.[Some ([|1|], "key")]) (tag "nested structural key" >> isEqualTo [1; 3; 5])
            assertThat (ResizeArray.groupByDict id (ResizeArray<int>())).Count (tag "empty input" >> isEqualTo 0)
            throwsNull (fun () -> ResizeArray.groupByDict id (null: ResizeArray<int>) |> ignore)
        )

        test ("groupByDict rejects null and None keys", fun _ ->
            throwsNull (fun () -> ResizeArray.groupByDict (fun _ -> (null: string)) (ResizeArray [1]) |> ignore)
            throwsNull (fun () -> ResizeArray.groupByDict (fun _ -> (None: int option)) (ResizeArray [1]) |> ignore)
            throwsWith ["groupByDict"; "null or None"] (fun () -> ResizeArray.groupByDict (fun _ -> (None: int option)) (ResizeArray [1]) |> ignore)
        )

        test ("min3 and max3 functions match a stable sort, also when equality disagrees with comparison", fun _ ->
            let values = [1; 2; 3]
            let inputs = [
                for a in values do
                    for b in values do
                        for c in values do
                            [a; b; c]
                            for d in values do
                                [a; b; c; d] ]
            for vs in inputs do
                let items = vs |> List.mapi (fun i v -> { Value = v; Name = string i }) |> ResizeArray
                // List.sortWith is a stable sort
                let stableIdx cmp = items |> List.ofSeq |> List.indexed |> List.sortWith (fun (_, x) (_, y) -> cmp x y) |> List.map fst
                let asc  = stableIdx (fun (x: TieItem) y -> compare x.Value y.Value)
                let desc = stableIdx (fun (x: TieItem) y -> compare y.Value x.Value)
                let first3 (idx: int list) = idx.[0], idx.[1], idx.[2]
                let names (x: TieItem, y: TieItem, z: TieItem) = [x.Name; y.Name; z.Name]
                let expectedNames (idx: int list) = [ for i in idx.[0..2] -> string i ]
                assertThat (ResizeArray.min3IndicesBy id items) (tag $"min3IndicesBy {vs}" >> isEqualTo (first3 asc))
                assertThat (ResizeArray.max3IndicesBy id items) (tag $"max3IndicesBy {vs}" >> isEqualTo (first3 desc))
                assertThat (names (ResizeArray.min3 items))     (tag $"min3 {vs}"   >> isEqualTo (expectedNames asc))
                assertThat (names (ResizeArray.max3 items))     (tag $"max3 {vs}"   >> isEqualTo (expectedNames desc))
                assertThat (names (ResizeArray.min3By id items)) (tag $"min3By {vs}" >> isEqualTo (expectedNames asc))
                assertThat (names (ResizeArray.max3By id items)) (tag $"max3By {vs}" >> isEqualTo (expectedNames desc))
        )

        test ("min and max propagate NaN, minNumber and maxNumber skip it, as in IEEE 754:2019", fun _ ->
            assertThat (show -0.0) (tag "the -0.0 literal is negative zero" >> isEqualTo "-0")
            // the table from https://github.com/dotnet/fsharp/issues/13207#issuecomment-1194411950 , inputs in either order
            let table = [
                // input        min    max    minNumber maxNumber
                [3.0; 7.0],  ("3",   "7",   "3",   "7")
                [3.0; nan],  ("NaN", "NaN", "3",   "3")
                [nan; nan],  ("NaN", "NaN", "NaN", "NaN")
                [-0.0; 0.0], ("-0",  "+0",  "-0",  "+0") ]
            for input, (mi, ma, miN, maN) in table do
                for vs in [input; List.rev input] do
                    let xs = ResizeArray vs
                    assertThat (show (ResizeArray.min xs))       (tag $"min {vs}"       >> isEqualTo mi)
                    assertThat (show (ResizeArray.max xs))       (tag $"max {vs}"       >> isEqualTo ma)
                    assertThat (show (ResizeArray.minNumber xs)) (tag $"minNumber {vs}" >> isEqualTo miN)
                    assertThat (show (ResizeArray.maxNumber xs)) (tag $"maxNumber {vs}" >> isEqualTo maN)
                    let fs = ResizeArray (List.map float32 vs)
                    assertThat (show32 (ResizeArray.min fs))       (tag $"float32 min {vs}"       >> isEqualTo mi)
                    assertThat (show32 (ResizeArray.max fs))       (tag $"float32 max {vs}"       >> isEqualTo ma)
                    assertThat (show32 (ResizeArray.minNumber fs)) (tag $"float32 minNumber {vs}" >> isEqualTo miN)
                    assertThat (show32 (ResizeArray.maxNumber fs)) (tag $"float32 maxNumber {vs}" >> isEqualTo maN)
        )

        test ("min, max, minNumber and maxNumber match a reference with NaN, -0.0 and +0.0 at every position", fun _ ->
            for vs in nanInputs do
                let hasNaN = vs |> List.exists Double.IsNaN
                let minN = refMinNumber vs
                let maxN = refMaxNumber vs
                let xs = ResizeArray vs
                assertThat (show (ResizeArray.min xs))       (tag $"min {vs}"       >> isEqualTo (if hasNaN then "NaN" else show minN))
                assertThat (show (ResizeArray.max xs))       (tag $"max {vs}"       >> isEqualTo (if hasNaN then "NaN" else show maxN))
                assertThat (show (ResizeArray.minNumber xs)) (tag $"minNumber {vs}" >> isEqualTo (show minN))
                assertThat (show (ResizeArray.maxNumber xs)) (tag $"maxNumber {vs}" >> isEqualTo (show maxN))
                let fs = ResizeArray (List.map float32 vs)
                assertThat (show32 (ResizeArray.min fs))       (tag $"float32 min {vs}"       >> isEqualTo (if hasNaN then "NaN" else show minN))
                assertThat (show32 (ResizeArray.max fs))       (tag $"float32 max {vs}"       >> isEqualTo (if hasNaN then "NaN" else show maxN))
                assertThat (show32 (ResizeArray.minNumber fs)) (tag $"float32 minNumber {vs}" >> isEqualTo (show minN))
                assertThat (show32 (ResizeArray.maxNumber fs)) (tag $"float32 maxNumber {vs}" >> isEqualTo (show maxN))
        )

        test ("minNumber and maxNumber work on any comparable type, keep the first of equal items and check their input", fun _ ->
            assertThat (ResizeArray.minNumber (ResizeArray [3; 1; 2])) (tag "minNumber int" >> isEqualTo 1)
            assertThat (ResizeArray.maxNumber (ResizeArray ["b"; "c"; "a"])) (tag "maxNumber string" >> isEqualTo "c")
            let ties = ResizeArray [ { Value = 2; Name = "a" }; { Value = 1; Name = "b" }; { Value = 1; Name = "c" }; { Value = 2; Name = "d" } ]
            assertThat (ResizeArray.min ties).Name       (tag "min ties"       >> isEqualTo "b")
            assertThat (ResizeArray.max ties).Name       (tag "max ties"       >> isEqualTo "a")
            assertThat (ResizeArray.minNumber ties).Name (tag "minNumber ties" >> isEqualTo "b")
            assertThat (ResizeArray.maxNumber ties).Name (tag "maxNumber ties" >> isEqualTo "a")
            throwsArg (fun () -> ResizeArray.minNumber (ResizeArray<float>()) |> ignore)
            throwsArg (fun () -> ResizeArray.maxNumber (ResizeArray<float>()) |> ignore)
            throwsNull (fun () -> ResizeArray.minNumber (null: ResizeArray<float>) |> ignore)
            throwsNull (fun () -> ResizeArray.maxNumber (null: ResizeArray<float>) |> ignore)
            throwsWith ["ResizeArray.minIndexBy: Count must be at least one"] (fun () -> ResizeArray.minIndexBy id (ResizeArray<float>()) |> ignore)
            throwsWith ["ResizeArray.maxIndexBy: Count must be at least one"] (fun () -> ResizeArray.maxIndexBy id (ResizeArray<float>()) |> ignore)
            throwsWith ["ResizeArray.max3IndicesBy: Count must be at least three"] (fun () -> ResizeArray.max3IndicesBy id (ResizeArray [1.0; 2.0]) |> ignore)
        )

        test ("minBy, maxBy and the other By functions ignore NaN keys at any position", fun _ ->
            let xs = ResizeArray [nan; 3.0; nan; 1.0; 2.0; nan]
            assertThat (ResizeArray.minBy id xs)         (tag "minBy"         >> isEqualTo 1.0)
            assertThat (ResizeArray.maxBy id xs)         (tag "maxBy"         >> isEqualTo 3.0)
            assertThat (ResizeArray.minIndexBy id xs)    (tag "minIndexBy"    >> isEqualTo 3)
            assertThat (ResizeArray.maxIndexBy id xs)    (tag "maxIndexBy"    >> isEqualTo 1)
            assertThat (ResizeArray.min2By id xs)        (tag "min2By"        >> isEqualTo (1.0, 2.0))
            assertThat (ResizeArray.max2IndicesBy id xs) (tag "max2IndicesBy" >> isEqualTo (1, 4))
            assertThat (ResizeArray.min3IndicesBy id xs) (tag "min3IndicesBy" >> isEqualTo (3, 4, 1))
            assertThat (ResizeArray.max3By id xs)        (tag "max3By"        >> isEqualTo (3.0, 2.0, 1.0))
            assertThat (ResizeArray.min3 xs)             (tag "min3"          >> isEqualTo (1.0, 2.0, 3.0))
            assertThat (ResizeArray.max2 xs)             (tag "max2"          >> isEqualTo (3.0, 2.0))
            // unlike min and max, which propagate NaN
            assertThat (Double.IsNaN (ResizeArray.min xs)) (tag "min propagates NaN" >> isTrue)
            assertThat (Double.IsNaN (ResizeArray.max xs)) (tag "max propagates NaN" >> isTrue)
            // not enough keys that are not NaN: NaN keys come last, in their original order
            let ys = ResizeArray [nan; 5.0; nan]
            assertThat (ResizeArray.min3IndicesBy id ys) (tag "min3IndicesBy one number" >> isEqualTo (1, 0, 2))
            assertThat (ResizeArray.max2IndicesBy id ys) (tag "max2IndicesBy one number" >> isEqualTo (1, 0))
            // all keys are NaN: the first element
            let nans = ResizeArray [ ("a", nan); ("b", nan); ("c", nan) ]
            assertThat (ResizeArray.minBy snd nans |> fst) (tag "minBy all NaN" >> isEqualTo "a")
            assertThat (ResizeArray.maxBy snd nans |> fst) (tag "maxBy all NaN" >> isEqualTo "a")
            assertThat (ResizeArray.minIndexBy snd nans)   (tag "minIndexBy all NaN" >> isEqualTo 0)
            // float32 keys
            let fs = ResizeArray [nanf; 2.0f; nanf; -1.0f]
            assertThat (ResizeArray.minBy id fs) (tag "float32 minBy" >> isEqualTo -1.0f)
            assertThat (ResizeArray.maxBy id fs) (tag "float32 maxBy" >> isEqualTo 2.0f)
        )

        test ("By functions and min2, max2, min3, max3 rank NaN last, matching a stable sort", fun _ ->
            for vs in nanInputs do
                let n = vs.Length
                // the index is in the item, to see which of several equal keys was returned
                let items = vs |> List.mapi (fun i v -> (i, v)) |> ResizeArray
                let key (_: int, v: float) = v
                let idx (i: int, _: float) = i
                // List.sortWith is a stable sort
                let stableIdx cmp = [0 .. n - 1] |> List.sortWith (fun i j -> cmp vs.[i] vs.[j])
                let asc = stableIdx nanLastAsc
                let desc = stableIdx nanLastDesc
                let values (idxs: int list) = idxs |> List.map (fun i -> show vs.[i])
                assertThat (ResizeArray.minIndexBy key items)  (tag $"minIndexBy {vs}" >> isEqualTo asc.[0])
                assertThat (ResizeArray.maxIndexBy key items)  (tag $"maxIndexBy {vs}" >> isEqualTo desc.[0])
                assertThat (idx (ResizeArray.minBy key items)) (tag $"minBy {vs}"      >> isEqualTo asc.[0])
                assertThat (idx (ResizeArray.maxBy key items)) (tag $"maxBy {vs}"      >> isEqualTo desc.[0])
                let xs = ResizeArray vs
                if n >= 2 then
                    assertThat (ResizeArray.min2IndicesBy key items) (tag $"min2IndicesBy {vs}" >> isEqualTo (asc.[0], asc.[1]))
                    assertThat (ResizeArray.max2IndicesBy key items) (tag $"max2IndicesBy {vs}" >> isEqualTo (desc.[0], desc.[1]))
                    let a, b = ResizeArray.min2By key items
                    assertThat (idx a, idx b) (tag $"min2By {vs}" >> isEqualTo (asc.[0], asc.[1]))
                    let a, b = ResizeArray.max2By key items
                    assertThat (idx a, idx b) (tag $"max2By {vs}" >> isEqualTo (desc.[0], desc.[1]))
                    let a, b = ResizeArray.min2 xs
                    assertThat [show a; show b] (tag $"min2 {vs}" >> isEqualTo (values asc.[0..1]))
                    let a, b = ResizeArray.max2 xs
                    assertThat [show a; show b] (tag $"max2 {vs}" >> isEqualTo (values desc.[0..1]))
                if n >= 3 then
                    assertThat (ResizeArray.min3IndicesBy key items) (tag $"min3IndicesBy {vs}" >> isEqualTo (asc.[0], asc.[1], asc.[2]))
                    assertThat (ResizeArray.max3IndicesBy key items) (tag $"max3IndicesBy {vs}" >> isEqualTo (desc.[0], desc.[1], desc.[2]))
                    let a, b, c = ResizeArray.min3By key items
                    assertThat (idx a, idx b, idx c) (tag $"min3By {vs}" >> isEqualTo (asc.[0], asc.[1], asc.[2]))
                    let a, b, c = ResizeArray.max3By key items
                    assertThat (idx a, idx b, idx c) (tag $"max3By {vs}" >> isEqualTo (desc.[0], desc.[1], desc.[2]))
                    let a, b, c = ResizeArray.min3 xs
                    assertThat [show a; show b; show c] (tag $"min3 {vs}" >> isEqualTo (values asc.[0..2]))
                    let a, b, c = ResizeArray.max3 xs
                    assertThat [show a; show b; show c] (tag $"max3 {vs}" >> isEqualTo (values desc.[0..2]))
        )

        #if !FABLE_COMPILER
        test ("minBy and maxBy also ignore keys that contain NaN, like a tuple", fun _ ->
            let xs = ResizeArray [ (nan, 0); (2.0, 1); (nan, 2); (1.0, 3) ]
            assertThat (ResizeArray.minBy id xs) (tag "minBy tuple" >> isEqualTo (1.0, 3))
            assertThat (ResizeArray.maxBy id xs) (tag "maxBy tuple" >> isEqualTo (2.0, 1))
            assertThat (ResizeArray.min3IndicesBy id xs) (tag "min3IndicesBy tuple" >> isEqualTo (3, 1, 0))
        )
        #endif

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
            // isEqualTo from Scriptorium.Nib, it used to be shadowed in Fable by an internal function of the same name in ResizeArrayT:
            assertThat orderings.Count (tag "randomSampleWith 3 of 3 should produce all 6 orderings" >> isEqualTo 6)

            // the last element must also be able to come first in a partial sample
            let firsts = Collections.Generic.HashSet<int>()
            for _ = 1 to 300 do
                firsts.Add (ResizeArray.randomSampleWith random 2 arr).[0] |> ignore
            assertThat firsts.Count (tag "randomSampleWith 2 of 3 should put every element first sometimes" >> isEqualTo 3)
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

        test ("failIfEmpty and failIfLessThan", fun _ ->
            let xs = ResizeArray [1; 2]
            assertThat (obj.ReferenceEquals(xs, ResizeArray.failIfEmpty "ok" xs)) (tag "failIfEmpty returns the input" >> isTrue)
            assertThat (obj.ReferenceEquals(xs, ResizeArray.failIfLessThan 2 "ok" xs)) (tag "failIfLessThan returns the input" >> isTrue)
            throwsArg (fun () -> ResizeArray.failIfEmpty "is empty" (ResizeArray<int>()) |> ignore)
            throwsArg (fun () -> ResizeArray.failIfLessThan 3 "too few" xs |> ignore)
        )

        test ("sliceNeg with positive and negative indices", fun _ ->
            let xs = ResizeArray [1; 2; 3; 4; 5]
            assertThat (List.ofSeq (ResizeArray.sliceNeg 1 3 xs)) (tag "sliceNeg 1 3" >> isEqualTo [2; 3; 4])
            assertThat (List.ofSeq (ResizeArray.sliceNeg -2 -1 xs)) (tag "sliceNeg -2 -1" >> isEqualTo [4; 5])
            assertThat (List.ofSeq (ResizeArray.sliceNeg 1 -2 xs)) (tag "sliceNeg 1 -2" >> isEqualTo [2; 3; 4])
            assertThat (List.ofSeq (ResizeArray.sliceNeg 2 2 xs)) (tag "sliceNeg 2 2" >> isEqualTo [3])
            assertThat (List.ofSeq (ResizeArray.sliceNeg 0 -1 xs)) (tag "sliceNeg 0 -1" >> isEqualTo [1; 2; 3; 4; 5])
            assertThat (obj.ReferenceEquals(xs, ResizeArray.sliceNeg 0 -1 xs)) (tag "sliceNeg returns a new ResizeArray" >> isFalse)
            assertThat (List.ofSeq (xs.SliceNeg(1, -2))) (tag "xs.SliceNeg(1, -2)" >> isEqualTo [2; 3; 4])
        )

        test ("sliceNeg returns empty when end index is one less than start index", fun _ ->
            let xs = ResizeArray [1; 2; 3; 4; 5]
            assertThat (ResizeArray.sliceNeg 3 2 xs).Count (tag "sliceNeg 3 2" >> isEqualTo 0)
            assertThat (ResizeArray.sliceNeg 0 -6 xs).Count (tag "sliceNeg 0 -6, like trim 0 5" >> isEqualTo 0)
        )

        test ("sliceNeg throws on invalid ranges with descriptive messages", fun _ ->
            let xs = ResizeArray [1; 2; 3; 4; 5]
            throwsIdx (fun () -> ResizeArray.sliceNeg 3 1 xs |> ignore)
            throwsWith ["ResizeArray.SliceNeg: Start index 3 is bigger than end index 1"] (fun () -> ResizeArray.sliceNeg 3 1 xs |> ignore)
            throwsWith ["End index -99 is out of range"] (fun () -> ResizeArray.sliceNeg 1 -99 xs |> ignore)
            throwsWith ["End index -6 is out of range"] (fun () -> ResizeArray.sliceNeg 1 -6 xs |> ignore)
            throwsWith ["End index 5 is out of range"] (fun () -> ResizeArray.sliceNeg 1 5 xs |> ignore)
            throwsWith ["Start index -6 is out of range"] (fun () -> ResizeArray.sliceNeg -6 2 xs |> ignore)
            throwsIdx (fun () -> ResizeArray.sliceNeg 0 0 (ResizeArray<int>()) |> ignore)
            throwsNull (fun () -> ResizeArray.sliceNeg 0 1 (null: ResizeArray<int>) |> ignore)
        )

        test ("obsolete slice still works like sliceNeg", fun _ ->
            let xs = ResizeArray [1; 2; 3; 4; 5]
            assertThat (List.ofSeq (obsoleteSlice 1 -2 xs)) (tag "slice 1 -2" >> isEqualTo [2; 3; 4])
            throwsNull (fun () -> obsoleteSlice 0 1 (null: ResizeArray<int>) |> ignore)
        )

        test ("sliceIdx and sliceLooped throw on null", fun _ ->
            throwsNull (fun () -> ResizeArray.sliceIdx 0 1 (null: ResizeArray<int>) |> ignore)
            throwsNull (fun () -> ResizeArray.sliceLooped 0 1 (null: ResizeArray<int>) |> ignore)
        )

        test ("sliceNeg and sliceIdx name an empty input in the message", fun _ ->
            let empty = ResizeArray<int>()
            throwsWith ["ResizeArray.SliceNeg: Can't slice an empty ResizeArray"] (fun () -> ResizeArray.sliceNeg 0 0 empty |> ignore)
            throwsWith ["ResizeArray.SliceNeg: Can't slice an empty ResizeArray"] (fun () -> empty.SliceNeg(0, -1) |> ignore)
            throwsWith ["ResizeArray.SliceIdx: Can't slice an empty ResizeArray"] (fun () -> ResizeArray.sliceIdx 0 0 empty |> ignore)
            throwsWith ["ResizeArray.SliceIdx: Can't slice an empty ResizeArray"] (fun () -> empty.SliceIdx(0, 0) |> ignore)
            assertThat (ResizeArray.sliceLooped 0 5 empty).Count (tag "sliceLooped on empty input" >> isEqualTo 0)
        )

        test ("sliceIdx and sliceLooped give the same results as the extension members", fun _ ->
            let xs = ResizeArray [1; 2; 3; 4; 5]
            assertThat (List.ofSeq (ResizeArray.sliceIdx 1 3 xs)) (tag "sliceIdx 1 3" >> isEqualTo (List.ofSeq (xs.SliceIdx(1, 3))))
            assertThat (List.ofSeq (ResizeArray.sliceLooped 6 -1 xs)) (tag "sliceLooped 6 -1" >> isEqualTo (List.ofSeq (xs.SliceLooped(6, -1))))
            assertThat (List.ofSeq (ResizeArray.sliceLooped 6 -1 xs)) (tag "sliceLooped 6 -1 values" >> isEqualTo [2; 3; 4; 5])
            assertThat (List.ofSeq (ResizeArray.sliceLooped 5 6 xs)) (tag "sliceLooped 5 6" >> isEqualTo [1; 2])
            throwsWith ["ResizeArray.SliceIdx: Start index -1 is out of range"] (fun () -> ResizeArray.sliceIdx -1 2 xs |> ignore)
            throwsWith ["ResizeArray.SliceIdx: End index 5 is out of range"] (fun () -> ResizeArray.sliceIdx 0 5 xs |> ignore)
            throwsWith ["ResizeArray.SliceIdx: Start index 3 is bigger than end index 2"] (fun () -> ResizeArray.sliceIdx 3 2 xs |> ignore)
        )

        test ("matches", fun _ ->
            let xs = ResizeArray [1; 2; 3; 4; 5]
            assertThat (ResizeArray.matches (ResizeArray [2; 3]) 1 xs) (tag "matches at index 1" >> isTrue)
            assertThat (ResizeArray.matches (ResizeArray [2; 4]) 1 xs) (tag "does not match" >> isFalse)
            assertThat (ResizeArray.matches (ResizeArray [5; 6]) 4 xs) (tag "not enough items" >> isFalse)
            throwsArg (fun () -> ResizeArray.matches (ResizeArray [1]) -1 xs |> ignore)
            throwsArg (fun () -> ResizeArray.matches (ResizeArray [1]) 5 xs |> ignore)
            throwsWith ["ResizeArray.matches: the searchFor ResizeArray is empty"] (fun () -> ResizeArray.matches (ResizeArray<int>()) 0 xs |> ignore)
            throwsNull (fun () -> ResizeArray.matches null 0 xs |> ignore)
            throwsNull (fun () -> ResizeArray.matches (ResizeArray [1]) 0 null |> ignore)
        )

        test ("findValue and findLastValue", fun _ ->
            let xs = ResizeArray [1; 2; 3; 2; 5]
            assertThat (ResizeArray.findValue 2 0 4 xs) (tag "findValue" >> isEqualTo 1)
            assertThat (ResizeArray.findValue 2 2 4 xs) (tag "findValue in range" >> isEqualTo 3)
            assertThat (ResizeArray.findValue 2 2 2 xs) (tag "findValue not in range" >> isEqualTo -1)
            assertThat (ResizeArray.findValue 6 0 4 xs) (tag "findValue not found" >> isEqualTo -1)
            assertThat (ResizeArray.findLastValue 2 0 4 xs) (tag "findLastValue" >> isEqualTo 3)
            assertThat (ResizeArray.findLastValue 2 0 2 xs) (tag "findLastValue in range" >> isEqualTo 1)
            assertThat (ResizeArray.findLastValue 6 0 4 xs) (tag "findLastValue not found" >> isEqualTo -1)
            throwsArg (fun () -> ResizeArray.findValue 1 -1 2 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findValue 1 0 5 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findValue 1 2 1 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findLastValue 1 -1 2 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findLastValue 1 0 5 xs |> ignore)
            throwsNull (fun () -> ResizeArray.findValue 1 0 0 (null: ResizeArray<int>) |> ignore)
            throwsNull (fun () -> ResizeArray.findLastValue 1 0 0 (null: ResizeArray<int>) |> ignore)
        )

        test ("findArray and findLastArray", fun _ ->
            let i = ResizeArray("abcde")
            let l = i.LastIndex
            let ab = ResizeArray("ab")
            let de = ResizeArray("de")
            assertThat (ResizeArray.findArray ab 0 l i) (tag "findArray ab" >> isEqualTo 0)
            assertThat (ResizeArray.findArray ab 1 l i) (tag "findArray ab from 1" >> isEqualTo -1)
            assertThat (ResizeArray.findLastArray ab 0 l i) (tag "findLastArray ab" >> isEqualTo 0)
            assertThat (ResizeArray.findLastArray ab 1 l i) (tag "findLastArray ab from 1" >> isEqualTo -1)
            assertThat (ResizeArray.findArray de 0 (l-1) i) (tag "findArray de till l-1" >> isEqualTo -1)
            assertThat (ResizeArray.findArray de 0 l i) (tag "findArray de" >> isEqualTo 3)
            assertThat (ResizeArray.findLastArray de 0 (l-1) i) (tag "findLastArray de till l-1" >> isEqualTo -1)
            assertThat (ResizeArray.findLastArray de 0 l i) (tag "findLastArray de" >> isEqualTo 3)
            let rep = ResizeArray [1; 2; 1; 2; 1; 2]
            assertThat (ResizeArray.findArray (ResizeArray [1; 2]) 0 5 rep) (tag "findArray first of repeated" >> isEqualTo 0)
            assertThat (ResizeArray.findLastArray (ResizeArray [1; 2]) 0 5 rep) (tag "findLastArray last of repeated" >> isEqualTo 4)
        )

        test ("findArray and findLastArray throw on invalid input", fun _ ->
            let xs = ResizeArray [1; 2; 3]
            let one = ResizeArray [1]
            throwsArg (fun () -> ResizeArray.findArray one -1 2 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findArray one 0 3 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findArray one 2 1 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findLastArray one -1 2 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findLastArray one 0 3 xs |> ignore)
            throwsArg (fun () -> ResizeArray.findLastArray one 2 1 xs |> ignore)
            throwsWith ["findArray"; "searchFor ResizeArray is empty"] (fun () -> ResizeArray.findArray (ResizeArray<int>()) 0 2 xs |> ignore)
            throwsWith ["findLastArray"; "searchFor ResizeArray is empty"] (fun () -> ResizeArray.findLastArray (ResizeArray<int>()) 0 2 xs |> ignore)
            throwsNull (fun () -> ResizeArray.findArray null 0 2 xs |> ignore)
            throwsNull (fun () -> ResizeArray.findLastArray null 0 2 xs |> ignore)
            throwsNull (fun () -> ResizeArray.findArray one 0 0 null |> ignore)
            throwsNull (fun () -> ResizeArray.findLastArray one 0 0 null |> ignore)
        )

        test ("mapIfResult", fun _ ->
            let xs = ResizeArray [1; 2; 3]
            let plus1 = ResizeArray.map ((+) 1)
            let applied = ResizeArray.mapIfResult (fun r -> r.Count > 0) plus1 xs
            assertThat (List.ofSeq applied) (tag "mapIfResult applied" >> isEqualTo [2; 3; 4])
            let notApplied = ResizeArray.mapIfResult (fun r -> r.Count > 10) plus1 xs
            assertThat (obj.ReferenceEquals(xs, notApplied)) (tag "mapIfResult not applied" >> isTrue)
            throwsNull (fun () -> ResizeArray.mapIfResult (fun _ -> true) id (null: ResizeArray<int>) |> ignore)
        )

        test ("mapIfInputAndResult", fun _ ->
            let xs = ResizeArray [1; 2; 3]
            let plus1 = ResizeArray.map ((+) 1)
            let applied = ResizeArray.mapIfInputAndResult (fun a -> a.Count > 0) (fun r -> r.Count > 0) plus1 xs
            assertThat (List.ofSeq applied) (tag "mapIfInputAndResult applied" >> isEqualTo [2; 3; 4])
            let inputFailed = ResizeArray.mapIfInputAndResult (fun a -> a.Count > 10) (fun _ -> true) plus1 xs
            assertThat (obj.ReferenceEquals(xs, inputFailed)) (tag "mapIfInputAndResult input failed" >> isTrue)
            let resultFailed = ResizeArray.mapIfInputAndResult (fun _ -> true) (fun r -> r.Count > 10) plus1 xs
            assertThat (obj.ReferenceEquals(xs, resultFailed)) (tag "mapIfInputAndResult result failed" >> isTrue)
            throwsNull (fun () -> ResizeArray.mapIfInputAndResult (fun _ -> true) (fun _ -> true) id (null: ResizeArray<int>) |> ignore)
        )

        test ("obsolete applyIfResult and applyIfInputAndResult still work", fun _ ->
            let xs = ResizeArray [1; 2; 3]
            let plus1 = ResizeArray.map ((+) 1)
            assertThat (List.ofSeq (obsoleteApplyIfResult (fun r -> r.Count > 0) plus1 xs)) (tag "applyIfResult" >> isEqualTo [2; 3; 4])
            assertThat (List.ofSeq (obsoleteApplyIfInputAndResult (fun _ -> true) (fun _ -> true) plus1 xs)) (tag "applyIfInputAndResult" >> isEqualTo [2; 3; 4])
        )

        test ("failIfEmpty and failIfLessThan throw on null", fun _ ->
            let nullArr : ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.failIfEmpty "is null" nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.failIfLessThan 3 "is null" nullArr |> ignore)
        )
    ])
