namespace global

open FsToolkit.ErrorHandling

/// Provides functions for working with result
[<AutoOpen>]
module Result =
    let retn x = result { return x }
