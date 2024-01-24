namespace DMG.ProviderInvoicing.IO.Logging

open System
open DMG.ProviderInvoicing.DT.Service
open DMG.ProviderInvoicing.IO.Host;
open Microsoft.Extensions.Logging
open LanguageExt;
open DMG.K8S.Framework.DotNet.Common.Logging

[<AutoOpen>]
// API for logging messages. Consumed by C#/F# IO adapters.
module Logger =
    let msgPrefix = "[PI]"
    let ILogger = StaticLogger.LoggerFactory.CreateLogger(HostConfiguration.GetServiceExecutableLoggerCategoryName())

    let private shouldAddSpaceAfterMsgPrefix msg =
        // if the message starts with a [, then assume caller added a sub-prefix with a trailing space
        // Ex: msg is "[IO-FAL] Vendor bill..."
        msg |> startsWith "[" |> not

    /// Add prefix to message and space between if necessary        
    let private buildMessage msg =
        ([msgPrefix; msg] |> join (if (shouldAddSpaceAfterMsgPrefix msg) then " " else emptyString))
        
    let Verbose msg = ILogger.LogTrace (buildMessage msg)

    let Debug msg = ILogger.LogDebug (buildMessage msg)

    let Info msg = ILogger.LogInformation (buildMessage msg)

    let Warning msg = ILogger.LogWarning (buildMessage msg)

    let Error (msg: string) = ILogger.LogError (buildMessage msg)

    /// Used for unexpected/unrecognized exceptions.
    let Exception (ex: Exception, msg: LanguageExt.Option<string>) = // TODO rename to ErrorException since it is actually a different type of Error
        msg
        |> FSharp.fs
        |> fun x ->
            match x with
            | None -> ILogger.LogError(ex, "Program Exception Caught")
            | Option.Some msg -> ILogger.LogError(ex, (buildMessage msg))
            
    /// Logs an emergency/critical level message and sends an alert            
    let Emergency msg = ILogger.LogCritical (buildMessage msg)
    
    /// Logs test message for some log levels. Used to diagnose Datadog behaviour.
    let LogTestMessages () =
        match HostConfiguration.GetHostInstanceCode () with
        | Sandbox -> 
            Verbose     "Logging PI Verbose (Trace) test message"
            Error       "Logging PI Error test message"
            Exception   (exn "Exception test", LanguageExt.Option.Some "Logging PI Exception (Error) test message")
            Emergency   "Logging PI Emergency (Critical) test message"
        | InstanceUnknown             
        | Test 
        | Staging 
        | Development 
        | Production ->
            ()                                    