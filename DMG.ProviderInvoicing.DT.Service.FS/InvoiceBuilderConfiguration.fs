namespace DMG.ProviderInvoicing.DT.Service.InvoiceBuilder

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

// Status for messages related for logging and error Processing. Enum is used for easy consumption in C#. 
type ProcessMessageStatus =
   | ConsiderProcessed = 0  // read next message from the topic
   | TransientError = 1     // retry the same message a few more times
   | FatalError = 2         // stop reading new messages from the topic
    
// All possible results for processing a job billing message for invoicing
type ProcessMessageResult =
    // *** Service1Consumer ***
    | FailedToDeserializeJobBillingMessage of JobBillingId * MessageFailureCount: int * Exception
    | JobBillingNotYetVerifiedForInvoicing  of JobBillingId
    | InvoiceCanceled of JobBillingId
    | ErrorCheckingIfJobIsCanceled of JobBillingId
    // This can be removed after conversion from job billing gross to job billing is deterministic
    | FailedToRetrieveJobToConstructJobBilling of JobBillingId * JobWorkId
    //| SuccessfullyInsertedApprovedJobBillingIntoLocalDatabase of JobBillingId TODO
    //| FailedToInsertJobBillingIntoLocalDatabase, TODO
    
    // *** Service2Builder ***    
    | VendorBillIsAlreadyCreatedInFal of JobBillingId
    | FailedToDetermineIfVendorBillIsCreatedInFal of JobBillingId
    | FailedToRetrieveDmgInvoiceNumberFromRedisToConstructProviderInvoice of JobBillingId
    | FailedToRetrievePropertyToConstructProviderInvoice of JobBillingId * PropertyId
    | FailedToRetrieveServiceLineToConstructProviderInvoice of JobBillingId * ServiceLineId
    | FailedToRetrieveServiceTypeToConstructProviderInvoice of JobBillingId * ServiceTypeId
    | FailedToQuerySorConcentratorToGetCatalogItemNamesToConstructProviderInvoice of JobBillingId // Unnecessary to have CatalogItemId since IO.SorConcentrator will log it in its error message. 
    | VendorBillLinesAreInvalid of ErrorMessage * JobBillingId  // Generic case of invalid vendor bill lines; the ErrorMessage carries the error.
    | FailedToInsertVendorBillIntoFalDatabase of JobBillingId
    | BypassVendorBillGenerationDueToNoBillableLines of JobBillingId * TicketId * JobBillingCostingScheme * IsPaidByCreditCard: bool
    | SuccessfullyGeneratedVendorBill of JobBillingId * TicketId * VendorBillInsertSuccess
    | FailedToApproveInvoice of TicketId

    // *** Common ***    
    | UnexpectedFailure

    member private this.CostSchemeText (costingScheme: JobBillingCostingScheme) : string =
        match costingScheme with
        | JobBillingCostingScheme.TimeAndMaterial -> "Time&Mtl"
        | JobBillingCostingScheme.FlatRate        -> "FlatRate"
        | JobBillingCostingScheme.ServiceBased    -> "SvcBased"
        // wild card is here just to remove warning
        | _                                       -> "Time&Mtl"
        
    // Build message and log level used for logging message based on result case
    member this.ToResultLogMessage() : ResultLogMessage =
        let resultMessageRaw = 
            match this with
            // *** Service1Consumer ***
            | FailedToDeserializeJobBillingMessage (jobBillingId, messageFailureCount, ex) ->
                { LogLevel = MessageLogLevel.Emergency; MessageText = $"Unable to deserialize job billing {jobBillingId.Value} message from topic. {ex.Message}" }
            | JobBillingNotYetVerifiedForInvoicing jobBillingId ->
                { LogLevel = MessageLogLevel.Ignore; MessageText = emptyString } // message would be redundant and adds noise
            | InvoiceCanceled jobBillingId ->
                { LogLevel = MessageLogLevel.Info; MessageText = $"JobBillingId {jobBillingId.Value} was marked canceled and not processed."  } 
            | ErrorCheckingIfJobIsCanceled jobBillingId ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"An error occurred checking if JobBillingId {jobBillingId.Value} was marked canceled."  } 
            | FailedToRetrieveJobToConstructJobBilling (jobBillingId, jobWorkId) ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to retrieve job {jobWorkId.Value} from SOR Concentrator while constructing verified job billing {jobBillingId.Value} for invoicing." }
            
            // *** Service2Builder ***    
            | VendorBillIsAlreadyCreatedInFal jobBillingId ->
                { LogLevel = MessageLogLevel.Warning; MessageText = $"Vendor bill is already created in FAL for verified job billing {jobBillingId.Value}." }
            | FailedToDetermineIfVendorBillIsCreatedInFal jobBillingId -> 
                { LogLevel = MessageLogLevel.Error; MessageText = $"Unable to determine if vendor bill is already created in FAL for job billing {jobBillingId.Value}." }
            | FailedToRetrieveDmgInvoiceNumberFromRedisToConstructProviderInvoice jobBillingId ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to generate DMG invoice number from Redis for job billing {jobBillingId.Value}." }
            | FailedToRetrievePropertyToConstructProviderInvoice (jobBillingId, propertyId) ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to retrieve property {propertyId.Value} from SOR Concentrator to get servicing address state code for job billing '{jobBillingId.Value}'." }
            | FailedToRetrieveServiceLineToConstructProviderInvoice (jobBillingId, serviceLineId) ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to retrieve service line {serviceLineId.Value} from SOR Concentrator to get service line name for job billing '{jobBillingId.Value}'." }                
            | FailedToRetrieveServiceTypeToConstructProviderInvoice (jobBillingId, serviceTypeId) ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to retrieve service type {serviceTypeId.Value} from SOR Concentrator to get service type name for job billing '{jobBillingId.Value}'." }                
            | FailedToQuerySorConcentratorToGetCatalogItemNamesToConstructProviderInvoice jobBillingId ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Unexpected SOR Concentrator error occurred while retrieving catalog items for job billing {jobBillingId.Value}." }
            | VendorBillLinesAreInvalid (errorMessage, jobBillingId) ->                
                { LogLevel = MessageLogLevel.Error; MessageText = $"Vendor bill lines are invalid for job billing {jobBillingId.Value}. {errorMessage.ToText()}" }
            | FailedToInsertVendorBillIntoFalDatabase jobBillingId ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to insert vendor bill into FAL for job billing {jobBillingId.Value}." }
            | BypassVendorBillGenerationDueToNoBillableLines (jobBillingId, ticketId, costingScheme, isPaidByCreditCard) ->
                { LogLevel = MessageLogLevel.Warning
                  MessageText = $"Bypassed generation of vendor bill due to zero billable lines."
                                + $" Job billing: {jobBillingId.Value}"
                                + $" Ticket: {ticketId.Value}"
                                + $" Costing: {this.CostSchemeText(costingScheme)}"
                                + $" CcPmt: {isPaidByCreditCard.ToString()}"}                
            | SuccessfullyGeneratedVendorBill (jobBillingId, ticketId, vendorBillInsertSuccess) ->
                { LogLevel = MessageLogLevel.Info
                  MessageText = $"Generated vendor bill {vendorBillInsertSuccess.DmgInvoiceNumber.Value}."
                                + $" Job billing: {jobBillingId.Value}"
                                + $" Ticket: {ticketId.Value}"
                                + $" Count: {vendorBillInsertSuccess.VendorBillLineCount.ToString()}"
                                + $" Costing: {this.CostSchemeText(vendorBillInsertSuccess.CostingScheme)}"
                                + $" CcPmt: {vendorBillInsertSuccess.JobBilling.Payment.IsPaidByCreditCard.ToString()}"}  
            | FailedToApproveInvoice ticketId ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to approve vendor bills for ticketId {ticketId.Value}." }
            | UnexpectedFailure ->
                { LogLevel = MessageLogLevel.Ignore; MessageText = emptyString } // message logging for exception handled elsewhere
            
        { resultMessageRaw with MessageText = normalizeString resultMessageRaw.MessageText } 

    // Determines whether message should be considered processed (i.e., a de-facto success)
    member this.MessageProcessingStatusResult() : ProcessMessageStatus =
        match this with
        // *** Service1Consumer ***
        | FailedToDeserializeJobBillingMessage (_, messageFailureCount, _)              -> ProcessMessageStatus.ConsiderProcessed   // TODO determine based on message failure count history
        | JobBillingNotYetVerifiedForInvoicing _                                        -> ProcessMessageStatus.ConsiderProcessed   // bypass a non-verified job billing message
        | InvoiceCanceled _                                                             -> ProcessMessageStatus.ConsiderProcessed   // bypass a canceled job billing
        // This can be removed after conversion from job billing gross to job billing is deterministic and does not require job        
        | FailedToRetrieveJobToConstructJobBilling _                                    -> ProcessMessageStatus.TransientError      // retry message later if job billing cannot be created
        | ErrorCheckingIfJobIsCanceled _                                                -> ProcessMessageStatus.TransientError      // retry message later to see if job billing was canceled
              
        // *** Service2Builder ***
        | VendorBillIsAlreadyCreatedInFal _                                             -> ProcessMessageStatus.ConsiderProcessed   // bypass message since its vendor bill is already invoiced
        | FailedToDetermineIfVendorBillIsCreatedInFal _                                 -> ProcessMessageStatus.TransientError      // retry message later since error occurred determining vendor bill status
        | FailedToRetrieveDmgInvoiceNumberFromRedisToConstructProviderInvoice _         -> ProcessMessageStatus.TransientError      // retry message later since provider invoice cannot be created without DMG invoice number
        | FailedToRetrievePropertyToConstructProviderInvoice _                          -> ProcessMessageStatus.TransientError      // retry message later since provider invoice cannot be created without property
        | FailedToRetrieveServiceLineToConstructProviderInvoice _                       -> ProcessMessageStatus.TransientError      // retry message later since provider invoice cannot be created without service line name
        | FailedToRetrieveServiceTypeToConstructProviderInvoice _                       -> ProcessMessageStatus.TransientError      // retry message later since provider invoice cannot be created without service type name
        | FailedToQuerySorConcentratorToGetCatalogItemNamesToConstructProviderInvoice _ -> ProcessMessageStatus.TransientError      // retry message later since provider invoice cannot be created without catalog item names
        | VendorBillLinesAreInvalid _                                                   -> ProcessMessageStatus.TransientError      // retry message later since an invalid vendor bill should not be inserted into FAL
        | FailedToInsertVendorBillIntoFalDatabase _                                     -> ProcessMessageStatus.TransientError      // retry message later if vendor bill failed to insert into FAL
        | BypassVendorBillGenerationDueToNoBillableLines _                              -> ProcessMessageStatus.ConsiderProcessed   // a bypass due to no billable lines is expected, and thus a success
        | SuccessfullyGeneratedVendorBill _                                             -> ProcessMessageStatus.ConsiderProcessed   // success
        | FailedToApproveInvoice _                                                      -> ProcessMessageStatus.TransientError      // retry message later 
        
        // *** Common ***
        | UnexpectedFailure _                                                           -> ProcessMessageStatus.TransientError      // retry

        
