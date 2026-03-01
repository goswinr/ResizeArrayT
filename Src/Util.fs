namespace ResizeArrayT

open System
open System.Collections.Generic

/// Open this module to get access to the operators +++ and ++
/// to combines the contents of two Sequences or ICollection<'T> into a new ResizeArray
module Operators =

    /// Operator +++ to shallow copy the contents of two Sequences (IEnumerable<'T>) into a new ResizeArray
    /// On ICollection<'T> you can alo use the operator ++ which is more optimized .
    let inline (+++) (a : seq<'T>) ( b : seq<'T>)  : ResizeArray<'T> =
        let r = new ResizeArray<'T>()
        r.AddRange a
        r.AddRange b
        r

    /// Operator ++ to shallow copy the contents of two ICollection<'T> into a new ResizeArray.
    /// The capacity of the new ResizeArray is the sum of the count of the two ICollection<'T>.
    /// This version is more optimized than the operator +++ for sequences. because it can preallocate the needed space correctly.
    let inline (++) (a : ICollection<'T>) ( b : ICollection<'T>) : ResizeArray<'T> =
        let l = new ResizeArray<'T>( a.Count + b.Count)
        l.AddRange a
        l.AddRange b
        l


[<Obsolete("not Obsolete but hidden, needs to be visible for inlining")>]
module UtilResizeArray =

    /// Converts negative indices to positive ones.
    /// Correct results from -length up to length-1
    /// e.g.: -1 is  last item .
    /// (from the release of F# 5 on a negative index can also be done with '^' prefix. E.g. ^0 for the last item)
    let inline negIdx i len =
        let ii = if i < 0 then len + i else i
        if ii < 0 || ii >= len then
            raise <| IndexOutOfRangeException $"UtilResizeArray.negIdx: Bad index {i} for items count {len}."
        ii


    let zeroLen() = raise <| ArgumentException $"ResizeArray.negIdxLooped: failed on zero Length."

    /// Any int will give a valid index for given collection size.
    /// Converts negative indices to positive ones and loops to start after last index is reached.
    /// Returns a valid index for a collection of 'length' items for any integer
    let inline negIdxLooped i length =
        if length <= 0 then zeroLen()
        let t = i % length
        if t >= 0 then t else t + length


    let inline toStringCore ofType (arr:ResizeArray<'T>) = // inline needed for Fable reflection
        if isNull arr then
            "null ResizeArray"
        else
            if arr.Count = 0 then
                $"empty ResizeArray<{ofType}>"
            elif arr.Count = 1 then
                $"ResizeArray<{ofType}> with 1 item"
            else
                $"ResizeArray<{ofType}> with {arr.Count} items"

    let inline toStringInline (arr:ResizeArray<'T>) = // inline needed for Fable reflection
        let t = typeof<'T>.Name //  Fable reflection works only inline
        toStringCore t arr

    // -------------------------------------------------------------
    // for Exceptions ( never inlined)
    // -------------------------------------------------------------


    let inline typeOfName<'T>() =
        #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
            "'T"
        #else
            typeof<'T>.Name
        #endif


    let inline debugTxt (i: int option) =
        match i with
        | None -> " "
        | Some i -> i.ToString()


    let itemInOneLineWithMaxChars charCount (item:'T) =
        let s =  $"{item}".Split('\n') |> Array.map (fun l -> l.Trim()) |> String.concat " "
        if s.Length > charCount then
            s.Substring(0, charCount) + " ..."
        else
            s

    /// Returns a string with the content of the ResizeArray up to 'entriesToPrint' entries.
    /// Includes the index of each entry.
    /// Includes the last entry.
    let contentAsString entriesToPrint (arr:ResizeArray<'T>) = // on .NET inline fails because it's using internal DefaultDictUtil
        let c = arr.Count
        if c > 0 && entriesToPrint > 0 then
            let b = Text.StringBuilder()
            b.AppendLine ":"  |> ignore
            for i,t in arr |> Seq.truncate (max 0 entriesToPrint) |> Seq.indexed do
                b.AppendLine $"  {i}: {itemInOneLineWithMaxChars 200 t}" |> ignore
            if c = entriesToPrint+1 then
                b.AppendLine $"  {c-1}: {itemInOneLineWithMaxChars 200 arr[c-1]}" |> ignore // print one more line if it's the last instead of "..."
            elif c > entriesToPrint + 1  then
                b.AppendLine "  ..." |> ignore
                b.AppendLine $"  {c-1}: {itemInOneLineWithMaxChars 200 arr[c-1]}" |> ignore
            b.ToString()
        else
            ""


    /// Throws an ArgumentNullException with a message that includes the function name.
    let nullExn (funcName:string) =
        raise (ArgumentNullException("ResizeArray." + funcName + ": input is null!"))

    /// Throws an IndexOutOfRangeException for getting a bad index with a message that includes the content of the ResizeArray.
    let badGetExn (i:int) (arr:ResizeArray<'T>) (funcName:string) =
        let t = typeOfName<'T>()
        raise (IndexOutOfRangeException $"ResizeArray.{funcName}: Can't get index {i} from:\n{toStringCore t arr}{contentAsString 5 arr}")

    /// Throws an IndexOutOfRangeException for setting a bad index with a message that includes the content of the ResizeArray.
    let badSetExn (i:int) (arr:ResizeArray<'T>) (funcName:string) (doingSet:'T) =
        let t = typeOfName<'T>()
        raise (IndexOutOfRangeException $"ResizeArray.{funcName}: Can't set index {i} to {doingSet} on:\n{toStringCore t arr}{contentAsString 5 arr}")

    /// Throws an ArgumentException with a message that includes the content of the ResizeArray.
    let fail (arr:ResizeArray<'T>) (funcAndReason:string)  =
        let t = typeOfName<'T>()
        raise (ArgumentException $"ResizeArray.{funcAndReason}:\n{toStringCore t arr}{contentAsString 5 arr}")

    /// Throws an ArgumentException with.
    let failSimpel (funcAndReason:string)  =
        raise (ArgumentException $"ResizeArray.{funcAndReason}")

    /// Throws a KeyNotFoundException with a message that includes the content of the ResizeArray.
    let failKey (arr:ResizeArray<'T>) (funcAndReason:string)  =
        let t = typeOfName<'T>()
        raise (KeyNotFoundException $"ResizeArray.{funcAndReason}:\n{toStringCore t arr}{contentAsString 5 arr}")

    /// Throws an IndexOutOfRangeException with a message that includes the content of the ResizeArray.
    let failIdx (arr:ResizeArray<'T>) (funcAndReason:string)  =
        let t = typeOfName<'T>()
        raise (IndexOutOfRangeException $"ResizeArray.{funcAndReason}:\n{toStringCore t arr}{contentAsString 5 arr}")


    /// A simple simple Wrapper for a ResizeArray.
    /// The sole purpose is to provide a better Exception message when an index is out of range.
    type DebugIndexer<'T>(arr:ResizeArray<'T>) = // [<Struct>] would fails for setter !
        member this.Item
            with get(i) =
                if i < 0 || i >= arr.Count then badGetExn i arr "DebugIdx.[i]"
                arr.[i]

            and set(i) (x:'T) =
                if i < 0 || i >= arr.Count then badSetExn i arr "DebugIdx.[i]" x
                arr.[i] <- x

        member this.Count = arr.Count

        member this.Array = arr

        override this.ToString() =
            let t =
            #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
                "'T"
            #else
                (typeof<'T>).Name
            #endif
            $"DebugIndexer for {toStringCore t arr}{contentAsString 5 arr}"

