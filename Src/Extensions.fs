namespace ResizeArrayT

open System
open System.Collections.Generic


#nowarn "44" //for opening the hidden but not Obsolete UtilResizeArray module
#nowarn "10001"
open UtilResizeArray
#warnon "10001"
#warnon "44" //

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
        /// Throws a descriptive exception if the index is out of range,
        /// including the bad index and the ResizeArray content.
        member xs.DebugIdx =
            new DebugIndexer<'T>(xs)

        /// <summary>Gets the element at the specified index. Same as this.[index] or this.Idx(index).
        /// Use this.GetNeg(index) if you want to use negative indices too.</summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is negative or the ResizeArray does not contain enough elements.</exception>
        member inline xs.Get index =
            if index < 0 || index >= xs.Count then badGetExn index xs "Get"
            xs.[index]

        /// Gets an item at index, same as this.[index] or this.Get(index)
        /// Throws a descriptive exception if the index is out of range.
        /// (Use this.GetNeg(i) member if you want to use negative indices too)
        member inline xs.Idx index =
            if index < 0 || index >= xs.Count then badGetExn index xs "Idx"
            xs.[index]

        /// <summary>Sets the element at the specified index.
        /// Use this.SetNeg(index, value) if you want to use negative indices too.</summary>
        /// <param name="index">The zero-based index of the element to set.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is negative or the ResizeArray does not contain enough elements.</exception>
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

        /// Gets or sets the last element of the ResizeArray. Same as this.[this.Count - 1].
        member inline xs.Last
            with get () =
                if xs.Count = 0 then badGetExn xs.LastIndex xs "Last"
                xs.[xs.Count - 1]
            and set (v: 'T) =
                if xs.Count = 0 then badSetExn xs.LastIndex xs "Last" v
                xs.[xs.Count - 1] <- v

        /// Gets or sets the second-last element of the ResizeArray. Same as this.[this.Count - 2].
        member inline xs.SecondLast
            with get () =
                if xs.Count < 2 then badGetExn (xs.Count - 2) xs "SecondLast"
                xs.[xs.Count - 2]
            and set (v: 'T) =
                if xs.Count < 2 then badSetExn (xs.Count - 2) xs "SecondLast" v
                xs.[xs.Count - 2] <- v


        /// Gets or sets the third-last element of the ResizeArray. Same as this.[this.Count - 3].
        member inline xs.ThirdLast
            with get () =
                if xs.Count < 3 then badGetExn (xs.Count - 3) xs "ThirdLast"
                xs.[xs.Count - 3]
            and set (v: 'T) =
                if xs.Count < 3 then badSetExn (xs.Count - 3) xs "ThirdLast" v
                xs.[xs.Count - 3] <- v

        /// Gets or sets the first element of the ResizeArray. Same as this.[0].
        member inline xs.First
            with get () =
                if xs.Count = 0 then badGetExn 0 xs "First"
                xs.[0]
            and set (v: 'T) =
                if xs.Count = 0 then badSetExn 0 xs "First" v
                xs.[0] <- v

        /// Gets the only element of the ResizeArray.
        /// Fails if the ResizeArray does not have exactly one element.
        member inline xs.FirstAndOnly : 'T =
            if xs.Count = 0 then badGetExn 0 xs "FirstAndOnly"
            if xs.Count > 1 then badGetExn 1 xs "FirstAndOnly, ResizeArray is expected to have exactly one item."
            xs.[0]


        /// Gets or sets the second element of the ResizeArray. Same as this.[1].
        member inline xs.Second
            with get () =
                if xs.Count < 2 then badGetExn 1 xs "Second"
                xs.[1]
            and set (v: 'T) =
                if xs.Count < 2 then badSetExn 1 xs "Second" v
                xs.[1] <- v

        /// Gets or sets the third element of the ResizeArray. Same as this.[2].
        member inline xs.Third
            with get () =
                if xs.Count < 3 then badGetExn 2 xs "Third"
                xs.[2]
            and set (v: 'T) =
                if xs.Count < 3 then badSetExn 2 xs "Third" v
                xs.[2] <- v

        /// Returns true if the ResizeArray is empty; otherwise, false.
        member inline xs.IsEmpty =
            xs.Count = 0


        /// Returns true if the ResizeArray has exactly one element; otherwise, false.
        member inline xs.IsSingleton =
            xs.Count = 1

        /// Returns true if the ResizeArray has one or more elements; otherwise, false.
        /// Same as xs.HasItems.
        member inline xs.IsNotEmpty =
            xs.Count > 0

        /// Returns true if the ResizeArray has one or more elements; otherwise, false.
        /// Same as xs.IsNotEmpty. Unlike ResizeArray.hasItems, this property does not test for an exact count.
        member inline xs.HasItems =
            xs.Count > 0


        /// <summary>Gets the element at the specified index. A negative index counts backward from the end; -1 is the last element.</summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is outside the ResizeArray or the ResizeArray is empty.</exception>
        member inline xs.GetNeg index =
            let len = xs.Count
            let ii = if index < 0 then len + index else index
            if ii < 0 || ii >= len then badGetExn index xs "GetNeg"
            xs.[ii]

        /// <summary>Sets the element at the specified index. A negative index counts backward from the end; -1 is the last element.</summary>
        /// <param name="index">The index of the element to set.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is outside the ResizeArray or the ResizeArray is empty.</exception>
        member inline xs.SetNeg index value =
            let len = xs.Count
            let ii = if index < 0 then len + index else index
            if ii < 0 || ii >= len then badSetExn index xs "SetNeg" value
            xs.[ii] <- value

        /// <summary>Gets the element at the specified index, treating the ResizeArray as circular in both directions.</summary>
        /// <param name="index">The index to normalize into the bounds of the ResizeArray.</param>
        /// <returns>The element at the normalized index.</returns>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the ResizeArray is empty.</exception>
        member inline xs.GetLooped index =
            let len = xs.Count
            if len = 0 then badGetExn index xs "GetLooped"
            let t = index % len
            let ii = if t >= 0 then t else t + len
            xs.[ii]

        /// <summary>Sets the element at the specified index, treating the ResizeArray as circular in both directions.</summary>
        /// <param name="index">The index to normalize into the bounds of the ResizeArray.</param>
        /// <param name="value">The new value.</param>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the ResizeArray is empty.</exception>
        member inline xs.SetLooped index value =
            let len = xs.Count
            if len = 0 then badSetExn index xs "SetLooped" value
            let t = index % len
            let ii = if t >= 0 then t else t + len
            xs.[ii] <- value


        /// Creates a new ResizeArray with the same items as the input ResizeArray.
        /// This is a shallow element copy. Same as xs.Clone().
        member this.Duplicate(): ResizeArray<'T> =
            this.GetRange(0, this.Count) // fastest way to create a shallow copy



        /// Shallow Structural equality comparison.
        /// Compares each element in both lists for equality.
        /// However nested ResizeArrays inside a ResizeArray are only compared for referential equality in .NET.
        /// When used in Fable (JavaScript) the nested ResizeArrays are compared for structural equality
        /// as per the Fable implementation of Javascript Arrays.
        /// (Like the default behavior of Collections.Generic.List)
        /// Does not raise ArgumentNullException if either or both lists are null.
        member this.IsEqualTo(other: ResizeArray<'T>) =
            isEqualTo this other


        /// Insert an item at the beginning of the list = index 0,
        /// (moving all other items up by one index)
        member inline xs.InsertAtStart x =
            xs.Insert(0, x)

        /// <summary>Removes and returns the last element of the ResizeArray.</summary>
        /// <remarks>In Fable, this emits <c>.pop()</c>. In .NET, it removes the element at Count - 1.</remarks>
        /// <returns>The removed element.</returns>
        /// <exception cref="T:System.ArgumentException">Thrown when the ResizeArray is empty.</exception>
        member inline xs.Pop() : 'T =
                if xs.Count = 0 then fail xs "Pop() failed on empty."
            #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
                Fable.Core.JsInterop.emitJsExpr xs "$0.pop()"
            #else
                let lastIndex = xs.Count - 1
                let value = xs.[lastIndex]
                xs.RemoveAt(lastIndex)
                value
            #endif

        /// <summary>Removes and returns the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <returns>The removed element.</returns>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is outside the ResizeArray.</exception>
        member inline xs.Pop(index: int) =
            if index < 0 || index >= xs.Count then badGetExn index xs ".Pop"
            let v = xs.[index]
            xs.RemoveAt(index)
            v


        /// <summary>Removes the last element of the ResizeArray without returning it.</summary>
        /// <remarks>In Fable, this emits <c>.pop()</c>. In .NET, it removes the element at Count - 1.</remarks>
        /// <exception cref="T:System.ArgumentException">Thrown when the ResizeArray is empty.</exception>
        member inline xs.PopOff() : unit =
                if xs.Count = 0 then fail xs "PopOff() failed on empty."
            #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
                Fable.Core.JsInterop.emitJsStatement xs "$0.pop()"
            #else
                let lastIndex = xs.Count - 1
                xs.RemoveAt(lastIndex)
            #endif

        /// Creates a new ResizeArray that contains a shallow copy of the elements. Same as xs.Duplicate().
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
        /// This member enables the F# slicing notation operator, for example xs.[1..3].
        /// The resulting ResizeArray includes the end index.
        /// Just like for F# arrays, out-of-bounds indices are ignored when getting a slice, but not when setting one.
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
        /// The end index is included.
        /// Just like for F# arrays, out-of-bounds indices raise an exception when setting a slice, but not when getting one.
        /// If the list of new values is longer than the slice, the extra values are ignored (just like for F# arrays).
        /// </summary>
        /// <remarks>
        /// With F# preview features enabled a negative index can also be done with '^' prefix. E.g. ^0 for the last item.
        /// </remarks>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when either bound is outside the ResizeArray, the start index is greater than the end index, or newValues contains too few elements.</exception>
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
        /// Returns a new ResizeArray containing the elements between the specified inclusive start and end indices.
        /// This member rejects out-of-bounds indices, while the F# slicing notation xs.[1..3] does not.
        /// To normalize negative or out-of-range indices, use SliceLooped.
        /// Do not confuse this method with the new xs.Slice(start , length) method, that is built into .NET
        /// </summary>
        /// <param name="startIdx">The inclusive start index of the slice.</param>
        /// <param name="endIdx">The inclusive end index of the slice.</param>
        /// <returns>A new ResizeArray containing the requested range.</returns>
        /// <exception cref="T:System.IndexOutOfRangeException">Thrown when either index is outside the ResizeArray or startIdx is greater than endIdx.</exception>
        /// <remarks>
        /// Alternative: with F# slicing notation (e.g. a.[1..3])
        /// With F# preview features enabled a negative index can also be done with '^' prefix. E.g. ^0 for the last item.
        /// </remarks>
        member xs.SliceIdx(startIdx:int , endIdx: int ) : ResizeArray<'T> =
            let count = xs.Count
            if startIdx < 0 || startIdx >= count then
                failIdx xs $"SliceIdx: Start index {startIdx} is out of range. Allowed values are 0 through {count - 1} for a ResizeArray of {count} items."
            if endIdx < 0 || endIdx >= count then
                failIdx xs $"SliceIdx: End index {endIdx} is out of range. Allowed values are 0 through {count - 1} for a ResizeArray of {count} items."
            if startIdx > endIdx then
                failIdx xs $"SliceIdx: Start index {startIdx} is bigger than end index {endIdx} for ResizeArray of {count} items"
            xs.GetRange(startIdx, endIdx - startIdx + 1)

        /// <summary>
        /// Returns a new ResizeArray containing the elements between the specified start and end indices after normalizing both indices with modulo.
        /// Both indices are inclusive, and negative and out-of-range indices are allowed.
        /// If the normalized start index is greater than the normalized end index, an empty ResizeArray is returned.
        /// For an empty input ResizeArray, an empty ResizeArray is returned.
        /// </summary>
        /// <param name="startIdx">The inclusive start index to normalize.</param>
        /// <param name="endIdx">The inclusive end index to normalize.</param>
        /// <returns>A new ResizeArray containing the requested range.</returns>
        /// <remarks>
        /// Alternative: with F# slicing notation (e.g. a.[1..3])
        /// With F# preview features enabled a negative index can also be done with '^' prefix. E.g. ^0 for the last item.
        /// </remarks>
        member xs.SliceLooped(startIdx:int , endIdx:int ) : ResizeArray<'T> =
            if xs.Count = 0 then
                ResizeArray<'T>()
            else
                let count = xs.Count
                let st = negIdxLooped startIdx count
                let en = negIdxLooped endIdx count
                let len = en - st + 1
                if len < 0 then
                    ResizeArray<'T>()
                else
                    xs.GetRange(st, len)


        /// Returns the input ResizeArray for chaining, or raises an exception if it is empty.
        member inline xs.FailIfEmpty (errorMessage: string) : ResizeArray<'T> =
            if xs.Count = 0 then failSimple $"FailIfEmpty: {errorMessage}"
            xs

        /// Returns the input ResizeArray for chaining, or raises an exception if it has fewer than count elements.
        member inline xs.FailIfLessThan(count, errorMessage: string)  : ResizeArray<'T> =
            if xs.Count < count then failSimple $"FailIfLessThan {count}: {errorMessage}"
            xs


        /// A string representation of the ResizeArray including the count of entries and the first 5 entries.
        /// When used in Fable this member is inlined for reflection to work.
        #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
        member inline arr.AsString : string =  // inline needed for Fable reflection
        #else
        member arr.AsString  :string =  // on .NET inline fails because it's using internal DefaultDictUtil
        #endif
            let t = toStringInline arr
            $"{t}{contentAsString 5 arr}"


        /// A string representation of the ResizeArray including the count of entries
        /// and the specified amount of entries.
        /// When used in Fable this member is inlined for reflection to work.
        #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
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
