namespace global

open LanguageExt
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

/// Provides functions for converting to C# language-ext types 
[<AutoOpen>]
module CSharpExtensions =
    // Convert F# Result to C# language-ext Either
    let toEither<'ok, 'error> result : Either<'error, 'ok> =
        result
        |> Result.either Prelude.Right<'error, 'ok> Prelude.Left<'error, 'ok>

    /// Convert F# option to C# language-ext option
    let toOptionCSharp (opt: 'T option) : LanguageExt.Option<'T> =
        match opt with
        | Some x -> Prelude.Some<'T> x
        | None -> Option.None