// Error messages that should be logged but not cause converting a job billing message into an invoice to fail.
type NonFailureErrorMessage =
    // *** Service1Consumer ***
    | CostingNotDefinedOnJob of JobBillingId * JobWorkId
    
    // *** Service2Builder ***    
    | FailedToFindServicingAddressOnProperty of JobBillingId * PropertyId    
    | FailedToFindStateCodeOnPropertyServicingAddress of JobBillingId * PropertyId
    | FailedToLookupStateNameFromServicingAddressStateCode of JobBillingId * PropertyId
    
    // Build message text for result case
    member this.ToLogMessageText() : string =
        match this with
        // *** Service1Consumer ***
        | CostingNotDefinedOnJob (jobBillingId, jobWorkId) ->
            $"Costing (Regular rate type) is not defined on job {jobWorkId.Value} while constructing verified job billing {jobBillingId.Value} for invoicing."                 
        
        // *** Service2Builder ***    
        | FailedToFindServicingAddressOnProperty (jobBillingId, propertyId) ->
            $"Failed to find servicing address on property {propertyId.Value} for job billing {jobBillingId.Value}."                 
        | FailedToFindStateCodeOnPropertyServicingAddress (jobBillingId, propertyId) ->
            $"Failed to find a state value on servicing address of property {propertyId.Value} for job billing {jobBillingId.Value}." 
        | FailedToLookupStateNameFromServicingAddressStateCode (jobBillingId, propertyId) ->
            $"Failed to lookup servicing address state name from code on property {propertyId.Value} for job billing {jobBillingId.Value}." 