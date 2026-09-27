module Tests.ParallelTests

// The Parallel module is not available in Fable.
#if !FABLE_COMPILER

open Scriptorium.Nib.Assertion
open type Scriptorium.Quill.Test

open ResizeArrayT
open Tests.Exceptions
open System

/// Compares by Value only, but Equals also checks the Name.
/// So the Name shows which of several items that compare as equal was returned.
[<CustomEquality; CustomComparison>]
type private Tie =
    { Value: int; Name: string }
    override this.Equals(o) =
        match o with
        | :? Tie as i -> i.Value = this.Value && i.Name = this.Name
        | _ -> false
    override this.GetHashCode() = hash (this.Value, this.Name)
    interface IComparable with
        member this.CompareTo(o) = compare this.Value (o :?> Tie).Value

/// Sizes around the chunk and partition thresholds of the Parallel module.
let private sizes = [0; 1; 2; 3; 7; 255; 256; 257; 1000; 10_007; 100_003]

let private nonEmptySizes = sizes |> List.filter (fun n -> n > 0)

let private randomInts (seed: int) (count: int) (maxValue: int) : ResizeArray<int> =
    let rand = Random(seed)
    ResizeArray.init count (fun _ -> rand.Next maxValue)

let private asList (xs: ResizeArray<'T>) : 'T list =
    List.ofSeq xs

/// Groups sorted by key, with sorted elements, since the order of Parallel.groupBy is not specified.
let private normalizeGroups (groups: ResizeArray<'K * ResizeArray<int>>) : ('K * int list) list =
    groups
    |> Seq.map (fun (k, g) -> k, g |> Seq.sort |> List.ofSeq)
    |> Seq.sortBy fst
    |> List.ofSeq

let tests =
    testList ("ResizeArray.Parallel Tests", [

        test ("Parallel.exists and forall match the sequential versions", fun _ ->
            for n in sizes do
                let xs = randomInts n n 1000
                for target in [-1; 0; 500; 999] do
                    let isTarget x = x = target
                    let isNotTarget x = x <> target
                    assertThat (ResizeArray.Parallel.exists isTarget xs) (tag $"exists {target} n={n}" >> isEqualTo (ResizeArray.exists isTarget xs))
                    assertThat (ResizeArray.Parallel.forall isNotTarget xs) (tag $"forall {target} n={n}" >> isEqualTo (ResizeArray.forall isNotTarget xs))
            throwsNull (fun () -> ResizeArray.Parallel.exists id (null: ResizeArray<bool>) |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.forall id (null: ResizeArray<bool>) |> ignore)
        )

        test ("Parallel.tryFind, tryFindIndex and tryPick return the first match", fun _ ->
            for n in sizes do
                let xs = randomInts (n + 1) n 1000
                for rest in [-1; 0; 7; 99] do
                    let p x = x % 100 = rest
                    let chooser x = if p x then Some(string x) else None
                    assertThat (ResizeArray.Parallel.tryFindIndex p xs) (tag $"tryFindIndex {rest} n={n}" >> isEqualTo (ResizeArray.tryFindIndex p xs))
                    assertThat (ResizeArray.Parallel.tryFind p xs) (tag $"tryFind {rest} n={n}" >> isEqualTo (ResizeArray.tryFind p xs))
                    assertThat (ResizeArray.Parallel.tryPick chooser xs) (tag $"tryPick {rest} n={n}" >> isEqualTo (ResizeArray.tryPick chooser xs))
                // every element after the first match matches too
                let ys = ResizeArray.init n id
                assertThat (ResizeArray.Parallel.tryFindIndex (fun y -> y >= n / 2) ys) (tag $"tryFindIndex half n={n}" >> isEqualTo (if n = 0 then None else Some(n / 2)))
            let nullArr: ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.Parallel.tryFindIndex (fun _ -> true) nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.tryFind (fun _ -> true) nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.tryPick Some nullArr |> ignore)
        )

        test ("Parallel.filter and partitionWith keep the order", fun _ ->
            for n in sizes do
                let xs = randomInts (n + 2) n 1000
                let p x = x % 3 = 0
                assertThat (asList (ResizeArray.Parallel.filter p xs)) (tag $"filter n={n}" >> isEqualTo (asList (ResizeArray.filter p xs)))
                let partitioner x = if p x then Choice1Of2(string x) else Choice2Of2(float x)
                let actual1, actual2 = ResizeArray.Parallel.partitionWith partitioner xs
                let expected1, expected2 = ResizeArray.partitionWith partitioner xs
                assertThat (asList actual1) (tag $"partitionWith Choice1Of2 n={n}" >> isEqualTo (asList expected1))
                assertThat (asList actual2) (tag $"partitionWith Choice2Of2 n={n}" >> isEqualTo (asList expected2))
            let nullArr: ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.Parallel.filter (fun _ -> true) nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.partitionWith Choice1Of2 nullArr |> ignore)
        )

        test ("Parallel.reduce and reduceBy combine the chunks in order", fun _ ->
            for n in nonEmptySizes do
                let xs = randomInts (n + 3) n 1000
                assertThat (ResizeArray.Parallel.reduce (+) xs) (tag $"reduce n={n}" >> isEqualTo (ResizeArray.reduce (+) xs))
                assertThat (ResizeArray.Parallel.reduceBy int64 (+) xs) (tag $"reduceBy n={n}" >> isEqualTo (ResizeArray.reduce (+) xs |> int64))
                // This reduction is associative but not commutative, it fails if two ranges are combined in the wrong order.
                let joinRanges (a, b) (c, d) =
                    if b + 1 <> c then failwith $"ranges ({a}, {b}) and ({c}, {d}) are not combined in order"
                    (a, d)
                let range = ResizeArray.init n id |> ResizeArray.Parallel.reduceBy (fun i -> (i, i)) joinRanges
                assertThat range (tag $"reduceBy in order n={n}" >> isEqualTo (0, n - 1))
            let empty = ResizeArray<int>()
            throwsArg (fun () -> ResizeArray.Parallel.reduce (+) empty |> ignore)
            throwsArg (fun () -> ResizeArray.Parallel.reduceBy id (+) empty |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.reduce (+) (null: ResizeArray<int>) |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.reduceBy id (+) (null: ResizeArray<int>) |> ignore)
        )

        test ("Parallel.min, max, minBy and maxBy match the sequential versions and return the first of equal items", fun _ ->
            for n in nonEmptySizes do
                let xs = randomInts (n + 4) n 1000
                assertThat (ResizeArray.Parallel.min xs) (tag $"min n={n}" >> isEqualTo (ResizeArray.min xs))
                assertThat (ResizeArray.Parallel.max xs) (tag $"max n={n}" >> isEqualTo (ResizeArray.max xs))
                // many equal keys, the index shows which of them is returned
                let indexed = ResizeArray.indexed xs
                let key (_, x) = x % 10
                assertThat (ResizeArray.Parallel.minBy key indexed) (tag $"minBy n={n}" >> isEqualTo (ResizeArray.minBy key indexed))
                assertThat (ResizeArray.Parallel.maxBy key indexed) (tag $"maxBy n={n}" >> isEqualTo (ResizeArray.maxBy key indexed))
                // items that compare as equal, but are not equal
                let ties = xs |> ResizeArray.mapi (fun i x -> { Value = x % 10; Name = string i })
                assertThat (ResizeArray.Parallel.min ties) (tag $"min ties n={n}" >> isEqualTo (ResizeArray.min ties))
                assertThat (ResizeArray.Parallel.max ties) (tag $"max ties n={n}" >> isEqualTo (ResizeArray.max ties))
            let empty = ResizeArray<int>()
            throwsArg (fun () -> ResizeArray.Parallel.min empty |> ignore)
            throwsArg (fun () -> ResizeArray.Parallel.max empty |> ignore)
            throwsArg (fun () -> ResizeArray.Parallel.minBy id empty |> ignore)
            throwsArg (fun () -> ResizeArray.Parallel.maxBy id empty |> ignore)
            let nullArr: ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.Parallel.min nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.max nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.minBy id nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.maxBy id nullArr |> ignore)
        )

        test ("Parallel.sum, sumBy, average and averageBy match the sequential versions", fun _ ->
            for n in sizes do
                let xs = randomInts (n + 5) n 1000
                assertThat (ResizeArray.Parallel.sum xs) (tag $"sum n={n}" >> isEqualTo (ResizeArray.sum xs))
                assertThat (ResizeArray.Parallel.sumBy int64 xs) (tag $"sumBy n={n}" >> isEqualTo (ResizeArray.sumBy int64 xs))
                if n > 0 then
                    // floats with integer values add up exactly, in any order
                    let fs = xs |> ResizeArray.map float
                    assertThat (ResizeArray.Parallel.average fs) (tag $"average n={n}" >> isEqualTo (ResizeArray.average fs))
                    assertThat (ResizeArray.Parallel.averageBy float xs) (tag $"averageBy n={n}" >> isEqualTo (ResizeArray.averageBy float xs))
            let emptyFloats = ResizeArray<float>()
            assertThat (ResizeArray.Parallel.sum emptyFloats) (tag "sum empty" >> isEqualTo 0.0)
            assertThat (ResizeArray.Parallel.sumBy int emptyFloats) (tag "sumBy empty" >> isEqualTo 0)
            throwsArg (fun () -> ResizeArray.Parallel.average emptyFloats |> ignore)
            throwsArg (fun () -> ResizeArray.Parallel.averageBy id emptyFloats |> ignore)
            let nullArr: ResizeArray<float> = null
            throwsNull (fun () -> ResizeArray.Parallel.sum nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.sumBy id nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.average nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.averageBy id nullArr |> ignore)
        )

        test ("Parallel.zip matches the sequential version", fun _ ->
            for n in sizes do
                let xs = randomInts (n + 6) n 1000
                let ys = xs |> ResizeArray.map string
                assertThat (asList (ResizeArray.Parallel.zip xs ys)) (tag $"zip n={n}" >> isEqualTo (asList (ResizeArray.zip xs ys)))
            throwsArg (fun () -> ResizeArray.Parallel.zip (ResizeArray [1; 2]) (ResizeArray [1]) |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.zip (null: ResizeArray<int>) (ResizeArray [1]) |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.zip (ResizeArray [1]) (null: ResizeArray<int>) |> ignore)
        )

        test ("Parallel.groupBy has the same groups as the sequential version", fun _ ->
            for n in sizes do
                let xs = randomInts (n + 7) n 1000
                let valueKey x = x % 37
                assertThat (normalizeGroups (ResizeArray.Parallel.groupBy valueKey xs)) (tag $"groupBy int n={n}" >> isEqualTo (normalizeGroups (ResizeArray.groupBy valueKey xs)))
                // reference type keys, including null
                let stringKey x = if x % 5 = 0 then null else string (x % 7)
                assertThat (normalizeGroups (ResizeArray.Parallel.groupBy stringKey xs)) (tag $"groupBy string n={n}" >> isEqualTo (normalizeGroups (ResizeArray.groupBy stringKey xs)))
                let optionKey x = if x % 5 = 0 then None else Some(x % 7)
                assertThat (normalizeGroups (ResizeArray.Parallel.groupBy optionKey xs)) (tag $"groupBy option n={n}" >> isEqualTo (normalizeGroups (ResizeArray.groupBy optionKey xs)))
            throwsNull (fun () -> ResizeArray.Parallel.groupBy id (null: ResizeArray<int>) |> ignore)
        )

        test ("Parallel.groupBy does not fail on nan keys", fun _ ->
            let xs = ResizeArray.init 2_003 id
            let floatGroups = ResizeArray.Parallel.groupBy (fun x -> if x % 2 = 0 then nan else 1.0) xs
            assertThat floatGroups.Count (tag "all nan keys in one group" >> isEqualTo 2)
            let float32Groups = ResizeArray.Parallel.groupBy (fun x -> if x % 2 = 0 then nanf else 1.0f) xs
            assertThat float32Groups.Count (tag "all nanf keys in one group" >> isEqualTo 2)
            // a tuple with nan is not equal to itself
            let tupleGroups = ResizeArray.Parallel.groupBy (fun x -> (nan, x % 2)) xs
            assertThat (tupleGroups |> Seq.sumBy (fun (_, g) -> g.Count)) (tag "all elements in tuple groups" >> isEqualTo xs.Count)
        )

        test ("Parallel sort functions match the sequential versions", fun _ ->
            let inputs = [
                for n in sizes do
                    $"random duplicates n={n}", randomInts (n + 8) n 10
                    $"random n={n}", randomInts (n + 9) n 1_000_000
                "sorted", ResizeArray.init 100_003 id
                "reversed", ResizeArray.init 100_003 (fun i -> -i)
                "all equal", ResizeArray.create 100_003 42 ]
            for name, xs in inputs do
                let expected = asList (ResizeArray.sort xs)
                let expectedDesc = List.rev expected
                let descending a b = compare b a
                assertThat (asList (ResizeArray.Parallel.sort xs)) (tag $"sort {name}" >> isEqualTo expected)
                assertThat (asList (ResizeArray.Parallel.sortDescending xs)) (tag $"sortDescending {name}" >> isEqualTo expectedDesc)
                assertThat (asList (ResizeArray.Parallel.sortWith descending xs)) (tag $"sortWith {name}" >> isEqualTo expectedDesc)
                let inPlace = ResizeArray(xs)
                ResizeArray.Parallel.sortInPlace inPlace
                assertThat (asList inPlace) (tag $"sortInPlace {name}" >> isEqualTo expected)
                let inPlaceWith = ResizeArray(xs)
                ResizeArray.Parallel.sortInPlaceWith descending inPlaceWith
                assertThat (asList inPlaceWith) (tag $"sortInPlaceWith {name}" >> isEqualTo expectedDesc)

                // The sort is not stable, so for equal keys check only the keys and that no element got lost.
                let indexed = ResizeArray.indexed xs
                let original = indexed |> List.ofSeq |> List.sort
                let checkByKey (fname: string) (expectedKeys: int list) (sorted: ResizeArray<int * int>) =
                    assertThat (sorted |> Seq.map snd |> List.ofSeq) (tag $"{fname} keys {name}" >> isEqualTo expectedKeys)
                    assertThat (sorted |> List.ofSeq |> List.sort) (tag $"{fname} elements {name}" >> isEqualTo original)
                checkByKey "sortBy" expected (ResizeArray.Parallel.sortBy snd indexed)
                checkByKey "sortByDescending" expectedDesc (ResizeArray.Parallel.sortByDescending snd indexed)
                let inPlaceBy = ResizeArray(indexed)
                ResizeArray.Parallel.sortInPlaceBy snd inPlaceBy
                checkByKey "sortInPlaceBy" expected inPlaceBy
        )

        test ("Parallel sort functions use Operators.compare", fun _ ->
            let words = ResizeArray ["b"; "a"; "Z"; "B"; "aa"; ""]
            assertThat (asList (ResizeArray.Parallel.sort words)) (tag "sort strings" >> isEqualTo ([""; "B"; "Z"; "a"; "aa"; "b"]))
            let manyWords = randomInts 10 10_007 1_000_000 |> ResizeArray.map (fun i -> if i % 2 = 0 then string i else (string i).ToUpper() + "x")
            assertThat (asList (ResizeArray.Parallel.sort manyWords)) (tag "sort many strings" >> isEqualTo (asList (ResizeArray.sort manyWords)))
            assertThat (asList (ResizeArray.Parallel.sortBy id manyWords)) (tag "sortBy many strings" >> isEqualTo (asList (ResizeArray.sort manyWords)))
            // nan sorts first, like with Array.sort
            let floats = randomInts 11 10_007 1000 |> ResizeArray.map (fun i -> if i % 100 = 0 then nan else float i)
            let bits (xs: ResizeArray<float>) = xs |> Seq.map BitConverter.DoubleToInt64Bits |> List.ofSeq
            assertThat (bits (ResizeArray.Parallel.sort floats)) (tag "sort floats with nan" >> isEqualTo (bits (ResizeArray.sort floats)))
            assertThat (Double.IsNaN (ResizeArray.Parallel.sort floats).[0]) (tag "nan first" >> isTrue)
        )

        test ("Parallel sort functions throw on null", fun _ ->
            let nullArr: ResizeArray<int> = null
            throwsNull (fun () -> ResizeArray.Parallel.sort nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.sortBy id nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.sortByDescending id nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.sortDescending nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.sortWith compare nullArr |> ignore)
            throwsNull (fun () -> ResizeArray.Parallel.sortInPlace nullArr)
            throwsNull (fun () -> ResizeArray.Parallel.sortInPlaceBy id nullArr)
            throwsNull (fun () -> ResizeArray.Parallel.sortInPlaceWith compare nullArr)
        )
    ])

#endif
