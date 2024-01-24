namespace DMG.ProviderInvoicing.DT.Service

/// DU representation of all environment variable names
type HostEnvironmentVariable =
    // Common/shared
    | ASPNETCORE_ENVIRONMENT
    | DMG_IS_RUNNING_LOCAL
    | DMG_PROVIDERINVOICING_AWS_ACCESS_KEY_ID // used for parameter store access
    | DMG_PROVIDERINVOICING_AWS_SECRET_ACCESS_KEY // used for parameter store access
    // Kafka Consumer specific
    | DMG_PROVIDERINVOICING_KAFKA_MESSAGE_FAILURE_SLEEP_IN_MINUTES
    // GraphQL specific
    | DMG_PROVIDERINVOICING_LCL_INTEGRATED
    | DMG_PROVIDERINVOICING_AUTH_BYPASS
    | DMG_PROVIDERINVOICING_GRAPHQL_CORS_ENDPOINTS
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_ITEM_CATALOG_ENTRY_ENDPOINT //  http://dmg-dataservices-itemcatalog-api.dataservices:88
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_JOB_POSTING_ENDPOINT
    | DMG_PROVIDERINVOICING_WORK_FULFILLMENT_SERVICE_ENDPOINT
    // SOR Concentrator caching
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_CATALOG_ENTRY
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_CUSTOMER
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_PROPERTY
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_PROVIDER_ORG
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_SERVICE_LINE
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_SERVICE_TYPE
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_USER
    | DMG_PROVIDERINVOICING_SOR_CONCENTRATOR_CACHE_TIMEOUT_IN_HOURS_PSA
    // Lookup caching
    | DMG_PROVIDERINVOICING_LOOKUP_CACHE_TIMEOUT_IN_HOURS_STATE    
    | DMG_PROVIDERINVOICING_LOOKUP_CACHE_TIMEOUT_IN_HOURS_PAYMENT_TERM    
    // Local Postgres Database
    | DMG_PROVIDERINVOICING_DATABASE_CONNECTION_STRING_PI    
    // FAL Postgres Database
    | DMG_PROVIDERINVOICING_DATABASE_CONNECTION_STRING_FAL    
    // Miscellaneous
    | DMG_PROVIDERINVOICING_SE_LOGGER_CATEGORY_NAME
    
    // Future use
    | DMG_PROVIDERINVOICING_SMTP_HOST_EMAIL
    | DMG_PROVIDERINVOICING_USERNAME
    | DMG_PROVIDERINVOICING_PASSWORD
    | DMG_PROVIDERINVOICING_SMTP_DOMAIN_NAME

module HostEnvironmentVariable =    
        
    let unionCache = Union.getCases<HostEnvironmentVariable> |> Seq.cache

    let ofString str =
        Union.createCase<HostEnvironmentVariable> unionCache str

    let toString case = Union.fromCaseToString<HostEnvironmentVariable> case
            
module HostEnvironmentSectionPath =
    type HostEnvironmentSectionPath =
        | SerilogMinimumLevel
        | SerilogEnvironmentProperties
        | RedisCacheServerUrl
    with
        override this.ToString() =
            match this with
            | SerilogMinimumLevel -> "Serilog:MinimumLevel:Default"
            | SerilogEnvironmentProperties -> "Serilog:Properties:Environment"
            | RedisCacheServerUrl -> "RedisCache:ServerUrl"

type HostConnectionString = 
    | ProviderInvoicingConnection
    | FinancialAbstractionLayerConnection
    with override this.ToString() =
            match this with
            | ProviderInvoicingConnection -> "postgres.ProviderInvoicing"   // database for the Provider Invoicing backend service            
            | FinancialAbstractionLayerConnection -> "postgres.Invoicing"   // persists VendorBill (NetSuite's version of provider invoice)

module HostSetting =
    type HostSettingDataSource =
        | HostEnvironmentVariable
        | HostAwsParameterStore