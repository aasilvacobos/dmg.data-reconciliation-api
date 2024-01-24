using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

public static class EventBusEventMapper
{
    // TODO change to use event required by PI instead of TicketWorkCompleteEvent
    public static Option<TicketNoChargeEvent> TryToTicketNoChargeEvent(EventBusEvent eventBusEvent) =>
        eventBusEvent.EventPayload switch 
        {
            TicketNoChargeEvent e => Option<TicketNoChargeEvent>.Some(e),
            _ => Option<TicketNoChargeEvent>.None
        };
}