namespace DMG.ProviderInvoicing.IO.Host

open System
open System.Reflection;
open System.IO
open DMG.ProviderInvoicing.DT.Service
open Microsoft.Extensions.Configuration
open FsToolkit.ErrorHandling

[<AutoOpen>]
module internal ApplicationSettings =
    let internal ASPNETCORE_ENVIRONMENT_VARIABLE_NAME = HostEnvironmentVariable.ASPNETCORE_ENVIRONMENT.ToString()
    // TODO use InstanceCode DU
    let internal hostEnvironment = Environment.GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT_VARIABLE_NAME)

    let private appSettingFileName = "appsettings.json"

    let configurationRoot =
        let mutable configurationBuilder =
            ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appSettingFileName, false, true)
                .AddConfigMapJsonFiles()
                .AddConfigMapJsonFiles("/vault")
                
        if isNullOrWhiteSpace hostEnvironment then
            // If the environment isn't found, then by default we're running in production mode.
            // The environment should always be found although.
            ()
        else
            configurationBuilder <- configurationBuilder.AddJsonFile($"appsettings.{hostEnvironment}.json", true, true)
            ()
            
        match hostEnvironment with
        | "Development" | "Sandbox" ->
            configurationBuilder <- configurationBuilder.AddUserSecrets()
        | _ ->
            ()
            
        configurationBuilder
            .AddEnvironmentVariables()
            .Build()
 
    let getRawValueFromConfigurationSection (applicationSettings: IConfigurationRoot) (sectionName: string) : string option =
        if applicationSettings.GetSection(sectionName).Exists() then
            Some (applicationSettings.GetValue<string>(sectionName))
        else
            None

    let getRawValueFromConnectionString (applicationSettings: IConfigurationRoot) (connectionStringName: string) : string option =
        let connectionString = applicationSettings.GetConnectionString(connectionStringName);

        if String.IsNullOrWhiteSpace(connectionString) <> true then
            Some connectionString
        else
            None
            
