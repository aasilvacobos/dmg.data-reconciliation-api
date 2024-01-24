using System;
using System.Diagnostics.Contracts;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

/// Types of service request
public enum TicketRequestType 
{
    RequestTypeUnknown,
    ServiceRequest,
    ProposalRequest,
    ServiceComplaint,
    GeneralQuery,
    InternalOpsRequest,
    NonRoutineServiceRequestRegular,
    RoutineServiceRequestRegular,
    NonRoutineServiceRequestComplaint,
    RoutineServiceRequestComplaint,
    NonRoutineProposalRequestRegular,
    NonRoutineServiceRequestRegularInspection
}

public enum TicketType
{
    Unspecified,
    Billing,
    Tracking,
    TrackingAndBilling
}

/// Transactional Data **AGGREGATE**: The customer ticket entity.
public record Ticket(
    TicketId                                    TicketId,
    CustomerId                                  CustomerId,
    string                                      TicketNumber,
    TicketRequestType                           RequestType,
    TicketType                                  TicketType,
    bool                                        IsWorkVerificationRequired,
    string                                      WorkOrderNumber,
    ContractTermSheetId                         ContractTermSheetId,
    Option<TicketAssociation>                   Association);
public record TicketAssociation(
    Lst<TicketAssociationWork>                  WorkAssociations,
    Lst<TicketAssociationTicket>                TicketAssociations);
public record TicketAssociationWork(
    JobWorkId                                   JobWorkId);
public record TicketAssociationTicket(
    TicketId                                    TicketId,
    // detail collections
    Lst<TicketAssociationWork>                  WorkAssociations);