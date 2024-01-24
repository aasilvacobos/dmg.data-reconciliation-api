namespace global

open System
open System.Globalization
open Microsoft.Toolkit

/// Purpose of module is to define function equivalents of common .NET String methods.
/// Due to the ubiquitous nature of this .NET primitive, the namespace is global and
/// the module is AutoOpen to allow the functions to be used in an unqualified manner.
[<AutoOpen>]
module StringFunction =
    let emptyString = String.Empty
    let nullToEmptyString (str: string) = if isNull str then emptyString else str
    let toUpper (str: string) = str.ToUpper()
    let toLower (str: string) = str.ToLower()
    let startsWith (prefix: string) (str: string) = str.StartsWith(prefix)
    let endsWith (suffix: string) (str: string) = str.EndsWith(suffix)
    let toCharArray (str: string) = str.ToCharArray()
    let replace (oldValue: string, newValue: string) (str: string) = str.Replace(oldValue, newValue)
    let trimStart (chars: char []) (str: string) = str.TrimStart(chars)
    let trimEnd (chars: char []) (str: string) = str.TrimEnd(chars)
    let trim (str: string) = str.Trim()

    let truncate (length: int) (str: string) =
        str
        // Convert null string to None in order to short circuit. Prevents null reference.
        |> Option.ofObj
        // Convert string to None if length is negative in order to short circuit. Prevents index exception.
        |> fun strOption -> if length < 0 then None else strOption
        |> Option.map (fun someStr -> StringExtensions.Truncate(someStr, length))
        |> Option.defaultValue Unchecked.defaultof<string>

    let concat (sep: string) (strings: string seq) = String.concat sep strings
    let replicate count str = String.replicate count str
    let isNullOrWhiteSpace str = String.IsNullOrWhiteSpace str
    let length = String.length
    let subString startIndex length (str: string) = str.Substring(startIndex, length)
    let subStringToEnd startIndex (str: string) = str.Substring(startIndex)

    let contains (comparisonStr: string) (str: string) =
        str
        // Convert null base string to None in order to short circuit. Prevents null reference.
        |> Option.ofObj
        // Convert null base string to None if comparison string is null in order to to short circuit. Prevents null reference
        |> Option.bind (fun strValue ->
            if (comparisonStr |> isNull) then
                None
            else
                Some strValue)
        |> Option.map (fun strValue -> strValue.Contains(comparisonStr))
        |> Option.defaultValue false

    let toTitleCase (str: string) =
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str)

    let join sep (strings: string seq) = String.Join(sep, strings)

    let split (separator: string) (str: string) =
        str.Split([| separator |], StringSplitOptions.None)

    let (|IsNullOrWhiteSpace|_|) str =
        match isNullOrWhiteSpace str with
        | true -> Some()
        | false -> None
