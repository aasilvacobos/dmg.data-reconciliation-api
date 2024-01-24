namespace global

[<AutoOpen>]
module public WrappedString =
    open System

    type ResultCreateString<'TWrappedString> =
        | CreatedString of 'TWrappedString
        | ErrorNullValue
        | ErrorEmptyStringOrWhiteSpace
        | ErrorInvalidFormat
        | ErrorException of Exception

    type ResultCreateStringOption<'TWrappedString> =
        | CreatedStringOption of 'TWrappedString option
        | ErrorEmptyStringOrWhiteSpace
        | ErrorInvalidFormat
        | ErrorException of Exception

    type IWrappedString =
        abstract Value: string

    /// Converts any type to a string for display. If the type is suffixed with "Type", it will be removed.
    let private wrappedTypeToDisplayName<'T> = // Questions: Does this need to be made into a function by adding a unit param? Will this work with eager evaluation?
        let wrappedTypeName = $"{typeof<'T>.Name}"

        if endsWith @"Type" wrappedTypeName then
            subString 0 (length wrappedTypeName - 4) wrappedTypeName
        else
            wrappedTypeName

    /// Base function. Attempts to create a wrapped string that is a required value (non-option).
    /// Value is expected to be valid, canonicalizable, not null, not an empty string and not whitespace.
    /// Otherwise, an error case will be returned.
    let createCore
        (canonicalize: string -> string)
        (isValid: string -> bool)
        ctor
        str
        : ResultCreateString<'wrappedType> =
        try
            if str |> isNull then
                ResultCreateString.ErrorNullValue
            else if str |> isNullOrWhiteSpace then
                ResultCreateString.ErrorEmptyStringOrWhiteSpace
            else
                let str' = canonicalize str

                if isValid str' then
                    ResultCreateString.CreatedString(ctor str')
                else
                    ResultCreateString.ErrorInvalidFormat
        with
        | ex -> ResultCreateString.ErrorException ex

    /// Create a wrapped string that is a required value (non-option).
    /// Value is expected to be valid, canonicalizable, not null, not an empty string and not whitespace.
    /// Otherwise, an error message will be returned.
    let tryCreate (canonicalize: string -> string) (isValid: string -> bool) ctor str : Result<'wrappedType, string> =
        match createCore canonicalize isValid ctor str with
        | ResultCreateString.CreatedString ws -> Ok ws
        | ResultCreateString.ErrorNullValue ->
            Error $"{wrappedTypeToDisplayName<'wrappedType>} has invalid null string value."
        | ResultCreateString.ErrorEmptyStringOrWhiteSpace ->
            Error $"{wrappedTypeToDisplayName<'wrappedType>} has invalid empty string value."
        | ResultCreateString.ErrorInvalidFormat -> Error $"{wrappedTypeToDisplayName<'wrappedType>} has invalid format."
        | ResultCreateString.ErrorException ex ->
            Error
                $"Unexpected exception on string value {wrappedTypeToDisplayName<'wrappedType>}. %s{ex.GetType().ToString()}: %s{ex.Message}"

    /// Create a wrapped string that is a required value (non-option).
    /// Value is expected to be valid, canonicalizable, not null, not an empty string and not whitespace.
    /// Otherwise, an exception will be thrown.
    let create canonicalize ctor str : 'wrappedType =
        let isValid str = true

        match createCore canonicalize isValid ctor str with
        | ResultCreateString.CreatedString ws -> ws
        | ResultCreateString.ErrorNullValue ->
            ArgumentNullException(
                paramName = nameof str,
                message = $"{wrappedTypeToDisplayName<'wrappedType>} has invalid null string value."
            )
            |> raise
        | ResultCreateString.ErrorEmptyStringOrWhiteSpace ->
            ArgumentException(
                paramName = nameof str,
                message = $"{wrappedTypeToDisplayName<'wrappedType>} has invalid empty string value."
            )
            |> raise
        | ResultCreateString.ErrorInvalidFormat ->
            ArgumentException(
                paramName = nameof str,
                message = $"{wrappedTypeToDisplayName<'wrappedType>} has invalid format."
            )
            |> raise
        | ResultCreateString.ErrorException ex ->
            Exception(
                $"Unexpected exception wrapping string value {wrappedTypeToDisplayName<'wrappedType>}. %s{ex.GetType().ToString()}: %s{ex.Message}",
                ex
            )
            |> raise

    /// Attempt to create a wrapped string option (not required).
    /// Value is expected to be valid and canonicalizable.
    /// If value is null, empty string or whitespace, a successful None will be returned.
    /// Otherwise, an error case will be returned.
    let tryCreateOption canonicalize isValid ctor str : Result<'wrappedType option, string> =
        match createCore canonicalize isValid ctor str with
        | ResultCreateString.CreatedString ws -> Ok(Some ws)
        | ResultCreateString.ErrorNullValue -> Ok None
        | ResultCreateString.ErrorEmptyStringOrWhiteSpace -> Ok None
        | ResultCreateString.ErrorInvalidFormat -> Error $"{wrappedTypeToDisplayName<'wrappedType>} has invalid format."
        | ResultCreateString.ErrorException ex ->
            Error
                $"Unexpected exception on string value {wrappedTypeToDisplayName<'wrappedType>}. %s{ex.GetType().ToString()}: %s{ex.Message}"

    /// Create a wrapped string option (not required).
    /// Value is expected to be valid and canonicalizable.
    /// If value is null, empty string or whitespace, a None will be returned.
    /// Otherwise, an exception will be thrown.
    let createOption canonicalize isValid ctor str : 'wrappedType option =
        match createCore canonicalize isValid ctor str with
        | ResultCreateString.CreatedString ws -> Some ws
        | ResultCreateString.ErrorNullValue -> None
        | ResultCreateString.ErrorEmptyStringOrWhiteSpace ->
            ArgumentException(
                paramName = nameof str,
                message = $"{wrappedTypeToDisplayName<'wrappedType>} has invalid empty string value."
            )
            |> raise
        | ResultCreateString.ErrorInvalidFormat ->
            ArgumentException(
                paramName = nameof str,
                message = $"{wrappedTypeToDisplayName<'wrappedType>} has invalid format."
            )
            |> raise
        | ResultCreateString.ErrorException ex ->
            Exception(
                message = $"{wrappedTypeToDisplayName<'wrappedType>} has invalid value; %s{ex.Message}",
                innerException = ex
            )
            |> raise

    /// Apply the given function to the wrapped value
    let apply f (s: IWrappedString) = s.Value |> f
    /// Get the wrapped value
    let value s = apply id s

    /// Get the value as option from an option wrapped value
    let valueOfOption (sOpt: 'wrappedType option) =
        sOpt
        |> Option.map (fun sno -> sno :> IWrappedString)
        |> (Option.map value)

    let equals left right = (value left) = (value right)
    let compareTo left right = (value left).CompareTo(value right)

    /// converts all whitespace to a space char and trim
    let singleLineTrimmed s =
        System
            .Text
            .RegularExpressions
            .Regex
            .Replace(s, "\s", " ")
            .Trim()

    let singleLineTrimmedToTitleCase s = s |> singleLineTrimmed |> toTitleCase
    let lengthValidator len (s: string) = s.Length <= len
