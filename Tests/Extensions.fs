namespace Tests
module Extensions =

    open ResizeArrayT


    #if FABLE_COMPILER
    open Fable.Mocha
    #else
    open Expecto
    #endif

    let tests = // : Test in Expect , TestCase in Mocha
      testList "extensions Tests" [

        test "Intro: 9=9" {Expect.equal 9 9 "Intro"}

        let a = resizeArray{ for i in 0 .. 9 ->  float i }
        let b = ResizeArray.init 10 (fun i -> float i)

        test "Get" {
            Expect.equal (a.Get 2) 2.0 "Get 2"
            Expect.equal (a.Get 2) a[2] "Get 2 Item"
            Expect.throws (fun () -> a.Get 10 |> ignore ) "Get 10"
            Expect.throws (fun () -> a.Get -1 |> ignore ) "Get -1"

        }
        test "Set" {
            let a = a.Clone()
            a.Set 2 3.0
            Expect.equal (a.Get 2) 3.0 "Set 2"
            a[2] <- 4.0
            Expect.equal (a.Get 2) 4.0 "Set 2 Item"
            Expect.throws (fun () -> a.Set 10 0.0 |> ignore ) "Set 10"
            Expect.throws (fun () -> a.Set -1 0.0 |> ignore ) "Set -1"
        }

        test "IsEqualTo" {
            Expect.isTrue (a.IsEqualTo b) "IsEqualTo"
            Expect.isTrue (a.IsEqualTo a) "IsEqualTo self"
            Expect.isTrue (a.IsEqualTo (a.Clone())) "IsEqualTo Clone"
            let b = a.Clone()
            b.Set 2 9.9
            Expect.isFalse (a.IsEqualTo b) "index 2 was set to 9.9"

            let bb = resizeArray {a.Clone()}
            let aa = resizeArray {a.Clone()}
            Expect.isTrue (ResizeArray.equals2 aa bb) "equals 2"

            let bb = resizeArray {a.Clone()}
            let aa = resizeArray {bb.First}
            Expect.isTrue (ResizeArray.equals2 aa bb) "equals 2"

            let c = resizeArray {resizeArray {resizeArray {5;6}}}
            let d = resizeArray {resizeArray {resizeArray {5;6}}}
            #if FABLE_COMPILER
            // because of https://github.com/fable-compiler/Fable/issues/3718
            Expect.isTrue (ResizeArray.equals c d) "equals does check inner array in Fable"
            #else
            Expect.isFalse (ResizeArray.equals c d) "equals doesn't check inner array .NET"
            #endif

        }

        // -- xs.LastIndex --
        testCase "LastIndex doesn't raises exception on empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>()
            let r =  xs.LastIndex
            Expect.equal -1 r "Expected -1"

        testCase "LastIndex returns Count - 1 on non-empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let lastIndex = xs.LastIndex
            Expect.equal lastIndex (xs.Count - 1) "Expected LastIndex to be equal to Count - 1"

        //---- xs.Last ----
        testCase "Last getter raises exception on empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.Last |> ignore
            Expect.throws testCode "Expected an ArgumentException"

        testCase "Last setter raises exception on empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.Last <- 1
            Expect.throws testCode "Expected an ArgumentException"

        testCase "Last getter returns last item on non-empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let lastItem = xs.Last
            Expect.equal lastItem 5 "Expected Last to be equal to the last item in the ResizeArray"

        testCase "Last setter changes last item on non-empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.Last <- 6
            Expect.equal xs.Last 6 "Expected Last to be changed to the new value"

        //---- xs.SecondLast ----
        testCase "SecondLast getter raises exception on ResizeArray with less than 2 items" <| fun _ ->
            let xs = ResizeArray<int>([1])
            let testCode = fun () -> xs.SecondLast |> ignore
            Expect.throws testCode "Expected an ArgumentException"

        testCase "SecondLast setter raises exception on ResizeArray with less than 2 items" <| fun _ ->
            let xs = ResizeArray<int>([1])
            let testCode = fun () -> xs.SecondLast <- 1
            Expect.throws testCode "Expected an ArgumentException"

        testCase "SecondLast getter returns second last item on ResizeArray with 2 or more items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let secondLastItem = xs.SecondLast
            Expect.equal secondLastItem 4 "Expected SecondLast to be equal to the second last item in the ResizeArray"

        testCase "SecondLast setter changes second last item on ResizeArray with 2 or more items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.SecondLast <- 6
            Expect.equal xs.SecondLast 6 "Expected SecondLast to be changed to the new value"

        //---- xs.ThirdLast ----
        testCase "ThirdLast getter raises exception on ResizeArray with less than 3 items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2])
            let testCode = fun () -> xs.ThirdLast |> ignore
            Expect.throws testCode "Expected an ArgumentException"

        testCase "ThirdLast setter raises exception on ResizeArray with less than 3 items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2])
            let testCode = fun () -> xs.ThirdLast <- 1
            Expect.throws testCode "Expected an ArgumentException"

        testCase "ThirdLast getter returns third last item on ResizeArray with 3 or more items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let thirdLastItem = xs.ThirdLast
            Expect.equal thirdLastItem 3 "Expected ThirdLast to be equal to the third last item in the ResizeArray"

        testCase "ThirdLast setter changes third last item on ResizeArray with 3 or more items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.ThirdLast <- 6
            Expect.equal xs.ThirdLast 6 "Expected ThirdLast to be changed to the new value"

        //---- xs.First ----
        testCase "First getter raises exception on empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.First |> ignore
            Expect.throws testCode "Expected an ArgumentException"

        testCase "First setter raises exception on empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.First <- 1
            Expect.throws testCode "Expected an ArgumentException"

        testCase "First getter returns first item on non-empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let firstItem = xs.First
            Expect.equal firstItem 1 "Expected First to be equal to the first item in the ResizeArray"

        testCase "First setter changes first item on non-empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.First <- 6
            Expect.equal xs.First 6 "Expected First to be changed to the new value"

        //---- xs.FirstAndOnly ----
        testCase "FirstAndOnly getter raises exception on empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>()
            let testCode = fun () -> xs.FirstAndOnly |> ignore
            Expect.throws testCode "Expected an ArgumentException"

        testCase "FirstAndOnly getter raises exception on ResizeArray with more than one item" <| fun _ ->
            let xs = ResizeArray<int>([1; 2])
            let testCode = fun () -> xs.FirstAndOnly |> ignore
            Expect.throws testCode "Expected an ArgumentException"

        testCase "FirstAndOnly getter returns the item on ResizeArray with exactly one item" <| fun _ ->
            let xs = ResizeArray<int>([1])
            let firstAndOnlyItem = xs.FirstAndOnly
            Expect.equal firstAndOnlyItem 1 "Expected FirstAndOnly to be equal to the only item in the ResizeArray"

        //---- xs.Second ----
        testCase "Second getter raises exception on ResizeArray with less than 2 items" <| fun _ ->
            let xs = ResizeArray<int>([1])
            let testCode = fun () -> xs.Second |> ignore
            Expect.throws testCode "Expected an ArgumentException"

        testCase "Second setter raises exception on ResizeArray with less than 2 items" <| fun _ ->
            let xs = ResizeArray<int>([1])
            let testCode = fun () -> xs.Second <- 1
            Expect.throws testCode "Expected an ArgumentException"

        testCase "Second getter returns second item on ResizeArray with 2 or more items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let secondItem = xs.Second
            Expect.equal secondItem 2 "Expected Second to be equal to the second item in the ResizeArray"

        testCase "Second setter changes second item on ResizeArray with 2 or more items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            xs.Second <- 6
            Expect.equal xs.Second 6 "Expected Second to be changed to the new value"

        //---- xs.Third ----
        // Similar tests can be written for Third

        //---- xs.IsEmpty ----
        testCase "IsEmpty returns true for empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>()
            Expect.isTrue xs.IsEmpty "Expected IsEmpty to be true for an empty ResizeArray"

        testCase "IsEmpty returns false for non-empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1])
            Expect.isFalse xs.IsEmpty "Expected IsEmpty to be false for a non-empty ResizeArray"

        //---- xs.IsSingleton ----
        testCase "IsSingleton returns true for ResizeArray with one item" <| fun _ ->
            let xs = ResizeArray<int>([1])
            Expect.isTrue xs.IsSingleton "Expected IsSingleton to be true for a ResizeArray with one item"

        testCase "IsSingleton returns false for ResizeArray with zero or more than one items" <| fun _ ->
            let xs = ResizeArray<int>([1; 2])
            Expect.isFalse xs.IsSingleton "Expected IsSingleton to be false for a ResizeArray with zero or more than one items"

        //---- xs.IsNotEmpty ----
        testCase "IsNotEmpty returns false for empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>()
            Expect.isFalse xs.IsNotEmpty "Expected IsNotEmpty to be false for an empty ResizeArray"

        testCase "IsNotEmpty returns true for non-empty ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1])
            Expect.isTrue xs.IsNotEmpty "Expected IsNotEmpty to be true for a non-empty ResizeArray"

        //---- xs.InsertAtStart ----
        testCase "InsertAtStart inserts item at the beginning of the ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            xs.InsertAtStart 0
            Expect.equal xs.First 0 "Expected InsertAtStart to insert the item at the beginning of the ResizeArray"

        //---- xs.GetNeg ----
        testCase "GetNeg gets an item in the ResizeArray by index, allowing for negative index" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let item = xs.GetNeg -1
            Expect.equal item 3 "Expected GetNeg to get the last item in the ResizeArray when index is -1"

        //---- xs.SetNeg ----
        testCase "SetNeg sets an item in the ResizeArray by index, allowing for negative index" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            xs.SetNeg -1 4
            Expect.equal xs.Last 4 "Expected SetNeg to set the last item in the ResizeArray when index is -1"

        //---- xs.GetLooped ----
        testCase "GetLooped gets an item in the ResizeArray by index, treating the ResizeArray as an endless loop" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let item = xs.GetLooped 3
            Expect.equal item 1 "Expected GetLooped to get the first item in the ResizeArray when index is equal to the count of the ResizeArray"

        //---- xs.SetLooped ----
        testCase "SetLooped sets an item in the ResizeArray by index, treating the ResizeArray as an endless loop" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            xs.SetLooped 3 4
            Expect.equal xs.First 4 "Expected SetLooped to set the first item in the ResizeArray when index is equal to the count of the ResizeArray"

        //---- xs.Pop ----
        testCase "Pop gets and removes the last item from the ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let item = xs.Pop()
            Expect.equal item 3 "Expected Pop to get the last item in the ResizeArray"
            Expect.equal xs.Count 2 "Expected Pop to remove the last item from the ResizeArray"

        //---- xs.Clone ----
        testCase "Clone creates a shallow copy of the ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3])
            let ys = xs.Clone()
            Expect.isTrue (ys.IsEqualTo xs) "Expected Clone to create a shallow copy of the ResizeArray"

        //---- xs.GetReverseIndex ----
        testCase "GetReverseIndex gets the index for the element offset elements away from the end of the ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([00; 11; 22; 33])
            let item = xs.[^1]
            Expect.equal item 22 "Expected GetReverseIndex to get the index of the second last item in the ResizeArray when offset is 1"

        //---- xs.GetSlice ----
        testCase "GetSlice gets a slice from the ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let slice = xs[1..3]
            Expect.isTrue (slice.IsEqualTo (ResizeArray<int>([2; 3; 4]))) "Expected GetSlice to get a slice from the ResizeArray"

        //---- xs.SetSlice ----
        testCase "SetSlice sets a slice in the ResizeArray" <| fun _ ->
            let xs = ResizeArray<int>([1; 2; 3; 4; 5])
            let newValues = ResizeArray<int>([6; 7; 8])
            xs[1..3] <-  newValues
            Expect.isTrue (xs.IsEqualTo (ResizeArray<int>([1; 6; 7; 8; 5]))) "Expected SetSlice to set a slice in the ResizeArray"

    ]
