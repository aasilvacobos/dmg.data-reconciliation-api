namespace global

/// Defines a type as an optional update
type UpdateOption<'T> =
    /// An update value
    | Update of 'T
    /// No value to update
    | NoUpdate

//[<AutoOpen>]
module UpdateOption =
    /// map f updateOption evaluates to match updateOption with NoUpdate -> NoUpdate | Update x -> Update (f x)
    let map f updateOption =
        match updateOption with
        | Update x -> Update(f x)
        | NoUpdate -> NoUpdate

    /// bind f updateOption evaluates to match updateOption with NoUpdate -> NoUpdate | Update x -> f x
    let bind f updateOption =
        match updateOption with
        | Update x -> f x
        | NoUpdate -> NoUpdate

    /// Creates UpdateOption from Option
    let ofOption opt =
        match opt with
        | Some x -> Update x
        | None -> NoUpdate

    /// Converts UpdateOption to Option
    let toOption updateOption =
        match updateOption with
        | Update x -> Some x
        | NoUpdate -> None

    /// Gets the value of the UpdateOption if the UpdateOption is Update, otherwise returns the specified default value.
    let defaultValue defVal updateOption =
        match updateOption with
        | Update x -> x
        | NoUpdate -> defVal
