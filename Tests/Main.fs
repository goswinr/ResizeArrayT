namespace Tests

open ResizeArrayT
open Tests.Module3 // Add reference to Module3 tests

module Main =

    #if FABLE_COMPILER

    open Fable.Mocha
    Mocha.runTests Tests.Extensions.tests |> ignore
    Mocha.runTests Tests.Module.tests |> ignore
    Mocha.runTests Tests.Module2.tests |> ignore
    Mocha.runTests Tests.Module3.tests |> ignore // Add Module3 tests

    #else

    open Expecto
    [<EntryPoint>]
    let main argv =
        runTestsWithCLIArgs [] [||] Tests.Extensions.tests
        |||
        runTestsWithCLIArgs [] [||] Tests.Module.tests
        |||
        runTestsWithCLIArgs [] [||] Tests.Module2.tests
        |||
        runTestsWithCLIArgs [] [||] Tests.Module3.tests // Add Module3 tests


    #endif