/// API to access the host (infrastructure) configuration settings. This is consumed by all other IO adapters
/// (C# and F#). Public functions should be named in PascalCase and have their parameters and have their return explicitly typed.
/// The Host adapter is the only independent IO adapter. All other IO adapters are dependent on it. Thus, it is critical
/// that this adapter never raises an exception.
module HostConfiguration =
    open LanguageExt
    
    ///////////////////////////////////////////////////////////////////////////////////////
    // Instance code methods

    let GetHostInstanceCode () : InstanceCode =
        hostEnvironment
        |> Option.ofObj
        |> fun rawOpt ->
            match rawOpt with
            | Some instanceCodeRaw -> Instance.getInstanceCode instanceCodeRaw
            | None -> InstanceUnknown

    let GetHostInstanceCodeString () : string =
        ()
        |> GetHostInstanceCode
        |> Union.fromCaseToString

    // TODO: All functions that check if the host is in a specific mode *should be removed* and replaced with an equality check on a given instance code - KA.
    let IsHostRunningInProduction () : bool =
        Instance.isProduction (GetHostInstanceCode())

    let IsHostRunningInTest () : bool = Instance.isTest (GetHostInstanceCode())

    let IsHostRunningInUnknownInstance () : bool  =
        Instance.isUnknown (GetHostInstanceCode())

    /// Returns direct reference to root of the configuration file
    let GetConfigRoot () : IConfigurationRoot = configurationRoot
    
    // TODO: Clay mentioned that this function will need to be implemented in the future.
    // TODO: should validate the app settings values that are shared between all SE projects (e.g.: instance code, log level, etc.
    let ValidateCommonConfiguration() = ()

    ///////////////////////////////////////////////////////////////////////////////////////        
    let private fromRawConfigurationToParseValueSectionName<'a>
        (applicationSettings: IConfigurationRoot)
        (rawToParseFunc: string option -> 'a option)
        // TODO: this parameter should be typed to a DU
        (applicationSettingsSectionName: string) =
        (applicationSettings, applicationSettingsSectionName)
        ||> getRawValueFromConfigurationSection
        |> rawToParseFunc

    let private fromRawConfigurationToParseValueConnectionString
        (applicationSettings: IConfigurationRoot)
        (connectionString : HostConnectionString) =
        (applicationSettings, connectionString.ToString())
        ||> getRawValueFromConnectionString

    let private fromRawConfigurationToParseValueEnvironmentVariable<'a>
        (applicationSettings: IConfigurationRoot)
        (rawToParseFunc: string option -> 'a)
        (hostEnvironmentVariable : HostEnvironmentVariable) =
        (applicationSettings, HostEnvironmentVariable.toString hostEnvironmentVariable)
        ||> getRawValueFromConfigurationSection
        |> rawToParseFunc

    let GetLogLevelString () : Option<string> =
        let mutable sectionName = HostEnvironmentSectionPath.SerilogMinimumLevel.ToString()
        
        // If we can't find the minimum level, try to find the properties level or otherwise the default from the parsing.
        if configurationRoot.GetSection(sectionName).Exists() <> true then
            sectionName <- HostEnvironmentSectionPath.SerilogEnvironmentProperties.ToString()
        else
            ()
        
        fromRawConfigurationToParseValueSectionName<string> configurationRoot HostService.parseRawSerilogLevel sectionName
        |> FSharp.fs

    let GetGraphQlCorsEndpoints () : Lst<string> =
        DMG_PROVIDERINVOICING_GRAPHQL_CORS_ENDPOINTS
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.buildGraphQlCorsList
    
    /// The logger category name for a service executable
    let GetServiceExecutableLoggerCategoryName () : string =
        DMG_PROVIDERINVOICING_SE_LOGGER_CATEGORY_NAME
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot id
        |> Option.defaultValue (Assembly.GetExecutingAssembly().FullName) 
    
    let GetSorConcentratorCacheTimeoutInHoursCatalogItem () : uint =
        DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_CATALOG_ENTRY
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue
    let GetSorConcentratorCacheTimeoutInHoursCustomer () : uint =
        DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_CUSTOMER
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue
    let GetSorConcentratorCacheTimeoutInHoursProperty () : uint =
        DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_PROPERTY
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue
    let GetSorConcentratorCacheTimeoutInHoursProviderOrg () : uint =
        DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_PROVIDER_ORG
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue
    let GetSorConcentratorCacheTimeoutInHoursServiceType () : uint =
        DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_SERVICE_TYPE
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue
    let GetSorConcentratorCacheTimeoutInHoursServiceLine () : uint =
        DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_SERVICE_LINE
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue
    let GetSorConcentratorCacheTimeoutInHoursUser () : uint =
        DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_USER
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue      
    let GetSorConcentratorCacheTimeoutInHoursPSA () : uint =
        DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_USER
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue      
    let GetLookupCacheTimeoutInHoursState () : uint =
        DMG_PROVIDERINVOICING_LOOKUP_CACHE_TIMEOUT_IN_HOURS_STATE
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue      
    let GetLookupCacheTimeoutInHoursPaymentTerm () : uint =
        DMG_PROVIDERINVOICING_LOOKUP_CACHE_TIMEOUT_IN_HOURS_PAYMENT_TERM
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseSorConcentratorCacheTimeoutInHoursRawValue      
    let GetKafkaMessageFailureSleepInMinutes () : uint =
        DMG_PROVIDERINVOICING_KAFKA_MESSAGE_FAILURE_SLEEP_IN_MINUTES
        |> fromRawConfigurationToParseValueEnvironmentVariable configurationRoot HostService.parseKafkaMessageFailureSleepInMinutes      
        
    let GetDatabaseConnectionStringProviderInvoicing () : Option<string> =
        HostConnectionString.ProviderInvoicingConnection
        |> fromRawConfigurationToParseValueConnectionString configurationRoot 
        |> FSharp.fs

    let GetDatabaseConnectionStringFinancialAbstractionLayer () : Option<string> =
        HostConnectionString.FinancialAbstractionLayerConnection
        |> fromRawConfigurationToParseValueConnectionString configurationRoot
        |> FSharp.fs
            
    let GetRedisCacheServerUrl () : Option<string> =         
        HostEnvironmentSectionPath.RedisCacheServerUrl.ToString()
        |> fromRawConfigurationToParseValueSectionName<string> configurationRoot HostService.parseRawRedisCacheUrl
        |> FSharp.fs
  
    let GetGraphQLAuthenticationToken () : Option<string> =
        // temporarily hard coding until the Host adapter works with .NET Core appsettings.
        match GetHostInstanceCode () with
        | Development -> FSharp.Core.Some "Development"
        | Sandbox -> FSharp.Core.Some "Sandbox"
        | Test -> FSharp.Core.Some  "Test"
        | Staging -> FSharp.Core.Some "Staging"
        | Production -> FSharp.Core.Some "ProductionTokenTBD"
        | InstanceUnknown -> None
        |> FSharp.fs    // converts to C# option  
        
    let GetFulfillmentApiServiceName () : string =
        @"FullfilmentApiService"
        
    let GetProviderBillingApiService () : string =
        @"ProviderBillingApiService"
        

    let GetSorConcentratorApiServiceName () : string =
        @"GetById-Api"

    let GetLookupItemsServiceName() : string =
        @"LookupItemsService"
        
    let IsGraphQlAuthenticationBypassed () : bool =
        let isAuthorizationBypassedSetting = false
        // TODO get from appsettings
//            getHostAppSettings ()
//            |> Option.bind (fun x -> x.IsGraphQlAuthenticationBypassed)
//            |> Option.defaultValue false

        (isAuthorizationBypassedSetting, GetHostInstanceCode())
        |> HostService.isGraphQlAuthenticationBypassed