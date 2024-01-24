namespace DMG.ProviderInvoicing.IO.Host

open System
open DMG.ProviderInvoicing.DT.Service

/// Functions for accessing environment variables
[<AutoOpen>]
module internal HostEnvironmentVariableStore =
    let private getEnvironmentVariable (envVar: HostEnvironmentVariable) =
        envVar
        |> Union.fromCaseToString
        |> Environment.GetEnvironmentVariable

    /// Gets memoized return value of the getEnvironmentVariable function
    let private getEnvironmentVariableMemoized =
        getEnvironmentVariable |> CacheIO.memoize

    /// Call this function to get any environment variable. Should handle null, empty string or
    /// any exception thrown will getting an environment variable value.
    /// TODO: Check for the security exception
    let internal tryGetEnvironmentVariableRawAsString (environmentVariable: HostEnvironmentVariable) =
        try
            let codeStr = getEnvironmentVariableMemoized environmentVariable

            if isNullOrWhiteSpace codeStr then
                None
            else
                Some codeStr
        with
        | e ->
            printfn $"Unexpected exception in tryGetEnvironmentVariableRawAsString: %s{e.Message}"
            printfn $"%s{e.StackTrace}"
            None
