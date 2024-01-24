namespace DMG.ProviderInvoicing.DT.Domain

open System

[<AutoOpen>]
module internal ErrorMessageUtil =
    let commaSpaced = ", "

    let normalizeString str = // TODO figure out if this can return a C# "nullable reference type"
        str
        |> Option.ofObj
        |> Option.defaultValue emptyString // prevents a null value

/// List of error messages throughout application. Copied from Denali - need to evolve this.
type ErrorMessage =
    | UnexpectedException of exn
    /// Use in scenario where expected errors are being detected but a case is not covered. (i.e., a wildcard case for message).
    | UndefinedError of string    
    | LogErrorTestMessage
    | ProviderBillingError of string
    
    // Database
    | ResourceNotFound
    | InvalidOperation of string
    | DatabasePingRequestTimeout
    | DatabaseUnavailable

    // General Validation
    | RequiredField of string
    | InvalidValue of string * string * string seq

    // String Validation
    | StringExceedsMaximumSize of string * int
    | StringIsEmptyOrWhiteSpace of string   // if value is provided for string, it cannot be empty or white space

    // External Systems
    | SorConcentratorEntityNotFound of Guid * string
    | SorConcentratorTopicConnectionFailure of string * string
    | SorConcentratorClientNotFound
    | WorkFulfillmentWriterClientNotFound
    | WorkFulfillmentReaderClientNotFound
    | IoAdapterClientNotFound of string
    | TimeoutExceeded of string * string
    | Unavailable of string * string
     
    // JobPhotoDeleteFulfillment
    | JobPhotoDeleteFulfilmentEmptyPhotoList 

    // JobDocumentDeleteFulfillment
    | JobDocumentDeleteFulfilmentEmptyDocumentList 

     // Job billing patch
    | JobBillingPatchFailureVersionMismatch 
    | JobBillingPatchProviderIdMissingInvalid of Guid
    | JobBillingPatchLineItemIsADuplicate
    | JobBillingPatchLineItemIsMissingCatalogItemRuleValues
    | JobBillingPatchLineCatalogItemRuleValueInvalid
    | JobBillingPatchLineCatalogItemRuleValueExceedsLabor

    // JobBillingReviewRequestCreate
    | JobBillingReviewRequestCreateAlreadyExists of Guid

    // JobBillingDisputeResponsePut
    | JobBillingDisputeResponsePutLineItemNotFound of Guid    
    | JobBillingDisputeResponseMissingConversationId of Guid
    | JobBillingDisputeResponsePutAlreadyExists
    | JobBillingDisputeResponsePutLineItemIsDuplicate

    // JobBillingSubmitForInvoicing
    | JobBillingSubmitForInvoicingBillingAlreadyInProgress of Guid

    // Vendor bill validation
    | VendorBillTotalAmountIsNegative of Guid
    
    // Provider invoice validation messages for user
    | JobNotFound
    | JobBillingNotFound
    | JobBillingUnavailableDueToInternalFulfillmentError of string
    | InvoiceFileDeleteCountExceedsMaximum of int * int
    | ServiceTypeBillableItemNotDefined
    | CatalogItemNotFound
    | CustomerNotFound
    | PropertyNotFound
    | ProviderOrgNotFound
    | ServiceLineNotFound
    | ServiceTypeNotFound 
    | UserNotFound
    | ProviderServiceAgreementNotFound
    | ContractTermSheetNotFound
    | TicketNotFound
    | TicketBillingNotFound
    
    //Shared Error Messages
    | FulfillmentMutationWorkIdNotFound of Guid
    | FulfillmentMutationProviderIdNotFound of Guid 
    | FulfillmentMutationSessionUserIdNotFound of Guid 
    | FulfillmentMutationUserDoesNotHavePermission of Guid 
    | FulfillmentMutationUserDoesNotBelongToProvider of Guid
    
    //Provider Billing Io Messages
    | ProviderBillingNotFound

    //Fal Io Messages
    | ProviderInvoiceNotFound

    member this.ToText() =
        match this with
        | FulfillmentMutationWorkIdNotFound id ->
            $"Job WorkId %s{id.ToString()} was not found in Fulfillment."
        | FulfillmentMutationProviderIdNotFound id ->
            $"ProviderId %s{id.ToString()} was not found in Fulfillment."
        | FulfillmentMutationSessionUserIdNotFound id ->
            $"Session UserId %s{id.ToString()} was not found in Fulfillment."
        | FulfillmentMutationUserDoesNotHavePermission id ->
            $"UserId %s{id.ToString()} does not have permission to perform the requested action."
        | FulfillmentMutationUserDoesNotBelongToProvider id ->
            $"UserId %s{id.ToString()} does not belong to provider."

        | UnexpectedException ex -> $"Unexpected exception. %s{ex.Message}"
        | UndefinedError msg -> $"Undefined error encountered. %s{msg}"        
        | LogErrorTestMessage -> "Logging Error test message"
        | ProviderBillingError msg -> msg
        
        // Local Database
        | ResourceNotFound -> @"Item not found."
        | InvalidOperation msg -> $"Invalid operation. %s{msg}"
        | DatabaseUnavailable -> "Provider invoicing database is unavailable."
        | DatabasePingRequestTimeout -> "Ping request to database timed out."

        // General Validation
        | RequiredField fieldName -> $"%s{fieldName} is required."
        | InvalidValue (fieldName, invalidValue, validValues) ->
            $"%s{fieldName} '{invalidValue |> normalizeString}' is invalid. Valid values are {validValues |> (concat commaSpaced)}."

        // String Validation
        | StringExceedsMaximumSize (fieldName, maxLength) -> $"%s{fieldName} exceeds its maximum size of {maxLength}."
        | StringIsEmptyOrWhiteSpace fieldName -> $"%s{fieldName} text cannot be empty."

        // External Systems
        | SorConcentratorEntityNotFound (id, sorName) ->
            $"Entity %s{id.ToString()} not found in SOR Concentrator for %s{sorName}."
        | SorConcentratorTopicConnectionFailure (sorName, exceptionMessage) ->
            $"Connection failure to SOR %s{sorName}. %s{exceptionMessage}"
        | SorConcentratorClientNotFound ->
            $"The client interface for the SOR concentrator could not be created."
        | WorkFulfillmentWriterClientNotFound ->
            $"The client interface for the work fulfillment writer API could not be created."
        | WorkFulfillmentReaderClientNotFound -> 
            $"The client interface for the work fulfillment reader API could not be created."
        | IoAdapterClientNotFound ioAdapterName -> 
            $"The client interface for the I/O adapter {ioAdapterName} could not be created."
        | TimeoutExceeded (ioAdapterName, remoteCall) ->
            $"The remote server for {ioAdapterName} timed out while trying to call { remoteCall }."
        | Unavailable (ioAdapterName, remoteCall) ->
            $"The remote server for {ioAdapterName} was unavailable when trying to call {remoteCall}."
        
        // JobPhotoDeleteFulfilment
        | JobPhotoDeleteFulfilmentEmptyPhotoList ->
            $"The photo ID list is empty."

        // JobDocumentDeleteFulfilment
        | JobDocumentDeleteFulfilmentEmptyDocumentList ->
            $"The document ID list is empty."

        // Job billing patch
        | JobBillingPatchFailureVersionMismatch ->
            $"Problem occurred. Please refresh the page."
        | JobBillingPatchProviderIdMissingInvalid id ->
            $"Job ProviderId %s{id.ToString()} was not found in Fulfillment."
        | JobBillingPatchLineItemIsADuplicate ->
            $"A duplicate line item was submitted for the job."
        | JobBillingPatchLineItemIsMissingCatalogItemRuleValues ->
            $"One or more line items are missing required Catalog Item Rules Values."
        | JobBillingPatchLineCatalogItemRuleValueInvalid ->
            $"One or more line items contain invalid Catalog Item Rule Values."
        | JobBillingPatchLineCatalogItemRuleValueExceedsLabor ->
            $"Total hours on Catalog Item Rule Values exceed total labor hours."

        // JobBillingReviewRequestCreate
        | JobBillingReviewRequestCreateAlreadyExists id -> 
            $"The provider %s{id.ToString()} has already submitted a review. Please wait for a response."

        // JobBillingDisputeResponsePut
        | JobBillingDisputeResponsePutLineItemNotFound id -> 
            $"Line item ID %s{id.ToString()} was not found."
        | JobBillingDisputeResponseMissingConversationId id ->
            $"Line item ID %s{id.ToString()} does not contain an active dispute."
        | JobBillingDisputeResponsePutAlreadyExists ->
            $"A response has already been submitted."
        | JobBillingDisputeResponsePutLineItemIsDuplicate ->
            $"The provided line item is a duplicate."

        // JobBillingSubmitForInvoicing
        | JobBillingSubmitForInvoicingBillingAlreadyInProgress id ->
            $"Job Billing Id %s{id.ToString()} was already submitted for invoicing."

        // Vendor bill validation 
        | VendorBillTotalAmountIsNegative jobBillingId ->
            $"Vendor bill generated from job billing %s{jobBillingId.ToString()} has a negative total amount (cost)."
        
        // Job billing review/verification UI messages 
        | JobNotFound -> "Job information could not be found."
        | JobBillingNotFound -> "Job billing information could not be found."
        | JobBillingUnavailableDueToInternalFulfillmentError errorMsg -> $"Job billing data is unavailable due to system issue. %s{errorMsg}"
        | InvoiceFileDeleteCountExceedsMaximum (attemptedDeleteCount, maxAllowedDeleteCount) ->
            $"Provider invoice file delete count (%i{attemptedDeleteCount}) must be between 1 and (%i{maxAllowedDeleteCount})."
        | ServiceTypeBillableItemNotDefined ->
            $"Material/equipment billable items have not been defined for this invoice's service type."
        | CatalogItemNotFound -> "Catalog item information could not be found."
        | CustomerNotFound -> "Customer information could not be found."
        | PropertyNotFound -> "Property information could not be found."
        | ProviderOrgNotFound -> "Provider org information could not be found."
        | ServiceLineNotFound -> "Service line information could not be found."
        | ServiceTypeNotFound -> "Service type information could not be found."
        | UserNotFound -> "User information could not be found."
        | ProviderServiceAgreementNotFound -> "Provider Service Agreement could not be found"
        | TicketNotFound -> "Ticket information could not be found."
        | TicketBillingNotFound -> "Ticket Billing information could not be found."
        
        //Provider Billing Io Messages
        | ProviderBillingNotFound ->
            $"Provider Billing was not found in the database."

        //Fal Io Messages
        | ProviderInvoiceNotFound ->
            $"Provider Invoice was not found in the database."
        
        |> normalizeString

