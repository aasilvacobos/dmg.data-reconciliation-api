namespace global

/// Provides functions for working with discriminated unions
[<AutoOpen>]
module Union =
    open Microsoft.FSharp.Reflection

    /// Convert a union into a sequence of UnionCaseInfo items
    let getCases<'union> =
        FSharpType.GetUnionCases typeof<'union>
        |> Array.toSeq

    /// Convert a union into a cached sequence of UnionCaseInfo items.
    /// This should be used instead of getCases for better performance via cache.
    let getCasesCached<'union> = getCases |> Seq.cache

    /// Try to create a specific case in a union from a string value.
    let tryCreateCase<'union> (unionCases: seq<UnionCaseInfo>) caseString =
        caseString
        |> Option.ofObj
        |> Option.map trim
        |> Option.bind (fun s ->
            match unionCases
                  |> Seq.toArray
                  |> Array.filter (fun case -> case.Name = s)
                with
            | [| case |] -> Some(FSharpValue.MakeUnion(case, [||]) :?> 'union)
            | _ -> None)

    /// Obsolete - Use tryCreateCase instead.
    let createCase<'union> = tryCreateCase<'union>

    /// Convert a case to its equivalent string. Use this instead of ToString().
    let fromCaseToString<'union> (unionCase: 'union) =
        let case, _ = FSharpValue.GetUnionFields(unionCase, typeof<'union>)
        case.Name

    /// Convert a sequence of UnionCaseInfo items to sequence of strings
    let asStrings (cases: seq<UnionCaseInfo>) = cases |> Seq.map (fun uci -> uci.Name)
