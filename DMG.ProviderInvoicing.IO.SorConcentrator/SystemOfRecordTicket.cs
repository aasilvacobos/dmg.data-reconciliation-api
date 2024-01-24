using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.DT.Domain;
using LanguageExt;
using static LanguageExt.Prelude;
using Dmg.Tickets.V1;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;

namespace DMG.ProviderInvoicing.IO.SorConcentrator; 

/// <summary>
/// API consumed by other I/O adapters to retrieve ticket from the SOR Concentrator.
/// </summary>
internal static class SystemOfRecordTicket
{
    internal static async Task<Either<ErrorMessage, DT.Domain.Ticket>> GetByIdAsync(TicketId ticketId) =>
        (await SorConcentratorClient.GetByIdAsync<Dmg.Tickets.V1.Ticket>(ticketId.Value, SorEntityName.Ticket))
        .Map(TicketMessageMapper.ToEntity)
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.TicketNotFound, // convert generic error message to entity specific message
                _ => errorMessage
            });
}