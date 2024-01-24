namespace global

[<AutoOpen>]
module public WrappedGuid =
    open System

    type IWrappedGuid =
        abstract Value: Guid

    type ResultCreateGuid<'TWrappedGuid> =
        | CreatedGuid of 'TWrappedGuid
        | ErrorEmptyGuid
        | ErrorException of Exception

    type ResultCreateGuidOption<'TWrappedGuid> =
        | CreatedGuidOption of 'TWrappedGuid option
        | ErrorEmptyGuid
        | ErrorException of Exception

    /// Converts any type to a string for display. If the type is suffixed with "Type", it will be removed.
    let private wrappedTypeToDisplayName<'T> = // Questions: Does this need to be made into a function by adding a unit param? Will this work with eager evaluation?
        let wrappedTypeName = $"{typeof<'T>.Name}"

        if endsWith @"Type" wrappedTypeName then
            subString 0 (length wrappedTypeName - 4) wrappedTypeName
        else
            wrappedTypeName

    /// Base function. Attempts to create a wrapped Guid that is a required value (non-option).
    /// Value is expected to be not-empty Guid. Otherwise, an error case will be returned.
    let createCore ctor guid : ResultCreateGuid<'TWrappedGuid :> IWrappedGuid> =
        try
            match (guid = Guid.Empty) with
            | true -> ResultCreateGuid.ErrorEmptyGuid
            | false -> guid |> ctor |> ResultCreateGuid.CreatedGuid
        with
        | ex -> ResultCreateGuid.ErrorException ex // this might be overkill

    /// Create a wrapped Guid that is a required value (non-option).
    /// Value is expected to be a non-empty Guid. Otherwise, an error message will be returned.
    let tryCreate ctor (guid: Guid) : Result<'TWrappedGuid :> IWrappedGuid, string> =
        match createCore ctor guid with
        | ResultCreateGuid.CreatedGuid wg -> Ok wg
        | ResultCreateGuid.ErrorEmptyGuid ->
            Error $"{wrappedTypeToDisplayName<'TWrappedGuid>} has invalid empty Guid value."
        | ResultCreateGuid.ErrorException ex ->
            Error
                $"Unexpected exception on guid value {wrappedTypeToDisplayName<'TWrappedGuid>}. %s{ex.GetType().ToString()}: %s{ex.Message}"

    /// Create a wrapped Guid that is a required value (non-option).
    /// Value is expected to be a non-empty guid. Otherwise, an exception will be thrown.
    let create ctor (guid: Guid) : 'TWrappedGuid :> IWrappedGuid =
        match createCore ctor guid with
        | ResultCreateGuid.CreatedGuid wg -> wg
        | ResultCreateGuid.ErrorEmptyGuid ->
            ArgumentException(
                paramName = nameof guid,
                message = $"{wrappedTypeToDisplayName<'TWrappedGuid>} has invalid empty Guid value."
            )
            |> raise
        | ResultCreateGuid.ErrorException ex ->
            Exception(
                $"Unexpected exception wrapping Guid value {wrappedTypeToDisplayName<'TWrappedGuid>}. %s{ex.GetType().ToString()}: %s{ex.Message}",
                ex
            )
            |> raise

    /// Attempt to create a wrapped Guid option (not required).
    /// If value is empty Guid, a successful None will be returned.
    /// Otherwise, an error case will be returned.
    let tryCreateOption ctor (guidNullable: Guid Nullable) : Result<('TWrappedGuid :> IWrappedGuid) option, string> =
        guidNullable
        |> Option.ofNullable
        |> Option.map (fun guid ->
            match createCore ctor guid with
            | ResultCreateGuid.CreatedGuid wg -> Ok(Some wg)
            | ResultCreateGuid.ErrorEmptyGuid -> Ok None
            | ResultCreateGuid.ErrorException ex ->
                Error
                    $"Unexpected exception on Guid value {wrappedTypeToDisplayName<'TWrappedGuid>}. %s{ex.GetType().ToString()}: %s{ex.Message}")
        |> Option.defaultValue (Ok None)

    /// Create a wrapped string Guid (not required).
    /// If value is empty Guid, a None will be returned.
    /// Otherwise, an exception will be thrown.
    let createOption ctor (guidNullable: Guid Nullable) : ('TWrappedGuid :> IWrappedGuid) option =
        guidNullable
        |> Option.ofNullable
        |> Option.map (fun guid ->
            match createCore ctor guid with
            | ResultCreateGuid.CreatedGuid wg -> Some wg
            | ResultCreateGuid.ErrorEmptyGuid -> None
            | ResultCreateGuid.ErrorException ex ->
                Exception(
                    message = $"{wrappedTypeToDisplayName<'TWrappedGuid>} has invalid value; %s{ex.Message}",
                    innerException = ex
                )
                |> raise)
        |> Option.defaultValue None

    /// Apply the given function to the wrapped value
    let apply f (wg: IWrappedGuid) = wg.Value |> f
    /// Get the wrapped value
    let value wg = apply id wg

    /// Get the value as option from an option wrapped value
    let valueOfOption (sOpt: ('TWrappedGuid :> IWrappedGuid) option) =
        sOpt
        |> Option.map (fun sno -> sno :> IWrappedGuid)
        |> Option.map value

    let equals left right = (value left) = (value right)
