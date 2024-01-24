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
internal static class SystemOfRecordTicketBilling
{
    internal static async Task<Either<ErrorMessage, DMG.TicketBilling.TicketBilling>> GetByIdAsync(TicketBillingId ticketBillingId) =>
        (await SorConcentratorClient.GetByIdAsync<DMG.TicketBilling.TicketBilling>(ticketBillingId.Value, SorEntityName.TicketBilling))
        .MapLeft(errorMessage =>
            errorMessage switch
            {
                ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.TicketBillingNotFound, // convert generic error message to entity specific message
                _ => errorMessage
            });
}