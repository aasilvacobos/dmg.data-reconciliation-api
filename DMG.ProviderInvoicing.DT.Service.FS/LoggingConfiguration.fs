namespace DMG.ProviderInvoicing.DT.Service

[<AutoOpen>]
module LoggerTypes =
    /// Log levels used by Serilog
    type LogLevel =
        | VERBOSE
        | DEBUG
        | WARN
        | INFO
        | ERROR
        | EXCEPTION
        | FATAL

module LogLevel =
    /// Caches the DU. Critical for conversion efficiency from string to case.
    let private unionCache = Union.getCases<LogLevel> |> Seq.cache

    let private ofString str =
        Union.createCase<LogLevel> unionCache str

    let toString case = Union.fromCaseToString<LogLevel> case
    let private defaultLogLevel = LogLevel.DEBUG

    let create (logLevelStrOption: string option) =
        logLevelStrOption
        |> Option.bind (fun str ->
            if (isNullOrWhiteSpace str) then
                None
            else
                str |> toUpper |> Option.ofObj)
        |> Option.bind ofString
        |> Option.defaultValue defaultLogLevel
