namespace global

/// Purpose of module is to define globally available functions for Option that are
/// both missing from the Option module and non-fluent/verbose to implement. Due to the
/// ubiquitous nature of the F# Option, the namespace for these functions is global and the
/// module is AutoOpen to allow the functions to be used in an unqualified manner. This is
/// an experimental approach to facilitate more fluent code.
[<AutoOpen>]
module OptionFunction =
    /// Does the option have some value? To be used instead of the IsSome method.
    let hasSomeValue (opt: 'a option) : bool = opt |> Option.exists (fun _ -> true)
