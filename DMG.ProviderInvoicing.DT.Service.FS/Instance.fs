namespace DMG.ProviderInvoicing.DT.Service


/// Codes representing all instances of the application
type InstanceCode =
    // running locally in IDE
    | Development
    /// sandbox app in AWS integrated to sandbox
    | Sandbox
    /// staging app in AWS integrated to staging
    /// (docker for any/every local box)
    | Staging
    /// test app in AWS integrated to test
    | Test
    /// production app in AWS integrated to production
    | Production
    /// unknown (invalid, undefined...)
    | InstanceUnknown

module Instance =
    /// Is running in unknown instance?
    let isUnknown =
        function
        | Development -> false
        | Sandbox -> false
        | Staging -> false
        | Test -> false
        | Production -> false
        | InstanceUnknown -> true // should be only true

    /// Is running in production?
    let isProduction =
        function
        | Development -> false
        | Sandbox -> false
        | Staging -> false
        | Test -> false
        | Production -> true // should be only true
        | InstanceUnknown -> false

    /// Is running in test?
    let isTest =
        function
        | Development -> false
        | Sandbox -> false
        | Staging -> false
        | Test -> true // should be only true
        | Production -> false
        | InstanceUnknown -> false

    /// Is instance integrated outbound to Portal Database?
    let isIntegratedOutboundWithPortal (isLocalIntegrated: bool) =
        function
        | Development -> isLocalIntegrated
        | Sandbox -> true
        | Staging -> true
        | Test -> true
        | Production -> true
        | InstanceUnknown -> false

    let private ofString (str: string) =
        let instanceCodeUnionCache = Union.getCases<InstanceCode> |> Seq.cache

        (instanceCodeUnionCache, str)
        ||> Union.createCase<InstanceCode>

    let getInstanceCode (instanceCodeRawString: string) =
        instanceCodeRawString
        |> ofString
        |> Option.defaultValue InstanceUnknown

    let getInstanceCodeStr (instanceCodeRawString: string) =
        instanceCodeRawString
        |> getInstanceCode
        |> Union.fromCaseToString
