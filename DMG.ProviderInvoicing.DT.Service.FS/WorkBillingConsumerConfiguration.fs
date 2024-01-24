namespace DMG.ProviderInvoicing.DT.Service.WorkBillingConsumer

open System
open DMG.ProviderInvoicing.DT.Domain
open DMG.ProviderInvoicing.DT.Service

// Allowed levels for logging messages related to invoice builder. Enum is used for easy consumption in C#.
type MessageLogLevel =
   | Ignore = 0
   | Debug = 1
   | Info = 2
   | Warning = 3
   | Error = 4
   | Emergency = 5

type ResultLogMessage =
    { LogLevel: MessageLogLevel
      MessageText: string }
    
// All possible results for processing a job billing message for work billing
type ProcessMessageResult =
    | FailedToDeserializeJobBillingMessage of JobBillingId * MessageFailureCount: int * Exception
    | FailedToDeserializeWorkBillingMessage of JobBillingId * MessageFailureCount: int
    | FailedToRetrieveJobToConstructJobBilling of JobBillingId * JobWorkId
    // | DBFailure of DMG.ProviderInvoicing.IO.Database.DBOperationError
    | SuccessfullyDeserializedWorkBillingMessage of JobBillingId
    | SuccessfullyGeneratedJobBill of JobBillingId
    // *** Common ***    
    | UnexpectedFailure

    // Build message and log level used for logging message based on result case
    member this.ToResultLogMessage() : ResultLogMessage =
        let resultMessageRaw = 
            match this with 
            | FailedToRetrieveJobToConstructJobBilling (jobBillingId, jobWorkId) ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to retrieve job {jobWorkId.Value} from SOR Concentrator while constructing verified job billing {jobBillingId.Value} for workbilling." }
            | UnexpectedFailure ->
                { LogLevel = MessageLogLevel.Ignore; MessageText = emptyString } // message logging for exception handled elsewhere
            
        { resultMessageRaw with MessageText = normalizeString resultMessageRaw.MessageText } 

    // Determines whether message should be considered processed (i.e., a de-facto success)
    member this.IsMessageConsideredProcessed() : bool =
        match this with
        | FailedToDeserializeJobBillingMessage _       -> false    // failed
        | FailedToDeserializeWorkBillingMessage _      -> false    // failed
        | FailedToRetrieveJobToConstructJobBilling _   -> false    // retry message later if job billing cannot be created
        // | DBFailure _                                  -> false    // faled
        | SuccessfullyDeserializedWorkBillingMessage _ -> true     // success
        | SuccessfullyGeneratedJobBill _               -> true     // success
        // *** Common ***
        | UnexpectedFailure _                          -> false    // retry
