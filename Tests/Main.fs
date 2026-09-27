namespace Tests

open type Scriptorium.Quill.Runner

module Main =

    // Scriptorium.Quill runs the same suite on .NET and on JS (Fable).
    // On JS runTests calls process.exit with the exit code once all tests are done,
    // so it must be called only once, with all test lists.
    [<EntryPoint>]
    let main _argv =
        runTests [
            Tests.Extensions.tests
            Tests.Module.tests
            Tests.Module2.tests
            Tests.Module3.tests
            Tests.FableCompat.tests
            #if !FABLE_COMPILER
            Tests.ParallelTests.tests
            #endif
        ]
