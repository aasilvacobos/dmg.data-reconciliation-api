namespace DMG.ProviderInvoicing.DT.Service

open FsToolkit.ErrorHandling

type TopicName = internal TopicName of string with
    member this.Value = let (TopicName s) = this in s
    interface IWrappedString with
        member this.Value = this.Value 
module TopicName =
    type private typeAlias = TopicName
    let private ctor = TopicName
    let private isValid (_: string) = true
    let value (wrappedValue: typeAlias) = wrappedValue.Value // redirect to interface property
    let create str =
        WrappedString.tryCreate id(*canonicalize*) isValid ctor str
        |> Option.ofResult
        |> Option.defaultValue (TopicName emptyString) // safe default
  
/// SOR entity/topic name
type SorEntityName =
    | Work
    | JobBilling
    | TicketBilling
    | Ticket
    | Customer
    | Property
    | ProviderOrg
    | User
    | ServiceLine
    | ServiceType
    | CatalogItem
    | CustomerInvoice
    | EventBusEvent
    | ProviderServiceAgreement
    | ContractTermSheetId
    with
        override this.ToString() = this |> Union.fromCaseToString
    
module SorConcentratorConfiguration =
    let GetSorTopicName (sorEntityName: SorEntityName) : TopicName =
        match sorEntityName with
        | Work ->  TopicName.create @"fulfilment.work.state" 
        | JobBilling -> TopicName.create @"fulfilment.workbilling.state"
        | TicketBilling -> TopicName.create @"customerinvoicing.ticketbilling.state"
        | Ticket -> TopicName.create @"fulfilment.ticket.state"
        | Customer -> TopicName.create @"customer.customer.state" 
        | Property -> TopicName.create @"customer.property.state"
        | ProviderOrg -> TopicName.create @"provider.org.state"
        | User -> TopicName.create @"user.user.state" 
        | ServiceLine -> TopicName.create @"dataservices.serviceline.state"
        | ServiceType ->TopicName.create @"dataservices.servicetype.state" 
        | CatalogItem ->TopicName.create @"itemcatalog.state"
        | CustomerInvoice -> TopicName.create @"finance.customerinvoice.state"
        | EventBusEvent -> TopicName.create @"dmg.eventbus.events"
        | ProviderServiceAgreement -> TopicName.create @"provideragreements.agreement.state"
        | ContractTermSheetId -> TopicName.create @"customer.termsheets.state"
        
    let GetSorEntityNames () =
        Union.getCases<SorEntityName>