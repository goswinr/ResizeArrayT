namespace ResizeArrayT

open System
open System.Collections.Generic
//open NiceString


//https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs


#nowarn "44" //for opening the hidden but not Obsolete UtilResizeArray module
open UtilResizeArray

/// This module has only one conversion function. Array.asResizeArray
module Array =

    /// Builds a new ResizeArray from the given Array.
    /// In Fable-JavaScript the ResizeArray is just casted to an Array without allocating a new ResizeArray.
    let inline asResizeArray (arr: 'T[]) : ResizeArray<'T> =
        #if FABLE_COMPILER_JAVASCRIPT
            unbox arr
        #else
            if isNull arr then nullExn ": Array.asResizeArray"
            let l = ResizeArray(arr.Length)
            for i = 0 to arr.Length - 1 do
                l.Add arr.[i]
            l
        #endif

/// The main module for functions on ResizeArray.
/// A ResizeArray is a System.Collections.Generic.List<'T>.
/// This module has all functions from the FSharp.Core.Array module implemented for ResizeArray. And more.
module ResizeArray =


    /// <summary>Gets an element from an Array. (Use Array.getNeg(i) function if you want to use negative indices too.)</summary>
    /// <param name="index">The input index.</param>
    /// <param name="arr">The input Array.</param>
    /// <returns>The value of the Array at the given index.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is negative or the input Array does not contain enough elements.</exception>
    let inline get index (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "get"
        arr.Get index


    /// <summary>Sets an element of a Array. (use Array.setNeg(i) function if you want to use negative indices too)</summary>
    /// <param name="index">The input index.</param>
    /// <param name="value">The input value.</param>
    /// <param name="arr">The input Array.</param>
    /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is negative or the input Array does not contain enough elements.</exception>
    let inline set index value (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "set"
        arr.Set index value


    //---------------------------------------------------
    // functions added that are not in FSharp.Core Array module)
    //----------------------------------------------------

    /// Raises an Exception if the Array is empty.
    /// (Useful for chaining)
    /// Returns the input Array
    let inline failIfEmpty (errorMessage: string) (arr: ResizeArray<'T>) =
        if arr.Count = 0 then
            failSimpel ("Array.FailIfEmpty: " + errorMessage)
        arr

    /// Raises an Exception if the Array has less then count items.
    /// (Useful for chaining)
    /// Returns the input Array
    let failIfLessThan (count) (errorMessage: string) (arr: ResizeArray<'T>) =
        if arr.Count < count then
            failSimpel $"Array.FailIfLessThan {count}: {errorMessage}"
        arr


    /// Gets an item in the Array by index.
    /// Allows for negative index too ( -1 is last item,  like Python)
    /// (a negative index can also be done with '^' prefix. E.g. ^0 for the last item)
    let inline getNeg index (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "getNeg"
        arr.GetNeg index


    /// Sets an item in the Array by index.
    /// Allows for negative index too ( -1 is last item,  like Python)
    /// (a negative index can also be done with '^' prefix. E.g. ^0 for the last item)
    let inline setNeg index value (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "setNeg"
        arr.SetNeg index value

    /// Any index will return a value.
    /// Array is treated as an endless loop in positive and negative direction
    let inline getLooped index (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "getLooped"
        arr.GetLooped index

    /// Any index will set a value.
    /// Array is treated as an endless loop in positive and negative direction
    let inline setLooped index value (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "setLooped"
        arr.SetLooped index value


    /// Gets the second last item in the Array.
    /// Same as this.[this.Count - 2]
    let inline secondLast (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "secondLast"
        arr.SecondLast

    /// Gets the third last item in the Array.
    /// Same as this.[this.Count - 3]
    let inline thirdLast (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "thirdLast"
        arr.ThirdLast

    /// Gets the first item in the Array.
    /// Same as this.[0]
    let inline first (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "first"
        arr.First

    /// Gets the the only item in the Array.
    /// Fails if the Array does not have exactly one element.
    let inline firstAndOnly (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "firstAndOnly"
        arr.FirstAndOnly

    /// Gets the second item in the Array.
    /// Same as this.[1]
    let inline second (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "second"
        arr.Second

    /// Gets the third item in the Array.
    /// Same as this.[2]
    let inline third (arr: ResizeArray<'T>) =
        if isNull arr then nullExn "third"
        arr.Third


    /// <summary>
    /// Give start and end index. The resulting ResizeArray includes the value at end index too.
    /// This function will fail on out of bound indices, while the F# slicing notation xs.[1..3] will not.
    /// To use negative indices or out of range indices use the ResizeArray.sliceLooped function.
    /// </summary>
    let sliceIdx (startIdx:int) (endIdx: int) (xs: ResizeArray<'T>): ResizeArray<'T> =
        let count = xs.Count
        let st  = startIdx //if startIdx< 0 then count + startIdx        else startIdx
        let len = endIdx   //if endIdx  < 0 then count + endIdx - st + 1 else endIdx - st + 1
        if st < 0 || st > count - 1 then
            failIdx xs $"sliceIdx: Start index {startIdx} is out of range. Allowed values are -{count} up to {count-1} for ResizeArray of {count} items"

        if st+len > count then
            failIdx xs $"sliceIdx: End index {endIdx} is out of range. Allowed values are -{count} up to {count-1} for ResizeArray of {count} items"

        if len < 0 then
            // let en = if endIdx<0 then count+endIdx else endIdx
            // let err = sprintf "ResizeArray.Slice: Start index '%A' (= %d) is bigger than end index '%A'(= %d) for ResizeArray of %d items" startIdx st endIdx en  count
            failIdx xs $"sliceIdx: Start index {startIdx} is bigger than end index {endIdx} for ResizeArray of {count} items"

        // ResizeArray.init len (fun i -> this.[st+i])
        xs.GetRange(st, len)

    /// <summary>
    /// Give start and end index. The resulting ResizeArray includes the value at end index too.
    /// Any index is valid, out of range indices set into range using modulo.
    /// Allows for negative indices too. ( -1 is last item, like in Python)
    /// The resulting ResizeArray includes the end index.
    /// For empty input ResizeArray an empty ResizeArray is returned.
    /// </summary>
    /// <remarks>
    /// Alternative: with F# slicing notation (e.g. a.[1..3])
    /// With F# preview features enabled a negative index can also be done with '^' prefix. E.g. ^0 for the last item.
    /// </remarks>
    let sliceLooped(startIdx:int) (endIdx:int ) (xs: ResizeArray<'T>): ResizeArray<'T> =
        if xs.Count = 0 then
            ResizeArray<'T>()
        else
            let count = xs.Count
            let st = UtilResizeArray.negIdxLooped startIdx count
            let en = UtilResizeArray.negIdxLooped endIdx count
            let len = en - st + 1
            if len < 0 then
                ResizeArray<'T>()
            else
                xs.GetRange(st, len)

    (*
    // Slice the Array given start and end index.
    // Allows for negative indices too. ( -1 is last item, like Python)
    // The resulting Array includes the end index.
    // Raises an IndexOutOfRangeException if indices are out of range.
    // If you don't want an exception to be raised for index overflow or overlap use Array.trim.
    // (A negative index can also be done with '^' prefix. E.g. ^0 for the last item, when F# Language preview features are enabled.)
    let slice startIdx endIdx (arr: ResizeArray<'T>) : ResizeArray<'T> =
            if isNull arr then nullExn "slice"
        #if FABLE_COMPILER
            let count = arr.Count
            let st  = if startIdx< 0 then count + startIdx        else startIdx
            let len = if endIdx  < 0 then count + endIdx - st + 1 else endIdx - st + 1
            if st < 0 || st > count - 1 then
                failIdx arr $"Slice: Start index {startIdx} is out of range. Allowed values are -{count} up to {count-1} for ResizeArray of {count} items"

            if st+len > count then
                failIdx arr $"Slice: End index {endIdx} is out of range. Allowed values are -{count} up to {count-1} for ResizeArray of {count} items"

            if len < 0 then
                // let en = if endIdx<0 then count+endIdx else endIdx
                // let err = sprintf "ResizeArray.Slice: Start index '%A' (= %d) is bigger than end index '%A'(= %d) for ResizeArray of %d items" startIdx st endIdx en  count
                failIdx arr $"Slice: Start index {startIdx} is bigger than end index {endIdx} for ResizeArray of {count} items"

            // ResizeArray.init len (fun i -> this.[st+i])
            arr.GetRange(st, len)
        #else
            arr.Slice(startIdx, endIdx)
        #endif
    *)

    /// Trim items from start and end.
    /// If the sum of fromStartCount and fromEndCount is bigger than arr.Count it returns an empty Array.
    /// If you want an exception to be raised for index overlap (total trimming is bigger than count) use Array.slice with negative end index.
    let trim fromStartCount fromEndCount (arr: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull arr then nullExn "trim"
        if fromStartCount < 0 then
            fail arr $"trim: fromStartCount can't be negative: {fromStartCount}"
        if fromEndCount < 0 then
            fail arr $"trim: fromEndCount can't be negative: {fromEndCount}"
        let c = arr.Count
        if fromStartCount + fromEndCount >= c then
            ResizeArray<'T>(0)
        else
            arr.GetRange(fromStartCount, c - fromStartCount - fromEndCount)



    //------------------------------------------------------------------
    //---------------------prev-this-next ------------------------------
    //------------------------------------------------------------------
    // these functions below also exist on Seq module in FsEx:

    /// Yields Seq from (first, second)  up to (second-last, last).
    /// Not looped.
    /// The resulting seq is one element shorter than the input ResizeArray.
    let windowed2 (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "windowed2"
        if resizeArray.Count < 2 then
            fail resizeArray "windowed2 input has less than two items"
        seq {
            for i = 0 to resizeArray.Count - 2 do
                resizeArray.[i], resizeArray.[i + 1]
        }

    /// Yields looped Seq from (first, second)  up to (last, first).
    /// The resulting seq has the same element count as the input ResizeArray.
    let thisNext (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "thisNext"
        if resizeArray.Count < 2 then
            fail resizeArray "thisNext input has less than two items"
        seq {
            for i = 0 to resizeArray.Count - 2 do
                resizeArray.[i], resizeArray.[i + 1]
            resizeArray.[resizeArray.Count - 1], resizeArray.[0]
        }

    /// Yields looped Seq from (last,first)  up to (second-last, last).
    /// The resulting seq has the same element count as the input ResizeArray.
    let prevThis (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "prevThis"
        if resizeArray.Count < 2 then
            fail resizeArray "prevThis input has less than two items"
        seq {
            resizeArray.[resizeArray.Count - 1], resizeArray.[0]
            for i = 0 to resizeArray.Count - 2 do
                resizeArray.[i], resizeArray.[i + 1]
        }

    /// Yields Seq from (first, second, third)  up to (third-last, second-last, last).
    /// Not looped.
    /// The resulting seq is two elements shorter than the input ResizeArray.
    let windowed3 (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "windowed3"
        if resizeArray.Count < 3 then
            fail resizeArray "windowed3 input has less than three items"
        seq {
            for i = 0 to resizeArray.Count - 3 do
                resizeArray.[i], resizeArray.[i + 1], resizeArray.[i + 2]
        }

    /// Yields looped Seq of  from (last, first, second)  up to (second-last, last, first).
    /// The resulting seq has the same element count as the input ResizeArray.
    let prevThisNext (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "prevThisNext"
        if resizeArray.Count < 3 then
            fail resizeArray "prevThisNext input has less than three items"
        seq {
            resizeArray.[resizeArray.Count - 1], resizeArray.[0], resizeArray.[1]
            for i = 0 to resizeArray.Count - 3 do
                resizeArray.[i], resizeArray.[i + 1], resizeArray.[i + 2]
            resizeArray.[resizeArray.Count - 2], resizeArray.[resizeArray.Count - 1], resizeArray.[0]
        }

    /// Yields Seq from (0,first, second)  up to (lastIndex-1 , second-last, last).
    /// Not looped.
    /// The resulting seq is one element shorter than the input ResizeArray.
    let windowed2i (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "windowed2i"
        if resizeArray.Count < 2 then
            fail resizeArray "windowed2i input has less than two items"
        seq {
            for i = 0 to resizeArray.Count - 2 do
                i, resizeArray.[i], resizeArray.[i + 1]
        }

    /// Yields looped Seq  from (0,first, second)  up to (lastIndex, last, first).
    /// The resulting seq has the same element count as the input ResizeArray.
    let iThisNext (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "iThisNext"
        if resizeArray.Count < 2 then
            fail resizeArray "iThisNext input has less than two items"
        seq {
            for i = 0 to resizeArray.Count - 2 do
                i, resizeArray.[i], resizeArray.[i + 1]
            resizeArray.Count - 1, resizeArray.[resizeArray.Count - 1], resizeArray.[0]
        }

    /// Yields Seq from (1, first, second, third)  up to (lastIndex-1 , third-last, second-last, last).
    /// Not looped.
    /// The resulting seq is two elements shorter than the input ResizeArray.
    let windowed3i (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "windowed3i"
        if resizeArray.Count < 3 then
            fail resizeArray "windowed3i input has less than three items"
        seq {
            for i = 0 to resizeArray.Count - 3 do
                i + 1, resizeArray.[i], resizeArray.[i + 1], resizeArray.[i + 2]
        }

    /// Yields looped Seq from (1, last, first, second)  up to (lastIndex, second-last, last, first)
    /// The resulting seq has the same element count as the input ResizeArray.
    let iPrevThisNext (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "iPrevThisNext"
        if resizeArray.Count < 3 then
            fail resizeArray "iPrevThisNext input has less than three items"
        seq {
            0, resizeArray.[resizeArray.Count - 1], resizeArray.[0], resizeArray.[1]

            for i = 0 to resizeArray.Count - 3 do
                i + 1, resizeArray.[i], resizeArray.[i + 1], resizeArray.[i + 2]

            resizeArray.Count - 1, resizeArray.[resizeArray.Count - 2], resizeArray.[resizeArray.Count - 1], resizeArray.[0]
        }

    /// <summary>Returns a ResizeArray that contains one item only.</summary>
    /// <param name="value">The input item.</param>
    /// <returns>The result ResizeArray of one item.</returns>
    let inline singleton value =
        // allow null values so that ResizeArray.singleton [] is valid
        // allow null values so that ResizeArray.singleton None is valid
        let res = ResizeArray(1)
        res.Add value
        res

    /// <summary>Considers List circular and move elements up for positive integers or down for negative integers.
    /// e.g.: rotate +1 [ a, b, c, d] = [ d, a, b, c]
    /// e.g.: rotate -1 [ a, b, c, d] = [ b, c, d, a]
    /// the amount can even be bigger than the list's size. I will just rotate more than one loop.</summary>
    /// <param name="amount">How many elements to shift forward. Or backward if number is negative</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The new result ResizeArray.</returns>
    let inline rotate amount (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "rotate"
        let r = ResizeArray(resizeArray.Count)
        for i = 0 to resizeArray.Count - 1 do
            r.Add <| resizeArray.[negIdxLooped (i - amount) resizeArray.Count]
        r

    /// <summary>Considers List circular and move elements up till condition is met for the first item.
    /// The algorithm takes elements from the end and put them at the start till the first element in the list meets the condition.
    /// If the first element in the input meets the condition no changes are made. But still a shallow copy is returned.</summary>
    /// <param name="condition">The condition to meet.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The new result ResizeArray.</returns>
    let inline rotateUpTill (condition: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "rotateUpTill"
        if resizeArray.Count = 0 then
            resizeArray
        elif condition resizeArray.[0] then
            resizeArray.Clone()
        else
            let mutable j = resizeArray.Count - 1
            let mutable fi = -1
            while j >= 0 && fi = -1 do
                if condition resizeArray.[j] then
                    fi <- j
                j <- j - 1

            if fi = -1 then
                fail resizeArray "rotateUpTill: no item in the list meets the condition"


            let r = ResizeArray(resizeArray.Count)
            for i = fi to resizeArray.Count - 1 do
                r.Add <| resizeArray.[i]
            for i = 0 to fi - 1 do
                r.Add <| resizeArray.[i]
            r

    /// <summary>Considers List circular and move elements up till condition is met for the last item.
    /// The algorithm takes elements from the end and put them at the start till the last element in the list meets the condition.
    /// If the last element in the input meets the condition no changes are made. But still a shallow copy is returned.</summary>
    /// <param name="condition">The condition to meet.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The new result ResizeArray.</returns>
    let inline rotateUpTillLast (condition: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "rotateUpTillLast"
        if resizeArray.Count = 0 then
            resizeArray
        elif condition resizeArray.[resizeArray.Count - 1] then
            resizeArray.Clone()
        else
            let mutable j = resizeArray.Count - 1
            let mutable fi = -1
            while j >= 0 && fi = -1 do
                if condition resizeArray.[j] then
                    fi <- j
                j <- j - 1

            if fi = -1 then
                fail resizeArray "rotateUpTillLast: no item in the list meets the condition"
            let r = ResizeArray(resizeArray.Count)
            for i = fi + 1 to resizeArray.Count - 1 do
                r.Add <| resizeArray.[i]
            for i = 0 to fi do
                r.Add <| resizeArray.[i]

            r

    /// <summary>Considers List circular and move elements down till condition is met for the first item.
    /// The algorithm takes elements from the start and put them at the end till the first element in the list meets the condition.
    /// If the first element in the input meets the condition no changes are made. But still a shallow copy is returned.</summary>
    /// <param name="condition">The condition to meet.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The new result ResizeArray.</returns>
    let inline rotateDownTill (condition: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "rotateDownTill"
        if resizeArray.Count = 0 then
            resizeArray
        elif condition resizeArray.[0] then
            resizeArray.Clone()
        else
            let k = resizeArray.Count
            let mutable fi = -1
            let mutable j = 0
            while j < k && fi = -1 do
                j <- j + 1
                let elm = resizeArray.[j]
                if condition elm then
                    fi <- j
                    j <- k // to break the loop

            if fi = -1 then
                fail resizeArray "rotateDownTill: no item in the list meets the condition"

            let r = ResizeArray(resizeArray.Count)
            for i = fi to resizeArray.Count - 1 do
                r.Add <| resizeArray.[i]
            for i = 0 to fi - 1 do
                r.Add <| resizeArray.[i]
            r

    /// <summary>Considers List circular and move elements down till condition is met for the last item.
    /// The algorithm takes elements from the start and put them at the end till the last element in the list meets the condition.
    /// If the last element in the input meets the condition no changes are made. But still a shallow copy is returned.</summary>
    /// <param name="condition">The condition to meet.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The new result ResizeArray.</returns>
    let inline rotateDownTillLast (condition: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "rotateDownTillLast"
        if resizeArray.Count = 0 then
            resizeArray
        elif condition resizeArray.[0] then
            resizeArray.Clone()
        else
            let k = resizeArray.Count
            let mutable fi = -1
            let mutable j = 0
            while j < k && fi = -1 do
                j <- j + 1
                let elm = resizeArray.[j]
                if condition elm then
                    fi <- j
                    j <- k // to break the loop

            if fi = -1 then
                fail resizeArray "rotateDownTillLast: no item in the list meets the condition"
            let r = ResizeArray(resizeArray.Count)
            for i = fi + 1 to resizeArray.Count - 1 do
                r.Add <| resizeArray.[i]
            for i = 0 to fi do
                r.Add <| resizeArray.[i]
            r

    /// Shallow Structural equality comparison in .NET.
    /// Compares each element in both lists for equality.
    /// However nested ResizeArrays inside a ResizeArray are only compared for referential equality.
    /// (Like the default behavior of Collections.Generic.List)
    /// Does not raise ArgumentNullException if either list is null.
    /// When used in Fable (JavaScript) the ResizeArrays are always compared for full structural equality
    /// see https://github.com/fable-compiler/Fable/issues/3718
    /// Use ResizeArray.equals2 or ResizeArray.equal3 for comparing nested ResizeArrays too.
    let equals (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'T>) : bool =
        isEqualTo resizeArray1 resizeArray2 // returns true if both are null

    /// Structural equality comparison of ResizeArrays nested in ResizeArrays in .NET.
    /// Compares each element in each nested list for equality.
    /// So that two levels deep nested 'T are still compared for equality in .NET.
    /// Does not raise ArgumentNullException if either list is null.
    /// When used in Fable (JavaScript) the ResizeArrays are always compared for full structural equality
    /// see https://github.com/fable-compiler/Fable/issues/3718
    let equals2 (resizeArrays1: ResizeArray<ResizeArray<'T>>) (resizeArrays2: ResizeArray<ResizeArray<'T>>) =
        if Object.ReferenceEquals (resizeArrays1, resizeArrays2) then // returns true if both are null
            true
        elif isNull resizeArrays1 || isNull resizeArrays2 then
            false
        elif resizeArrays1.Count <> resizeArrays2.Count then
            false
        else
            let mutable i = 0
            let mutable isEqual = true
            let k = resizeArrays1.Count
            while i < k do
                let r1 = resizeArrays1.[i]
                let r2 = resizeArrays2.[i]
                i <- i + 1
                if not <| equals r1 r2 then
                    isEqual <- false
                    i <- k // break the loop
            isEqual

    /// Structural equality comparison of ResizeArrays nested in ResizeArrays nested in ResizeArrays in .NET.
    /// Compares each element in each twice nested list for equality.
    /// So that three levels deep nested 'T are still compared for equality.
    /// Does not raise ArgumentNullException if either list is null.
    /// When used in Fable (JavaScript) the ResizeArrays are always compared for full structural equality
    /// see https://github.com/fable-compiler/Fable/issues/3718
    let equals3 (resizeArrays1: ResizeArray<ResizeArray<ResizeArray<'T>>>) (resizeArrays2: ResizeArray<ResizeArray<ResizeArray<'T>>>) =
        if Object.ReferenceEquals (resizeArrays1, resizeArrays2) then // returns true if both are null
            true
        elif isNull resizeArrays1 || isNull resizeArrays2 then
            false
        elif resizeArrays1.Count <> resizeArrays2.Count then
            false
        else
            let mutable i = 0
            let mutable isEqual = true
            let k = resizeArrays1.Count
            while i < k do
                let r1 = resizeArrays1.[i]
                let r2 = resizeArrays2.[i]
                i <- i + 1
                if not <|  equals2 r1 r2 then
                    isEqual <- false
                    i <- k // break the loop
            isEqual


    /// Returns true if the given ResizeArray has just one item.
    /// Same as  ResizeArray.hasOne
    let inline isSingleton (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "isSingleton"
        resizeArray.Count = 1

    /// Returns true if the given ResizeArray has just one item.
    /// Same as  ResizeArray.isSingleton
    let inline hasOne (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "hasOne"
        resizeArray.Count = 1

    /// Returns true if the given ResizeArray is not empty.
    let inline isNotEmpty (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "isNotEmpty"
        resizeArray.Count <> 0

    /// Returns true if the given ResizeArray has count items.
    let inline hasItems count (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "hasItems"
        resizeArray.Count = count

    /// Returns true if the given ResizeArray has equal or more than count items.
    let inline hasMinimumItems count (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "hasMinimumItems"
        resizeArray.Count >= count

    /// Returns true if the given ResizeArray has equal or less than count items.
    let inline hasMaximumItems count (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "hasMaximumItems"
        resizeArray.Count <= count

    /// Swap the values of two given indices in ResizeArray
    let inline swap i j (resizeArray: ResizeArray<'T>) : unit =
        if isNull resizeArray then nullExn "swap"
        if i < 0 || j < 0 || i >= resizeArray.Count || j >= resizeArray.Count then
            fail resizeArray $"swap: index i={i} and j={j} can't be less than 0 or bigger than last index {(resizeArray.Count-1)} "
        if i <> j then
            let ti = resizeArray.[i]
            resizeArray.[i] <- resizeArray.[j]
            resizeArray.[j] <- ti


    /// internal, only for finding MinMax values
    [<RequireQualifiedAccess>]
    module private MinMax =
        //TODO test keeping of order if equal !

        (*
        let inline simple cmpF (resizeArray:ResizeArray<'T>) =
            if resizeArray.Count < 1 then
                fail resizeArray "MinMax.simple: Count must be at least one"
            let mutable m = resizeArray.[0]
            for i=1 to resizeArray.Count-1 do
                if cmpF resizeArray.[i] m then m <- resizeArray.[i]
            m
        *)

        let inline simple2 cmpF (resizeArray: ResizeArray<'T>) =
            let mutable m1 = resizeArray.[0]
            let mutable m2 = resizeArray.[1]
            for i = 1 to resizeArray.Count - 1 do
                let this = resizeArray.[i]
                if cmpF this m1 then
                    m2 <- m1
                    m1 <- this
                elif cmpF this m2 then
                    m2 <- this
            m1, m2


        /// If any are equal then the  order is kept by using ( a=b || ) since the compare operate does not include the equal test
        let inline sort3 cmp a b c =
            if a = b || cmp a b then
                if cmp b c then a, b, c
                elif cmp a c then a, c, b
                else c, a, b
            elif a = c || cmp a c then
                b, a, c
            elif b = c || cmp b c then
                b, c, a
            else
                c, b, a


        /// If any are equal then the  order is kept by using ( a=b || ) since the compare operate does not include the equal test
        let inline indexOfSort3By f cmp aa bb cc =
            let a = f aa
            let b = f bb
            let c = f cc
            if a = b || cmp a b then
                if cmp b c then 0, 1, 2
                elif cmp a c then 0, 2, 1
                else 2, 0, 1
            elif a = c || cmp a c then
                1, 0, 2
            elif b = c || cmp b c then
                1, 2, 0
            else
                2, 1, 0

        let inline simple3 cmpF (resizeArray: ResizeArray<'T>) =
            let e1 = resizeArray.[0]
            let e2 = resizeArray.[1]
            let e3 = resizeArray.[2]
            // sort first 3
            let mutable m1, m2, m3 = sort3 cmpF e1 e2 e3 // otherwise would fail on sorting first 3, test on ResizeArray([5;6;3;1;2;0])|> ResizeArray.max3
            for i = 3 to resizeArray.Count - 1 do
                let this = resizeArray.[i]
                if cmpF this m1 then
                    m3 <- m2
                    m2 <- m1
                    m1 <- this
                elif cmpF this m2 then
                    m3 <- m2
                    m2 <- this
                elif cmpF this m3 then
                    m3 <- this
            m1, m2, m3

        let inline indexByFun cmpF func (resizeArray: ResizeArray<'T>) =
            let mutable f = func resizeArray.[0]
            let mutable mf = f
            let mutable ii = 0
            for i = 1 to resizeArray.Count - 1 do
                f <- func resizeArray.[i]
                if cmpF f mf then
                    ii <- i
                    mf <- f
            ii

        let inline index2ByFun cmpF func (resizeArray: ResizeArray<'T>) =
            let mutable i1 = 0
            let mutable i2 = 1
            let mutable mf1 = func resizeArray.[i1]
            let mutable mf2 = func resizeArray.[i2]
            let mutable f = mf1 // placeholder
            for i = 1 to resizeArray.Count - 1 do
                f <- func resizeArray.[i]
                if cmpF f mf1 then
                    i2 <- i1
                    i1 <- i
                    mf2 <- mf1
                    mf1 <- f
                elif cmpF f mf2 then
                    i2 <- i
                    mf2 <- f
            i1, i2

        let inline index3ByFun (cmpOp: 'U -> 'U -> bool) (byFun: 'T -> 'U) (resizeArray: ResizeArray<'T>) =
            // sort first 3
            let mutable i1, i2, i3 = indexOfSort3By byFun cmpOp resizeArray.[0] resizeArray.[1] resizeArray.[2] // otherwise would fail on sorting first 3, test on ResizeArray([5;6;3;1;2;0])|> ResizeArray.max3
            let mutable e1 = byFun resizeArray.[i1]
            let mutable e2 = byFun resizeArray.[i2]
            let mutable e3 = byFun resizeArray.[i3]
            let mutable f = e1 // placeholder
            for i = 3 to resizeArray.Count - 1 do
                f <- byFun resizeArray.[i]
                if cmpOp f e1 then
                    i3 <- i2
                    i2 <- i1
                    i1 <- i
                    e3 <- e2
                    e2 <- e1
                    e1 <- f
                elif cmpOp f e2 then
                    i3 <- i2
                    i2 <- i
                    e3 <- e2
                    e2 <- f
                elif cmpOp f e3 then
                    i3 <- i
                    e3 <- f
            i1, i2, i3

    (* covered by part copied from Array module:
        let min resizeArray =     resizeArray |> MinMax.simple (<)
        let max resizeArray =     resizeArray |> MinMax.simple (>)
        let minBy f resizeArray = let i = resizeArray |> MinMax.indexByFun (<) f in resizeArray.[i]
        let maxBy f resizeArray = let i = resizeArray |> MinMax.indexByFun (>) f in resizeArray.[i]
        *)


    /// <summary>Returns the index of the smallest of all elements of the ResizeArray, compared via Operators.max on the function result.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    /// <returns>The index of the smallest element.</returns>
    let inline minIndexBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : int =
        if isNull resizeArray then nullExn "minIndexBy"
        resizeArray |> MinMax.indexByFun (<) projection

    /// <summary>Returns the index of the greatest of all elements of the ResizeArray, compared via Operators.max on the function result.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    /// <returns>The index of the maximum element.</returns>
    let inline maxIndexBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : int =
        if isNull resizeArray then nullExn "maxIndexBy"
        resizeArray |> MinMax.indexByFun (>) projection

    [<Obsolete("use minIndexBy instead.")>]
    let inline minIndBy f xs = minIndexBy f xs

    [<Obsolete("use maxIndexBy instead.")>]
    let inline maxIndBy f xs = maxIndexBy f xs

    /// Returns the smallest and the second smallest element of the ResizeArray.
    /// If they are equal then the order is kept
    let inline min2 (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "min2"
        if resizeArray.Count < 2 then fail resizeArray "min2: Count must be at least two"
        resizeArray |> MinMax.simple2 (<)

    /// Returns the biggest and the second biggest element of the ResizeArray.
    /// If they are equal then the  order is kept
    let inline max2 (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "max2"
        if resizeArray.Count < 2 then fail resizeArray "max2: Count must be at least two"
        resizeArray |> MinMax.simple2 (>)

    // TODO make consistent xml docstring on below functions:

    /// Returns the smallest and the second smallest element of the ResizeArray.
    /// Elements are compared by applying the predicate function first.
    /// If they are equal after function is applied then the order is kept
    let inline min2By f (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "min2By"
        if resizeArray.Count < 2 then fail resizeArray "min2By: Count must be at least two"
        let i, ii = resizeArray |> MinMax.index2ByFun (<) f
        resizeArray.[i], resizeArray.[ii]

    /// Returns the biggest and the second biggest element of the ResizeArray.
    /// Elements are compared by applying the predicate function first.
    /// If they are equal after function is applied then the order is kept
    let inline max2By f (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "max2By"
        if resizeArray.Count < 2 then fail resizeArray "max2By: Count must be at least two"
        let i, ii = resizeArray |> MinMax.index2ByFun (>) f
        resizeArray.[i], resizeArray.[ii]

    /// Returns the indices of the smallest and the second smallest element of the ResizeArray.
    /// Elements are compared by applying the predicate function first.
    /// If they are equal after function is applied then the order is kept
    let inline min2IndicesBy f (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "min2IndicesBy"
        if resizeArray.Count < 2 then fail resizeArray "min2IndicesBy: Count must be at least two"
        resizeArray |> MinMax.index2ByFun (<) f

    /// Returns the indices of the biggest and the second biggest element of the ResizeArray.
    /// Elements are compared by applying the predicate function first.
    /// If they are equal after function is applied then the order is kept
    let inline max2IndicesBy f (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "max2IndicesBy"
        if resizeArray.Count < 2 then fail resizeArray "max2IndicesBy: Count must be at least two"
        resizeArray |> MinMax.index2ByFun (>) f

    [<Obsolete("use min2IndicesBy instead.")>]
    let inline min2IndBy f xs = min2IndicesBy f xs

    [<Obsolete("use max2IndicesBy instead.")>]
    let inline max2IndBy f xs = max2IndicesBy f xs

    /// Returns the smallest three elements of the ResizeArray.
    /// The first element is the smallest, the second is the second smallest and the third is the third smallest.
    /// If they are equal then the order is kept
    let inline min3 (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "min3"
        if resizeArray.Count < 3 then fail resizeArray "min3: Count must be at least three"
        resizeArray |> MinMax.simple3 (<)

    /// Returns the biggest three elements of the ResizeArray.
    /// The first element is the biggest, the second is the second biggest and the third is the third biggest.
    /// If they are equal then the order is kept
    let inline max3 (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "max3"
        if resizeArray.Count < 3 then fail resizeArray "max3: Count must be at least three"
        resizeArray |> MinMax.simple3 (>)

    /// Returns the smallest three elements of the ResizeArray.
    /// The first element is the smallest, the second is the second smallest and the third is the third smallest.
    /// Elements are compared by applying the predicate function first.
    /// If they are equal after function is applied then the order is kept
    let inline min3By f (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "min3By"
        if resizeArray.Count < 3 then fail resizeArray "min3By: Count must be at least three"
        let i, ii, iii = resizeArray |> MinMax.index3ByFun (<) f
        resizeArray.[i], resizeArray.[ii], resizeArray.[iii]

    /// Returns the biggest three elements of the ResizeArray.
    /// The first element is the biggest, the second is the second biggest and the third is the third biggest.
    /// Elements are compared by applying the predicate function first.
    /// If they are equal after function is applied then the order is kept
    let inline max3By f (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "max3By"
        if resizeArray.Count < 3 then fail resizeArray "max3By: Count must be at least three"
        let i, ii, iii = resizeArray |> MinMax.index3ByFun (>) f
        resizeArray.[i], resizeArray.[ii], resizeArray.[iii]

    /// Returns the indices of the three smallest elements of the ResizeArray.
    /// The first element is the index of the smallest, the second is the index of the second smallest and the third is the index of the third smallest.
    /// Elements are compared by applying the predicate function first.
    /// If they are equal after function is applied then the order is kept
    let inline min3IndicesBy f (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "min3IndicesBy"
        if resizeArray.Count < 3 then fail resizeArray "min3IndicesBy: Count must be at least three"
        resizeArray |> MinMax.index3ByFun (<) f

    /// Returns the indices of the three biggest elements of the ResizeArray.
    /// The first element is the index of the biggest, the second is the index of the second biggest and the third is the index of the third biggest.
    /// Elements are compared by applying the predicate function first.
    /// If they are equal after function is applied then the order is kept
    let inline max3IndicesBy f (resizeArray:ResizeArray<'T>) =
        if isNull resizeArray then nullExn "max3IndicesBy"
        if resizeArray.Count < 3 then fail resizeArray "max3Indices: Count must be at least three"
        resizeArray |> MinMax.index3ByFun (>) f

    [<Obsolete("use min3IndicesBy instead.")>]
    let inline min3IndBy f xs = min3IndicesBy f xs

    [<Obsolete("use max3IndicesBy instead.")>]
    let inline max3IndBy f xs = max3IndicesBy f xs

    /// Return the length or count of the collection.
    /// Same as ResizeArray.length
    let inline count (resizeArray: ResizeArray<'T>) : int =
        if isNull resizeArray then nullExn "count"
        resizeArray.Count

    /// Counts for how many items of the collection the predicate returns true.
    /// Same as ResizeArray.filter and then ResizeArray.length
    let inline countIf (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : int = //countBy is something else !!
        if isNull resizeArray then nullExn "countIf"
        let mutable k = 0
        for i = 0 to resizeArray.Count - 1 do
            if predicate resizeArray.[i] then k <- k + 1
        k

    /// Adds an object to the end of the ResizeArray.
    let inline add (item:'T) (resizeArray: ResizeArray<'T>) : unit =
        if isNull resizeArray then nullExn "add"
        resizeArray.Add item

    /// Builds a new ResizeArray from the given Array.
    /// (Use the asResizeArray function if you want to just cast an Array to a ResizeArray in Fable-JavaScript)
    let inline ofArray (arr: 'T[]) : ResizeArray<'T> =
        if isNull arr then nullExn "ofArray"
        let l = ResizeArray(arr.Length)
        for i = 0 to arr.Length - 1 do
            l.Add arr.[i]
        l


    /// Build a ResizeArray from the given IList Interface.
    let inline ofIList (arr: IList<'T>) : ResizeArray<'T> =
        if isNull arr then nullExn "ofIList"
        let l = ResizeArray(arr.Count)
        for i = 0 to arr.Count - 1 do
            l.Add arr.[i]
        l

    /// Return a fixed-length Array containing the elements of the input ResizeArray as a copy.
    /// This function always allocates a new Array and copies the elements.
    /// (Use the asArray function if you want to just cast a ResizeArray to an Array in Fable-JavaScript)
    let inline toArray (resizeArray: ResizeArray<'T>) : 'T[] =
        if isNull resizeArray then nullExn "toArray"
        resizeArray.ToArray()

    /// Return a fixed-length Array containing the elements of the input ResizeArray as a copy.
    /// When this function is used in Fable (JavaScript) the ResizeArray is just casted to an Array.
    /// In .NET a new Array is still allocated and the elements are copied.
    let inline asArray (resizeArray: ResizeArray<'T>) : 'T[] =
        if isNull resizeArray then nullExn "asArray"
        #if FABLE_COMPILER_JAVASCRIPT
        unbox resizeArray
        #else
        resizeArray.ToArray()
        #endif

    /// <summary>
    /// Splits the collection into two collections, containing the elements for which the
    /// given function returns <c>Choice1Of2</c> or <c>Choice2Of2</c>, respectively. This function is similar to
    /// <c>ResizeArray.partition</c>, but it allows the returned collections to have different element types.</summary>
    let inline partitionBy (partitioner: 'T -> Choice<'U1,'U2>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U1> * ResizeArray<'U2> =
        if isNull resizeArray then nullExn "partitionBy"
        let results1 = ResizeArray()
        let results2 = ResizeArray()
        for i = 0 to resizeArray.Count - 1 do
            match partitioner resizeArray.[i] with
            | Choice1Of2 value -> results1.Add value
            | Choice2Of2 value -> results2.Add value
        results1, results2

    /// <summary>
    /// Splits the collection into three collections, containing the elements for which the
    /// given function returns <c>Choice1Of3</c>, <c>Choice2Of3</c> or <c>Choice3Of3</c>, respectively. This function is similar to
    /// <c>ResizeArray.partition3</c>, but it allows the returned collections to have different element types.</summary>
    /// <param name="partitioner">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>Three ResizeArrays. </returns>
    let partition3By (partitioner: 'T -> Choice<'U1,'U2,'U3>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U1> * ResizeArray<'U2> * ResizeArray<'U3> =
        if isNull resizeArray then nullExn "partition3By"
        let results1 = ResizeArray()
        let results2 = ResizeArray()
        let results3 = ResizeArray()
        for i = 0 to resizeArray.Count - 1 do
            match partitioner resizeArray.[i] with
            | Choice1Of3 value -> results1.Add value
            | Choice2Of3 value -> results2.Add value
            | Choice3Of3 value -> results3.Add value
        results1, results2, results3


    /// <summary>
    /// Splits the collection into four collections, containing the elements for which the
    /// given function returns <c>Choice1Of4</c>, <c>Choice2Of4</c>, <c>Choice3Of4</c> or <c>Choice4Of4</c>, respectively. This function is similar to
    /// <c>ResizeArray.partition4</c>, but it allows the returned collections to have different element types.</summary>
    /// <param name="partitioner">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>Four ResizeArrays. </returns>
    let partition4By (partitioner: 'T -> Choice<'U1,'U2,'U3,'U4>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U1> * ResizeArray<'U2> * ResizeArray<'U3> * ResizeArray<'U4> =
        if isNull resizeArray then nullExn "partition4By"
        let results1 = ResizeArray()
        let results2 = ResizeArray()
        let results3 = ResizeArray()
        let results4 = ResizeArray()
        for i = 0 to resizeArray.Count - 1 do
            match partitioner resizeArray.[i] with
            | Choice1Of4 value -> results1.Add value
            | Choice2Of4 value -> results2.Add value
            | Choice3Of4 value -> results3.Add value
            | Choice4Of4 value -> results4.Add value
        results1, results2, results3, results4

    /// <summary>
    /// Splits the collection into five collections, containing the elements for which the
    /// given function returns <c>Choice1Of5</c>, <c>Choice2Of5</c>, <c>Choice3Of5</c>, <c>Choice4Of5</c> or <c>Choice5Of5</c>, respectively. This function is similar to
    /// <c>ResizeArray.partition5</c>, but it allows the returned collections to have different element types.</summary>
    /// <param name="partitioner">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>Five ResizeArrays. </returns>
    let partition5By (partitioner: 'T -> Choice<'U1,'U2,'U3,'U4,'U5>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U1> * ResizeArray<'U2> * ResizeArray<'U3> * ResizeArray<'U4> * ResizeArray<'U5> =
        if isNull resizeArray then nullExn "partition5By"
        let results1 = ResizeArray()
        let results2 = ResizeArray()
        let results3 = ResizeArray()
        let results4 = ResizeArray()
        let results5 = ResizeArray()
        for i = 0 to resizeArray.Count - 1 do
            match partitioner resizeArray.[i] with
            | Choice1Of5 value -> results1.Add value
            | Choice2Of5 value -> results2.Add value
            | Choice3Of5 value -> results3.Add value
            | Choice4Of5 value -> results4.Add value
            | Choice5Of5 value -> results5.Add value
        results1, results2, results3, results4, results5

    /// <summary>Splits the collection into three collections,
    /// first  containing the elements for which the given predicate1 returns <c>true</c> ,
    /// second containing the elements for which the given predicate2 returns <c>true</c> (and all previous predicates returned <c>false</c>),
    /// third the rest.</summary>
    /// <param name="predicate1">The first function to test the input elements.</param>
    /// <param name="predicate2">The second function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>Three ResizeArrays. </returns>
    let partition3 (predicate1: 'T -> bool) (predicate2: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> * ResizeArray<'T> * ResizeArray<'T> =
        if isNull resizeArray then nullExn "partition3"
        let p1True = ResizeArray()
        let p2True = ResizeArray()
        let allFalse = ResizeArray()
        let len = resizeArray.Count
        for i = 0 to len - 1 do
            let el = resizeArray.[i]
            if predicate1 el then p1True.Add el
            elif predicate2 el then p2True.Add el
            else allFalse.Add el

        p1True, p2True, allFalse

    /// <summary>Splits the collection into four collections,
    /// first  containing the elements for which the given predicate1 returns <c>true</c> ,
    /// second containing the elements for which the given predicate2 returns <c>true</c> (and all previous predicates returned <c>false</c>),
    /// third  containing the elements for which the given predicate3 returns <c>true</c> (and all previous predicates returned <c>false</c>),
    /// fourth the rest.</summary>
    /// <param name="predicate1">The first  function to test the input elements.</param>
    /// <param name="predicate2">The second function to test the input elements.</param>
    /// <param name="predicate3">The third  function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>Four ResizeArrays. </returns>
    let partition4 (predicate1: 'T -> bool) (predicate2: 'T -> bool) (predicate3: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> * ResizeArray<'T> * ResizeArray<'T> * ResizeArray<'T> =
        if isNull resizeArray then nullExn "partition4"
        let p1True = ResizeArray()
        let p2True = ResizeArray()
        let p3True = ResizeArray()
        let allFalse = ResizeArray()
        let len = resizeArray.Count
        for i = 0 to len - 1 do
            let el = resizeArray.[i]
            if predicate1 el then p1True.Add el
            elif predicate2 el then p2True.Add el
            elif predicate3 el then p3True.Add el
            else allFalse.Add el

        p1True, p2True, p3True, allFalse

    /// <summary>Splits the collection into five collections,
    /// first  containing the elements for which the given predicate1 returns <c>true</c> ,
    /// second containing the elements for which the given predicate2 returns <c>true</c> (and all previous predicates returned <c>false</c>),
    /// third  containing the elements for which the given predicate3 returns <c>true</c> (and all previous predicates returned <c>false</c>),
    /// fourth containing the elements for which the given predicate4 returns <c>true</c> (and all previous predicates returned <c>false</c>),
    /// fifth the rest.</summary>
    /// <param name="predicate1">The first  function to test the input elements.</param>
    /// <param name="predicate2">The second function to test the input elements.</param>
    /// <param name="predicate3">The third  function to test the input elements.</param>
    /// <param name="predicate4">The fourth function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>Five ResizeArrays. </returns>
    let partition5 (predicate1: 'T -> bool) (predicate2: 'T -> bool) (predicate3: 'T -> bool) (predicate4: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> * ResizeArray<'T> * ResizeArray<'T> * ResizeArray<'T> * ResizeArray<'T> =
        if isNull resizeArray then nullExn "partition5"
        let p1True = ResizeArray()
        let p2True = ResizeArray()
        let p3True = ResizeArray()
        let p4True = ResizeArray()
        let allFalse = ResizeArray()
        let len = resizeArray.Count
        for i = 0 to len - 1 do
            let el = resizeArray.[i]
            if predicate1 el then p1True.Add el
            elif predicate2 el then p2True.Add el
            elif predicate3 el then p3True.Add el
            elif predicate4 el then p4True.Add el
            else allFalse.Add el
        p1True, p2True, p3True, p4True, allFalse

        /// Applies a function to List
    /// If resulting List meets the resultPredicate it is returned, otherwise the original input is returned.
    let inline applyIfResult (resultPredicate: ResizeArray<'T> -> bool) (transform: ResizeArray<'T> -> ResizeArray<'T>) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "applyIfResult"
        let r = transform resizeArray
        if resultPredicate r then r else resizeArray

    /// Applies a function to List if it meets the inputPredicate, otherwise just returns input.
    /// If resulting List meets the resultPredicate it is returned, otherwise original input is returned.
    let inline applyIfInputAndResult (inputPredicate: ResizeArray<'T> -> bool) (resultPredicate: ResizeArray<'T> -> bool) (transform: ResizeArray<'T> -> ResizeArray<'T>) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "applyIfInputAndResult"
        if inputPredicate resizeArray then
            let r = transform resizeArray
            if resultPredicate r then r
            else resizeArray
        else
            resizeArray

    /// Returns all elements that exists more than once in ResizeArray.
    /// Each element that exists more than once is only returned once.
    /// Returned order is by first occurrence of first duplicate.
    let duplicates (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "duplicates"
        let h = HashSet<'T>()
        let t = HashSet<'T>()
        // first Add should be false, second Add true, to recognize the first occurrence of a duplicate:
        resizeArray.FindAll(System.Predicate(fun x -> if h.Add x then false else t.Add x))

    /// Returns all elements that exists more than once in ResizeArray.
    /// Each element that exists more than once is only returned once.
    /// Returned order is by first occurrence of first duplicate.
    let duplicatesBy (f: 'T -> 'U) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "duplicatesBy"
        let h = HashSet<'U>()
        let t = HashSet<'U>()
        // first Add should be false, second Add true, to recognize the first occurrence of a duplicate:
        resizeArray.FindAll(System.Predicate(fun x -> let y = f x in if h.Add y then false else t.Add y))


    /// <summary>Returns a new collection containing only the elements of the collection
    /// for which the given predicate run on the index returns <c>true</c>.</summary>
    /// <param name="predicate">The function to test the current index.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>A ResizeArray containing the elements for which the given predicate returns true.</returns>
    let inline filteri (predicate: int -> 'T-> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "filteri"
        let res = ResizeArray()
        for i = 0 to resizeArray.Count - 1 do
            let t = resizeArray.[i]
            if predicate i t then
                res.Add(t)
        res

    /// <summary>Returns the index of the first element in the ResizeArray that satisfies the given indexed predicate.</summary>
    /// <param name="predicate">The function to test each indexed element against.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The index of the first element that satisfies the predicate, or None if not found.</returns>
    let inline tryFindIndexi (predicate:int -> 'T -> bool) (resizeArray: ResizeArray<'T>) : option<int> =
        if isNull resizeArray then nullExn "tryFindIndexi" // TODO or return None ?
        let mutable i = 0
        let mutable result = None
        let k = resizeArray.Count
        while i < k  do
            let elm = resizeArray.[i]
            i <- i + 1
            if predicate i elm then
                result <- Some i
                i <- k // break the loop
        result

    /// <summary>Returns the index of the first element in the ResizeArray that satisfies the given indexed predicate.</summary>
    /// <param name="predicate">The function to test each indexed element against.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The index of the first element that satisfies the predicate, or an exception.</returns>
    /// <exception cref="System.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
    let findIndexi (predicate:int -> 'T -> bool) (resizeArray: ResizeArray<'T>) : int=
        if isNull resizeArray then nullExn "findIndexi"
        match tryFindIndexi predicate resizeArray with
        | Some i -> i
        | None -> failKey resizeArray "findIndexi did not find for given predicate in"



    //--------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------
    // ------------- implementation adapted form FSharp.Core Array module: ------------------------------------------------
    //
    //               alternatives:
    //               https://github.com/fsprojects/FSharpx.Collections/blob/master/src/FSharpx.Collections/ResizeArray.fs
    //               https://github.com/jack-pappas/ExtCore/blob/master/ExtCore/Collections.ResizeArray.fs
    //               https://github.com/dotnet/fsharp/tree/master/src/utils
    //---------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------




    /// a StructBox for keys in case the key type is itself a type using null as a representation
    [<Struct; NoComparison; NoEquality>]
    type private StructBox<'T when 'T: equality>(value: 'T) = // from fsharp/FSharp.Core/seqCore.fs
        member x.Value = value
        static member Comparer =
            let gComparer = HashIdentity.Structural<'T>
            { new IEqualityComparer<StructBox<'T>> with
                member _.GetHashCode(v) = gComparer.GetHashCode(v.Value)
                member _.Equals(a, b) = gComparer.Equals(a.Value, b.Value) }

    /// <summary>Returns a new ResizeArray that contains all pairings (or combinations)  of elements from the first and second ResizeArrays.</summary>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <returns>The resulting ResizeArray of pairs of length: resizeArray1.Count * resizeArray2.Count.</returns>
    let allPairs (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'U>) : ResizeArray<'T * 'U> =
        if isNull resizeArray1 then nullExn "allPairs first"
        if isNull resizeArray2 then nullExn "allPairs second"
        let res = ResizeArray(resizeArray1.Count * resizeArray2.Count)
        for i = 0 to resizeArray1.Count - 1 do
            for j = 0 to resizeArray2.Count - 1 do
                res.Add(resizeArray1.[i], resizeArray2.[j])

        res


    /// <summary>Builds a new ResizeArray that contains the elements of the first ResizeArray followed by the elements of the second ResizeArray.
    /// When used with the pipeline operator |>  the first and only argument to this function with be at the start of the resulting list.
    /// This can be counter intuitive. Use the function ResizeArray.prepend instead to append the first argument at the end of the second argument.</summary>
    /// <param name="resizeArray1">The input ResizeArray that will be at the beginning.</param>
    /// <param name="resizeArray2">The input ResizeArray that will be at the end.</param>
    /// <returns>The resulting ResizeArray of length: resizeArray1.Count + resizeArray2.Count..</returns>
    let inline append (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'T>) =
        if isNull resizeArray1 then nullExn "append first"
        if isNull resizeArray2 then nullExn "append second"
        let res = resizeArray1.Clone()
        res.AddRange(resizeArray2)
        res

    /// <summary>Builds a new ResizeArray that contains the elements of the second ResizeArray followed by the elements of the first ResizeArray.
    /// When used with the pipeline operator |>  the first and only argument to this function with be at the end of the resulting list.
    /// Compared to ResizeArray.append this function has the order of its arguments flipped</summary>
    /// <param name="resizeArray2">The input ResizeArray that will be at the end.</param>
    /// <param name="resizeArray1">The input ResizeArray that will be at the beginning.</param>
    /// <returns>The resulting ResizeArray of length: resizeArray2.Count + resizeArray1.Count..</returns>
    let inline prepend (resizeArray2: ResizeArray<'T>) (resizeArray1: ResizeArray<'T>) =
        if isNull resizeArray1 then nullExn "prepend first"
        if isNull resizeArray2 then nullExn "prepend second"
        let res = resizeArray1.Clone()
        res.AddRange(resizeArray2)
        res

    /// <summary>Returns the average of the elements in the ResizeArray.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when <c>ResizeArray</c> is empty.</exception>
    /// <returns>The average of the elements in the ResizeArray.</returns>
    let inline average (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "average"
        if resizeArray.Count = 0 then
            fail resizeArray "average: Count must be at least one"
        let mutable acc = LanguagePrimitives.GenericZero< ^T>
        for i = 0 to resizeArray.Count - 1 do
            acc <- Checked.(+) acc resizeArray.[i]
        LanguagePrimitives.DivideByInt< ^T> acc resizeArray.Count


    /// <summary>Returns the average of the elements generated by applying the function to each element of the ResizeArray.</summary>
    /// <param name="projection">The function to transform the ResizeArray elements before averaging.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when <c>ResizeArray</c> is empty.</exception>
    /// <returns>The computed average.</returns>
    let inline averageBy (projection: 'T -> ^Key) (resizeArray: ResizeArray<'T>) : ^Key =
        if isNull resizeArray then nullExn "averageBy"
        if resizeArray.Count = 0 then
            fail resizeArray "averageBy: Count must be at least one"
        let mutable acc = LanguagePrimitives.GenericZero< ^Key>
        for i = 0 to resizeArray.Count - 1 do
            acc <- Checked.(+) acc (projection resizeArray.[i])
        LanguagePrimitives.DivideByInt< ^Key> acc resizeArray.Count

    /// <summary>Applies the given function to each element of the ResizeArray.
    /// Returns the ResizeArray comprised of the results "x" for each element where
    /// the function returns Some(x)</summary>
    /// <param name="chooser">The function to generate options from the elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The ResizeArray of results.</returns>
    let inline choose (chooser: 'T -> 'U option) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
        if isNull resizeArray then nullExn "choose"
        let result = ResizeArray()
        for i = 0 to resizeArray.Count - 1 do
            match chooser resizeArray.[i] with
            | None -> ()
            | Some value -> result.Add value
        result

    /// <summary>Divides the input ResizeArray into chunks of size at most <c>chunkSize</c>.</summary>
    /// <param name="chunkSize">The maximum size of each chunk.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The ResizeArray divided into chunks.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when <c>chunkSize</c> is not positive.</exception>
    let chunkBySize chunkSize (resizeArray: ResizeArray<'T>) : ResizeArray<ResizeArray<'T>> =
        if isNull resizeArray then nullExn "chunkBySize"
        if chunkSize <= 0 then
            fail resizeArray $"chunkBySize: chunkSize {chunkSize} must be bigger than 0"
        let len = resizeArray.Count
        if len = 0 then
            ResizeArray(0)
        elif chunkSize > len then
            ResizeArray([ resizeArray.Clone() ])
        else
            let chunkCount = (len - 1) / chunkSize + 1
            let res = ResizeArray(chunkCount)
            let mutable sub = ResizeArray(0)
            for i = 0 to resizeArray.Count - 1 do
                if i % chunkSize = 0 then
                    sub <- ResizeArray(chunkSize)
                    res.Add(sub)
                sub.Add resizeArray.[i]
            res

    /// <summary>For each element of the ResizeArray, applies the given function. Concatenates all the results and return the combined ResizeArray.</summary>
    /// <param name="mapping">The function to create sub-ResizeArrays from the input ResizeArray elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The concatenation of the sub-ResizeArrays.</returns>
    let collect (mapping: 'T -> ResizeArray<'U>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
        if isNull resizeArray then nullExn "collect"
        //let collect (mapping: 'T -> seq<'U>)  (resizeArray: ResizeArray<'T>) : ResizeArray<'U> = // tests don't pass like that
        let res = ResizeArray(resizeArray.Count)
        for i = 0 to resizeArray.Count - 1 do
            let e = resizeArray.[i]
            res.AddRange(mapping e)
        res

    /// <summary>Compares two ResizeArrays using the given comparison function, element by element.</summary>
    /// <param name="comparer">A function that takes an element from each ResizeArray and returns an int.
    /// If it evaluates to a non-zero value iteration is stopped and that value is returned.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <returns>Returns the first non-zero result from the comparison function. If the first ResizeArray has
    /// a larger element, the return value is always positive. If the second ResizeArray has a larger
    /// element, the return value is always negative. When the elements are equal in the two
    /// ResizeArrays, 1 is returned if the first ResizeArray is longer, 0 is returned if they are equal in
    /// length, and -1 is returned when the second ResizeArray is longer.</returns>
    let inline compareWith ( (*[<InlineIfLambda>]*) comparer: 'T -> 'T -> int) (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'T>) =
        if isNull resizeArray1 then nullExn "compareWith first"
        if isNull resizeArray2 then nullExn "compareWith second"
        let length1 = resizeArray1.Count
        let length2 = resizeArray2.Count
        let mutable i = 0
        let mutable result = 0
        if length1 < length2 then
            while i < resizeArray1.Count && result = 0 do
                result <- comparer resizeArray1.[i] resizeArray2.[i]
                i <- i + 1
        else
            while i < resizeArray2.Count && result = 0 do
                result <- comparer resizeArray1.[i] resizeArray2.[i]
                i <- i + 1
        if result <> 0 then result
        elif length1 = length2 then 0
        elif length1 < length2 then -1
        else 1


    /// <summary>Builds a new ResizeArray that contains the elements of each of the given sequence of sequences.</summary>
    /// <param name="resizeArrays">The input sequence of ResizeArrays.</param>
    /// <returns>The concatenation of the sequence of input ResizeArrays.</returns>
    let concat (resizeArrays: ResizeArray<ResizeArray<'T>>) : ResizeArray<'T> =
        if isNull resizeArrays then nullExn "concat"
        //let concat (resizeArrays: ResizeArray<ResizeArray<'T>>) : ResizeArray<'T> =  // test don't pass with this
        //if resizeArrays.Count = 0 then
        if resizeArrays.Count = 0 then
            ResizeArray(0)
        else
            let res = ResizeArray() //resizeArrays.[0].Clone()
            for i = 0 to resizeArrays.Count - 1 do
                let r = resizeArrays.[i]
                if isNull r then nullExn "concat inner"
                res.AddRange(r)
            //for i=1 to resizeArrays.(resizeArray.Count-1) do res.AddRange(resizeArrays.[i])
            res

    /// <summary>Tests if the ResizeArray contains the specified element.</summary>
    /// <param name="value">The value to locate in the input ResizeArray.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns><c>true</c> if the input ResizeArray contains the specified element; false otherwise.</returns>
    let inline contains value (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "contains"
        resizeArray.Contains(value)

    /// <summary>Builds a new ResizeArray that contains the elements of the given ResizeArray.
    /// A shallow copy by calling resizeArray.GetRange(0,resizeArray.Count) </summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>A copy of the input ResizeArray.</returns>
    let inline copy (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "copy"
        resizeArray.GetRange(0, resizeArray.Count) // fastest way to create a shallow copy

    /// <summary>Builds a new ResizeArray that contains the elements of the given ResizeArray.
    /// A shallow copy by calling resizeArray.GetRange(0,resizeArray.Count) </summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>A copy of the input ResizeArray.</returns>
    let inline clone (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "clone"
        resizeArray.GetRange(0, resizeArray.Count) // fastest way to create a shallow copy


    /// <summary>Reads a range of elements from the first ResizeArray and write them into the second. The target ResizeArray must already have the required minimum size to fit targetStartIndex + count.</summary>
    /// <param name="source">The source ResizeArray.</param>
    /// <param name="sourceIndex">The starting index of the source ResizeArray.</param>
    /// <param name="target">The target ResizeArray.</param>
    /// <param name="targetStartIndex">The starting index of the target ResizeArray.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when any of sourceIndex,targetStartIndex or count are negative, or when there aren't enough elements in source or target.</exception>
    let inline blit (source: ResizeArray<'T>) (sourceIndex: int) (target: ResizeArray<'T>) (targetStartIndex: int) (count: int) : unit =
        if isNull source then nullExn "blit source"
        if isNull target then nullExn "blit target"
        if sourceIndex < 0 then
            fail source $"blit: sourceIndex {sourceIndex} cannot be negative."
        if targetStartIndex < 0 then
            fail target $"blit:targetStartIndex {targetStartIndex} cannot be negative."
        if count < 0 then
            fail source $"blit: count {count} cannot be negative."
        if source.Count < sourceIndex + count then
            fail source $"blit: source.Count {source.Count} is smaller than  sourceIndex {sourceIndex} + count {count}."
        if target.Count < targetStartIndex + count then
            fail target $"blit: target.Count {target.Count} is smaller than  targetStartIndex {targetStartIndex} + count {count}."
        let mutable j = targetStartIndex
        for i = sourceIndex to sourceIndex + count - 1 do
            target.[j] <- source.[i]
            j <- j + 1

    /// <summary>Reads a range of elements from the first ResizeArray and write them into the second. The target ResizeArray increases in size if needed.
    /// But it needs to have  minimum <c>targetStartIndex</c> elements already.</summary>
    /// <param name="source">The source ResizeArray.</param>
    /// <param name="sourceIndex">The starting index of the source ResizeArray.</param>
    /// <param name="target">The target ResizeArray.</param>
    /// <param name="targetStartIndex">The starting index of the target ResizeArray.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when any of sourceIndex, targetStartIndex or count are negative, or when there aren't enough elements in source.</exception>
    let inline blitExtend (source: ResizeArray<'T>) (sourceIndex: int) (target: ResizeArray<'T>) (targetStartIndex: int) (count: int) : unit =
        if isNull source then nullExn "blitExtend source"
        if isNull target then nullExn "blitExtend target"
        if sourceIndex < 0 then
            fail source $"blit: sourceIndex {sourceIndex} cannot be negative."
        if targetStartIndex < 0 then
            fail target $"blit: targetStartIndex {targetStartIndex} cannot be negative."
        if count < 0 then
            fail source $"blit: count {count} cannot be negative."
        if source.Count < sourceIndex + count then
            fail source $"blit: source.Count {source.Count} is smaller than  sourceIndex {sourceIndex} + count {count}."
        if target.Count < targetStartIndex then
            fail target $"blit: target.Count {target.Count} is smaller than  targetStartIndex {targetStartIndex}."
        let mutable j = targetStartIndex
        let tLasti = target.Count - 1
        for i = sourceIndex to sourceIndex + count - 1 do
            if j > tLasti then
                target.Add(source.[i]) //increases in size if needed
            else
                target.[j] <- source.[i]

            j <- j + 1


    let inline private countByImpl (comparer: IEqualityComparer<'SafeKey>) (projection: 'T -> 'SafeKey) (getKey: 'SafeKey -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'Key * int> =
        let length = resizeArray.Count
        if length = 0 then
            ResizeArray(0)
        else
            let dict = Dictionary comparer
            // Build the groupings
            for i = 0 to resizeArray.Count - 1 do
                let v = resizeArray.[i]
                let safeKey = projection v
                let mutable prev = Unchecked.defaultof<_>
                if dict.TryGetValue(safeKey, &prev) then
                    dict.[safeKey] <- prev + 1
                else
                    dict.[safeKey] <- 1

            let res = ResizeArray(dict.Count)

            for group in dict do
                res.Add(getKey group.Key, group.Value)

            res

    /// <summary>Applies a key-generating function to each element of a ResizeArray and returns a ResizeArray yielding unique
    /// keys and their number of occurrences in the original ResizeArray.</summary>
    /// <param name="projection">A function transforming each item of the input ResizeArray into a key to be
    /// compared against the others.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    let countBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'Key * int> =
        if isNull resizeArray then nullExn "countBy"
        #if FABLE_COMPILER
        countByImpl StructBox<'Key>.Comparer (fun t -> StructBox(projection t)) (fun sb -> sb.Value) resizeArray
        #else
        if typeof<'Key>.IsValueType then
            // We avoid wrapping a StructBox, because under 64 JIT we get some "hard" tail-calls which affect performance
            countByImpl HashIdentity.Structural<'Key> projection id resizeArray
        else
            // Wrap a StructBox around all keys in case the key type is itself a type using null as a representation
            countByImpl StructBox<'Key>.Comparer (fun t -> StructBox(projection t)) (fun sb -> sb.Value) resizeArray
        #endif


    /// <summary>Creates a ResizeArray whose elements are all initially the given value.</summary>
    /// <param name="count">The length of the ResizeArray to create.</param>
    /// <param name="value">The value for the elements.</param>
    /// <returns>The created ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when count is negative.</exception>
    let create (count: int) (value: 'T) =
        if count < 0 then
            failSimpel $"create: count ({count}) cannot be negative."
        let resizeArray = ResizeArray(count)
        for i = 0 to count - 1 do
            resizeArray.Add value
        resizeArray


    /// <summary>Returns a ResizeArray that contains no duplicate entries according to generic hash and
    /// equality comparisons on the entries.
    /// If an element occurs multiple times in the ResizeArray then the later occurrences are discarded.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    let distinct (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "distinct"
        let temp = ResizeArray()
        let hashSet = HashSet<'T>(HashIdentity.Structural<'T>)
        for i = 0 to resizeArray.Count - 1 do
            let v = resizeArray.[i]
            if hashSet.Add(v) then temp.Add v

        temp


    /// <summary>Returns a ResizeArray that contains no duplicate entries according to the
    /// generic hash and equality comparisons on the keys returned by the given key-generating function.
    /// If an element occurs multiple times in the ResizeArray then the later occurrences are discarded.</summary>
    /// <param name="projection">A function transforming the ResizeArray items into comparable keys.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    let distinctBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "distinctBy"
        if resizeArray.Count < 2 then
            resizeArray.Clone() // 0 or 1 item, just clone
        else
            let temp = ResizeArray()
            let hashSet = HashSet<'Key>(HashIdentity.Structural<_>)
            for i = 0 to resizeArray.Count - 1 do
                let v = resizeArray.[i]
                if hashSet.Add(projection v) then temp.Add v

            temp

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs

    /// <summary> Returns a new empty ResizeArray of the given type. With initial capacity zero.
    /// Upon adding the first element to the ResizeArray an internal Array of size 4 is allocated, and then increased in multiples of two
    /// as required. </summary>
    [<GeneralizableValue>]
    [<RequiresExplicitTypeArguments>] // without this it does no create a new ResizeArray, see https://github.com/fsharp/fslang-suggestions/issues/602
    let inline empty<'T> : ResizeArray<'T> =
        ResizeArray<'T>()


    /// <summary>Returns the only element of the ResizeArray.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The only element of the ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the input does not have precisely one element.</exception>
    let exactlyOne (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "exactlyOne"
        if resizeArray.Count = 1 then
            resizeArray.[0]
        else
            fail resizeArray $"exactlyOne: ResizeArray has {resizeArray.Count} elements, not one."


    /// <summary>Returns a new list with the distinct elements of the input ResizeArray which do not appear in the itemsToExclude sequence.
    /// Uses generic hash and equality comparisons to compare values.</summary>
    /// <param name="itemsToExclude">A sequence whose elements that also occur in the input ResizeArray will cause those elements to be
    /// removed from the result.</param>
    /// <param name="resizeArray">A ResizeArray whose elements that are not also in itemsToExclude will be returned.</param>
    /// <returns>A ResizeArray that contains the distinct elements of <c>ResizeArray</c> that do not appear in <c>itemsToExclude</c>.</returns>
    let except (itemsToExclude: seq<'T>) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "except resizeArray"
        if isNull itemsToExclude then nullExn "except itemsToExclude"
        if resizeArray.Count = 0 then
            resizeArray
        else
            let cached = HashSet(itemsToExclude, HashIdentity.Structural)
            let res = ResizeArray()

            for i = 0 to resizeArray.Count - 1 do
                let e = resizeArray.[i]
                if cached.Add e then res.Add e

            res

    /// <summary>Tests if none of the elements of the ResizeArray satisfies the given predicate.
    /// The predicate is applied to the elements of the input ResizeArray. If any application
    /// returns true then the overall result is <c>false</c> and no further elements are tested.
    /// Otherwise, true is returned.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns><c>false</c> if any result from <c>predicate</c> is <c>true</c>.</returns>
    let notExists (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "notExists"
        not (resizeArray.Exists(System.Predicate predicate))



    /// <summary>Tests if any element of the ResizeArray satisfies the given predicate.
    /// The predicate is applied to the elements of the input ResizeArray. If any application
    /// returns true then the overall result is <c>true</c> and no further elements are tested.
    /// Otherwise, false is returned.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns><c>true</c> if any result from <c>predicate</c> is <c>true</c>.</returns>
    let exists (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : bool =
        if isNull resizeArray then nullExn "exists"
        resizeArray.Exists(System.Predicate predicate)


    /// <summary>Tests if any pair of corresponding elements of the ResizeArrays satisfies the given predicate.
    /// The predicate is applied to matching elements in the two collections up to the lesser of the
    /// two lengths of the collections. If any application returns true then the overall result is
    /// true and no further elements are tested. Otherwise, if one collections is longer
    /// than the other then the <c>ArgumentException</c> exception is raised.
    /// Otherwise, false is returned.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <returns><c>true</c> if any result from <c>predicate</c> is <c>true</c>.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    let exists2 (predicate: 'T -> 'U -> bool) (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'U>) : bool =
        if isNull resizeArray1 then nullExn "exists2 first" // TODO Or return false?
        if isNull resizeArray2 then nullExn "exists2 second"
        let len1 = resizeArray1.Count
        if len1 <> resizeArray2.Count then
            fail resizeArray1 $"exists2: count of resizeArray1 {len1} does not match resizeArray2 {resizeArray2.Count}."
        let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt predicate
        let mutable i = 0
        let mutable found = false
        while i < len1  do
            let e1 = resizeArray1.[i]
            let e2 = resizeArray2.[i]
            i <- i + 1
            if f.Invoke(e1, e2) then
                found <- true
                i <- len1 // break out of the loop
        found

    /// <summary>Fills a range of elements of the ResizeArray with the given value. Extends the ResizeArray if needed</summary>
    /// <param name="target">The target ResizeArray.</param>
    /// <param name="startIndex">The index of the first element to set.</param>
    /// <param name="count">The number of elements to set.</param>
    /// <param name="value">The value to set.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when either startIndex or count is negative.</exception>
    let fill (target: ResizeArray<'T>) (startIndex: int) (count: int) (value: 'T) =
        if isNull target then nullExn "fill"
        if startIndex < 0 then
            fail target $"fill: startIndex {startIndex} cannot be negative."
        if count < 0 then
            fail target $"fill: count {count} cannot be negative."
        if target.Count < startIndex then
            fail target $"fill: target.Count {target.Count} is smaller than  startIndex {startIndex}."
        let tLasti = target.Count - 1
        for j = startIndex to startIndex + count - 1 do
            if j > tLasti then
                target.Add value
            else
                target.[j] <- value

    /// <summary>Returns a new collection containing only the elements of the collection
    /// for which the given predicate returns <c>true</c>.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>A ResizeArray containing the elements for which the given predicate returns true.</returns>
    let inline filter (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "filter"
        // TODO replace with F# implementation using [<InlineIfLambda>] for performance?? Test on non Lambdas too.
        resizeArray.FindAll(System.Predicate predicate)


    /// <summary>Returns the first element for which the given function returns <c>true</c>.
    /// Raise <see cref="T:System.Collections.Generic.KeyNotFoundException"/> if no such element exists.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown if <c>predicate</c> never returns true.</exception>
    /// <returns>The first element for which <c>predicate</c> returns true.</returns>
    let find (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "find"
        let elementIndex = resizeArray.FindIndex(System.Predicate predicate)
        match elementIndex with
        | -1 -> failKey resizeArray "find did not find for given predicate in"
        | index -> resizeArray.[index]

    /// <summary>Returns the last element for which the given function returns <c>true</c>.
    /// Raise <see cref="T:System.Collections.Generic.KeyNotFoundException"/> if no such element exists.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown if <c>predicate</c> never returns true.</exception>
    /// <returns>The last element for which <c>predicate</c> returns true.</returns>
    let findBack (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "findBack"
        let mutable i = resizeArray.Count - 1
        let mutable result = Unchecked.defaultof<'T>
        while i >= 0  do
            let elm = resizeArray.[i]
            i <- i - 1
            if predicate elm then
                result <- elm
                i <- -2 // break out of the loop
        if i = -2 then
            result
        else
            failKey resizeArray "findBack did not find for given predicate in"


    /// <summary>Returns the index of the first element in the ResizeArray that satisfies the given predicate.
    /// Raises <see cref="T:System.Collections.Generic.KeyNotFoundException"/> if
    /// none of the elements satisfy the predicate.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown if <c>predicate</c> never returns true.</exception>
    /// <returns>The index of the first element in the ResizeArray that satisfies the given predicate.</returns>
    let findIndex (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "findIndex"
        let elementIndex = resizeArray.FindIndex(System.Predicate predicate)
        match elementIndex with
        | -1 -> failKey resizeArray "findIndex did not find for given predicate in"
        | index -> index


    /// <summary>Returns the index of the last element in the ResizeArray
    /// that satisfies the given predicate. Raise <see cref="T:System.Collections.Generic.KeyNotFoundException"/> if
    /// none of the elements satisfy the predicate.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown if <c>predicate</c> never returns true.</exception>
    /// <returns>The index of the last element in the ResizeArray that satisfies the given predicate.</returns>
    let findIndexBack (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "findIndexBack"
        let mutable i = resizeArray.Count - 1
        let mutable found = false
        while i >= 0 && not found do
            if predicate resizeArray.[i] then
                found <- true
            else
                i <- i - 1
        if found then
            i
        else
            failKey resizeArray "findIndexBack did not find for given predicate in"


    /// <summary>Applies a function to each element of the collection, threading an accumulator argument
    /// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c> then computes
    /// <c>f (... (f s i0)...) iN</c></summary>
    /// <param name="folder">The function to update the state given the input elements.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The final state.</returns>
    let fold<'T, 'State> (folder: 'State -> 'T -> 'State) (state: 'State) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "fold"
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(folder)
        let mutable state = state
        for i = 0 to resizeArray.Count - 1 do
            state <- f.Invoke(state, resizeArray.[i])
        state


    /// <summary>Applies a function to pairs of elements drawn from the two collections,
    /// left-to-right, threading an accumulator argument
    /// through the computation. The two input
    /// ResizeArrays must have the same lengths, otherwise an <c>ArgumentException</c> is
    /// raised.</summary>
    /// <param name="folder">The function to update the state given the input elements.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    /// <returns>The final state.</returns>
    let fold2<'T1, 'T2, 'State> folder (state: 'State) (resizeArray1: 'T1 ResizeArray) (resizeArray2: 'T2 ResizeArray) =
        if isNull resizeArray1 then nullExn "fold2 first"
        if isNull resizeArray2 then nullExn "fold2 second"
        if resizeArray1.Count <> resizeArray2.Count then
            fail resizeArray1  $"fold2: count of resizeArray1 {resizeArray1.Count} does not match resizeArray2 {resizeArray2.Count}."
        let f = OptimizedClosures.FSharpFunc<_, _, _, _>.Adapt(folder)
        let mutable state = state
        for i = 0 to resizeArray1.Count - 1 do
            state <- f.Invoke(state, resizeArray1.[i], resizeArray2.[i])
        state


    /// <summary>Applies a function to each element of the ResizeArray, starting from the end, threading an accumulator argument
    /// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c> then computes
    /// <c>f i0 (...(f iN s))</c></summary>
    /// <param name="folder">The function to update the state given the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The state object after the folding function is applied to each element of the ResizeArray.</returns>
    let foldBack<'T, 'State> (folder: 'T -> 'State -> 'State) (resizeArray: ResizeArray<'T>) (state: 'State) =
        if isNull resizeArray then nullExn "foldBack"
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(folder)
        let mutable res = state
        for i = resizeArray.Count - 1 downto 0 do
            res <- f.Invoke(resizeArray.[i], res)
        res


    /// <summary>Apply a function to pairs of elements drawn from the two collections, right-to-left,
    /// threading an accumulator argument through the computation. The two input
    /// ResizeArrays must have the same lengths, otherwise an <c>ArgumentException</c> is
    /// raised.</summary>
    /// <param name="folder">The function to update the state given the input elements.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <param name="state">The initial state.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    /// <returns>The final state.</returns>
    let foldBack2<'T1, 'T2, 'State> folder (resizeArray1: 'T1 ResizeArray) (resizeArray2: 'T2 ResizeArray) (state: 'State) =
        if isNull resizeArray1 then nullExn "foldBack2 first"
        if isNull resizeArray2 then nullExn "foldBack2 second"
        let len = resizeArray1.Count
        if len <> resizeArray2.Count then
            fail resizeArray1 $"foldBack2: count of resizeArray1 {len} does not match resizeArray2 {resizeArray2.Count}."
        let mutable res = state
        let f = OptimizedClosures.FSharpFunc<_, _, _, _>.Adapt(folder)
        for i = len - 1 downto 0 do
            res <- f.Invoke(resizeArray1.[i], resizeArray2.[i], res)
        res


    /// <summary>Tests if all elements of the ResizeArray satisfy the given predicate.
    /// The predicate is applied to the elements of the input collection. If any application
    /// returns false then the overall result is false and no further elements are tested.
    /// Otherwise, true is returned.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns><c>true</c> if all of the ResizeArray elements satisfy the predicate.</returns>
    let forall (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "forall"
        #if FABLE_COMPILER
        let len = resizeArray.Count
        let mutable i = 0
        let mutable result = true
        while i < len && result do
            if not (predicate resizeArray.[i]) then
                result <- false
            i <- i + 1
        result
        #else
        resizeArray.TrueForAll(System.Predicate predicate)
        #endif


    /// <summary>Tests if all corresponding elements of the ResizeArray satisfy the given predicate pairwise.
    /// The predicate is applied to matching elements in the two collections up to the lesser of the
    /// two lengths of the collections. If any application returns false then the overall result is
    /// false and no further elements are tested. Otherwise, if one collection is longer
    /// than the other then the <c>ArgumentException</c> exception is raised.
    /// Otherwise, true is returned.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    /// <returns><c>true</c> if all of the ResizeArray elements satisfy the predicate.</returns>
    let forall2 (predicate: 'T -> 'U -> bool) (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'U>) : bool =
        if isNull resizeArray1 then nullExn "forall2 first"
        if isNull resizeArray2 then nullExn "forall2 second"
        let len1 = resizeArray1.Count
        if len1 <> resizeArray2.Count then
            fail resizeArray1 $"forall2: count of resizeArray1 {len1} does not match resizeArray2 {resizeArray2.Count}."
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(predicate)
        let mutable i = 0
        let mutable result = true
        while i < len1 && result do
            if not (f.Invoke(resizeArray1.[i], resizeArray2.[i])) then
                result <- false
            i <- i + 1
        result


    /// <summary>Builds a new ResizeArray that contains the given sub-range specified by starting index and length.</summary>
    /// <param name="startIndex">The index of the first element of the sub ResizeArray.</param>
    /// <param name="count">The length of the sub ResizeArray.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The created sub ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when either startIndex or count is negative, or when there aren't enough elements in the input ResizeArray.</exception>
    let sub (startIndex: int) (count: int) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "sub"
        if startIndex < 0 then
            fail resizeArray $"sub: startIndex '{startIndex}' cannot be negative."
        if count < 0 then
            fail resizeArray $"sub: count '{count}' cannot be negative."
        if resizeArray.Count < startIndex + count then
            fail resizeArray $"sub: resizeArray.Count {resizeArray.Count} is smaller than startIndex {startIndex} + count {count}."
        resizeArray.GetRange(startIndex, count)


    let inline private groupByImpl (comparer: IEqualityComparer<'SafeKey>) (keyf: 'T -> 'SafeKey) (getKey: 'SafeKey -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'Key * ResizeArray<'T>> =
        let length = resizeArray.Count
        if length = 0 then
            ResizeArray(0)
        else
            let dict = Dictionary<_, ResizeArray<_>> comparer
            // Build the groupings
            for i = 0 to length - 1 do
                let v = resizeArray.[i]
                let safeKey = keyf v
                let mutable prev = Unchecked.defaultof<_>
                if dict.TryGetValue(safeKey, &prev) then
                    prev.Add v
                else
                    let prev = ResizeArray()
                    dict.[safeKey] <- prev
                    prev.Add v
            // Return the resizeArray-of-resizeArrays.
            let result = ResizeArray(dict.Count)
            for group in dict do
                result.Add(getKey group.Key, group.Value)
            result

    /// <summary>Applies a key-generating function to each element of a ResizeArray and yields a ResizeArray of
    /// unique keys. Each unique key contains a ResizeArray of all elements that match
    /// to this key.</summary>
    /// <param name="projection">A function that transforms an element of the ResizeArray into a comparable key. Null or Option.None is allowed as key.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    let groupBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'Key * ResizeArray<'T>> =
        if isNull resizeArray then nullExn "groupBy"
        #if FABLE_COMPILER
        groupByImpl StructBox<'Key>.Comparer (fun t -> StructBox(projection t)) (fun sb -> sb.Value) resizeArray
        #else
        if typeof<'Key>.IsValueType then
            // We avoid wrapping a StructBox, because under 64 JIT we get some "hard" tail-calls which affect performance
            groupByImpl HashIdentity.Structural<'Key> projection id resizeArray
        else
            // Wrap a StructBox around all keys in case the key type is itself a type using null as a representation
            groupByImpl StructBox<'Key>.Comparer (fun t -> StructBox(projection t)) (fun sb -> sb.Value) resizeArray
        #endif

    /// <summary>Applies a key-generating function to each element of a ResizeArray and yields a Dict of
    /// unique keys and respective elements that match to this key. As opposed to ResizeArray.groupBy the key may not be null or Option.None</summary>
    /// <param name="projection">A function that transforms an element of the ResizeArray into a comparable key. As opposed to ResizeArray.groupBy the key may not be null or Option.None </param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    let groupByDict (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : Dictionary<'Key, ResizeArray<'T>> =
        if isNull resizeArray then nullExn "groupByDict"
        let dict = Dictionary<'Key, ResizeArray<'T>>()
        // Build the groupings
        for i = 0 to resizeArray.Count - 1 do
            let v = resizeArray.[i]
            let k = projection v
            match dict.TryGetValue k with
            | true, r -> r.Add v
            | _ ->
                let r = ResizeArray()
                dict.[k] <- r
                r.Add v
        dict



    /// <summary>Returns the first element of the ResizeArray.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The first element of the ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    let inline head (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "head"
        if resizeArray.Count = 0 then
            fail resizeArray "head: The input ResizeArray is empty."
        else
            resizeArray.[0]


    /// <summary>Builds a new ResizeArray whose elements are the corresponding elements of the input ResizeArray
    /// paired with the integer index (from 0) of each element.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The ResizeArray of indexed elements.</returns>
    let indexed (resizeArray: ResizeArray<'T>) : ResizeArray<int * 'T> =
        if isNull resizeArray then nullExn "indexed"
        let res = ResizeArray(resizeArray.Count)
        for i = 0 to resizeArray.Count - 1 do
            res.Add(i, resizeArray.[i])

        res

    /// <summary>Creates a ResizeArray given the dimension and a generator function to compute the elements.</summary>
    /// <param name="count">The number of elements to initialize.</param>
    /// <param name="initializer">The function to generate the initial values for each index.</param>
    /// <returns>The created ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when count is negative.</exception>
    let inline init (count: int) (initializer: int -> 'T) =
        if count < 0 then
            failSimpel $"init: count ({count}) is negative."
        let res = ResizeArray(count)
        for i = 0 to count - 1 do
            res.Add(initializer i)
        res

    /// <summary>Return a new ResizeArray with a new item inserted before the given index.(does NOT modify in place !)</summary>
    /// <param name="index">The index where the item should be inserted.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when index is below not within resizeArray.Count.</exception>
    let insertAt (index: int) (value: 'T) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "insertAt"
        if index < 0 || index > resizeArray.Count then
            fail resizeArray $"insertAt: index {index} not within resizeArray.Count {resizeArray.Count}."
        let r = ResizeArray(resizeArray.Count + 1)
        for i = 0 to index - 1 do
            r.Add resizeArray.[i]
        r.Add value
        for i = index to resizeArray.Count - 1 do
            r.Add resizeArray.[i]
        r


    /// <summary>Return a new ResizeArray with new items inserted before the given index.(does NOT modify in place !) If required increases the count of the ResizeArray.</summary>
    /// <param name="index">The index where the items should be inserted.</param>
    /// <param name="values">The values to insert.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when index is below not within resizeArray.Count.</exception>
    let insertManyAt (index: int) (values: ICollection<'T>) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "insertManyAt"
        if index < 0 || index > resizeArray.Count then
            fail resizeArray $"insertManyAt: index {index} and  not within resizeArray.Count {resizeArray.Count}."
        let r = ResizeArray(resizeArray.Count + values.Count)
        for i = 0 to index - 1 do
            r.Add resizeArray.[i]
        for v in values do
            r.Add v
        for i = index to resizeArray.Count - 1 do
            r.Add resizeArray.[i]
        r

    /// <summary>Returns true if the given ResizeArray is empty, otherwise false.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns><c>true</c> if the ResizeArray is empty.</returns>
    let inline isEmpty (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "isEmpty"
        resizeArray.Count = 0

    /// <summary>Gets an element from a ResizeArray.</summary>
    /// <param name="index">The input index.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The value of the ResizeArray at the given index.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the index is negative or the input ResizeArray does not contain enough elements.</exception>
    let inline item index (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "item"
        resizeArray.Get index

    /// <summary>Applies the given function to each element of the ResizeArray.</summary>
    /// <param name="action">The function to apply.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    let inline iter ( (*[<InlineIfLambda>]*) action) (resizeArray: ResizeArray<'T>) = // TODO activate InlineIfLambda
        if isNull resizeArray then nullExn "iter"
        for i = 0 to resizeArray.Count - 1 do
            action resizeArray.[i]


    /// <summary>Applies the given function to pair of elements drawn from matching indices in two ResizeArrays. The
    /// two ResizeArrays must have the same lengths, otherwise an <c>ArgumentException</c> is
    /// raised.</summary>
    /// <param name="action">The function to apply.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    let inline iter2 (action: 'T -> 'U -> unit) (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'U>) =
        if isNull resizeArray1 then nullExn "iter2 first"
        if isNull resizeArray2 then nullExn "iter2 second"
        if resizeArray1.Count <> resizeArray2.Count then
            fail resizeArray1  $"iter2: count of resizeArray1 {resizeArray1.Count} does not match resizeArray2 {resizeArray2.Count}."
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(action)
        for i = 0 to resizeArray1.Count - 1 do
            f.Invoke(resizeArray1.[i], resizeArray2.[i])


    /// <summary>Applies the given function to each element of the ResizeArray. The integer passed to the
    /// function indicates the index of element.</summary>
    /// <param name="action">The function to apply to each index and element.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    let inline iteri (action: int -> 'T -> unit) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "iteri"
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(action)
        for i = 0 to resizeArray.Count - 1 do
            f.Invoke(i, resizeArray.[i])


    /// <summary>Applies the given function to pair of elements drawn from matching indices in two ResizeArrays,
    /// also passing the index of the elements. The two ResizeArrays must have the same lengths,
    /// otherwise an <c>ArgumentException</c> is raised.</summary>
    /// <param name="action">The function to apply to each index and pair of elements.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    let inline iteri2 (action: int -> 'T -> 'U -> unit) (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'U>) =
        if isNull resizeArray1 then nullExn "iteri2 first"
        if isNull resizeArray2 then nullExn "iteri2 second"
        if resizeArray1.Count <> resizeArray2.Count then
            fail resizeArray1 $"iteri2: count of resizeArray1 {resizeArray1.Count} does not match resizeArray2 {resizeArray2.Count}."
        let f = OptimizedClosures.FSharpFunc<_, _, _, _>.Adapt(action)
        for i = 0 to resizeArray1.Count - 1 do
            f.Invoke(i, resizeArray1.[i], resizeArray2.[i])


    /// <summary>Returns the last element of the ResizeArray.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The last element of the ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the input does not have any elements.</exception>
    let inline last (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "last"
        if resizeArray.Count = 0 then
            fail resizeArray "last: The input ResizeArray is empty."
        resizeArray.[resizeArray.Count - 1]


    /// <summary>Returns the length of a ResizeArray. You can also use property resizeArray.Count.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The length or count of the ResizeArray.</returns>
    let inline length (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "length"
        resizeArray.Count

    /// <summary>Builds a new ResizeArray whose elements are the results of applying the given function
    /// to each of the elements of the ResizeArray.</summary>
    /// <param name="mapping">The function to transform elements of the ResizeArray.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The ResizeArray of transformed elements.</returns>
    let inline map (mapping: 'T -> 'U) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
        if isNull resizeArray then nullExn "map"
        // TODO replace with F# implementation using [<InlineIfLambda>] for performance?? Test on non Lambdas too.
        resizeArray.ConvertAll(System.Converter mapping)

    /// <summary>Builds a new Array whose elements are the results of applying the given function
    /// to each of the elements of the ResizeArray.</summary>
    /// <param name="mapping">The function to transform elements of the ResizeArray.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The Array of transformed elements.</returns>
    let inline mapToArray (mapping: 'T -> 'U) (resizeArray: ResizeArray<'T>) : 'U[] =
        if isNull resizeArray then nullExn "mapToArray"
        let res = Array.zeroCreate<'U> resizeArray.Count
        for i = 0 to resizeArray.Count - 1 do
            res.[i] <- mapping resizeArray.[i]
        res

    /// <summary>Builds a new ResizeArray whose elements are the results of applying the given function
    /// to each of the elements of the Array.</summary>
    /// <param name="mapping">The function to transform elements of the ResizeArray.</param>
    /// <param name="arr">The input Array.</param>
    /// <returns>The ResizeArray of transformed elements.</returns>
    let inline mapFromArray (mapping: 'T -> 'U) (arr: array<'T>) : ResizeArray<'U> =
        if isNull arr then nullExn "mapFromArray"
        let res = ResizeArray<'U>(arr.Length)
        for i = 0 to arr.Length - 1 do
            res.Add(mapping arr.[i])
        res

    /// <summary>Builds a new ResizeArray whose elements are the results of applying the given function
    /// to each of the elements of a collection with IList interface.</summary>
    /// <param name="mapping">The function to transform elements of the ResizeArray.</param>
    /// <param name="list">The input collection with IList interface.</param>
    /// <returns>The ResizeArray of transformed elements.</returns>
    let inline mapFromIList (mapping: 'T -> 'U) (list: IList<'T>) : ResizeArray<'U> =
        if isNull list then nullExn "mapFromIList"
        let res = ResizeArray<'U>(list.Count)
        for i = 0 to list.Count - 1 do
            res.Add(mapping list.[i])
        res

    /// <summary>Builds a new ResizeArray whose elements are the results of applying the given function
    /// to each of the elements of an IEnumerable.</summary>
    /// <param name="mapping">The function to transform elements of the ResizeArray.</param>
    /// <param name="sequence">The input IEnumerable.</param>
    /// <returns>The ResizeArray of transformed elements.</returns>
    let inline mapFromSeq (mapping: 'T -> 'U) (sequence: seq<'T>) : ResizeArray<'U> =
        if isNull sequence then nullExn "mapFromSeq"
        let res = ResizeArray<'U>()
        use e = sequence.GetEnumerator()
        while e.MoveNext() do
            res.Add(mapping e.Current)
        res

    /// <summary>Builds a new collection whose elements are the results of applying the given function
    /// to the corresponding elements of the two collections pairwise. The two input
    /// ResizeArrays must have the same lengths, otherwise an <c>ArgumentException</c> is
    /// raised.</summary>
    /// <param name="mapping">The function to transform the pairs of the input elements.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    /// <returns>The ResizeArray of transformed elements.</returns>
    let map2 (mapping: 'T1 -> 'T2 -> 'U) (resizeArray1: ResizeArray<'T1>) (resizeArray2: ResizeArray<'T2>) : ResizeArray<'U> =
        if isNull resizeArray1 then nullExn "map2 first"
        if isNull resizeArray2 then nullExn "map2 second"
        if resizeArray1.Count <> resizeArray2.Count then
            fail resizeArray1 $"map2: count of resizeArray1 {resizeArray1.Count} does not match resizeArray2 {resizeArray2.Count}."
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(mapping)
        let res = ResizeArray<'U>(resizeArray1.Count)
        for i = 0 to resizeArray1.Count - 1 do
            res.Add(f.Invoke(resizeArray1.[i], resizeArray2.[i]))
        res

    /// <summary>Builds a new collection whose elements are the results of applying the given function
    /// to the corresponding triples from the three collections. The three input
    /// ResizeArrays must have the same length, otherwise an <c>ArgumentException</c> is
    /// raised.</summary>
    /// <param name="mapping">The function to transform the pairs of the input elements.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <param name="resizeArray3">The third input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    /// <returns>The ResizeArray of transformed elements.</returns>
    let map3 (mapping: 'T1 -> 'T2 -> 'T3 -> 'U) (resizeArray1: ResizeArray<'T1>) (resizeArray2: ResizeArray<'T2>) (resizeArray3: ResizeArray<'T3>) : ResizeArray<'U> =
        if isNull resizeArray1 then nullExn "map3 first"
        if isNull resizeArray2 then nullExn "map3 second"
        if isNull resizeArray3 then nullExn "map3 third"
        let len1 = resizeArray1.Count
        if len1 <> resizeArray2.Count || len1 <> resizeArray3.Count then
            fail resizeArray1 $"map3: count of resizeArray1 {len1} does not match resizeArray2 {resizeArray2.Count} or resizeArray3 {resizeArray3.Count}."
        let f = OptimizedClosures.FSharpFunc<_, _, _, _>.Adapt(mapping)
        let res = ResizeArray(len1)
        for i = 0 to resizeArray1.Count - 1 do
            res.Add <| f.Invoke(resizeArray1.[i], resizeArray2.[i], resizeArray3.[i])
        res


    /// <summary>Combines map and fold. Builds a new ResizeArray whose elements are the results of applying the given function
    /// to each of the elements of the input ResizeArray. The function is also used to accumulate a final value.</summary>
    /// <param name="mapping">The function to transform elements from the input ResizeArray and accumulate the final value.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The ResizeArray of transformed elements, and the final accumulated value.</returns>
    let mapFold<'T, 'State, 'Result> (mapping: 'State -> 'T -> 'Result * 'State) state (resizeArray: ResizeArray<'T>) : ResizeArray<'Result> * 'State =
        if isNull resizeArray then nullExn "mapFold"
        match resizeArray.Count with
        | 0 -> ResizeArray(0), state
        | len ->
            let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(mapping)
            let mutable acc = state
            let res = ResizeArray(len)
            for i = 0 to len - 1 do
                let h, s = f.Invoke(acc, resizeArray.[i])
                res.Add h
                acc <- s
            res, acc

    /// <summary>Combines map and foldBack. Builds a new ResizeArray whose elements are the results of applying the given function
    /// to each of the elements of the input ResizeArray. The function is also used to accumulate a final value.</summary>
    /// <param name="mapping">The function to transform elements from the input ResizeArray and accumulate the final value.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The ResizeArray of transformed elements, and the final accumulated value.</returns>
    let mapFoldBack<'T, 'State, 'Result> (mapping: 'T -> 'State -> 'Result * 'State) (resizeArray: ResizeArray<'T>) state : ResizeArray<'Result> * 'State =
        if isNull resizeArray then nullExn "mapFoldBack"
        match resizeArray.Count with
        | 0 -> ResizeArray(0), state
        | len ->
            let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(mapping)
            let mutable acc = state
            let res = ResizeArray(len)
            for i = 0 to len - 1 do
                res.Add Unchecked.defaultof<'Result> // needs to be filled already because of 'downto' loop
            for i = len - 1 downto 0 do
                let h, s = f.Invoke(resizeArray.[i], acc)
                res.[i] <- h
                acc <- s
            res, acc

    /// <summary>Builds a new ResizeArray whose elements are the results of applying the given function
    /// to each of the elements of the ResizeArray. The integer index passed to the
    /// function indicates the index of element being transformed.</summary>
    /// <param name="mapping">The function to transform elements and their indices.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The ResizeArray of transformed elements.</returns>
    let mapi (mapping: int -> 'T -> 'U) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
        if isNull resizeArray then nullExn "mapi"
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(mapping)
        let res = ResizeArray(resizeArray.Count)
        for i = 0 to resizeArray.Count - 1 do
            res.Add <| f.Invoke(i, resizeArray.[i])
        res

    /// <summary>Builds a new collection whose elements are the results of applying the given function
    /// to the corresponding elements of the two collections pairwise, also passing the index of
    /// the elements. The two input ResizeArrays must have the same lengths, otherwise an <c>ArgumentException</c> is
    /// raised.</summary>
    /// <param name="mapping">The function to transform pairs of input elements and their indices.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    /// <returns>The ResizeArray of transformed elements.</returns>
    let mapi2 (mapping: int -> 'T1 -> 'T2 -> 'U) (resizeArray1: ResizeArray<'T1>) (resizeArray2: ResizeArray<'T2>) : ResizeArray<'U> =
        if isNull resizeArray1 then nullExn "mapi2 first"
        if isNull resizeArray2 then nullExn "mapi2 second"
        let f = OptimizedClosures.FSharpFunc<_, _, _, _>.Adapt(mapping)
        if resizeArray1.Count <> resizeArray2.Count then
            fail resizeArray1 $"mapi2: count of resizeArray1 {resizeArray1.Count} does not match resizeArray2 {resizeArray2.Count}."
        let res = ResizeArray(resizeArray1.Count)
        for i = 0 to resizeArray1.Count - 1 do
            res.Add <| f.Invoke(i, resizeArray1.[i], resizeArray2.[i])
        res

    /// <summary>Returns the greatest of all elements of the ResizeArray, compared via Operators.max on the function result.
    /// Throws ArgumentException for empty ResizeArrays.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    /// <returns>The maximum element.</returns>
    let inline max (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "max"
        if resizeArray.Count = 0 then
            fail resizeArray "max: Count must be at least one"
        let mutable acc = resizeArray.[0]
        for i = 1 to resizeArray.Count - 1 do
            let curr = resizeArray.[i]
            if curr > acc then acc <- curr
        acc

    /// <summary>Returns the greatest of all elements of the ResizeArray, compared via Operators.max on the function result.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    /// <returns>The maximum element.</returns>
    let inline maxBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : 'T =
        if isNull resizeArray then nullExn "maxBy"
        if resizeArray.Count = 0 then
            fail resizeArray "maxBy: Count must be at least one"
        let mutable accV = resizeArray.[0]
        if resizeArray.Count = 1 then
            accV // if len = 1 then don't call the projection not even once !
        else
            let mutable acc = projection accV
            for i = 1 to resizeArray.Count - 1 do
                let currV = resizeArray.[i]
                let curr = projection currV
                if curr > acc then
                    acc <- curr
                    accV <- currV
            accV

    /// <summary>Returns the lowest of all elements of the ResizeArray, compared via Operators.min.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    /// <returns>The minimum element.</returns>
    let inline min (resizeArray: ResizeArray<'T>) : 'T =
        if isNull resizeArray then nullExn "min"
        if resizeArray.Count = 0 then
            fail resizeArray "min: Count must be at least one"
        let mutable acc = resizeArray.[0]
        for i = 1 to resizeArray.Count - 1 do
            let curr = resizeArray.[i]
            if curr < acc then acc <- curr
        acc

    /// <summary>Returns the lowest of all elements of the ResizeArray, compared via Operators.min on the function result.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    /// <returns>The minimum element.</returns>
    let inline minBy ( (*[<InlineIfLambda>]*) projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : 'T =
        if isNull resizeArray then nullExn "minBy"
        if resizeArray.Count = 0 then
            fail resizeArray "minBy: Count must be at least one"
        let mutable accV = resizeArray.[0]
        if resizeArray.Count = 1 then
            accV // if len = 1 then don't call the projection not even once !
        else
            let mutable acc = projection accV
            for i = 1 to resizeArray.Count - 1 do
                let currV = resizeArray.[i]
                let curr = projection currV
                if curr < acc then
                    acc <- curr
                    accV <- currV
            accV

    /// <summary>Builds a ResizeArray from the given list.</summary>
    /// <param name="list">The input list.</param>
    /// <returns>The ResizeArray of elements from the list.</returns>
    let ofList (list: list<'T>) =
        let res = ResizeArray<_>()
        let mutable l = list
        while not l.IsEmpty do
            res.Add l.Head
            l <- l.Tail
        res

    /// <summary>Builds a new ResizeArray from the given enumerable object. </summary>
    /// <param name="source">The input sequence.</param>
    /// <returns>The ResizeArray of elements from the sequence.</returns>
    let ofSeq (source: seq<'T>) =
        if isNull source then nullExn "ofSeq"
        ResizeArray(source)


    /// <summary>Returns a ResizeArray of each element in the input ResizeArray and its predecessor, with the
    /// exception of the first element which is only returned as the predecessor of the second element.
    /// If the ResizeArray has 0 or 1 item an empty ResizeArray is returned</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    let pairwise (resizeArray: ResizeArray<'T>) : ResizeArray<'T * 'T> =
        if isNull resizeArray then nullExn "pairwise"
        if resizeArray.Count < 2 then
            ResizeArray(0)
        else
            init (resizeArray.Count - 1) (fun i -> resizeArray.[i], resizeArray.[i + 1])


    /// <summary>Splits the collection into two collections, containing the
    /// elements for which the given predicate returns <c>true</c> and <c>false</c>
    /// respectively.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>A pair of ResizeArrays. The first containing the elements the predicate evaluated to <c>true</c> ,
    /// and the second containing those evaluated to <c>false</c>.</returns>
    let partition (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> * ResizeArray<'T> =
        if isNull resizeArray then nullExn "partition"
        let trueResults = ResizeArray()
        let falseResults = ResizeArray()
        let len = resizeArray.Count
        for i = 0 to len - 1 do
            let el = resizeArray.[i]
            if predicate el then
                trueResults.Add el
            else
                falseResults.Add el
        trueResults, falseResults


    /// <summary>Returns a ResizeArray with all elements permuted according to the specified permutation.</summary>
    /// <param name="indexMap">The function that maps input indices to output indices.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The output ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when indexMap does not produce a valid permutation.</exception>
    let permute (indexMap: int -> int) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "permute"
        let res = resizeArray.Clone()
        let inv = Array.zeroCreate resizeArray.Count
        for i = 0 to resizeArray.Count - 1 do
            let j = indexMap i
            if j < 0 || j >= resizeArray.Count then
                fail resizeArray $"permute: the indexMap generated {j} from {i} but only 0 to {(resizeArray.Count-1)} is allowed"
            res.[j] <- resizeArray.[i]
            inv.[j] <- 1uy
        for i = 0 to resizeArray.Count - 1 do
            if inv.[i] <> 1uy then
                fail resizeArray $"permute: the indexMap function did not generated {i} a new value for "
        res


    /// <summary>Applies the given function to successive elements, returning the first
    /// result where function returns <c>Some(x)</c> for some <c>x</c>. If the function
    /// never returns <c>Some(x)</c> then <see cref="T:System.Collections.Generic.KeyNotFoundException"/> is raised.</summary>
    /// <param name="chooser">The function to generate options from the elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown if every result from
    /// <c>chooser</c> is <c>None</c>.</exception>
    /// <returns>The first result.</returns>
    let pick (chooser: 'T -> 'U option) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "pick"
        let mutable i = 0
        let mutable result = None
        while i < resizeArray.Count && result.IsNone do
            result <-  chooser resizeArray.[i]
            i <- i + 1
        match result with
        | Some res -> res
        | None -> failKey resizeArray $"pick: Key not found in {resizeArray.Count} elements"

    /// <summary>Starting from last element going backwards. Applies the given function to successive elements, returning the first
    /// result where function returns <c>Some(x)</c> for some <c>x</c>. If the function
    /// never returns <c>Some(x)</c> then <see cref="T:System.Collections.Generic.KeyNotFoundException"/> is raised.</summary>
    /// <param name="chooser">The function to generate options from the elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown if every result from
    /// <c>chooser</c> is <c>None</c>.</exception>
    /// <returns>The first result. From the end of the ResizeArray.</returns>
    let pickBack (chooser: 'T -> 'U option) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "pickBack"
        let mutable i = resizeArray.Count - 1
        let mutable result = None
        while i >= 0 && result.IsNone do
            result <- chooser resizeArray.[i]
            i <- i - 1
        match result with
        | Some res -> res
        | None -> failKey resizeArray $"pickBack: Key not found in {resizeArray.Count} elements"

    /// <summary>Applies a function to each element of the ResizeArray, threading an accumulator argument
    /// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c>
    /// then computes <c>f (... (f i0 i1)...) iN</c>.
    /// Raises ArgumentException if the ResizeArray has size zero.</summary>
    /// <param name="reduction">The function to reduce a pair of elements to a single element.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    /// <returns>The final result of the reductions.</returns>
    let reduce (reduction: 'T -> 'T -> 'T) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "reduce"
        if resizeArray.Count = 0 then
            fail resizeArray "reduce: Count must be at least one"
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(reduction)
        let mutable res = resizeArray.[0]
        for i = 1 to resizeArray.Count - 1 do
            res <- f.Invoke(res, resizeArray.[i])
        res


    /// <summary>Applies a function to each element of the ResizeArray, starting from the end, threading an accumulator argument
    /// through the computation. If the input function is <c>f</c> and the elements are <c>i0...iN</c>
    /// then computes <c>f i0 (...(f iN-1 iN))</c>.</summary>
    /// <param name="reduction">A function that takes in the next-to-last element of the list and the
    /// current accumulated result to produce the next accumulated result.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
    /// <returns>The final result of the reductions.</returns>
    let reduceBack (reduction: 'T -> 'T -> 'T) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "reduceBack"
        if resizeArray.Count = 0 then
            fail resizeArray "reduceBack: Count must be at least one"
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(reduction)
        let mutable res = resizeArray.Last
        for i = resizeArray.Count - 2 downto 0 do
            res <- f.Invoke(resizeArray.[i], res)
        res


    /// <summary>Return a new ResizeArray with the item at a given index removed. (does NOT modify in place !)</summary>
    /// <param name="index">The index of the item to be removed.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when index is outside 0..resizeArray.Length - 1</exception>
    let removeAt (index: int) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "removeAt"
        if index < 0 || index >= resizeArray.Count then
            fail resizeArray $"removeAt: index {index} not within resizeArray.Count {resizeArray.Count}."
        let r = resizeArray.Clone()
        r.RemoveAt(index)
        r

    /// <summary>Return a new ResizeArray with the number of items starting at a given index removed. (does NOT modify in place !)</summary>
    /// <param name="index">The index of the first item to be removed.</param>
    /// <param name="count">The number of items to remove.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when index is outside 0..resizeArray.Length - count</exception>
    let removeManyAt (index: int) (count: int) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "removeManyAt"
        if index < 0 || index > resizeArray.Count - count then
            fail resizeArray $"removeManyAt: index {index} and count {count} not within resizeArray.Count {resizeArray.Count}."
        let r = resizeArray.Clone()
        r.RemoveRange(index, count)
        r

    /// <summary>Creates a ResizeArray by replicating the given initial value.</summary>
    /// <param name="count">The number of elements to replicate.</param>
    /// <param name="initial">The value to replicate</param>
    /// <returns>The generated ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when count is negative.</exception>
    let replicate count (initial: 'T) =
        if count < 0 then
            failSimpel $"replicate: count {count} cannot be negative "
        let arr = ResizeArray(count)
        for _ = 0 to count - 1 do
            arr.Add(initial)
        arr

    /// <summary>Returns a new ResizeArray with the elements in reverse order.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The reversed ResizeArray.</returns>
    let rev (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "rev"
        let len = resizeArray.Count
        let result = ResizeArray(len)
        for i = len - 1 downto 0 do
            result.Add resizeArray.[i]
        result

    /// <summary>Reverses the order of ResizeArray in place.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>unit</returns>
    let revInPlace (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "revInPlace"
        resizeArray.Reverse()


    /// <summary>Like <c>fold</c>, but return the intermediary and final results.</summary>
    /// <param name="folder">The function to update the state given the input elements.</param>
    /// <param name="stateInit">The initial state.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The ResizeArray of state values. This ResizeArray has the initial state and the state for all elements in the input ResizeArray,
    /// so one more item than the input.</returns>
    let scan<'T, 'State> (folder: 'State -> 'T -> 'State) (stateInit: 'State) (resizeArray: ResizeArray<'T>) : ResizeArray<'State> =
        if isNull resizeArray then nullExn "scan"
        let folder = OptimizedClosures.FSharpFunc<_, _, _>.Adapt folder
        // Holds the initial and intermediate state values.
        let results = ResizeArray(resizeArray.Count + 1)
        results.Add stateInit
        // Fold over the specified range of items.
        let mutable state = stateInit
        for i = 0 to resizeArray.Count - 1 do
            state <- folder.Invoke(state, resizeArray.[i])
            results.Add state
        results

    /// <summary>Like <c>foldBack</c>, but return both the intermediary and final results.</summary>
    /// <param name="folder">The function to update the state given the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <param name="stateInit">The initial state.</param>
    /// <returns>The ResizeArray of state values. Count = input count + 1 </returns>
    let scanBack<'T, 'State> (folder: 'T -> 'State -> 'State) (resizeArray: ResizeArray<'T>) (stateInit: 'State) : ResizeArray<'State> =
        if isNull resizeArray then nullExn "scanBack"
        let folder = OptimizedClosures.FSharpFunc<_, _, _>.Adapt folder
        let results = ResizeArray(resizeArray.Count + 1) // Holds the initial and intermediate state values.
        for _ = 0 to resizeArray.Count do // to fill up list +1 with default values
            results.Add stateInit
        // Fold over the specified range of items.
        let mutable state = stateInit
        for i = resizeArray.Count - 1 downto 0 do
            state <- folder.Invoke(resizeArray.[i], state)
            results.[i] <- state
        results


    /// <summary>Builds a new ResizeArray that contains the elements of the given ResizeArray, excluding the first N elements.</summary>
    /// <param name="count">The number of elements to skip.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>A copy of the input ResizeArray, after removing the first N elements.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when count is negative or exceeds the number of
    /// elements in the ResizeArray.</exception>
    let skip count (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "skip"
        if count < 0 || count > resizeArray.Count then
            fail resizeArray $"skip: count {count} is not in range of 0 to resizeArray.Count {resizeArray.Count} "

        if count = resizeArray.Count then
            ResizeArray()
        else
            resizeArray.GetRange(count, resizeArray.Count - count)


    /// <summary>Bypasses elements in a ResizeArray while the given predicate returns <c>true</c>, and then returns
    /// the remaining elements in a new ResizeArray.</summary>
    /// <param name="predicate">A function that evaluates an element of the ResizeArray to a boolean value.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The created sub ResizeArray.</returns>
    let skipWhile (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "skipWhile"
        let mutable i = 0
        while i < resizeArray.Count && predicate resizeArray.[i] do
            i <- i + 1
        if i = resizeArray.Count then
            ResizeArray()
        else
            resizeArray.GetRange(i, resizeArray.Count - i)


    /// <summary>Sorts the elements of a ResizeArray, returning a new ResizeArray. Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
    /// This means "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
    /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
    /// For a stable sort, consider using Seq.Sort</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>A new sorted ResizeArray.</returns>
    let sort<'T when 'T: comparison> (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "sort"
        let r = resizeArray.GetRange(0, resizeArray.Count) // fastest way to create a shallow copy
        r.Sort(Operators.compare)
        r


    /// <summary>Sorts the elements of a ResizeArray, using the given projection for the keys and returning a new ResizeArray.
    /// Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
    /// This means "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
    /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
    /// For a stable sort, consider using Seq.sort.</summary>
    /// <param name="projection">The function to transform ResizeArray elements into the type that is compared.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The sorted ResizeArray.</returns>
    let sortBy<'T, 'Key when 'Key: comparison> (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "sortBy"
        let r = resizeArray.GetRange(0, resizeArray.Count) // fastest way to create a shallow copy
        r.Sort(fun x y -> Operators.compare (projection x) (projection y))
        r


    /// <summary>Sorts the elements of a ResizeArray, in descending order, using the given projection for the keys and returning a new ResizeArray.
    /// Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
    /// This means in ascending sorting "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
    /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
    /// For a stable sort, consider using Seq.sort.</summary>
    /// <param name="projection">The function to transform ResizeArray elements into the type that is compared.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The sorted ResizeArray.</returns>
    let inline sortByDescending<'T, 'Key when 'Key: comparison> (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "sortByDescending"
        let r = resizeArray.GetRange(0, resizeArray.Count) // fastest way to create a shallow copy
        r.Sort(fun x y -> Operators.compare (projection y) (projection x)) // x and y are swapped for descending order
        r


    /// <summary>Sorts the elements of a ResizeArray, in descending order, returning a new ResizeArray. Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
    /// This means in ascending sorting "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
    /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
    /// For a stable sort, consider using Seq.sort.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The sorted ResizeArray.</returns>
    let sortDescending<'T when 'T: comparison> (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "sortDescending"
        let r = resizeArray.GetRange(0, resizeArray.Count) // fastest way to create a shallow copy
        r.Sort(Operators.compare) // Operators.compare is need to match sorting of Array.sort
        r.Reverse()
        r


    /// <summary>Sorts the elements of a ResizeArray by mutating the ResizeArray in-place, using the given comparison function.
    /// Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
    /// This means in ascending sorting "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
    /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
    /// For a stable sort, consider using Seq.sort.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    let sortInPlace<'T when 'T: comparison> (resizeArray: ResizeArray<'T>) : unit =
        if isNull resizeArray then nullExn "sortInPlace"
        resizeArray.Sort(Operators.compare) // Operators.compare is need to match sorting of Array.sort


    /// <summary>Sorts the elements of a ResizeArray by mutating the ResizeArray in-place, using the given projection for the keys.
    /// Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
    /// This means in ascending sorting "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
    /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
    /// For a stable sort, consider using Seq.sort.</summary>
    /// <param name="projection">The function to transform ResizeArray elements into the type that is compared.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    let sortInPlaceBy<'T, 'Key when 'Key: comparison> (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : unit =
        if isNull resizeArray then nullExn "sortInPlaceBy"
        resizeArray.Sort(fun x y -> Operators.compare (projection x) (projection y))

    /// <summary>Sorts the elements of a ResizeArray by mutating the ResizeArray in-place, using the given comparison function as the order.
    /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
    /// For a stable sort, consider using Seq.sort.</summary>
    /// <param name="comparer">The function to compare pairs of ResizeArray elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    let sortInPlaceWith (comparer: 'T -> 'T -> int) (resizeArray: ResizeArray<'T>) : unit =
        if isNull resizeArray then nullExn "sortInPlaceWith"
        resizeArray.Sort(comparer)

    /// <summary>Sorts the elements of a ResizeArray, using the given comparison function as the order, returning a new ResizeArray.
    /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
    /// For a stable sort, consider using Seq.sort.</summary>
    /// <param name="comparer">The function to compare pairs of ResizeArray elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The sorted ResizeArray.</returns>
    let sortWith (comparer: 'T -> 'T -> int) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "sortWith"
        let r = resizeArray.GetRange(0, resizeArray.Count) // fastest way to create a shallow copy
        r.Sort(comparer)
        r

    /// <summary>Splits a ResizeArray into two ResizeArrays, at the given index.</summary>
    /// <param name="index">The index at which the ResizeArray is split.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The two split ResizeArrays.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when split index exceeds the number of elements in the ResizeArray.</exception>
    let splitAt index (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "splitAt"
        if index < 0 || index > resizeArray.Count then
            fail resizeArray $"splitAt: index {index} is not in range  0 to resizeArray.Count-1 ({(resizeArray.Count-1)})"
        resizeArray.GetRange(0, index), resizeArray.GetRange(index, resizeArray.Count - index)


    /// <summary>Splits the input ResizeArray into at most <c>chunkCount</c> chunks.
    /// If the list can not be split evenly the initial elements will be one bigger than the later elements. Just like with Array.splitInto.</summary>
    /// <param name="chunkCount">The maximum number of chunks.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The ResizeArray split into chunks.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when <c>count</c> is not positive.</exception>
    let splitInto (chunkCount: int) (resizeArray: ResizeArray<'T>) : ResizeArray<ResizeArray<'T>> =
        if isNull resizeArray then nullExn "splitInto"
        if chunkCount < 1 then
            fail resizeArray $"splitInto: count {chunkCount} is less than 1"
        let len = resizeArray.Count
        if len = 0 then
            ResizeArray(0)
        else
            let mutable chunksize = (float len) / (float chunkCount) |> ceil |> int
            let oneBiggerFor = len % chunkCount // so the first few list might be bigger
            let mutable k = 0
            let mutable sub = ResizeArray(chunksize)
            let res = ResizeArray(chunkCount)
            res.Add(sub)
            for i = 0 to len - 1 do
                //printfn "i %d v: %d chunksize %d" i resizeArray.[i] chunksize
                if k = chunksize then
                    sub <- ResizeArray(chunksize)
                    res.Add(sub)
                    k <- 0
                    if res.Count = oneBiggerFor + 1 then // reduce list size once
                        chunksize <- chunksize - 1
                sub.Add(resizeArray.[i])
                k <- k + 1
            res


    /// <summary>Returns the sum of the elements in the ResizeArray.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The resulting sum.</returns>
    let inline sum (resizeArray: ^T ResizeArray) : ^T =
        if isNull resizeArray then nullExn "sum"
        let mutable acc = LanguagePrimitives.GenericZero< ^T>
        for i = 0 to resizeArray.Count - 1 do
            acc <- Checked.(+) acc resizeArray.[i]
        acc


    /// <summary>Returns the sum of the results generated by applying the function to each element of the ResizeArray.</summary>
    /// <param name="projection">The function to transform the ResizeArray elements into the type to be summed.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The resulting sum.</returns>
    let inline sumBy (projection: 'T -> ^Key) (resizeArray: ResizeArray<'T>) : ^Key =
        if isNull resizeArray then nullExn "sumBy"
        let mutable acc = LanguagePrimitives.GenericZero< ^Key>
        for i = 0 to resizeArray.Count - 1 do
            acc <- Checked.(+) acc (projection resizeArray.[i])
        acc


    /// <summary>Returns a new ResizeArray containing the elements of the original except the first element.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the ResizeArray is empty.</exception>
    /// <returns>A new ResizeArray containing the elements of the original except the first element.</returns>
    let tail (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "tail"
        if resizeArray.Count = 0 then
            fail resizeArray "tail: input ResizeArray is empty"
        resizeArray.GetRange(1, resizeArray.Count - 1)


    /// <summary>Returns the first N elements of the ResizeArray.
    /// Throws <c>ArgumentException</c> if the count exceeds the number of elements in the ResizeArray.
    /// Use <c>ResizeArray.truncate</c> to returns as many items as the ResizeArray contains instead of throwing an exception.</summary>
    /// <param name="count">The number of items to take.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty or count exceeds the number of elements in the list.</exception>
    let take count (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "take"
        if count < 0 then
            fail resizeArray $"take: count {count} cannot be negative."
        if count = 0 then
            ResizeArray(0)
        else
            if count > resizeArray.Count then
                fail resizeArray $"take: count {count} > resizeArray.Count {resizeArray.Count}."
            resizeArray.GetRange(0, count)



    /// <summary>Returns a ResizeArray that contains all elements of the original ResizeArray while the
    /// given predicate returns <c>true</c>, and then returns no further elements.</summary>
    /// <param name="predicate">A function that evaluates to false when no more items should be returned.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    let takeWhile (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "takeWhile"
        if resizeArray.Count = 0 then
            ResizeArray(0)
        else
            let mutable count = 0
            while count < resizeArray.Count && predicate resizeArray.[count] do
                count <- count + 1
            resizeArray.GetRange(0, count)



    /// <summary>Builds a list from the given ResizeArray.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The list of ResizeArray elements.</returns>
    let toList (resizeArray: ResizeArray<'T>) : list<'T> =
        if isNull resizeArray then nullExn "toList"
        let mutable res = []
        for i = resizeArray.Count - 1 downto 0 do
            res <- resizeArray.[i] :: res
        res


    /// <summary>Views the given ResizeArray as a sequence. using Seq.readonly </summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The sequence of ResizeArray elements.</returns>
    let toSeq (resizeArray: ResizeArray<'T>) : seq<'T> =
        if isNull resizeArray then nullExn "toSeq"
        Seq.readonly resizeArray


    /// <summary>Returns the transpose of the given sequence of ResizeArrays.</summary>
    /// <param name="resizeArrays">The input sequence of ResizeArrays.</param>
    /// <returns>The transposed ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    let transpose (resizeArrays: ResizeArray<ResizeArray<'T>>) : ResizeArray<ResizeArray<'T>> =
        if isNull resizeArrays then nullExn "transpose"
        // originally let transpose (resizeArrays: seq<ResizeArray<'T>>) : ResizeArray<ResizeArray<'T>> =
        let len = resizeArrays.Count
        if len = 0 then
            ResizeArray(0)
        else
            let lenInner = resizeArrays.[0].Count
            for j = 1 to len - 1 do
                if lenInner <> resizeArrays.[j].Count then
                    fail resizeArrays $"transpose: the count {resizeArrays.[j].Count} in sub ResizeArray {j} does not match the count of the first ResizeArray {lenInner}."
            let result = ResizeArray(lenInner)
            for i = 0 to lenInner - 1 do
                let sub = ResizeArray(len)
                result.Add(sub)
                for j = 0 to len - 1 do
                    sub.Add(resizeArrays.[j].[i])
            result


    /// <summary>Returns at most N elements in a new ResizeArray. When count is negative return empty ResizeArray.</summary>
    /// <param name="count">The maximum number of items to return.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    let truncate count (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "truncate"
        if count <= 0 then
            ResizeArray(0)
        else
            let c = Operators.min count resizeArray.Count
            resizeArray.GetRange(0, c)



    /// <summary>Returns the only element of the ResizeArray or <c>None</c> if ResizeArray is empty or contains more than one element.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The only element of the ResizeArray or <c>None</c>.</returns>
    let tryExactlyOne (resizeArray: ResizeArray<'T>) : option<'T> =
        if isNull resizeArray then nullExn "tryExactlyOne"
        if resizeArray.Count = 1 then Some resizeArray.[0] else None


    /// <summary>Returns the first element for which the given function returns <c>true</c>.
    /// Return None if no such element exists.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The first element that satisfies the predicate, or <c>None</c>.</returns>
    let tryFind (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : option<'T> =
        if isNull resizeArray then nullExn "tryFind"
        let elementIndex = resizeArray.FindIndex(System.Predicate predicate)
        match elementIndex with
        | -1 -> None
        | index -> Some resizeArray.[index]


    /// <summary>Returns the last element for which the given function returns <c>true</c>.
    /// Return None if no such element exists.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The last element that satisfies the predicate, or <c>None</c>.</returns>
    let tryFindBack (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : option<'T> =
        if isNull resizeArray then nullExn "tryFindBack"
        let mutable i = resizeArray.Count - 1
        let mutable result = None
        while i >= 0  do
            let element = resizeArray.[i]
            i <- i - 1
            if predicate element then
                result <- Some element
                i <- -1 // break the loop
        result


    /// <summary>Returns the index of the first element in the ResizeArray
    /// that satisfies the given predicate.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The index of the first element that satisfies the predicate, or <c>None</c>.</returns>
    let tryFindIndex (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : option<int> =
        if isNull resizeArray then nullExn "tryFindIndex"
        let elementIndex = resizeArray.FindIndex(System.Predicate predicate)
        match elementIndex with
        | -1 -> None
        | index -> Some index


    /// <summary>Returns the index of the last element in the ResizeArray
    /// that satisfies the given predicate.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The index of the last element that satisfies the predicate, or <c>None</c>.</returns>
    let tryFindIndexBack (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : option<int> =
        if isNull resizeArray then nullExn "tryFindIndexBack"
        let mutable i = resizeArray.Count - 1
        let mutable result = None
        while i >= 0  && result.IsNone do
            if predicate resizeArray.[i] then
                result <- Some i
            i <- i - 1
        result


    /// <summary>Returns the first element of the ResizeArray, or
    /// <c>None</c> if the ResizeArray is empty.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The first element of the ResizeArray or <c>None</c>.</returns>
    let tryHead (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "tryHead"
        if resizeArray.Count = 0 then None else Some resizeArray.[0]


    /// <summary>Tries to find the nth element in the ResizeArray.
    /// Returns <c>None</c> if index is negative or the input ResizeArray does not contain enough elements.</summary>
    /// <param name="index">The index of element to retrieve.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The nth element of the ResizeArray or <c>None</c>.</returns>
    let tryItem index (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "tryItem"
        if index < 0 || index >= resizeArray.Count then
            None
        else
            Some(resizeArray.[index])


    /// <summary>Returns the last element of the ResizeArray.
    /// Return <c>None</c> if no such element exists.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The last element of the ResizeArray or <c>None</c>.</returns>
    let tryLast (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "tryLast"
        if resizeArray.Count = 0 then
            None
        else
            Some resizeArray.[resizeArray.Count - 1]


    /// <summary>Applies the given function to successive elements, returning the first
    /// result where function returns <c>Some(x)</c> for some <c>x</c>. If the function
    /// never returns <c>Some(x)</c> then <c>None</c> is returned.</summary>
    /// <param name="chooser">The function to transform the ResizeArray elements into options.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The first transformed element that is <c>Some(x)</c>.</returns>
    let tryPick chooser (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "tryPick"
        let mutable i = 0
        let mutable result = None
        while i < resizeArray.Count && result.IsNone do
            result <- chooser resizeArray.[i]
            i <- i + 1
        result


    /// <summary>Applies the given function to successive elements from the end of the list, returning the first
    /// result where function returns <c>Some(x)</c> for some <c>x</c>. If the function
    /// never returns <c>Some(x)</c> then <c>None</c> is returned.</summary>
    /// <param name="chooser">The function to transform the ResizeArray elements into options.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The first transformed element searched from the end of the list that is <c>Some(x)</c>.</returns>
    let tryPickBack chooser (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "tryPickBack"
        let mutable i = resizeArray.Count - 1
        let mutable result = None
        while i >= 0 && result.IsNone do
            result <- chooser resizeArray.[i]
            i <- i - 1
        result



    /// <summary>Returns a ResizeArray that contains the elements generated by the given computation.
    /// The given initial <c>state</c> argument is passed to the element generator.</summary>
    /// <param name="generator">A function that takes in the current state and returns an option tuple of the next
    /// element of the ResizeArray and the next state value.</param>
    /// <param name="state">The initial state value.</param>
    /// <returns>The result ResizeArray.</returns>
    let unfold<'T, 'State> (generator: 'State -> ('T * 'State) option) (state: 'State) :ResizeArray<'T>=
        let res = ResizeArray()
        let mutable currentState = state
        let mutable continueLoop = true
        while continueLoop do
            match generator currentState with
            | None -> continueLoop <- false
            | Some(x, nextState) ->
            res.Add(x)
            currentState <- nextState
        res


    /// <summary>Splits a ResizeArray of pairs into two ResizeArrays.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The two ResizeArrays.</returns>
    let unzip (resizeArray: ResizeArray<'T * 'U>) : ResizeArray<'T> * ResizeArray<'U> =
        if isNull resizeArray then nullExn "unzip"
        let len = resizeArray.Count
        let res1 = ResizeArray(len)
        let res2 = ResizeArray(len)
        for i = 0 to resizeArray.Count - 1 do
            let x, y = resizeArray.[i]
            res1.Add <| x
            res2.Add <| y
        res1, res2


    /// <summary>Splits a ResizeArray of triples into three ResizeArrays.</summary>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The tuple of three ResizeArrays.</returns>
    let unzip3 (resizeArray: ResizeArray<'T * 'U * 'V>) =
        if isNull resizeArray then nullExn "unzip3"
        let len = resizeArray.Count
        let res1 = ResizeArray(len)
        let res2 = ResizeArray(len)
        let res3 = ResizeArray(len)
        for i = 0 to resizeArray.Count - 1 do
            let x, y, z = resizeArray.[i]
            res1.Add <| x
            res2.Add <| y
            res3.Add <| z
        res1, res2, res3

    /// <summary>Return a new ResizeArray with the item at a given index set to the new value. (does NOT modify in place !)</summary>
    /// <param name="index">The index of the item to be replaced.</param>
    /// <param name="value">The new value.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when index is not within resizeArray.Count </exception>
    let updateAt (index: int) (value: 'T) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
        if isNull resizeArray then nullExn "updateAt"
        if index < 0 || index >= resizeArray.Count then
            badSetExn index resizeArray "updateAt" value
        let r = resizeArray.Clone()
        r.[index] <- value
        r


    /// <summary>Returns a new ResizeArray containing only the elements of the ResizeArray
    /// for which the given predicate returns <c>true</c>.</summary>
    /// <param name="predicate">The function to test the input elements.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>a ResizeArray containing the elements for which the given predicate returns true.</returns>
    let where (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "where"
        filter predicate resizeArray


    /// <summary>Returns a ResizeArray of sliding windows containing elements drawn from the input ResizeArray. Each window is returned as a fresh ResizeArray.</summary>
    /// <param name="windowSize">The number of elements in each window.</param>
    /// <param name="resizeArray">The input ResizeArray.</param>
    /// <returns>The result ResizeArray.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when windowSize is not positive.</exception>
    let windowed windowSize (resizeArray: ResizeArray<'T>) =
        if isNull resizeArray then nullExn "windowed"
        if windowSize <= 0 then
            fail resizeArray $"windowed: windowSize {windowSize} cannot be negative or 0."
        let len = resizeArray.Count
        if windowSize > len then
            ResizeArray(0)
        else
            let res = ResizeArray(len - windowSize + 1)
            for i = 0 to len - windowSize do
                res.Add <| resizeArray.GetRange(i, windowSize)
            res


    /// <summary>Combines the two ResizeArrays into a ResizeArray of pairs. The two ResizeArrays must have equal lengths, otherwise an <c>ArgumentException</c> is raised.</summary>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    /// <returns>The ResizeArray of tupled elements.</returns>
    let zip (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'U>) =
        if isNull resizeArray1 then nullExn "zip first"
        if isNull resizeArray2 then nullExn "zip second"
        let len1 = resizeArray1.Count
        if len1 <> resizeArray2.Count then
            fail resizeArray1 "zip: count of resizeArray1 {len1} does not match resizeArray2 {resizeArray2.Count}."
        let res = ResizeArray(len1)
        for i = 0 to resizeArray1.Count - 1 do
            res.Add(resizeArray1.[i], resizeArray2.[i])
        res


    /// <summary>Combines three ResizeArrays into a ResizeArray of pairs. The three ResizeArrays must have equal lengths, otherwise an <c>ArgumentException</c> is raised.</summary>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <param name="resizeArray3">The third input ResizeArray.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
    /// <returns>The ResizeArray of tupled elements.</returns>
    let zip3 (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'U>) (resizeArray3: ResizeArray<'V>) =
        if isNull resizeArray1 then nullExn "zip3 first"
        if isNull resizeArray2 then nullExn "zip3 second"
        if isNull resizeArray3 then nullExn "zip3 third"
        let len1 = resizeArray1.Count
        if len1 <> resizeArray2.Count || len1 <> resizeArray3.Count then
            fail resizeArray1 $"zip3: count of resizeArray1 {len1} does not match resizeArray2 {resizeArray2.Count} or resizeArray3 {resizeArray3.Count}."
        let res = ResizeArray(len1)
        for i = 0 to resizeArray1.Count - 1 do
            res.Add(resizeArray1.[i], resizeArray2.[i], resizeArray3.[i])
        res


    /// <summary>Like .zip, but when one of the two input lists is exhausted, the getDefaultVal function is used for the rest of the output.</summary>
    /// <param name="getGefaultVal">A function that takes the current index and the current value of the longer list and returns a default value for the shorter list.</param>
    /// <param name="resizeArray1">The first input ResizeArray.</param>
    /// <param name="resizeArray2">The second input ResizeArray.</param>
    /// <returns>The ResizeArray of tupled elements.</returns>
    let zipDefault (getGefaultVal: int -> 'T -> 'T) (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'T>)  =
        if isNull resizeArray1 then nullExn "zipDefault first"
        if isNull resizeArray2 then nullExn "zipDefault second"
        // first is longer
        if resizeArray1.Count > resizeArray2.Count  then
            let res = ResizeArray(resizeArray1.Count)
            for i = 0 to resizeArray2.LastIndex do
                res.Add(resizeArray1.[i] ,resizeArray2.[i])
            for i = resizeArray2.Count to resizeArray1.LastIndex do
                let x = resizeArray1.[i]
                res.Add(x, getGefaultVal i x)
            res
        // second is longer
        elif resizeArray2.Count > resizeArray1.Count then
            let res = ResizeArray(resizeArray2.Count)
            for i = 0 to resizeArray1.LastIndex do
                res.Add(resizeArray1.[i] ,resizeArray2.[i])
            for i = resizeArray1.Count to resizeArray2.LastIndex do
                let x = resizeArray2.[i]
                res.Add(getGefaultVal i x, x)
            res
        // equal length
        else
            let res = ResizeArray(resizeArray1.Count)
            for i = 0 to resizeArray1.LastIndex do
                res.Add(resizeArray1.[i] ,resizeArray2.[i])
            res



    /// Treats the ResizeArray as a loop.
    /// For each element, it gets the Prev-This and the This-Next combination using the combineAdjacent function.
    /// Finally creates a new type using itself and the Prev-This and the This-Next combination for each element
    /// The first element uses the last element as previous, and the last element uses the first element as next.
    /// On each element it caches the This-Next combination and uses it for the next element as Prev-This.
    let mapPrevNext(combineAdjacent: 'T -> 'T -> 'U) (mergePrevAndNextCombineResults: 'T -> 'U -> 'U -> 'V) (xs:ResizeArray<'T>) : ResizeArray<'V> =
        let res = ResizeArray<'V>(xs.Count)
        if xs.Count = 0 then
            res
        else
            let mutable prev = xs.[xs.Count-1]
            let mutable this = xs.[0]
            let mutable firstCombined = combineAdjacent prev this
            for i = 1 to xs.Count-1 do
                let next = xs.[i]
                let nextCombined = combineAdjacent this next
                res.Add(mergePrevAndNextCombineResults this firstCombined nextCombined)
                prev <- this
                this <- next
                firstCombined <- nextCombined
            // close the loop
            let nextCombined = combineAdjacent this xs.[0]
            res.Add(mergePrevAndNextCombineResults this firstCombined nextCombined)
            res



















    #if FABLE_COMPILER
    // Fable doesn't support System.Threading.Tasks.Parallel.For
    // the Parallel operations on ResizeArray are just sequential in Fable JavaScript
    // module ResizeArray.Parallel = ResizeArray.ResizeArray // or just shadow it ??
    #else

    /// Parallel operations on ResizeArray using Threading.Tasks.Parallel.For
    /// The API is aligned with from FSharp.Core.Array.Parallel module
    module Parallel =

        open System.Threading.Tasks

        /// <summary>Apply the given function to each element of the ResizeArray. Return
        /// the ResizeArray comprised of the results "x" for each element where
        /// the function returns Some(x).
        /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
        /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
        /// <param name="chooser">The function to generate options from the elements.</param>
        /// <param name="resizeArray">The input ResizeArray.</param>
        /// <returns>The ResizeArray of results.</returns>
        let choose (chooser: 'T -> option<'U>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
            if isNull resizeArray then nullExn "Parallel.choose"
            let inputLength = resizeArray.Count
            let isChosen: bool[] = Array.zeroCreate inputLength
            let results: 'U[] = Array.zeroCreate inputLength
            let mutable outputLength = 0
            Parallel.For(
                0,
                inputLength,
                (fun () -> 0),
                (fun i _ count ->
                    match chooser resizeArray.[i] with
                    | None -> count
                    | Some v ->
                        isChosen.[i] <- true
                        results.[i] <- v
                        count + 1),
                Action<int>(fun x -> System.Threading.Interlocked.Add(&outputLength, x) |> ignore)
            ) |> ignore

            let output = ResizeArray(outputLength)
            for i = 0 to isChosen.Length - 1 do
                if isChosen.[i] then output.Add results.[i]
            output

        /// <summary>For each element of the ResizeArray, apply the given function. Concatenate all the results and return the combined ResizeArray.
        /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
        /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
        /// <param name="mapping"></param>
        /// <param name="resizeArray">The input ResizeArray.</param>
        /// <returns>'U[]</returns>
        let collect (mapping: 'T -> ResizeArray<'U>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
            if isNull resizeArray then nullExn "Parallel.collect"
            let inputLength = resizeArray.Count
            let result = create inputLength Unchecked.defaultof<_>
            Parallel.For(0, inputLength, (fun i -> result.[i] <- mapping resizeArray.[i]))|> ignore
            concat result


        /// <summary>Create a ResizeArray given the dimension and a generator function to compute the elements.
        /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
        /// The order in which the given function is applied to indices is not specified.</summary>
        /// <param name="count"></param>
        /// <param name="initializer"></param>
        /// <returns>The ResizeArray of results.</returns>
        let init count (initializer: int -> 'T) : ResizeArray<'T> =
            let result = create count Unchecked.defaultof<_>
            Parallel.For(0, count, (fun i -> result.[i] <- initializer i)) |> ignore
            result


        /// <summary>Apply the given function to each element of the ResizeArray.
        /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
        /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
        /// <param name="action"></param>
        /// <param name="resizeArray">The input ResizeArray.</param>
        let iter (action: 'T -> unit) (resizeArray: ResizeArray<'T>) =
            if isNull resizeArray then nullExn "Parallel.iter"
            Parallel.For(0, resizeArray.Count, (fun i -> action resizeArray.[i])) |> ignore


        /// <summary>Apply the given function to each element of the ResizeArray. The integer passed to the
        /// function indicates the index of element.
        /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
        /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
        /// <param name="action"></param>
        /// <param name="resizeArray">The input ResizeArray.</param>
        let iteri (action: int -> 'T -> unit) (resizeArray: ResizeArray<'T>) =
            if isNull resizeArray then nullExn "Parallel.iteri"
            let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(action)
            Parallel.For(0, resizeArray.Count, (fun i -> f.Invoke(i, resizeArray.[i])))
            |> ignore


        /// <summary>Build a new ResizeArray whose elements are the results of applying the given function
        /// to each of the elements of the ResizeArray.
        /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
        /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
        /// <param name="mapping"></param>
        /// <param name="resizeArray">The input ResizeArray.</param>
        /// <returns>The ResizeArray of results.</returns>
        let map (mapping: 'T -> 'U) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
            if isNull resizeArray then nullExn "Parallel.map"
            let inputLength = resizeArray.Count
            let result = create inputLength Unchecked.defaultof<_>
            Parallel.For(0, inputLength, (fun i -> result.[i] <- mapping resizeArray.[i]))
            |> ignore

            result


        /// <summary>Build a new ResizeArray whose elements are the results of applying the given function
        /// to each of the elements of the ResizeArray. The integer index passed to the
        /// function indicates the index of element being transformed.
        /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
        /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
        /// <param name="mapping"></param>
        /// <param name="resizeArray">The input ResizeArray.</param>
        /// <returns>The ResizeArray of results.</returns>
        let mapi (mapping: int -> 'T -> 'U) (resizeArray: ResizeArray<'T>) =
            if isNull resizeArray then nullExn "Parallel.mapi"
            let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(mapping)
            let inputLength = resizeArray.Count
            let result = create inputLength Unchecked.defaultof<_>
            Parallel.For(0, inputLength, (fun i -> result.[i] <- f.Invoke(i, resizeArray.[i])))
            |> ignore
            result


        /// <summary>Split the collection into two collections, containing the
        /// elements for which the given predicate returns <c>true</c> and <c>false</c>
        /// respectively
        /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
        /// The order in which the given function is applied to indices is not specified.</summary>
        /// <param name="predicate">The function to test the input elements.</param>
        /// <param name="resizeArray">The input ResizeArray.</param>
        /// <returns>The two ResizeArrays of results.</returns>
        let partition (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
            if isNull resizeArray then nullExn "Parallel.partition"
            let inputLength = resizeArray.Count
            let isTrue = Array.zeroCreate inputLength
            let mutable trueLength = 0
            Parallel.For(
                0,
                inputLength,
                (fun () -> 0),
                (fun i _ trueCount ->
                    if predicate resizeArray.[i] then
                        isTrue.[i] <- true
                        trueCount + 1
                    else
                        trueCount),
                Action<int>(fun x -> System.Threading.Interlocked.Add(&trueLength, x) |> ignore)
            )
            |> ignore

            let res1 = ResizeArray(trueLength)
            let res2 = ResizeArray(inputLength - trueLength)
            for i = 0 to isTrue.Length - 1 do
                if isTrue.[i] then
                    res1.Add resizeArray.[i]
                else
                    res2.Add resizeArray.[i]
            res1, res2

    #endif


