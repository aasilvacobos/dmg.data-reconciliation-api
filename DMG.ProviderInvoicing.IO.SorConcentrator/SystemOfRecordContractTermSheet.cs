using DMG.Proto.CustomerContracts;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Service;
using DMG.ProviderInvoicing.IO.SorConcentrator.Common;
using DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;
using LanguageExt;
using static LanguageExt.Prelude;
using Dmg.Work.V1;

namespace DMG.ProviderInvoicing.IO.SorConcentrator; 

/// <summary>
/// API consumed by other I/O adapters to retrieve job data from the SOR Concentrator.
/// </summary>
internal static class SystemOfRecordCustomerTermSheet {

    internal static async Task<Either<ErrorMessage, ContractTermsSheet>> GetByIdAsync(ContractTermSheetId contractTermSheetId) =>
        await SorConcentratorClient.GetByIdAsync<ContractTermsSheet>(contractTermSheetId.Value, SorEntityName.ContractTermSheetId)
            .Map(x => x.MapLeft(errorMessage =>
        errorMessage switch
        {
            ErrorMessage.SorConcentratorEntityNotFound => ErrorMessage.ContractTermSheetNotFound, // convert generic error message to job specific message 
            _ => errorMessage
        }));
}