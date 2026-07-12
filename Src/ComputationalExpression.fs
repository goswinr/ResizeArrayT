namespace ResizeArrayT

open System



/// This module is automatically opened when the namespace ResizeArrayT is opened.
/// It provides a computational expression builder for ResizeArray<'T>.
/// <c>resizeArray { ... }</c>
/// This builder allows you to create a ResizeArray just like you would create an IEnumerable with seq expressions <c>seq { ... }</c>.
[<AutoOpen>]
module AutoOpenComputationalExpression  =

    //[<InlineIfLambda>] needs F# 6.0

    //TODO: optimize with
    // [<InlineIfLambda>] as in https://gist.github.com/Tarmil/afcf5f50e45e90200eb7b01615b0ffc0
    // or https://github.com/fsharp/fslang-design/blob/main/FSharp-6.0/FS-1099-list-collector.md
    // or https://github.com/fsbolero/Bolero/blob/master/src/Bolero.Server/Html.fs

    // https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions
    // https://fssnip.net/8aq/title/Computation-Expression-Stub

    //[<NoComparison; NoEquality>]
    /// Builds ResizeArray values using F# computation-expression syntax.
    type ComputationalExpressionBuilderResizeArray<'T> () =

        /// <summary>Adds one value to the ResizeArray produced by the computation expression.</summary>
        /// <param name="x">The value to add.</param>
        member inline _.Yield (x: 'T) =
            fun (r: ResizeArray<'T>) ->
                r.Add(x)

        /// <summary>Adds all values from a sequence to the ResizeArray produced by the computation expression.</summary>
        /// <param name="xs">The sequence of values to add.</param>
        member inline _.YieldFrom (xs: #seq<'T>) =
            fun (r: ResizeArray<'T>) ->
                r.AddRange(xs)

        /// <summary>Combines two consecutive computation-expression bodies.</summary>
        /// <param name="f">The first body.</param>
        /// <param name="g">The second body.</param>
        member inline _.Combine ([<InlineIfLambda>] f, [<InlineIfLambda>] g) =
            fun (r: ResizeArray<'T>) ->
                f r;
                g r

        /// <summary>Delays creation of a computation-expression body until it is executed.</summary>
        /// <param name="f">The function that creates the delayed body.</param>
        member inline _.Delay ([<InlineIfLambda>] f) =
            fun (r: ResizeArray<'T>) -> (f()) r

        /// Called for empty else branches of if...then expressions in computation expressions.
        member inline _.Zero () =
            ignore

        /// <summary>Iterates a sequence and executes the body for each value.
        /// This always allocates a sequence and an enumerator, even for <c>i = 0 to x</c>.
        /// Use a while loop to avoid that allocation.</summary>
        /// <param name="xs">The sequence to iterate.</param>
        /// <param name="body">The body to execute for each value.</param>
        member inline _.For (xs: seq<'U>, [<InlineIfLambda>] body: 'U -> ResizeArray<'T> -> unit) =
            fun (r: ResizeArray<'T>) ->
                use e = xs.GetEnumerator()
                while e.MoveNext() do
                    body e.Current r

        // This DOES not work unfortunately
        // member inline _.For (fromIndex:int, toIndex:int, [<InlineIfLambda>] body: int -> ResizeArray<'T> -> unit) =
        //     fun (r: ResizeArray<'T>) ->
        //         for i=fromIndex to toIndex do
        //             body i r

        /// <summary>Repeatedly executes the body while the predicate returns true.</summary>
        /// <param name="predicate">The function that controls iteration.</param>
        /// <param name="body">The body to execute.</param>
        member inline _.While ([<InlineIfLambda>] predicate: unit -> bool, [<InlineIfLambda>] body: ResizeArray<'T> -> unit) =
            fun (r: ResizeArray<'T>) ->
                while predicate () do
                    body r

        /// <summary>Executes the computation-expression body and returns the resulting ResizeArray.</summary>
        /// <param name="body">The body to execute.</param>
        member inline _.Run ([<InlineIfLambda>] body: ResizeArray<'T> -> unit) =
            let r = ResizeArray<'T>()
            do body r
            r

        /// <summary>Executes the body and invokes the handler if the body raises an exception.</summary>
        /// <param name="body">The body to execute.</param>
        /// <param name="handler">The exception handler.</param>
        member inline  _.TryWith([<InlineIfLambda>] body: ResizeArray<'T> -> unit, [<InlineIfLambda>] handler: exn ->  ResizeArray<'T> -> unit) =
            fun (r: ResizeArray<'T>) ->
                try body r
                with e -> handler e r

        /// <summary>Executes the compensation after the body completes or raises an exception.</summary>
        /// <param name="body">The body to execute.</param>
        /// <param name="compensation">The compensation to execute afterwards.</param>
        member inline  _.TryFinally([<InlineIfLambda>] body: ResizeArray<'T> -> unit, [<InlineIfLambda>] compensation:  ResizeArray<'T> -> unit) =
            fun (r: ResizeArray<'T>) ->
                try body r
                finally compensation  r

        /// <summary>Executes the body and disposes the supplied resource afterwards.</summary>
        /// <param name="disposable">The resource to dispose.</param>
        /// <param name="body">The body to execute with the resource.</param>
        member inline this.Using(disposable: #IDisposable, [<InlineIfLambda>] body: #IDisposable -> ResizeArray<'T> -> unit) =
            this.TryFinally( body disposable ,  fun (_: ResizeArray<'T>)  ->
                if not <| Object.ReferenceEquals(disposable,null) then // might be disposed already
                    disposable.Dispose()
            )


    /// A computational expression builder for ResizeArray<'T>.
    /// <c>resizeArray { ... }</c>
    /// It allows you to create a ResizeArray just like you would create an IEnumerable with seq expressions <c>seq { ... }</c>.
    let resizeArray<'T> = new ComputationalExpressionBuilderResizeArray<'T> ()
