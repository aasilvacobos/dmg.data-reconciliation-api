namespace global

/// Provides functions for working with async
[<AutoOpen>]
module Async =
    let map f xAsync =
        async {
            let! x = xAsync // get the contents of xAsync
            return f x // apply the function
        }

    let retn x = async { return x }
