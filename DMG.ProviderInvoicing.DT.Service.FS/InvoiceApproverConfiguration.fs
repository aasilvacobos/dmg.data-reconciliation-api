namespace DMG.ProviderInvoicing.DT.Service.InvoiceApprover

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
    { LogLevel:         MessageLogLevel
      MessageText:      string }
    
// All possible results for processing a job billing message for invoicing
type ProcessMessageResult =
    // *** Service1Consumer ***
    | FailedToDeserializeCustomerInvoiceMessage of CustomerInvoiceExternalId * MessageFailureCount: int * Exception
    | NotAllLinesCreatedYetOnCustomerInvoice of CustomerInvoiceExternalId
    | TicketOnCustomerInvoiceNotFound of CustomerInvoiceExternalId * TicketId
    | TicketNotFound of TicketId
    | FailedToConnectToSorConcentratorToRetrieveTicketOnCustomerInvoice of CustomerInvoiceExternalId * TicketId
    | FailedToConnectToSorConcentratorToRetrieveTicket of TicketId
    | FailedToUpdateVendorBillStatusAsApprovedInFal of Guid // use guid here because it could be a billing ticket or a tracking ticket
    | SuccessfullyApprovedAllVendorBills
    | UnexpectedFailure
    | FailedToDeserializeEventBusMessage
    | IgnoredEventBusMessage of eventType:string
    | FailedToAddPendingApproval of TicketId

    // Build message and log level used for logging message based on result case
    member this.ToResultLogMessage() : ResultLogMessage =
        let resultMessageRaw = 
            match this with
            // *** Service1Consumer ***
            | FailedToDeserializeCustomerInvoiceMessage (customerInvoiceExternalId, messageFailureCount, ex) ->
                { LogLevel = MessageLogLevel.Emergency; MessageText = $"Unable to deserialize customer invoice {customerInvoiceExternalId.Value} message from topic. {ex.Message}" }
            | NotAllLinesCreatedYetOnCustomerInvoice _ ->
                { LogLevel = MessageLogLevel.Ignore; MessageText = emptyString }    // a log message would add noise
            | TicketOnCustomerInvoiceNotFound (customerInvoiceExternalId, ticketId) ->
                { LogLevel = MessageLogLevel.Emergency; MessageText = $"Ticket {ticketId.Value} does not exist for customer invoice {customerInvoiceExternalId.Value}." }
            | TicketNotFound ticketId ->
                { LogLevel = MessageLogLevel.Emergency; MessageText = $"Ticket {ticketId.Value} does not exist." }
            | FailedToConnectToSorConcentratorToRetrieveTicketOnCustomerInvoice (customerInvoiceExternalId, ticketId) ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Unexpected SOR Concentrator query/connection error trying to retrieve ticket {ticketId.Value} for customer invoice {customerInvoiceExternalId.Value}." }
            | FailedToConnectToSorConcentratorToRetrieveTicket (ticketId) ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Unexpected SOR Concentrator query/connection error trying to retrieve ticket {ticketId.Value}." }
            | FailedToUpdateVendorBillStatusAsApprovedInFal ticketGuid ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to update vendor bill statuses to approved in FAL for ticket {ticketGuid}." }                
            | SuccessfullyApprovedAllVendorBills ->
                { LogLevel = MessageLogLevel.Ignore; MessageText = emptyString }    // a log message would add noise
            | UnexpectedFailure ->
                { LogLevel = MessageLogLevel.Debug; MessageText = emptyString }    // message logging for exception handled elsewhere  //2023-08-02 -- TJ -- changed to debug due to invoices not getting approved but no error being logged, hopefully this will help  
            | FailedToDeserializeEventBusMessage ->
                { LogLevel = MessageLogLevel.Error; MessageText = emptyString }    
            | IgnoredEventBusMessage eventType ->
                { LogLevel = MessageLogLevel.Debug; MessageText = $"ignoring event type: {eventType}" }    
             | FailedToAddPendingApproval ticketGuid ->
                { LogLevel = MessageLogLevel.Error; MessageText = $"Failed to add pending approval entry in provider invoicing database for ticket {ticketGuid}." }                

        { resultMessageRaw with MessageText = normalizeString resultMessageRaw.MessageText } 

    // Determines whether message should be considered processed (i.e., a de-facto success)
    member this.IsMessageConsideredProcessed() : bool =
        match this with
        | FailedToDeserializeCustomerInvoiceMessage (_, messageFailureCount, _)         -> false    // retry, TODO determine based on message failure count history
        | NotAllLinesCreatedYetOnCustomerInvoice _                                      -> true     // bypass a non-completed customer invoice message (this is expected)
        | TicketOnCustomerInvoiceNotFound _                                             -> true     // serious error, but skip message since it "'shouldn't happen' [in production]" - Neil Taylor, 1/23/2023
        | TicketNotFound _                                                              -> false    // serious error
        | FailedToConnectToSorConcentratorToRetrieveTicketOnCustomerInvoice _           -> false    // error should resolve after retries
        | FailedToConnectToSorConcentratorToRetrieveTicket _                            -> false    // error should resolve after retries
        | FailedToUpdateVendorBillStatusAsApprovedInFal _                               -> false    // error should resolve after retries
        | SuccessfullyApprovedAllVendorBills _                                          -> true     // success
        | UnexpectedFailure _                                                           -> false    // error should resolve after retries
        | FailedToDeserializeEventBusMessage _                                          -> true     // incorrect message format     
        | IgnoredEventBusMessage _                                                      -> true     // ignoring message   
        | FailedToAddPendingApproval _                                                  -> false    // error should resolve after retries
