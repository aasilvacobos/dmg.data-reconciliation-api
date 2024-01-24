namespace DMG.ProviderInvoicing.DT.Service

open System

/// Service for consumption by Host I/O adapter
module HostService =
    let private SorConcentratorCacheInHoursDefault : uint32 = 1u;
    let private KafkaMessageFailureSleepInMinutesDefault : uint32 = 1u;
    let DatabaseCacheTimeOutInHours = 24
    let DefaultSerilogLogLevel = "Debug"
    let SerilogLogLevels = [ "Debug"; "Verbose"; "Information"; "Warning"; "Error"; "Fatal" ]
    
    type private EnvironmentVariableBooleanValue =
        /// Value expected for string to converted to boolean true
        | FALSE = 0
        | TRUE = 1

    /// Convert raw host setting value to positive int or default. If default parameter is 0, then it will be treated as 1.
    let private parseHostSettingRawValueToPositiveInt (defaultPositiveInt: uint) (hostSettingRawValue: string option) : uint =
        hostSettingRawValue
        |> Option.map UInt32.TryParse
        |> Option.bind (fun (isSuccess, intValue) ->
            if isSuccess && intValue > 0u then
                Some intValue
            else
                None)
        |> Option.defaultValue (if defaultPositiveInt = 0u then 1u else defaultPositiveInt)

    /// Parse values for any SOR Concentrator cache timeout setting
    let parseSorConcentratorCacheTimeoutInHoursRawValue (hostSettingRawValue: string option) : uint =
        parseHostSettingRawValueToPositiveInt SorConcentratorCacheInHoursDefault hostSettingRawValue

    /// Parse sleep in minutes for Kafka message failure
    let parseKafkaMessageFailureSleepInMinutes (hostSettingRawValue: string option) : uint =
        parseHostSettingRawValueToPositiveInt KafkaMessageFailureSleepInMinutesDefault hostSettingRawValue
    
    /// Convert host setting raw value to boolean
    let ofHostSettingValueToBoolean (hostSettingRawValue: string option) =
        match hostSettingRawValue with
        | Some str -> (str |> toUpper) = EnvironmentVariableBooleanValue.TRUE.ToString()
        | None -> false

    // Parses the Redis Cache value from an optional string.
    let parseRawRedisCacheUrl (cacheUrl: string option) = 
        match cacheUrl with
        | Some url -> 
            if String.IsNullOrWhiteSpace url <> true then 
                Some url
            else
                None
        | None -> None
        
    let parseRawSerilogLevel (logLevel: string option) =
        match logLevel with
        | Some level ->
            if String.IsNullOrWhiteSpace level <> true && SerilogLogLevels |> List.exists (fun x -> x = level) then
                Some level
            else
                None
        | None -> None

    let parseDatabaseCacheTimeoutInHoursRawValue (hostSettingRawValue: string option) : int =
        hostSettingRawValue
        |> Option.map Int32.TryParse
        |> Option.bind (fun (isSuccess, intValue) ->
            if isSuccess && intValue > 0 then
                Some intValue
            else
                None)
        |> Option.defaultValue DatabaseCacheTimeOutInHours

    /// Is GraphQL authentication bypassed based for host setting and given instance?
    let isGraphQlAuthenticationBypassed (authBypassRawValue: bool, instanceCode) =
        authBypassRawValue
        &&
        // prevent bypass in production
        not (Instance.isProduction instanceCode)

    // TODO: This function isn't being used.
    let buildKafkaBrokerList (hostSettingRawValue: string option) =
        hostSettingRawValue
        // cannot split on semicolon because it used as delimiter between environment variables
        |> Option.map (fun kafkaBrokerListString -> kafkaBrokerListString.Split('|'))
        |> Option.defaultValue Array.empty

    open LanguageExt

    let buildGraphQlCorsList (hostSettingRawValue: string option) =
        hostSettingRawValue
        |> Option.map (fun corsList -> corsList.Split('|'))
        |> Option.defaultValue Array.empty
        |> List.ofArray
        |> FSharp.fs