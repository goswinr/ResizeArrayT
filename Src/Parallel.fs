namespace ResizeArrayT

// Fable doesn't support System.Threading.Tasks.Parallel.For
#if !FABLE_COMPILER

open System
open System.Collections.Generic

#nowarn "44" //for opening the hidden but not Obsolete UtilResizeArray module
#nowarn "10001"
open UtilResizeArray
#warnon "10001"
#warnon "44"

/// This module only holds the ResizeArray.Parallel module.
/// It is AutoOpen, so that ResizeArray.Parallel is available after 'open ResizeArrayT',
/// next to the functions of the main ResizeArray module in Module.fs.
/// (F# can't split one module over several files.)
[<AutoOpen>]
module ResizeArrayParallel =

    module ResizeArray =

        // The Parallel module used to be nested in the main ResizeArray module.
        // This open keeps its functions available without the ResizeArray prefix, as before.
        open ResizeArrayT.ResizeArray

        /// Parallel operations on ResizeArray using Threading.Tasks.Parallel.For
        /// The API is aligned with from FSharp.Core.Array.Parallel module
        module Parallel =

            open System.Threading
            open System.Threading.Tasks
            open System.Collections.Concurrent

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
            /// <param name="mapping">The function to transform each input element into a ResizeArray.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The combined ResizeArray of mapped elements.</returns>
            let collect (mapping: 'T -> ResizeArray<'U>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U> =
                if isNull resizeArray then nullExn "Parallel.collect"
                let inputLength = resizeArray.Count
                let result = create inputLength Unchecked.defaultof<_>
                Parallel.For(0, inputLength, (fun i -> result.[i] <- mapping resizeArray.[i]))|> ignore
                concat result


            /// <summary>Create a ResizeArray given the dimension and a generator function to compute the elements.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// The order in which the given function is applied to indices is not specified.</summary>
            /// <param name="count">The number of elements to create.</param>
            /// <param name="initializer">The function used to initialize each element from its index.</param>
            /// <returns>The ResizeArray of results.</returns>
            let init count (initializer: int -> 'T) : ResizeArray<'T> =
                let result = create count Unchecked.defaultof<_>
                Parallel.For(0, count, (fun i -> result.[i] <- initializer i)) |> ignore
                result


            /// <summary>Apply the given function to each element of the ResizeArray.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
            /// <param name="action">The function to apply to each element.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            let iter (action: 'T -> unit) (resizeArray: ResizeArray<'T>) =
                if isNull resizeArray then nullExn "Parallel.iter"
                Parallel.For(0, resizeArray.Count, (fun i -> action resizeArray.[i])) |> ignore


            /// <summary>Apply the given function to each element of the ResizeArray. The integer passed to the
            /// function indicates the index of element.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
            /// <param name="action">The function to apply to each index and element.</param>
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
            /// <param name="mapping">The function to transform each element.</param>
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
            /// <param name="mapping">The function to transform each index and element.</param>
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


            // Returns the count of elements for which the predicate is true, and a mask of them.
            let private countAndCollectTrueItems (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : int * bool[] =
                let inputLength = resizeArray.Count
                let isTrue: bool[] = Array.zeroCreate inputLength
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
                    Action<int>(fun x -> Interlocked.Add(&trueLength, x) |> ignore)
                )
                |> ignore
                trueLength, isTrue


            /// <summary>Returns a new ResizeArray containing only the elements of the ResizeArray
            /// for which the given predicate returns <c>true</c>. The order of the elements is preserved.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// The order in which the given function is applied to elements of the input ResizeArray is not specified.</summary>
            /// <param name="predicate">The function to test the input elements.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>A ResizeArray containing the elements for which the given predicate returns true.</returns>
            let filter (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
                if isNull resizeArray then nullExn "Parallel.filter"
                let trueLength, isTrue = countAndCollectTrueItems predicate resizeArray
                let res = ResizeArray(trueLength)
                for i = 0 to isTrue.Length - 1 do
                    if isTrue.[i] then
                        res.Add resizeArray.[i]
                res


            /// <summary>Split the collection into two collections, containing the
            /// elements for which the given predicate returns <c>true</c> and <c>false</c>
            /// respectively.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// The order in which the given function is applied to indices is not specified.</summary>
            /// <param name="predicate">The function to test the input elements.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The two ResizeArrays of results.</returns>
            let partition (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) =
                if isNull resizeArray then nullExn "Parallel.partition"
                let trueLength, isTrue = countAndCollectTrueItems predicate resizeArray
                let res1 = ResizeArray(trueLength)
                let res2 = ResizeArray(resizeArray.Count - trueLength)
                for i = 0 to isTrue.Length - 1 do
                    if isTrue.[i] then
                        res1.Add resizeArray.[i]
                    else
                        res2.Add resizeArray.[i]
                res1, res2


            /// <summary>Splits the collection into two ResizeArrays, by applying the given partitioning function
            /// to each element. Returns Choice1Of2 elements in the first ResizeArray and
            /// Choice2Of2 elements in the second ResizeArray. Element order is preserved in both of the created ResizeArrays.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// The order in which the given function is applied to elements of the input ResizeArray is not specified.
            /// The partitioner function must be thread-safe.</summary>
            /// <param name="partitioner">The function to transform and classify each input element into one of two output types.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>A tuple of two ResizeArrays. The first containing values from Choice1Of2 results and the second
            /// containing values from Choice2Of2 results.</returns>
            let partitionWith (partitioner: 'T -> Choice<'U1, 'U2>) (resizeArray: ResizeArray<'T>) : ResizeArray<'U1> * ResizeArray<'U2> =
                if isNull resizeArray then nullExn "Parallel.partitionWith"
                let len = resizeArray.Count
                let isChoice1: bool[] = Array.zeroCreate len
                let results1: 'U1[] = Array.zeroCreate len
                let results2: 'U2[] = Array.zeroCreate len
                let mutable count1 = 0
                Parallel.For(
                    0,
                    len,
                    (fun () -> 0),
                    (fun i _ count ->
                        match partitioner resizeArray.[i] with
                        | Choice1Of2 x ->
                            isChoice1.[i] <- true
                            results1.[i] <- x
                            count + 1
                        | Choice2Of2 x ->
                            results2.[i] <- x
                            count),
                    Action<int>(fun x -> Interlocked.Add(&count1, x) |> ignore)
                )
                |> ignore
                let res1 = ResizeArray(count1)
                let res2 = ResizeArray(len - count1)
                for i = 0 to len - 1 do
                    if isChoice1.[i] then
                        res1.Add results1.[i]
                    else
                        res2.Add results2.[i]
                res1, res2


            /// <summary>Tests if any element of the ResizeArray satisfies the given predicate.
            /// The predicate is applied to the elements of the input ResizeArray in parallel. If any application
            /// returns true then the overall result is true and testing of other elements in all threads is stopped at system's earliest convenience.
            /// Otherwise, <c>false</c> is returned.</summary>
            /// <param name="predicate">The function to test the input elements.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>True if any result from <c>predicate</c> is true.</returns>
            let exists (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : bool =
                if isNull resizeArray then nullExn "Parallel.exists"
                let pResult =
                    Parallel.For(
                        0,
                        resizeArray.Count,
                        (fun i (pState: ParallelLoopState) ->
                            if predicate resizeArray.[i] then
                                pState.Stop())
                    )
                not pResult.IsCompleted


            /// <summary>Tests if all elements of the ResizeArray satisfy the given predicate.
            /// The predicate is applied to the elements of the input ResizeArray in parallel. If any application
            /// returns false then the overall result is false and testing of other elements in all threads is stopped at system's earliest convenience.
            /// Otherwise, true is returned.</summary>
            /// <param name="predicate">The function to test the input elements.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>True if all of the ResizeArray elements satisfy the predicate.</returns>
            let forall (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : bool =
                if isNull resizeArray then nullExn "Parallel.forall"
                // forall predicate <==> not (exists (not predicate))
                not (exists (fun x -> not (predicate x)) resizeArray)


            // Returns the lowest index for which the predicate is true, or no value.
            let private tryFindIndexAux (funcName: string) (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : Nullable<int64> =
                if isNull resizeArray then nullExn funcName
                let pResult =
                    Parallel.For(
                        0,
                        resizeArray.Count,
                        (fun i (pState: ParallelLoopState) ->
                            if predicate resizeArray.[i] then
                                pState.Break())
                    )
                pResult.LowestBreakIteration


            /// <summary>Returns the index of the first element in the ResizeArray
            /// that satisfies the given predicate.
            /// Returns <c>None</c> if no such element exists.
            /// The predicate is applied to the elements of the input ResizeArray in parallel.
            /// Once an element satisfies the predicate, no further elements after it are tested, at system's earliest convenience.</summary>
            /// <param name="predicate">The function to test the input elements.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The index of the first element that satisfies the predicate, or None.</returns>
            let tryFindIndex (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : int option =
                let i = tryFindIndexAux "Parallel.tryFindIndex" predicate resizeArray
                if i.HasValue then
                    Some(int (i.GetValueOrDefault()))
                else
                    None


            /// <summary>Returns the first element for which the given function returns <c>true</c>.
            /// Returns <c>None</c> if no such element exists.
            /// The predicate is applied to the elements of the input ResizeArray in parallel.
            /// Once an element satisfies the predicate, no further elements after it are tested, at system's earliest convenience.</summary>
            /// <param name="predicate">The function to test the input elements.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The first element that satisfies the predicate, or None.</returns>
            let tryFind (predicate: 'T -> bool) (resizeArray: ResizeArray<'T>) : 'T option =
                let i = tryFindIndexAux "Parallel.tryFind" predicate resizeArray
                if i.HasValue then
                    Some resizeArray.[int (i.GetValueOrDefault())]
                else
                    None


            /// <summary>Applies the given function to successive elements, returning the first
            /// result where the function returns <c>Some(x)</c> for some <c>x</c>. If the function
            /// never returns <c>Some(x)</c> then <c>None</c> is returned.
            /// The chooser is applied to the elements of the input ResizeArray in parallel.
            /// Once it returns <c>Some(x)</c>, no further elements after it are tested, at system's earliest convenience.</summary>
            /// <param name="chooser">The function to transform the ResizeArray elements into options.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The first transformed element that is <c>Some(x)</c>.</returns>
            let tryPick (chooser: 'T -> 'U option) (resizeArray: ResizeArray<'T>) : 'U option =
                if isNull resizeArray then nullExn "Parallel.tryPick"
                let allChosen = ConcurrentDictionary<int, 'U>()
                let pResult =
                    Parallel.For(
                        0,
                        resizeArray.Count,
                        (fun i (pState: ParallelLoopState) ->
                            match chooser resizeArray.[i] with
                            | None -> ()
                            | Some chosen ->
                                allChosen.[i] <- chosen
                                pState.Break())
                    )
                let lowest = pResult.LowestBreakIteration
                if lowest.HasValue then
                    Some allChosen.[int (lowest.GetValueOrDefault())]
                else
                    None


            // The following two parameters were benchmarked in FSharp.Core and found to be optimal.
            // Benchmark was run using: 11th Gen Intel Core i9-11950H 2.60GHz, 1 CPU, 16 logical and 8 physical cores
            let private maxPartitions = Environment.ProcessorCount // The maximum number of partitions to use
            let private minChunkSize = 256 // The minimum size of a chunk to be processed in parallel

            // Splits the indices from 0 to maxIdxExclusive-1 into at most maxPartitions chunks of struct(offset, count).
            // Returns just one chunk if maxIdxExclusive is smaller than minSize.
            let private chunksUpToWithMinChunkSize (maxIdxExclusive: int) (minSize: int) : struct (int * int)[] =
                let chunkSize =
                    if maxIdxExclusive < minSize then maxIdxExclusive
                    elif maxIdxExclusive % maxPartitions = 0 then maxIdxExclusive / maxPartitions
                    else (maxIdxExclusive / maxPartitions) + 1
                let chunks = ResizeArray()
                let mutable offset = 0
                while offset + chunkSize < maxIdxExclusive do
                    chunks.Add(struct (offset, chunkSize))
                    offset <- offset + chunkSize
                chunks.Add(struct (offset, maxIdxExclusive - offset))
                chunks.ToArray()

            let private chunksUpTo (maxIdxExclusive: int) : struct (int * int)[] =
                chunksUpToWithMinChunkSize maxIdxExclusive minChunkSize


            /// <summary>Applies a projection function to each element of the ResizeArray in parallel, reducing elements in each thread with a dedicated 'reduction' function.
            /// After processing the entire input, results from all threads are reduced together.
            /// Raises ArgumentException if the ResizeArray is empty.
            /// The ResizeArray is split into contiguous chunks. The order in which the chunks are processed is not specified.
            /// But the elements within each chunk, and then the results of the chunks, are reduced in their original order.
            /// So the 'reduction' function needs to be associative, but it does not need to be commutative.
            /// Compared to the non-parallel ResizeArray.reduce, the 'reduction' function is invoked a few more times to combine the results of the chunks.</summary>
            /// <param name="projection">The function to project from elements of the input ResizeArray.</param>
            /// <param name="reduction">The function to reduce a pair of projected elements to a single element.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
            /// <returns>The final result of the reductions.</returns>
            let reduceBy (projection: 'T -> 'U) (reduction: 'U -> 'U -> 'U) (resizeArray: ResizeArray<'T>) : 'U =
                if isNull resizeArray then nullExn "Parallel.reduceBy"
                if resizeArray.Count = 0 then
                    fail resizeArray "Parallel.reduceBy: Count must be at least one"
                let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt(reduction)
                let chunks = chunksUpToWithMinChunkSize resizeArray.Count 2 // split even small inputs, the projection might be expensive
                let chunkResults: 'U[] = Array.zeroCreate chunks.Length
                Parallel.For(
                    0,
                    chunks.Length,
                    fun chunkIdx ->
                        let struct (offset, len) = chunks.[chunkIdx]
                        let mutable res = projection resizeArray.[offset]
                        for i = offset + 1 to offset + len - 1 do
                            res <- f.Invoke(res, projection resizeArray.[i])
                        chunkResults.[chunkIdx] <- res
                )
                |> ignore
                let mutable finalResult = chunkResults.[0]
                for i = 1 to chunkResults.Length - 1 do
                    finalResult <- f.Invoke(finalResult, chunkResults.[i])
                finalResult


            /// <summary>Applies a function to each element of the ResizeArray in parallel, threading an accumulator argument
            /// through the computation for each thread involved in the computation. After processing the entire input, results from all threads are reduced together.
            /// Raises ArgumentException if the ResizeArray is empty.
            /// The ResizeArray is split into contiguous chunks. The order in which the chunks are processed is not specified.
            /// But the elements within each chunk, and then the results of the chunks, are reduced in their original order.
            /// So the 'reduction' function needs to be associative, but it does not need to be commutative.
            /// Compared to the non-parallel ResizeArray.reduce, the 'reduction' function is invoked a few more times to combine the results of the chunks.</summary>
            /// <param name="reduction">The function to reduce a pair of elements to a single element.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
            /// <returns>The final result of the reductions.</returns>
            let reduce (reduction: 'T -> 'T -> 'T) (resizeArray: ResizeArray<'T>) : 'T =
                if isNull resizeArray then nullExn "Parallel.reduce"
                if resizeArray.Count = 0 then
                    fail resizeArray "Parallel.reduce: Count must be at least one"
                reduceBy id reduction resizeArray


            /// <summary>Returns the greatest of all elements of the ResizeArray.
            /// NaN propagates: if any element is NaN, NaN is returned, like ResizeArray.max.
            /// If several elements are the greatest, the first one of them is returned.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.</summary>
            /// <remarks>This is the 'maximum' operation of IEEE 754:2019: NaN propagates and +0.0 is bigger than -0.0.
            /// See https://github.com/dotnet/fsharp/issues/13207#issuecomment-1194411950</remarks>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
            /// <returns>The maximum element, or NaN.</returns>
            let inline max (resizeArray: ResizeArray<'T>) : 'T =
                if isNull resizeArray then nullExn "Parallel.max"
                if resizeArray.Count = 0 then
                    fail resizeArray "Parallel.max: Count must be at least one"
                if typeof<'T> = typeof<float> then retype (reduceBy id maximumOf (retype resizeArray: ResizeArray<float>))
                elif typeof<'T> = typeof<float32> then retype (reduceBy id maximumOf (retype resizeArray: ResizeArray<float32>))
                else reduceBy id (MinMax.propagateNaN2 (<=)) resizeArray


            /// <summary>Returns the element of the ResizeArray with the greatest projected key.
            /// NaN keys are ignored like in ResizeArray.maxBy: elements with a NaN key are only returned if all keys are NaN.
            /// If several keys are the greatest, the first element of them is returned.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.</summary>
            /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
            /// <returns>The maximum element.</returns>
            let inline maxBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : 'T =
                if isNull resizeArray then nullExn "Parallel.maxBy"
                if resizeArray.Count = 0 then
                    fail resizeArray "Parallel.maxBy: Count must be at least one"
                resizeArray.[MinMax.parallelIndexByFun (>) projection resizeArray]


            /// <summary>Returns the smallest of all elements of the ResizeArray.
            /// NaN propagates: if any element is NaN, NaN is returned, like ResizeArray.min.
            /// If several elements are the smallest, the first one of them is returned.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.</summary>
            /// <remarks>This is the 'minimum' operation of IEEE 754:2019: NaN propagates and -0.0 is smaller than +0.0.
            /// See https://github.com/dotnet/fsharp/issues/13207#issuecomment-1194411950</remarks>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
            /// <returns>The minimum element, or NaN.</returns>
            let inline min (resizeArray: ResizeArray<'T>) : 'T =
                if isNull resizeArray then nullExn "Parallel.min"
                if resizeArray.Count = 0 then
                    fail resizeArray "Parallel.min: Count must be at least one"
                if typeof<'T> = typeof<float> then retype (reduceBy id minimumOf (retype resizeArray: ResizeArray<float>))
                elif typeof<'T> = typeof<float32> then retype (reduceBy id minimumOf (retype resizeArray: ResizeArray<float32>))
                else reduceBy id (MinMax.propagateNaN2 (>=)) resizeArray


            /// <summary>Returns the element of the ResizeArray with the smallest projected key.
            /// NaN keys are ignored like in ResizeArray.minBy: elements with a NaN key are only returned if all keys are NaN.
            /// If several keys are the smallest, the first element of them is returned.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.</summary>
            /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
            /// <returns>The minimum element.</returns>
            let inline minBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : 'T =
                if isNull resizeArray then nullExn "Parallel.minBy"
                if resizeArray.Count = 0 then
                    fail resizeArray "Parallel.minBy: Count must be at least one"
                resizeArray.[MinMax.parallelIndexByFun (<) projection resizeArray]


            /// <summary>Returns the sum of the elements in the ResizeArray. Returns zero for an empty ResizeArray.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// For floating point numbers the result may differ slightly from ResizeArray.sum, because the elements are added in a different order.</summary>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The resulting sum.</returns>
            let inline sum (resizeArray: ^T ResizeArray) : ^T =
                if isNull resizeArray then nullExn "Parallel.sum"
                if resizeArray.Count = 0 then
                    LanguagePrimitives.GenericZero< ^T>
                else
                    reduceBy id (fun a b -> Checked.(+) a b) resizeArray


            /// <summary>Returns the sum of the results generated by applying the function to each element of the ResizeArray.
            /// Returns zero for an empty ResizeArray.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// For floating point numbers the result may differ slightly from ResizeArray.sumBy, because the elements are added in a different order.</summary>
            /// <param name="projection">The function to transform the ResizeArray elements into the type to be summed.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The resulting sum.</returns>
            let inline sumBy (projection: 'T -> ^Key) (resizeArray: ResizeArray<'T>) : ^Key =
                if isNull resizeArray then nullExn "Parallel.sumBy"
                if resizeArray.Count = 0 then
                    LanguagePrimitives.GenericZero< ^Key>
                else
                    reduceBy projection (fun a b -> Checked.(+) a b) resizeArray


            /// <summary>Returns the average of the elements in the ResizeArray.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// For floating point numbers the result may differ slightly from ResizeArray.average, because the elements are added in a different order.</summary>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
            /// <returns>The average of the elements in the ResizeArray.</returns>
            let inline average (resizeArray: ^T ResizeArray) : ^T =
                if isNull resizeArray then nullExn "Parallel.average"
                if resizeArray.Count = 0 then
                    fail resizeArray "Parallel.average: Count must be at least one"
                let total = reduceBy id (fun a b -> Checked.(+) a b) resizeArray
                LanguagePrimitives.DivideByInt< ^T> total resizeArray.Count


            /// <summary>Returns the average of the elements generated by applying the function to each element of the ResizeArray.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// For floating point numbers the result may differ slightly from ResizeArray.averageBy, because the elements are added in a different order.</summary>
            /// <param name="projection">The function to transform the ResizeArray elements before averaging.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArray is empty.</exception>
            /// <returns>The computed average.</returns>
            let inline averageBy (projection: 'T -> ^Key) (resizeArray: ResizeArray<'T>) : ^Key =
                if isNull resizeArray then nullExn "Parallel.averageBy"
                if resizeArray.Count = 0 then
                    fail resizeArray "Parallel.averageBy: Count must be at least one"
                let total = reduceBy projection (fun a b -> Checked.(+) a b) resizeArray
                LanguagePrimitives.DivideByInt< ^Key> total resizeArray.Count


            /// <summary>Combines the two ResizeArrays into a ResizeArray of pairs. The two ResizeArrays must have equal lengths, otherwise an <c>ArgumentException</c> is raised.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.</summary>
            /// <param name="resizeArray1">The first input ResizeArray.</param>
            /// <param name="resizeArray2">The second input ResizeArray.</param>
            /// <exception cref="T:System.ArgumentException">Thrown when the input ResizeArrays differ in length.</exception>
            /// <returns>The ResizeArray of tupled elements.</returns>
            let zip (resizeArray1: ResizeArray<'T>) (resizeArray2: ResizeArray<'U>) : ResizeArray<'T * 'U> =
                if isNull resizeArray1 then nullExn "Parallel.zip first"
                if isNull resizeArray2 then nullExn "Parallel.zip second"
                let len1 = resizeArray1.Count
                if len1 <> resizeArray2.Count then
                    fail resizeArray1 $"Parallel.zip: count of resizeArray1 {len1} does not match resizeArray2 {resizeArray2.Count}."
                let res: ('T * 'U)[] = Array.zeroCreate len1
                let chunks = chunksUpTo len1
                Parallel.For(
                    0,
                    chunks.Length,
                    fun chunkIdx ->
                        let struct (offset, len) = chunks.[chunkIdx]
                        for i = offset to offset + len - 1 do
                            res.[i] <- (resizeArray1.[i], resizeArray2.[i])
                )
                |> ignore
                ResizeArray(res)


            let inline private groupByImplParallel
                (comparer: IEqualityComparer<'SafeKey>)
                ([<InlineIfLambda>] keyf: 'T -> 'SafeKey)
                ([<InlineIfLambda>] getKey: 'SafeKey -> 'Key)
                (resizeArray: ResizeArray<'T>)
                : ResizeArray<'Key * ResizeArray<'T>> =
                let len = resizeArray.Count
                let counts = ConcurrentDictionary<'SafeKey, int ref>(maxPartitions, Operators.min len 1_000, comparer)
                let valueFactory = Func<'SafeKey, int ref>(fun _ -> ref 0)
                // Remember the counter of the group of each element, so that the second pass does not need to look up the key again.
                // Unlike Array.Parallel.groupBy in FSharp.Core, this does not throw for keys that are not equal to themselves, like (nan, 1).
                let counters: int ref[] = Array.zeroCreate len
                let chunks = chunksUpTo len
                Parallel.For(
                    0,
                    chunks.Length,
                    fun chunkIdx ->
                        let struct (offset, chunkLen) = chunks.[chunkIdx]
                        for i = offset to offset + chunkLen - 1 do
                            let counter = counts.GetOrAdd(keyf resizeArray.[i], valueFactory)
                            counters.[i] <- counter
                            Interlocked.Increment(&counter.contents) |> ignore
                )
                |> ignore

                let result = ResizeArray<'Key * ResizeArray<'T>>(counts.Count)
                let groupOfCounter = Dictionary<int ref, ResizeArray<'T>>(counts.Count, HashIdentity.Reference)
                for kvp in counts do
                    let group = zeroCreate kvp.Value.Value
                    result.Add(getKey kvp.Key, group)
                    groupOfCounter.[kvp.Value] <- group

                Parallel.For(
                    0,
                    chunks.Length,
                    fun chunkIdx ->
                        let struct (offset, chunkLen) = chunks.[chunkIdx]
                        for i = offset to offset + chunkLen - 1 do
                            let counter = counters.[i]
                            let group = groupOfCounter.[counter]
                            group.[Interlocked.Decrement(&counter.contents)] <- resizeArray.[i]
                )
                |> ignore
                result


            /// <summary>Applies a key-generating function to each element of a ResizeArray in parallel and yields a ResizeArray of
            /// unique keys. Each unique key contains a ResizeArray of all elements that match to this key.
            /// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For" />.
            /// The order in which the given function is applied to elements of the input ResizeArray is not specified.
            /// Unlike with ResizeArray.groupBy, the order of the keys and of the elements within each group is not specified either.
            /// Like in Array.Parallel.groupBy, all float and float32 nan keys are put in one group.</summary>
            /// <param name="projection">A function that transforms an element of the ResizeArray into a comparable key. Null or Option.None is allowed as key.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The result ResizeArray.</returns>
            let groupBy (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'Key * ResizeArray<'T>> =
                if isNull resizeArray then nullExn "Parallel.groupBy"
                if typeof<'Key>.IsValueType then
                    if typeof<'Key> = typeof<float> || typeof<'Key> = typeof<float32> then
                        // Use nan = nan equality, like Array.Parallel.groupBy
                        let erComparer = HashIdentity.FromFunctions<'Key> LanguagePrimitives.GenericHash LanguagePrimitives.GenericEqualityER
                        groupByImplParallel erComparer projection id resizeArray
                    else
                        groupByImplParallel HashIdentity.Structural<'Key> projection id resizeArray
                else
                    // Wrap a StructBox around all keys in case the key type is itself a type using null as a representation
                    groupByImplParallel StructBox<'Key>.Comparer (fun t -> StructBox(projection t)) (fun sb -> sb.Value) resizeArray


            // ---------------------------------------------------------------------
            // Parallel sorting, ported from FSharp.Core array.fs.
            // The elements of the ResizeArray are sorted in a temporary array.
            // ---------------------------------------------------------------------

            // Sets the pivot to be the median of {first, mid, last} and returns its index.
            let inline private pickPivot
                ([<InlineIfLambda>] cmpAtIndex: int -> int -> int)
                ([<InlineIfLambda>] swapAtIndex: int -> int -> unit)
                (orig: ArraySegment<'T>)
                : int =
                let inline swapIfGreater (i: int) (j: int) =
                    if cmpAtIndex i j > 0 then
                        swapAtIndex i j
                let firstIdx = orig.Offset
                let lastIdx = orig.Offset + orig.Count - 1
                let midIdx = orig.Offset + orig.Count / 2
                swapIfGreater firstIdx midIdx
                swapIfGreater firstIdx lastIdx
                swapIfGreater midIdx lastIdx
                midIdx

            // Splits the segment into a left part with elements smaller or equal to the pivot
            // and a right part with elements bigger or equal to the pivot.
            let inline private partitionIntoTwo
                ([<InlineIfLambda>] cmpWithPivot: int -> int)
                ([<InlineIfLambda>] swapAtIndex: int -> int -> unit)
                (orig: ArraySegment<'T>)
                : ArraySegment<'T> * ArraySegment<'T> =
                let mutable leftIdx = orig.Offset + 1 // Leftmost is already < pivot
                let mutable rightIdx = orig.Offset + orig.Count - 2 // Rightmost is already > pivot
                while leftIdx < rightIdx do
                    while cmpWithPivot leftIdx < 0 do
                        leftIdx <- leftIdx + 1
                    while cmpWithPivot rightIdx > 0 do
                        rightIdx <- rightIdx - 1
                    if leftIdx < rightIdx then
                        swapAtIndex leftIdx rightIdx
                        leftIdx <- leftIdx + 1
                        rightIdx <- rightIdx - 1
                let lastIdx = orig.Offset + orig.Count - 1
                // There might be more elements being (=)pivot. Exclude them from further work
                while cmpWithPivot leftIdx >= 0 && leftIdx > orig.Offset do
                    leftIdx <- leftIdx - 1
                while cmpWithPivot rightIdx <= 0 && rightIdx < lastIdx do
                    rightIdx <- rightIdx + 1
                ArraySegment<'T>(orig.Array, orig.Offset, leftIdx - orig.Offset + 1),
                ArraySegment<'T>(orig.Array, rightIdx, lastIdx - rightIdx + 1)

            let private partitionIntoTwoUsingComparer (cmp: 'T -> 'T -> int) (orig: ArraySegment<'T>) : ArraySegment<'T> * ArraySegment<'T> =
                let arr = orig.Array
                let inline swapAt i j =
                    let tmp = arr.[i]
                    arr.[i] <- arr.[j]
                    arr.[j] <- tmp
                let pivotIdx = pickPivot (fun i j -> cmp arr.[i] arr.[j]) swapAt orig
                let pivotItem = arr.[pivotIdx]
                partitionIntoTwo (fun idx -> cmp arr.[idx] pivotItem) swapAt orig

            let private partitionIntoTwoUsingKeys (keyComparer: IComparer<'Key>) (keys: 'Key[]) (orig: ArraySegment<'T>) : ArraySegment<'T> * ArraySegment<'T> =
                let arr = orig.Array
                let inline swapAt i j =
                    let tmpKey = keys.[i]
                    keys.[i] <- keys.[j]
                    keys.[j] <- tmpKey
                    let tmp = arr.[i]
                    arr.[i] <- arr.[j]
                    arr.[j] <- tmp
                let pivotIdx = pickPivot (fun i j -> keyComparer.Compare(keys.[i], keys.[j])) swapAt orig
                let pivotKey = keys.[pivotIdx]
                partitionIntoTwo (fun idx -> keyComparer.Compare(keys.[idx], pivotKey)) swapAt orig

            let inline private sortInPlaceHelper
                (arr: 'T[])
                ([<InlineIfLambda>] partitioningFunc: ArraySegment<'T> -> ArraySegment<'T> * ArraySegment<'T>)
                ([<InlineIfLambda>] sortingFunc: ArraySegment<'T> -> unit)
                : unit =
                let rec sortChunk (segment: ArraySegment<'T>) freeWorkers =
                    match freeWorkers with
                    // Really small arrays are not worth creating a Task for, sort them immediately as well
                    | 0
                    | 1 -> sortingFunc segment
                    | _ when segment.Count <= minChunkSize -> sortingFunc segment
                    | _ ->
                        let left, right = partitioningFunc segment
                        // If either of the two is too small, sort small segments straight away.
                        // If the other happens to be big, leave it with all workers in its recursive step
                        if left.Count <= minChunkSize || right.Count <= minChunkSize then
                            sortChunk left freeWorkers
                            sortChunk right freeWorkers
                        else
                            // Pivot-based partitions might be unbalanced. Split free workers for left/right proportional to their size
                            let itemsPerWorker = Operators.max ((left.Count + right.Count) / freeWorkers) 1
                            let workersForLeftTask =
                                (left.Count / itemsPerWorker)
                                |> Operators.max 1
                                |> Operators.min (freeWorkers - 1)
                            let leftTask = Task.Run(fun () -> sortChunk left workersForLeftTask)
                            sortChunk right (freeWorkers - workersForLeftTask)
                            leftTask.Wait()
                sortChunk (ArraySegment<'T>(arr, 0, arr.Length)) maxPartitions

            let private sortInPlaceWithHelper (partitioningComparer: 'T -> 'T -> int) (sortingComparer: IComparer<'T>) (arr: 'T[]) : unit =
                let partitioningFunc = partitionIntoTwoUsingComparer partitioningComparer
                let sortingFunc = fun (s: ArraySegment<'T>) -> System.Array.Sort<'T>(arr, s.Offset, s.Count, sortingComparer)
                sortInPlaceHelper arr partitioningFunc sortingFunc

            // Sorts the array in place using F# generic comparison, the same as Operators.compare.
            let private sortInPlaceGeneric (arr: 'T[]) : unit =
                let comparer = LanguagePrimitives.FastGenericComparer<'T>
                sortInPlaceWithHelper (fun a b -> comparer.Compare(a, b)) comparer arr

            // Sorts the keys in place using F# generic comparison, and moves the values along with them.
            let private sortKeysAndValuesInPlace (keys: 'Key[]) (values: 'T[]) : unit =
                let keyComparer = LanguagePrimitives.FastGenericComparer<'Key>
                let partitioningFunc = partitionIntoTwoUsingKeys keyComparer keys
                let sortingFunc = fun (s: ArraySegment<'T>) -> System.Array.Sort<'Key, 'T>(keys, values, s.Offset, s.Count, keyComparer)
                sortInPlaceHelper values partitioningFunc sortingFunc

            // Applies the projection to all elements in parallel.
            let private projectKeys (projection: 'T -> 'Key) (arr: 'T[]) : 'Key[] =
                let keys: 'Key[] = Array.zeroCreate arr.Length
                let chunks = chunksUpTo arr.Length
                Parallel.For(
                    0,
                    chunks.Length,
                    fun chunkIdx ->
                        let struct (offset, len) = chunks.[chunkIdx]
                        for i = offset to offset + len - 1 do
                            keys.[i] <- projection arr.[i]
                )
                |> ignore
                keys

            let private reverseInPlace (arr: 'T[]) : unit =
                let chunks = chunksUpTo (arr.Length / 2)
                let lastIdx = arr.Length - 1
                Parallel.For(
                    0,
                    chunks.Length,
                    fun chunkIdx ->
                        let struct (offset, len) = chunks.[chunkIdx]
                        for i = offset to offset + len - 1 do
                            let tmp = arr.[i]
                            arr.[i] <- arr.[lastIdx - i]
                            arr.[lastIdx - i] <- tmp
                )
                |> ignore

            // Writes the sorted elements back into the ResizeArray of the same length.
            let private copyBack (sorted: 'T[]) (resizeArray: ResizeArray<'T>) : unit =
                for i = 0 to sorted.Length - 1 do
                    resizeArray.[i] <- sorted.[i]


            /// <summary>Sorts the elements of a ResizeArray in parallel, returning a new ResizeArray. Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
            /// This means "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
            /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
            /// For a stable sort, consider using Seq.sort.</summary>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>A new sorted ResizeArray.</returns>
            let sort<'T when 'T: comparison> (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
                if isNull resizeArray then nullExn "Parallel.sort"
                let arr = resizeArray.ToArray()
                sortInPlaceGeneric arr
                ResizeArray(arr)


            /// <summary>Sorts the elements of a ResizeArray in parallel, using the given projection for the keys and returning a new ResizeArray.
            /// Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
            /// This means "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
            /// The projection is applied to each element exactly once, in parallel.
            /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
            /// For a stable sort, consider using Seq.sort.</summary>
            /// <param name="projection">The function to transform ResizeArray elements into the type that is compared.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The sorted ResizeArray.</returns>
            let sortBy<'T, 'Key when 'Key: comparison> (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
                if isNull resizeArray then nullExn "Parallel.sortBy"
                let arr = resizeArray.ToArray()
                sortKeysAndValuesInPlace (projectKeys projection arr) arr
                ResizeArray(arr)


            /// <summary>Sorts the elements of a ResizeArray in parallel, in descending order, using the given projection for the keys and returning a new ResizeArray.
            /// Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
            /// This means in ascending sorting "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
            /// The projection is applied to each element exactly once, in parallel.
            /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
            /// For a stable sort, consider using Seq.sort.</summary>
            /// <param name="projection">The function to transform ResizeArray elements into the type that is compared.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The sorted ResizeArray.</returns>
            let sortByDescending<'T, 'Key when 'Key: comparison> (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
                if isNull resizeArray then nullExn "Parallel.sortByDescending"
                let arr = resizeArray.ToArray()
                sortKeysAndValuesInPlace (projectKeys projection arr) arr
                reverseInPlace arr
                ResizeArray(arr)


            /// <summary>Sorts the elements of a ResizeArray in parallel, in descending order, returning a new ResizeArray. Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
            /// This means in ascending sorting "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
            /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
            /// For a stable sort, consider using Seq.sort.</summary>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The sorted ResizeArray.</returns>
            let sortDescending<'T when 'T: comparison> (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
                if isNull resizeArray then nullExn "Parallel.sortDescending"
                let arr = resizeArray.ToArray()
                sortInPlaceGeneric arr
                reverseInPlace arr
                ResizeArray(arr)


            /// <summary>Sorts the elements of a ResizeArray in place in parallel, using generic comparison.
            /// Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
            /// This means in ascending sorting "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
            /// The elements are sorted in a temporary array and then copied back into the ResizeArray.
            /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
            /// For a stable sort, consider using Seq.sort.</summary>
            /// <param name="resizeArray">The input ResizeArray.</param>
            let sortInPlace<'T when 'T: comparison> (resizeArray: ResizeArray<'T>) : unit =
                if isNull resizeArray then nullExn "Parallel.sortInPlace"
                let arr = resizeArray.ToArray()
                sortInPlaceGeneric arr
                copyBack arr resizeArray


            /// <summary>Sorts the elements of a ResizeArray in place in parallel, using the given projection for the keys.
            /// Elements are compared using <see cref="M:Microsoft.FSharp.Core.Operators.compare"/>.
            /// This means in ascending sorting "Z" is before "a". This is different from Collections.Generic.Sort() where "a" is before "Z" using IComparable interface.
            /// The projection is applied to each element exactly once, in parallel.
            /// The elements are sorted in a temporary array and then copied back into the ResizeArray.
            /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
            /// For a stable sort, consider using Seq.sort.</summary>
            /// <param name="projection">The function to transform ResizeArray elements into the type that is compared.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            let sortInPlaceBy<'T, 'Key when 'Key: comparison> (projection: 'T -> 'Key) (resizeArray: ResizeArray<'T>) : unit =
                if isNull resizeArray then nullExn "Parallel.sortInPlaceBy"
                let arr = resizeArray.ToArray()
                sortKeysAndValuesInPlace (projectKeys projection arr) arr
                copyBack arr resizeArray


            /// <summary>Sorts the elements of a ResizeArray in place in parallel, using the given comparison function as the order.
            /// The elements are sorted in a temporary array and then copied back into the ResizeArray.
            /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
            /// For a stable sort, consider using Seq.sort.</summary>
            /// <param name="comparer">The function to compare pairs of ResizeArray elements.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            let sortInPlaceWith (comparer: 'T -> 'T -> int) (resizeArray: ResizeArray<'T>) : unit =
                if isNull resizeArray then nullExn "Parallel.sortInPlaceWith"
                let arr = resizeArray.ToArray()
                sortInPlaceWithHelper comparer (ComparisonIdentity.FromFunction comparer) arr
                copyBack arr resizeArray


            /// <summary>Sorts the elements of a ResizeArray in parallel, using the given comparison function as the order, returning a new ResizeArray.
            /// This is NOT a stable sort, i.e. the original order of equal elements is not necessarily preserved.
            /// For a stable sort, consider using Seq.sort.</summary>
            /// <param name="comparer">The function to compare pairs of ResizeArray elements.</param>
            /// <param name="resizeArray">The input ResizeArray.</param>
            /// <returns>The sorted ResizeArray.</returns>
            let sortWith (comparer: 'T -> 'T -> int) (resizeArray: ResizeArray<'T>) : ResizeArray<'T> =
                if isNull resizeArray then nullExn "Parallel.sortWith"
                let arr = resizeArray.ToArray()
                sortInPlaceWithHelper comparer (ComparisonIdentity.FromFunction comparer) arr
                ResizeArray(arr)

#endif
