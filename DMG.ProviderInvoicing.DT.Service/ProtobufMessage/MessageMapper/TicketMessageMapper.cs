using DMG.ProviderInvoicing.DT.Domain;
using LanguageExt;
using static LanguageExt.Prelude;
using Dmg.Tickets.V1;
using Google.Protobuf.Collections;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;
using TicketType = DMG.ProviderInvoicing.DT.Domain.TicketType;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between ticket protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class TicketMessageMapper 
{
    public static TicketRequestType ToRequestType(RequestType requestTypeMessage) =>
        requestTypeMessage
            switch 
            {
                RequestType.ServiceRequest => TicketRequestType.ServiceRequest,
                RequestType.ProposalRequest => TicketRequestType.ProposalRequest,
                RequestType.ServiceComplaint => TicketRequestType.ServiceComplaint,
                RequestType.GeneralQuery => TicketRequestType.GeneralQuery,
                RequestType.InternalOpsRequest => TicketRequestType.InternalOpsRequest,
                RequestType.NonRoutineServiceRequestRegular => TicketRequestType.NonRoutineServiceRequestRegular,
                RequestType.RoutineServiceRequestRegular => TicketRequestType.RoutineServiceRequestRegular,
                RequestType.NonRoutineServiceRequestComplaint => TicketRequestType.NonRoutineServiceRequestComplaint,
                RequestType.RoutineServiceRequestComplaint => TicketRequestType.RoutineServiceRequestComplaint,
                RequestType.NonRoutineProposalRequestRegular => TicketRequestType.NonRoutineProposalRequestRegular,
                RequestType.NonRoutineServiceRequestRegularInspection => TicketRequestType.NonRoutineServiceRequestRegularInspection,
                RequestType.Unknown => TicketRequestType.RequestTypeUnknown,
                _ => TicketRequestType.RequestTypeUnknown
            };

    public static TicketType ToTicketType(Dmg.Tickets.V1.TicketType ticketType) =>
        ticketType
            switch
            {
                Dmg.Tickets.V1.TicketType.Unspecified => TicketType.Unspecified,
                Dmg.Tickets.V1.TicketType.Billing => TicketType.Billing,
                Dmg.Tickets.V1.TicketType.Tracking => TicketType.Tracking,
                Dmg.Tickets.V1.TicketType.TrackingAndBilling => TicketType.TrackingAndBilling
            };
    
    /// <summary>
    /// Function to convert a gRPC work association to a <see cref="WorkAssociation"/>.
    /// </summary>
    /// <param name="workAssociation">The work association to convert.</param>
    /// <returns>The converted domain object.</returns>
    private static TicketAssociationWork ToTicketAssociationWork(Dmg.Tickets.V1.WorkAssociation workAssociation)
        => new(new JobWorkId(ParseGuidStringDefaultToEmptyGuid(workAssociation.WorkId)));

    private static Lst<TicketAssociationWork> ToTicketAssociationWorks(RepeatedField<Dmg.Tickets.V1.WorkAssociation> workAssociationMessages) =>
        workAssociationMessages.Freeze()
            .Filter(workAssociationMessage => !string.IsNullOrWhiteSpace(workAssociationMessage.WorkId))    // Yes, this is occurring - and frequently!
            .Map(ToTicketAssociationWork);
    
    /// <summary>
    /// Function to convert a gRPC ticket association to a <see cref="TicketAssociationTicket"/>.
    /// </summary>
    /// <param name="ticketAssociationMessage">The ticket association to convert.</param>
    /// <returns>The converted domain object.</returns>
    private static TicketAssociationTicket ToTicketAssociationTicket(Dmg.Tickets.V1.TicketAssociation ticketAssociationMessage)
        => new(
            new TicketId(ParseGuidStringDefaultToEmptyGuid(ticketAssociationMessage.TicketId)),
            Optional(ticketAssociationMessage.WorkAssociations)
                .Map(ToTicketAssociationWorks)
                .ToList()
                .Flatten());

    /// <summary>
    /// Function to convert gRPC associations to a <see cref="Domain.TicketAssociation"/>.
    /// </summary>
    /// <param name="associations">The associations to convert.</param>
    /// <returns>The converted domain object.</returns>
    private static Domain.TicketAssociation ToTicketAssociation(Dmg.Tickets.V1.Associations associations)
        => new(
            Optional(associations.WorkAssociations)
                .Map(ToTicketAssociationWorks)
                .ToList()
                .Flatten(),
            Optional(associations.TicketAssociations)
                .Match(
                    ticketAssociationMessages => ticketAssociationMessages.Map(ToTicketAssociationTicket).Freeze(),
                    () => Lst<TicketAssociationTicket>.Empty));
    
    public static DT.Domain.Ticket ToEntity(Dmg.Tickets.V1.Ticket ticketMessage) =>
        new(new(ParseGuidStringDefaultToEmptyGuid(ticketMessage.TicketId)),
            new(ParseGuidStringDefaultToEmptyGuid(ticketMessage.CustomerId)),
// TODO - (MW-20220725) - These field should be NonEmptyText.
            ticketMessage.TicketNumber,
            ToRequestType(ticketMessage.RequestType),
            ToTicketType(ticketMessage.TicketType),            
            ticketMessage.WorkVerificationRequired,
// TODO - (MW-20220725) - These field should be NonEmptyText.
            ticketMessage.WorkOrderNumber,
            new(ParseGuidStringDefaultToEmptyGuid(ticketMessage.ContractTermSheetId)),
            Optional(ticketMessage.Associations)
                .Match(x => Some(ToTicketAssociation(x)), 
                       () => None)); 
}