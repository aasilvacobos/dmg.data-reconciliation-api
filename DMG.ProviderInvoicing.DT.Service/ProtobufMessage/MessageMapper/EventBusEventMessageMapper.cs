using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.DT.Domain;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper; 

public static class EventBusEventMessageMapper
{
    /// Place holder until the event PI needs is created
    public static TicketNoChargeEvent ToEntity(EventBus.TicketNoChargeEvent message) =>
        new (new TicketId(ParseGuidStringDefaultToEmptyGuid(message.TicketId)));
    
    private static IEventPayload ToEntityPayload(EventBus.EventBusEvent message) =>
        message.PayloadCase switch
        {
            EventBus.EventBusEvent.PayloadOneofCase.TicketNoChargeEvent => ToEntity(message.TicketNoChargeEvent),
            _ => new OtherEvent()
        };
    
    public static Option<TicketNoChargeEvent> TryTicketNoCharge(EventBus.EventBusEvent message) =>
        message.PayloadCase switch
        {
            EventBus.EventBusEvent.PayloadOneofCase.TicketNoChargeEvent => Some(ToEntity(message.TicketNoChargeEvent)),
            _ => Option<TicketNoChargeEvent>.None
        };
     

    public static EventBusEvent ToEntity(EventBus.EventBusEvent message) =>
        new(new EventId(ParseGuidStringDefaultToEmptyGuid(message.EventId)),
            ToDateTimeOffsetDefaultToMinimumDate(message.WhenUtc),
            NonEmptyText.NewUnsafe(message.EventName.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)),
            ToEntityPayload(message),
            TryParseGuidString(message.TargetId));
}