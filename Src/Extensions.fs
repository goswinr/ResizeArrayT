namespace ResizeArrayT

open System
open System.Collections.Generic


#nowarn "44" //for opening the hidden but not Obsolete UtilResizeArray module
open UtilResizeArray

/// Extension methods for ResizeArray<'T>.
/// This module is automatically opened when the namespace ResizeArrayT is opened.
[<AutoOpen>]
module AutoOpenResizeArrayExtensions =


    let internal isEqualTo (this: ResizeArray<'T>) (other: ResizeArray<'T>) =
        if Object.ReferenceEquals(this, other) then // true if both are null
            true // both are the same instance
        elif isNull this || isNull other then
            false // one is null, the other not
        elif this.Count <> other.Count then
            false // different count
        else
            let comparer = EqualityComparer<'T>.Default // for  structural equality to be implemented on this class without putting the <'T when 'T : equality> constraint on 'T?
            let mutable i = 0
            let mutable isEqual = true
            let k = this.Count
            while i < k do
                let r1 = this.[i]
                let r2 = other.[i]
                i <- i + 1
                if not <| comparer.Equals(r1, r2)  then
                    isEqual <- false
                    i <- k // break the loop
            isEqual

    type List<'T> with


        /// Use for Debugging index get/set operations.
        /// Just replace 'myList.[3]' with 'myList.DebugIdx.[3]'
        /// Throws a nice descriptive Exception if the index is out of range
        /// including the bad index and the ResizeArray content.
        member xs.DebugIdx =
            new DebugIndexer<'T>(xs)

        /// Gets an item at index, same as this.[index] or this.Idx(index)
        /// Throws a descriptive Exception if the index is out of range.
        /// (Use this.GetNeg(i) member if you want to use negative indices too)
        member inline xs.Get index =
            if index < 0 || index >= xs.Count then badGetExn index xs "Get"
            xs.[index]

        /// Gets an item at index, same as this.[index] or this.Get(index)
        /// Throws a descriptive Exception if the index is out of range.
        /// (Use this.GetNeg(i) member if you want to use negative indices too)
        member inline xs.Idx index =
            if index < 0 || index >= xs.Count then badGetExn index xs "Idx"
            xs.[index]

        /// Sets an item at index
        /// (Use this.SetNeg(i) member if you want to use negative indices too)
        member inline xs.Set index value =
            if index < 0 || index >= xs.Count then badSetExn index xs "Set" value
            xs.[index] <- value


        /// Gets the index of the last item in the ResizeArray.
        /// Equal to this.Count - 1
        /// Returns -1 for empty ResizeArray.
        member inline xs.LastIndex =
            // don't fail so that a loop for i=0 to xs.LastIndex will work for empty ResizeArray
            //if xs.Count = 0 then IndexOutOfRangeException
            xs.Count - 1

        /// Get (or set) the last item in the ResizeArray.
        /// Equal to this.[this.Count - 1]
        member inline xs.Last
            with get () =
                if xs.Count = 0 then badGetExn xs.LastIndex xs "Last"
                xs.[xs.Count - 1]
            and set (v: 'T) =
                if xs.Count = 0 then badSetExn xs.LastIndex xs "Last" v
                xs.[xs.Count - 1] <- v

        /// Get (or set) the second last item in the ResizeArray.
        /// Equal to this.[this.Count - 2]
        member inline xs.SecondLast
            with get () =
                if xs.Count < 2 then badGetExn (xs.Count - 2) xs "SecondLast"
                xs.[xs.Count - 2]
            and set (v: 'T) =
                if xs.Count < 2 then badSetExn (xs.Count - 2) xs "SecondLast" v
                xs.[xs.Count - 2] <- v


        /// Get (or set) the third last item in the ResizeArray.
        /// Equal to this.[this.Count - 3]
        member inline xs.ThirdLast
            with get () =
                if xs.Count < 3 then badGetExn (xs.Count - 3) xs "ThirdLast"
                xs.[xs.Count - 3]
            and set (v: 'T) =
                if xs.Count < 3 then badSetExn (xs.Count - 3) xs "ThirdLast" v
                xs.[xs.Count - 3] <- v

        /// Get (or set) the first item in the ResizeArray.
        /// Equal to this.[0]
        member inline xs.First
            with get () =
                if xs.Count = 0 then badGetExn 0 xs "First"
                xs.[0]
            and set (v: 'T) =
                if xs.Count = 0 then badSetExn 0 xs "First" v
                xs.[0] <- v

        /// Gets the the only item in the ResizeArray.
        /// Fails if the ResizeArray does not have exactly one element.
        member inline xs.FirstAndOnly : 'T =
            if xs.Count = 0 then badGetExn 0 xs "FirstAndOnly"
            if xs.Count > 1 then badGetExn 1 xs "FirstAndOnly, ResizeArray is expected to have exactly one item."
            xs.[0]


        /// Get (or set) the second item in the ResizeArray.
        /// Equal to this.[1]
        member inline xs.Second
            with get () =
                if xs.Count < 2 then badGetExn 1 xs "Second"
                xs.[1]
            and set (v: 'T) =
                if xs.Count < 2 then badSetExn 1 xs "Second" v
                xs.[1] <- v

        /// Get (or set) the third item in the ResizeArray.
        /// Equal to this.[2]
        member inline xs.Third
            with get () =
                if xs.Count < 3 then badGetExn 2 xs "Third"
                xs.[2]
            and set (v: 'T) =
                if xs.Count < 3 then badSetExn 2 xs "Third" v
                xs.[2] <- v

        /// Checks if this.Count = 0
        member inline xs.IsEmpty =
            xs.Count = 0


        /// Checks if this.Count = 1
        member inline xs.IsSingleton =
            xs.Count = 1

        /// Checks if this.Count > 0
        /// Same as xs.HasItems
        member inline xs.IsNotEmpty =
            xs.Count > 0

        /// Checks if this.Count > 0
        /// Same as xs.IsNotEmpty
        member inline xs.HasItems =
            xs.Count > 0


        /// Gets an item in the ResizeArray by index.
        /// Allows for negative index too ( -1 is last item,  like Python)
        /// (From the release of F# 5 on a negative index can also be done with '^' prefix. E.g. ^0 for the last item)
        member inline xs.GetNeg index =
            let len = xs.Count
            let ii = if index < 0 then len + index else index
            if ii < 0 || ii >= len then badGetExn index xs "GetNeg"
            xs.[ii]

        /// Sets an item in the ResizeArray by index.
        /// Allows for negative index too ( -1 is last item,  like Python)
        /// (from the release of F# 5 on a negative index can also be done with '^' prefix. E.g. ^0 for the last item)
        member inline xs.SetNeg index value =
            let len = xs.Count
            let ii = if index < 0 then len + index else index
            if ii < 0 || ii >= len then badSetExn index xs "SetNeg" value
            xs.[ii] <- value

        /// Any index will return a value.
        /// ResizeArray is treated as an endless loop in positive and negative direction
        member inline xs.GetLooped index =
            let len = xs.Count
            if len = 0 then badGetExn index xs "GetLooped"
            let t = index % len
            let ii = if t >= 0 then t else t + len
            xs.[ii]

        /// Any index will set a value.
        /// ResizeArray is treated as an endless loop in positive and negative direction
        member inline xs.SetLooped index value =
            let len = xs.Count
            if len = 0 then badSetExn index xs "SetLooped" value
            let t = index % len
            let ii = if t >= 0 then t else t + len
            xs.[ii] <- value


        /// Creates a new ResizeArray with the same items as the input ResizeArray.
        /// Shallow copy only.
        member this.Duplicate(): ResizeArray<'T> =
            this.GetRange(0, this.Count) // fastest way to create a shallow copy



        /// Shallow Structural equality comparison.
        /// Compares each element in both lists for equality.
        /// However nested ResizeArrays inside a ResizeArray are only compared for referential equality in .NET.
        /// When used in Fable (JavaScript) the nested ResizeArrays are compared for structural equality
        /// as per the Fable implementation of Javascript Arrays.
        /// (Like the default behavior of Collections.Generic.List)
        /// Does not Raises ArgumentNullException if either or both list are null.
        member this.IsEqualTo(other: ResizeArray<'T>) =
            isEqualTo this other


        /// Insert an item at the beginning of the list = index 0,
        /// (moving all other items up by one index)
        member inline xs.InsertAtStart x =
            xs.Insert(0, x)

        /// Get and remove last item from ResizeArray
        member inline xs.Pop() =
            if xs.Count = 0 then fail xs "Pop() failed on empty."
            let i = xs.Count - 1
            let v = xs.[i]
            xs.RemoveAt(i)
            v

        /// Get and remove item at index from ResizeArray
        member inline xs.Pop(index: int) =
            if index < 0 || index >= xs.Count then badGetExn index xs ".Pop"
            let v = xs.[index]
            xs.RemoveAt(index)
            v

        /// Creates a shallow copy of the list
        /// (for a ResizeArray of structs this is like a deep copy)
        member inline xs.Clone() =
            xs.GetRange(0, xs.Count) // fastest way to create a shallow copy

        /// <summary>Get the index for the element offset elements away from the end of the collection.
        /// This member exists to support F# indexing from back: ^0 is last item, ^1 is second last</summary>
        /// <param name="offset">The offset from the end.</param>
        /// <returns>The corresponding index from the start.</returns>
        member xs.GetReverseIndex(_, offset: int) : int =  // The first parameter, 'rank'  is unused in ResizeArray
            if offset < 0 then
                failIdx xs $"[^{offset}]: index from back is negative."
            if offset >= xs.Count then
                failIdx xs $"[^{offset}]: index from back is equal or bigger than resizeArray.Count"
            xs.Count - offset - 1

        /// <summary>
        /// This member enables F# slicing notation operator. e.g:  xs.[1..3].
        /// The resulting ResizeArray includes the end index.
        /// Just like for F# arrays out of bounds indices are ignored for getting a slice. (But not for setting it.)
        /// The start index is inclusive and the end index is also inclusive.
        /// </summary>
        /// <remarks>
        /// With F# preview features enabled a negative index can also be done with '^' prefix. E.g. ^0 for the last item.
        /// </remarks>
        member xs.GetSlice(startIdx: option<int>, endIdx: option<int>) : ResizeArray<'T> =
            //.GetSlice maps onto slicing operator .[1..3]
            let stIdx =
                match startIdx with
                | None -> 0
                | Some si -> max 0 si // start index must be >= 0

            let enIdx =
                match endIdx with
                | None -> xs.Count - 1
                | Some ei -> min ei (xs.Count - 1)

            // end must be same or bigger than start
            // if enIdx >= 0 && stIdx > enIdx then
            let len = enIdx - stIdx + 1
            if len < 0 then
                new ResizeArray<'T>() // empty ResizeArray
            else
                xs.GetRange(stIdx, len)

        /// <summary>
        /// This member enables F# slicing notation operator e.g.: xs.[1..3] &lt;- ys.
        /// The the end index is included.
        /// Just like for F# arrays out of bounds indices raise an Exception for setting a slice. (But not for getting it.)
        /// If the list of new values is longer than the slice the extra values are ignored.( just like for F# arrays)
        /// </summary>
        /// <remarks>
        /// With F# preview features enabled a negative index can also be done with '^' prefix. E.g. ^0 for the last item.
        /// </remarks>
        member xs.SetSlice(startIdx: option<int>, endIdx: option<int>, newValues: IList<'T>) : unit =
            //.SetSlice maps onto slicing operator .[1..3] <- xs
            let count = xs.Count
            let stIdx =
                match startIdx with
                | None -> 0
                | Some si ->
                    if si < 0 || si >= count then
                        failIdx xs  $"SetSlice: [{debugTxt startIdx}..{debugTxt endIdx}], start index must be between 0 and {count - 1} for ResizeArray of {count} items."
                    si

            let enIdx =
                match endIdx with
                | None -> count - 1
                | Some ei ->
                    if ei < 0 || ei >= count then
                        failIdx xs  $"SetSlice: [{debugTxt startIdx}..{debugTxt endIdx}], end index must be between 0 and {count - 1} for ResizeArray of {count} items."
                    else
                        ei

            // end must be same or bigger than start
            if enIdx >= 0 && stIdx > enIdx then
                failIdx xs $"[{debugTxt startIdx}..{debugTxt endIdx}, The given start index must be smaller than or equal to the end index for ResizeArray of {count} items."

            let countToAdd = enIdx - stIdx + 1
            if newValues.Count < countToAdd then
                failIdx xs $"[{debugTxt startIdx}..{debugTxt endIdx}, SetSlice expected {countToAdd} item in newValues IList but only found {newValues.Count}"

            for i = stIdx to enIdx do
                xs.[i] <- newValues.[i - stIdx]

        /// <summary>
        /// Give start and end index. The resulting ResizeArray includes the value at end index too.
        /// This function will fail on out of bound indices, while the F# slicing notation xs.[1..3] will not.
        /// To use negative indices or out of range indices use the SliceLooped method.
        /// Do not confuse this method with the new xs.Slice(start , length) method, that is built into .NET
        /// </summary>
        /// <param name="startIdx">The start index of the slice.</param>
        /// <param name="endIdx">The end index of the slice.</param>
        /// <remarks>
        /// Alternative: with F# slicing notation (e.g. a.[1..3])
        /// With F# preview features enabled a negative index can also be done with '^' prefix. E.g. ^0 for the last item.
        /// </remarks>
        member xs.SliceIdx(startIdx:int , endIdx: int ) : ResizeArray<'T> =
            let count = xs.Count
            let st  = startIdx //if startIdx< 0 then count + startIdx        else startIdx
            let len = endIdx   //if endIdx  < 0 then count + endIdx - st + 1 else endIdx - st + 1
            if st < 0 || st > count - 1 then
                failIdx xs $"SliceIdx: Start index {startIdx} is out of range. Allowed values are -{count} up to {count-1} for ResizeArray of {count} items"

            if st+len > count then
                failIdx xs $"SliceIdx: End index {endIdx} is out of range. Allowed values are -{count} up to {count-1} for ResizeArray of {count} items"

            if len < 0 then
                // let en = if endIdx<0 then count+endIdx else endIdx
                // let err = sprintf "ResizeArray.Slice: Start index '%A' (= %d) is bigger than end index '%A'(= %d) for ResizeArray of %d items" startIdx st endIdx en  count
                failIdx xs $"SliceIdx: Start index {startIdx} is bigger than end index {endIdx} for ResizeArray of {count} items"

            // ResizeArray.init len (fun i -> this.[st+i])
            xs.GetRange(st, len)

        /// <summary>
        /// Give start and end index. The resulting ResizeArray includes the value at end index too.
        /// Any index is valid, out of range indices set into range using modulo.
        /// Allows for negative indices too. ( -1 is last item, like in Python)
        /// The resulting ResizeArray includes the end index.
        /// If the start index is bigger than the end index an empty ResizeArray is returned.
        /// For empty input ResizeArray an empty ResizeArray is returned.
        /// </summary>
        /// <param name="startIdx">The start index of the slice.</param>
        /// <param name="endIdx">The end index of the slice.</param>
        /// <remarks>
        /// Alternative: with F# slicing notation (e.g. a.[1..3])
        /// With F# preview features enabled a negative index can also be done with '^' prefix. E.g. ^0 for the last item.
        /// </remarks>
        member xs.SliceLooped(startIdx:int , endIdx:int ) : ResizeArray<'T> =
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


        /// Raises an Exception if the ResizeArray is empty
        /// (Useful for chaining)
        /// Returns the input ResizeArray
        member inline xs.FailIfEmpty (errorMessage: string) : ResizeArray<'T> =
            if xs.Count = 0 then failSimpel $".FailIfEmpty: {errorMessage}"
            xs

        /// Raises an Exception if the ResizeArray has less then count items.
        /// (Useful for chaining)
        /// Returns the input ResizeArray
        member inline xs.FailIfLessThan(count, errorMessage: string)  : ResizeArray<'T> =
            if xs.Count < count then failSimpel $"FailIfLessThan {count}: {errorMessage}"
            xs


        /// A string representation of the ResizeArray including the count of entries and the first 5 entries.
        /// When used in Fable this member is inlined for reflection to work.
        #if FABLE_COMPILER
        member inline arr.AsString : string =  // inline needed for Fable reflection
        #else
        member arr.AsString  :string =  // on .NET inline fails because it's using internal DefaultDictUtil
        #endif
            let t = toStringInline arr
            $"{t}{contentAsString 5 arr}"


        /// A string representation of the ResizeArray including the count of entries
        /// and the specified amount of entries.
        /// When used in Fable this member is inlined for reflection to work.
        #if FABLE_COMPILER
        member inline arr.ToString (entriesToPrint:int)  : string =  // inline needed for Fable reflection
        #else
        member arr.ToString (entriesToPrint:int)  : string  = // on .NET inline fails because it's using internal DefaultDictUtil
        #endif
            let t = toStringInline arr
            $"{t}{contentAsString entriesToPrint arr}"


        // override xs.ToString() =  // override is not allowed a extension member
        //     let t = typeOfName<'T>()
        //     $"{toStringCore t xs}{contentAsString 2 xs}"

        /// A property like the ToString() method,
        /// But with richer formatting
        /// Listing includes the first 6 items
        [<Obsolete("Use arr.AsString instead")>]
        member xs.ToNiceString =
            //xs.AsString //fails in Fable because not inlined
            let t = typeOfName<'T>()
            $"{toStringCore t xs}{contentAsString 6 xs}"

        /// A property like the ToString() method,
        /// But with richer formatting
        /// Listing includes the first 50 items
        [<Obsolete("Use arr.ToString(countOfItemsToPrint) instead")>]
        member xs.ToNiceStringLong =
            //xs.ToString(50) fails in Fable because not inlined
            let t = typeOfName<'T>()
            $"{toStringCore t xs}{contentAsString 50 xs}"
