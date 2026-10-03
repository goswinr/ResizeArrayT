namespace ResizeArrayT

open System
open System.Collections.Generic

/// Invalid argument to a ResizeArrayT operation.
type ResizeArrayTArgumentException(message: string) =
    #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
    inherit Exception(message)
    #else
    inherit ArgumentException(message)
    #endif

/// A required ResizeArray or projected value was null.
type ResizeArrayTArgumentNullException(paramName: string, message: string) =
    #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
    inherit Exception(message)
    member _.ParamName = paramName
    #else
    inherit ArgumentNullException(paramName, message)
    #endif

/// No matching item was found.
type ResizeArrayTKeyNotFoundException(message: string) =
    #if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
    inherit Exception(message)
    #else
    inherit KeyNotFoundException(message)
    #endif
