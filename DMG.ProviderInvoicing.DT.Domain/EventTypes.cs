using System;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain;

/// An event payload of an event bus event
public interface IEventPayload { } 
    
/// Will not be used in PI. It is an example event used to build logic until the event we need is created. 
public record TicketNoChargeEvent(
    TicketId                 TicketId) : IEventPayload; 

/// Event payload for events other than the ones PI consumes. 
public record OtherEvent : IEventPayload; 

/// General event from event bus
/// https://buf.build/divisions-maintenance-group/dmg-eventbus-proto/docs/main:dmg.event_bus.v1
public record EventBusEvent(
    EventId                  EventId,
    DateTimeOffset           WhenDateTime, 
    NonEmptyText             EventName, 
    IEventPayload            EventPayload,
    // optionals
    Option<Guid>             TargetId);