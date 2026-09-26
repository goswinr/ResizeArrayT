namespace Tests
module Extensions =

    open ResizeArrayT


    open Scriptorium.Nib.Assertion
    open type Scriptorium.Quill.Test
    open Exceptions

#nowarn "44" // to test the obsolete Duplicate alias
    let private obsoleteDuplicate (xs: ResizeArray<int>) : ResizeArray<int> = xs.Duplicate()
#warnon "44"

    let tests = // : TestCase in Scriptorium.Quill
      testList ("extensions Tests", [

        test ("Intro: 9=9", fun _ -> assertThat 9 (tag "Intro" >> isEqualTo 9))

        let a = resizeArray{ for i in 0 .. 9 ->  float i }
        let b = ResizeArray.init 10 (fun i -> float i)

        test ("Get", fun _ ->
            assertThat (a.Get 2) (tag "Get 2" >> isEqualTo 2.0)
            assertThat (a.Get 2) (tag "Get 2 Item" >> isEqualTo a[2])
            assertThat (fun () -> a.Get 10 |> ignore ) (tag "Get 10" >> throws)
            assertThat (fun () -> a.Get -1 |> ignore ) (tag "Get -1" >> throws)

        )
        test ("Set", fun _ ->
            let a = a.Clone()
            a.Set 2 3.0
            assertThat (a.Get 2) (tag "Set 2" >> isEqualTo 3.0)
            a[2] <- 4.0
            assertThat (a.Get 2) (tag "Set 2 Item" >> isEqualTo 4.0)
            assertThat (fun () -> a.Set 10 0.0 |> ignore ) (tag "Set 10" >> throws)
            assertThat (fun () -> a.Set -1 0.0 |> ignore ) (tag "Set -1" >> throws)
        )

        test ("IsEqualTo", fun _ ->
            assertThat (a.IsEqualTo b) (tag "IsEqualTo" >> isTrue)
            assertThat (a.IsEqualTo a) (tag "IsEqualTo self" >> isTrue)
            assertThat (a.IsEqualTo (a.Clone())) (tag "IsEqualTo Clone" >> isTrue)
            let b = a.Clone()
            b.Set 2 9.9
            assertThat (a.IsEqualTo b) (tag "index 2 was set to 9.9" >> isFalse)

            let bb = resizeArray {a.Clone()}
            let aa = resizeArray {a.Clone()}
            assertThat (ResizeArray.equals2 aa bb) (tag "equals 2" >> isTrue)

            let bb = resizeArray {a.Clone()}
            let aa = resizeArray {bb.First}
            assertThat (ResizeArray.equals2 aa bb) (tag "equals 2" >> isTrue)

            let c = resizeArray {resizeArray {resizeArray {5;6}}}
            let d = resizeArray {resizeArray {resizeArray {5;6}}}
            #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
            // because of https://github.com/fable-compiler/Fable/issues/3718
            assertThat (ResizeArray.equals c d) (tag "equals does check inner array in Fable" >> isTrue)
            #else
            assertThat (ResizeArray.equals c d) (tag "equals doesn't check inner array .NET" >> isFalse)
            #endif

        )

        // -- xs.LastIndex --
        test ("LastIndex doesn't raises exception on empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>()
            let r =  xs.LastIndex
            assertThat -1 (tag "Expected -1" >> isEqualTo r)
        )

        test ("LastIndex returns Count - 1 on non-empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let lastIndex = xs.LastIndex
            assertThat lastIndex (tag "Expected LastIndex to be equal to Count - 1" >> isEqualTo (xs.Count - 1))
        )

        //---- xs.Last ----
        test ("Last getter raises exception on empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.Last |> ignore
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("Last setter raises exception on empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.Last <- 1
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("Last getter returns last item on non-empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let lastItem = xs.Last
            assertThat lastItem (tag "Expected Last to be equal to the last item in the ResizeArray" >> isEqualTo 5)
        )

        test ("Last setter changes last item on non-empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.Last <- 6
            assertThat xs.Last (tag "Expected Last to be changed to the new value" >> isEqualTo 6)
        )

        //---- xs.SecondLast ----
        test ("SecondLast getter raises exception on ResizeArray with less than 2 items", fun _ ->
            let xs = ResizeArray<int>([1])
            let testCode = fun () -> xs.SecondLast |> ignore
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("SecondLast setter raises exception on ResizeArray with less than 2 items", fun _ ->
            let xs = ResizeArray<int>([1])
            let testCode = fun () -> xs.SecondLast <- 1
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("SecondLast getter returns second last item on ResizeArray with 2 or more items", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let secondLastItem = xs.SecondLast
            assertThat secondLastItem (tag "Expected SecondLast to be equal to the second last item in the ResizeArray" >> isEqualTo 4)
        )

        test ("SecondLast setter changes second last item on ResizeArray with 2 or more items", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.SecondLast <- 6
            assertThat xs.SecondLast (tag "Expected SecondLast to be changed to the new value" >> isEqualTo 6)
        )

        //---- xs.ThirdLast ----
        test ("ThirdLast getter raises exception on ResizeArray with less than 3 items", fun _ ->
            let xs = ResizeArray<int>([1; 2])
            let testCode = fun () -> xs.ThirdLast |> ignore
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("ThirdLast setter raises exception on ResizeArray with less than 3 items", fun _ ->
            let xs = ResizeArray<int>([1; 2])
            let testCode = fun () -> xs.ThirdLast <- 1
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("ThirdLast getter returns third last item on ResizeArray with 3 or more items", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let thirdLastItem = xs.ThirdLast
            assertThat thirdLastItem (tag "Expected ThirdLast to be equal to the third last item in the ResizeArray" >> isEqualTo 3)
        )

        test ("ThirdLast setter changes third last item on ResizeArray with 3 or more items", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.ThirdLast <- 6
            assertThat xs.ThirdLast (tag "Expected ThirdLast to be changed to the new value" >> isEqualTo 6)
        )

        //---- xs.First ----
        test ("First getter raises exception on empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.First |> ignore
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("First setter raises exception on empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.First <- 1
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("First getter returns first item on non-empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let firstItem = xs.First
            assertThat firstItem (tag "Expected First to be equal to the first item in the ResizeArray" >> isEqualTo 1)
        )

        test ("First setter changes first item on non-empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.First <- 6
            assertThat xs.First (tag "Expected First to be changed to the new value" >> isEqualTo 6)
        )

        //---- xs.FirstAndOnly ----
        test ("FirstAndOnly getter raises exception on empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.FirstAndOnly |> ignore
            assertThat testCode (tag "Expected an IndexOutOfRangeException" >> throws)
        )

        test ("FirstAndOnly getter raises exception on ResizeArray with more than one item", fun _ ->
            let xs = ResizeArray<int>([1; 2])
            let testCode = fun () -> xs.FirstAndOnly |> ignore
            assertThat testCode (tag "Expected an IndexOutOfRangeException" >> throws)
        )

        test ("FirstAndOnly error message says exactly one item is expected", fun _ ->
            throwsIdx (fun () -> ResizeArray<int>().FirstAndOnly |> ignore)
            throwsIdx (fun () -> ResizeArray<int>([1; 2]).FirstAndOnly |> ignore)
            throwsWith ["ResizeArray.FirstAndOnly: Expected exactly one item"; "empty ResizeArray<"] (fun () -> ResizeArray<int>().FirstAndOnly |> ignore)
            throwsWith ["ResizeArray.FirstAndOnly: Expected exactly one item"; "with 2 items"] (fun () -> ResizeArray<int>([1; 2]).FirstAndOnly |> ignore)
            throwsWith ["ResizeArray.firstAndOnly: input is null"] (fun () -> ResizeArray.firstAndOnly (null: ResizeArray<int>) |> ignore)
        )

        test ("FirstAndOnly getter returns the item on ResizeArray with exactly one item", fun _ ->
            let xs = ResizeArray<int>([1])
            let firstAndOnlyItem = xs.FirstAndOnly
            assertThat firstAndOnlyItem (tag "Expected FirstAndOnly to be equal to the only item in the ResizeArray" >> isEqualTo 1)
        )

        //---- xs.Second ----
        test ("Second getter raises exception on ResizeArray with less than 2 items", fun _ ->
            let xs = ResizeArray<int>([1])
            let testCode = fun () -> xs.Second |> ignore
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("Second setter raises exception on ResizeArray with less than 2 items", fun _ ->
            let xs = ResizeArray<int>([1])
            let testCode = fun () -> xs.Second <- 1
            assertThat testCode (tag "Expected an ArgumentException" >> throws)
        )

        test ("Second getter returns second item on ResizeArray with 2 or more items", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let secondItem = xs.Second
            assertThat secondItem (tag "Expected Second to be equal to the second item in the ResizeArray" >> isEqualTo 2)
        )

        test ("Second setter changes second item on ResizeArray with 2 or more items", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.Second <- 6
            assertThat xs.Second (tag "Expected Second to be changed to the new value" >> isEqualTo 6)
        )

        //---- xs.Third ----
        // Similar tests can be written for Third

        //---- xs.IsEmpty ----
        test ("IsEmpty returns true for empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>()
            assertThat xs.IsEmpty (tag "Expected IsEmpty to be true for an empty ResizeArray" >> isTrue)
        )

        test ("IsEmpty returns false for non-empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1])
            assertThat xs.IsEmpty (tag "Expected IsEmpty to be false for a non-empty ResizeArray" >> isFalse)
        )

        //---- xs.IsSingleton ----
        test ("IsSingleton returns true for ResizeArray with one item", fun _ ->
            let xs = ResizeArray<int>([1])
            assertThat xs.IsSingleton (tag "Expected IsSingleton to be true for a ResizeArray with one item" >> isTrue)
        )

        test ("IsSingleton returns false for ResizeArray with zero or more than one items", fun _ ->
            let xs = ResizeArray<int>([1; 2])
            assertThat xs.IsSingleton (tag "Expected IsSingleton to be false for a ResizeArray with zero or more than one items" >> isFalse)
        )

        //---- xs.IsNotEmpty ----
        test ("IsNotEmpty returns false for empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>()
            assertThat xs.IsNotEmpty (tag "Expected IsNotEmpty to be false for an empty ResizeArray" >> isFalse)
        )

        test ("IsNotEmpty returns true for non-empty ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1])
            assertThat xs.IsNotEmpty (tag "Expected IsNotEmpty to be true for a non-empty ResizeArray" >> isTrue)
        )

        //---- xs.InsertAtStart ----
        test ("InsertAtStart inserts item at the beginning of the ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            xs.InsertAtStart 0
            assertThat xs.First (tag "Expected InsertAtStart to insert the item at the beginning of the ResizeArray" >> isEqualTo 0)
        )

        //---- xs.GetNeg ----
        test ("GetNeg gets an item in the ResizeArray by index, allowing for negative index", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let item = xs.GetNeg -1
            assertThat item (tag "Expected GetNeg to get the last item in the ResizeArray when index is -1" >> isEqualTo 3)
        )

        //---- xs.SetNeg ----
        test ("SetNeg sets an item in the ResizeArray by index, allowing for negative index", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            xs.SetNeg -1 4
            assertThat xs.Last (tag "Expected SetNeg to set the last item in the ResizeArray when index is -1" >> isEqualTo 4)
        )

        //---- xs.GetLooped ----
        test ("GetLooped gets an item in the ResizeArray by index, treating the ResizeArray as an endless loop", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let item = xs.GetLooped 3
            assertThat item (tag "Expected GetLooped to get the first item in the ResizeArray when index is equal to the count of the ResizeArray" >> isEqualTo 1)
        )

        //---- xs.SetLooped ----
        test ("SetLooped sets an item in the ResizeArray by index, treating the ResizeArray as an endless loop", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            xs.SetLooped 3 4
            assertThat xs.First (tag "Expected SetLooped to set the first item in the ResizeArray when index is equal to the count of the ResizeArray" >> isEqualTo 4)
        )

        //---- xs.Pop ----
        test ("Pop gets and removes the last item from the ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let item = xs.Pop()
            assertThat item (tag "Expected Pop to get the last item in the ResizeArray" >> isEqualTo 3)
            assertThat xs.Count (tag "Expected Pop to remove the last item from the ResizeArray" >> isEqualTo 2)
        )

        //---- xs.Clone ----
        test ("Clone creates a shallow copy of the ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let ys = xs.Clone()
            assertThat (ys.IsEqualTo xs) (tag "Expected Clone to create a shallow copy of the ResizeArray" >> isTrue)
        )

        //---- xs.GetReverseIndex ----
        test ("GetReverseIndex gets the index for the element offset elements away from the end of the ResizeArray", fun _ ->
            let xs = ResizeArray<int>([00; 11; 22; 33])
            let item = xs.[^1]
            assertThat item (tag "Expected GetReverseIndex to get the index of the second last item in the ResizeArray when offset is 1" >> isEqualTo 22)
        )

        //---- xs.GetSlice ----
        test ("GetSlice gets a slice from the ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let slice = xs[1..3]
            assertThat (slice.IsEqualTo (ResizeArray<int>([2; 3; 4]))) (tag "Expected GetSlice to get a slice from the ResizeArray" >> isTrue)
        )

        test ("SliceIdx uses an inclusive end index", fun _ ->
            let xs = ResizeArray<int>([0..4])
            assertThat ((xs.SliceIdx(0, 0)).IsEqualTo(ResizeArray<int>([0]))) (tag "Expected the first item" >> isTrue)
            assertThat ((xs.SliceIdx(1, 3)).IsEqualTo(ResizeArray<int>([1; 2; 3]))) (tag "Expected indices 1 through 3" >> isTrue)
            assertThat ((xs.SliceIdx(2, 4)).IsEqualTo(ResizeArray<int>([2; 3; 4]))) (tag "Expected indices 2 through 4" >> isTrue)
            assertThat ((xs.SliceIdx(0, 4)).IsEqualTo(ResizeArray<int>([0..4]))) (tag "Expected the full range" >> isTrue)
            assertThat ((xs.SliceIdx(4, 4)).IsEqualTo(ResizeArray<int>([4]))) (tag "Expected the final item" >> isTrue)
        )

        test ("SliceIdx rejects invalid ranges", fun _ ->
            let xs = ResizeArray<int>([0..4])
            assertThat (fun () -> xs.SliceIdx(-1, 2) |> ignore) (tag "Expected a negative start index to fail" >> throws)
            assertThat (fun () -> xs.SliceIdx(0, -1) |> ignore) (tag "Expected a negative end index to fail" >> throws)
            assertThat (fun () -> xs.SliceIdx(5, 5) |> ignore) (tag "Expected a start index at Count to fail" >> throws)
            assertThat (fun () -> xs.SliceIdx(0, 5) |> ignore) (tag "Expected an end index past Count to fail" >> throws)
            assertThat (fun () -> xs.SliceIdx(3, 2) |> ignore) (tag "Expected start greater than end to fail" >> throws)
            assertThat (fun () -> ResizeArray<int>().SliceIdx(0, 0) |> ignore) (tag "Expected slicing an empty ResizeArray to fail" >> throws)
        )

        //---- xs.SetSlice ----
        test ("SetSlice sets a slice in the ResizeArray", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let newValues = ResizeArray<int>([6; 7; 8])
            xs[1..3] <-  newValues
            assertThat (xs.IsEqualTo (ResizeArray<int>([1; 6; 7; 8; 5]))) (tag "Expected SetSlice to set a slice in the ResizeArray" >> isTrue)
        )

        //---- xs.Copy, xs.Clone ----
        test ("Copy and Clone create independent shallow copies", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            for name, copy in ["Copy", xs.Copy(); "Clone", xs.Clone(); "obsolete Duplicate", obsoleteDuplicate xs] do
                assertThat (copy.IsEqualTo xs) (tag $"{name} has the same items" >> isTrue)
                assertThat (obj.ReferenceEquals(xs, copy)) (tag $"{name} creates a new ResizeArray" >> isFalse)
                copy.[0] <- 99
                assertThat xs.[0] (tag $"{name} does not change the original" >> isEqualTo 1)
        )

        //---- xs.ToString(n) ----
        test ("ToString(n) prints n entries, then ... and the last entry", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let s = xs.ToString(2).Replace("\r\n", "\n").Trim()
            assertThat s (tag "two entries and the last" >> isEqualTo "ResizeArray<Int32> with 5 items:\n  0: 1\n  1: 2\n  ...\n  4: 5")
        )

        test ("ToString(Int32.MaxValue) prints all entries once", fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let s = xs.ToString(System.Int32.MaxValue).Replace("\r\n", "\n").Trim()
            assertThat s (tag "all entries, no ellipsis" >> isEqualTo "ResizeArray<Int32> with 3 items:\n  0: 1\n  1: 2\n  2: 3")
        )

    ])
