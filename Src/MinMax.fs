namespace ResizeArrayT

open System

// Internal, only for finding min/max values.
// cmp is (<) for the smallest and (>) for the biggest values.
//
// NaN handling:
// IEEE 754:2019 defines 'minimum' and 'maximum', which propagate NaN,
// and 'minimumNumber' and 'maximumNumber', which skip NaN.
// Both are legitimate, but different operations. See https://github.com/dotnet/fsharp/issues/13207#issuecomment-1194411950
// ResizeArray.min and max propagate NaN, ResizeArray.minNumber and maxNumber skip it.
// All other functions, like minBy, minIndexBy or min2, rank NaN after all other values.
// Every comparison with NaN is false, so a loop that only replaces its current best value when cmp is true
// never picks a NaN, unless it starts with one. That's why the functions below only need extra NaN checks
// while their current best value is NaN.
// For float and float32 the functions in UtilResizeArray are used instead of propagateNaN and skipNaN,
// to also treat -0.0 as smaller than +0.0.
[<RequireQualifiedAccess>]
module internal MinMax =

    /// True for NaN, or for a key that contains NaN, like the tuple (nan, 1).
    /// NaN is the only value that is not smaller than or equal to itself.
    /// This uses comparison and not equality, so it does not add an equality constraint.
    let inline isNaN (x: 'T) : bool = not (x <= x)

    // The index counterpart of a stable sorting network for three values.
    // Since cmp is strict (< or >), equal keys always stay in their original index order.
    let inline indexOfSort3By f cmp aa bb cc =
        let a = f aa
        let b = f bb
        let c = f cc
        if cmp b a then
            if cmp c b then 2, 1, 0
            elif cmp c a then 1, 2, 0
            else 1, 0, 2
        else
            if cmp c a then 2, 0, 1
            elif cmp c b then 0, 2, 1
            else 0, 1, 2

    /// The index of the best key from index first to last, inclusive.
    /// NaN keys are skipped, if all keys are NaN, first is returned.
    let inline indexByFunIn cmp func (first: int) (last: int) (resizeArray: ResizeArray<'T>) : int =
        let mutable ii = first
        let mutable mf = func resizeArray.[first]
        let mutable i = first + 1
        // skip leading NaN keys, but keep index first if all keys are NaN
        while i <= last && isNaN mf do
            let f = func resizeArray.[i]
            if not (isNaN f) then
                ii <- i
                mf <- f
            i <- i + 1
        // from here on NaN keys are skipped without any check, because cmp is false for NaN
        while i <= last do
            let f = func resizeArray.[i]
            if cmp f mf then
                ii <- i
                mf <- f
            i <- i + 1
        ii

    /// The index of the best key. NaN keys are skipped, if all keys are NaN, 0 is returned.
    let inline indexByFun cmp func (resizeArray: ResizeArray<'T>) : int =
        indexByFunIn cmp func 0 (resizeArray.Count - 1) resizeArray

    #if !FABLE_COMPILER
    /// indexByFun for the Parallel module: each chunk of the ResizeArray is searched in parallel.
    /// Then the best indices of the chunks are compared in their original order,
    /// so the result is the same as from indexByFun. This calls func once more for each chunk.
    let inline parallelIndexByFun cmp func (resizeArray: ResizeArray<'T>) : int =
        let count = resizeArray.Count
        let p = Environment.ProcessorCount
        let chunkSize = count / p + (if count % p = 0 then 0 else 1)
        let chunkCount = count / chunkSize + (if count % chunkSize = 0 then 0 else 1)
        let chunkBests = Array.zeroCreate chunkCount
        Threading.Tasks.Parallel.For(0, chunkCount, fun c ->
            let first = c * chunkSize
            let last = Operators.min count (first + chunkSize) - 1
            chunkBests.[c] <- indexByFunIn cmp func first last resizeArray
        ) |> ignore
        let chunkBests = ResizeArray chunkBests
        chunkBests.[indexByFun cmp (fun i -> func resizeArray.[i]) chunkBests]
    #endif

    /// The indices of the best two keys. NaN keys are ranked last.
    let inline index2ByFun cmp func (resizeArray: ResizeArray<'T>) : int * int =
        let mutable i1 = 0
        let mutable i2 = 1
        let mutable mf1 = func resizeArray.[i1]
        let mutable mf2 = func resizeArray.[i2]
        // While a best key is NaN, any key that is not NaN is better.
        let mutable nan1 = isNaN mf1
        let mutable nan2 = isNaN mf2
        for i = 1 to resizeArray.Count - 1 do // starts at 1 to put the first two in order
            let f = func resizeArray.[i]
            if cmp f mf1 || (nan1 && not (isNaN f)) then
                i2 <- i1
                mf2 <- mf1
                nan2 <- nan1
                i1 <- i
                mf1 <- f
                nan1 <- false
            elif cmp f mf2 || (nan2 && not (isNaN f)) then
                i2 <- i
                mf2 <- f
                nan2 <- false
        i1, i2

    /// The indices of the best three keys. NaN keys are ranked last.
    let inline index3ByFun (cmp: 'U -> 'U -> bool) (func: 'T -> 'U) (resizeArray: ResizeArray<'T>) : int * int * int =
        // like cmp, but any key that is not NaN is also better than a NaN key
        let inline better a b = cmp a b || (isNaN b && not (isNaN a))
        // sort first 3
        let mutable i1, i2, i3 = indexOfSort3By func better resizeArray.[0] resizeArray.[1] resizeArray.[2] // otherwise would fail on sorting first 3, test on ResizeArray([5;6;3;1;2;0])|> ResizeArray.max3
        let mutable e1 = func resizeArray.[i1]
        let mutable e2 = func resizeArray.[i2]
        let mutable e3 = func resizeArray.[i3]
        // While a best key is NaN, any key that is not NaN is better.
        let mutable nan1 = isNaN e1
        let mutable nan2 = isNaN e2
        let mutable nan3 = isNaN e3
        for i = 3 to resizeArray.Count - 1 do
            let f = func resizeArray.[i]
            if cmp f e1 || (nan1 && not (isNaN f)) then
                i3 <- i2
                e3 <- e2
                nan3 <- nan2
                i2 <- i1
                e2 <- e1
                nan2 <- nan1
                i1 <- i
                e1 <- f
                nan1 <- false
            elif cmp f e2 || (nan2 && not (isNaN f)) then
                i3 <- i2
                e3 <- e2
                nan3 <- nan2
                i2 <- i
                e2 <- f
                nan2 <- false
            elif cmp f e3 || (nan3 && not (isNaN f)) then
                i3 <- i
                e3 <- f
                nan3 <- false
        i1, i2, i3

    /// The best two values. NaN is ranked last.
    let inline simple2 cmp (resizeArray: ResizeArray<'T>) : 'T * 'T =
        let i1, i2 = index2ByFun cmp id resizeArray
        resizeArray.[i1], resizeArray.[i2]

    /// The best three values. NaN is ranked last.
    let inline simple3 cmp (resizeArray: ResizeArray<'T>) : 'T * 'T * 'T =
        let i1, i2, i3 = index3ByFun cmp id resizeArray
        resizeArray.[i1], resizeArray.[i2], resizeArray.[i3]

    /// The IEEE 754:2019 'minimum' or 'maximum' for any 'T: NaN propagates.
    /// keep is (>=) for the minimum and (<=) for the maximum.
    /// Unlike cmp, 'not keep' is also true for NaN, so a NaN is taken, and then the loop stops.
    let inline propagateNaN keep (resizeArray: ResizeArray<'T>) : 'T =
        let mutable acc = resizeArray.[0]
        let mutable foundNaN = isNaN acc
        let mutable i = 1
        while not foundNaN && i < resizeArray.Count do
            let x = resizeArray.[i]
            if not (keep x acc) then // x is better or NaN
                acc <- x
                foundNaN <- isNaN x
            i <- i + 1
        acc

    /// propagateNaN for two values, as a reduction for the Parallel module.
    let inline propagateNaN2 keep (a: 'T) (b: 'T) : 'T =
        if not (keep b a) && not (isNaN a) then b else a

    /// The IEEE 754:2019 'minimumNumber' or 'maximumNumber' for any 'T: NaN is skipped.
    let inline skipNaN cmp (resizeArray: ResizeArray<'T>) : 'T =
        resizeArray.[indexByFun cmp id resizeArray]
