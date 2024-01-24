namespace DMG.ProviderInvoicing.DT.Service.JobBillingWriter

open System
open DMG.ProviderInvoicing.DT.Domain
open DMG.ProviderInvoicing.DT.Service

// Allowed levels for logging messages related to job billing writer. Enum is used for easy consumption in C#.
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
    
// All possible results for processing a job billing message to put to FAL
type ProcessMessageResult =
    | FailedToDeserializeJobBillingMessage of JobBillingId
    | FailedToRetrieveJobToConstructJobBilling of JobBillingId * JobWorkId
    | FailedToPutJobBillingIntoFalDatabase of JobBillingId
    | SuccessfullyPutJobBillingIntoFalDatabase of JobBillingId
    | UnexpectedFailure

    // Build message and log level used for logging message based on result case
    member this.ToResultLogMessage() : ResultLogMessage =
        let resultMessageRaw = 
            match this with
            | FailedToDeserializeJobBillingMessage jobBillingId ->
                { LogLevel = MessageLogLevel.Emergency; MessageText = $"Unable to deserialize job billing {jobBillingId.Value} message in order to put into FAL." }
            | FailedToRetrieveJobToConstructJobBilling (jobBillingId, jobWorkId) ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to retrieve job {jobWorkId.Value} from SOR Concentrator to construct job billing {jobBillingId.Value} in order to put into FAL." }
            | FailedToPutJobBillingIntoFalDatabase jobBillingId ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to put job billing {jobBillingId.Value} into FAL." }
            | SuccessfullyPutJobBillingIntoFalDatabase jobBillingId ->
                { LogLevel = MessageLogLevel.Info; MessageText = $"Successfully put job billing {jobBillingId.Value} into FAL." }
            | UnexpectedFailure ->
                { LogLevel = MessageLogLevel.Ignore; MessageText = emptyString } // message logging for exception handled elsewhere
            
        { resultMessageRaw with MessageText = normalizeString resultMessageRaw.MessageText } 

    // Determines whether message should be considered processed (i.e., a de-facto success)
    member this.IsMessageConsideredProcessed() : bool =
        match this with
        | FailedToDeserializeJobBillingMessage _                                        -> false    // retry message later if message cannot be deserialized
        | FailedToRetrieveJobToConstructJobBilling _                                    -> false    // retry message later if job billing cannot be created
        | FailedToPutJobBillingIntoFalDatabase _                                        -> false    // retry message later if vendor bill failed to insert into FAL
        | SuccessfullyPutJobBillingIntoFalDatabase _                                    -> true     // success
        | UnexpectedFailure _                                                           -> false    // retry